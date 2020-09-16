using System.Text;
using System.Net;
using System;
using System.IO;
using System.Threading.Tasks;
using Helpers;
using System.Globalization;

namespace HttpdLib
{
    public class HttpClientHandler
    {
        private const string CrLf = "\r\n";
        private static readonly byte[] CrLfBuffer = Encoding.UTF8.GetBytes(CrLf);

		private readonly TcpClientAdapter _client;
		private readonly HttpRequestParser _requestParser;
        private readonly EndPoint _remoteEndPoint;
        private Stream _stream;

		public delegate Task THandler(HttpRequest rq, HttpResponse rp);

		THandler Handler = null;

		public HttpClientHandler(TcpClientAdapter client, HttpRequestParser requestParser, THandler handler)
        {
            _remoteEndPoint = client.RemoteEndPoint;
            _client = client;
			_requestParser = requestParser;
			Handler = handler;

			//LogInfo("Got Client {0}", _remoteEndPoint);

			Task.Factory.StartNew(Process);
        }

        private async Task InitializeStream()
        {
            if (_client is SSlTcpClientAdapter)
            {
                await ((SSlTcpClientAdapter)_client).AuthenticateAsServer().ConfigureAwait(false);
            }

            _stream = new BufferedStream(_client.Stream, 8096);
        }

        private async void Process()
        {
            try
            {
                await InitializeStream();

                while (_client.Connected)
                {
                    var limitedStream = new NotFlushingStream(new LimitedStream(_stream));
					var request = await _requestParser.Parse(new MyStreamReader(limitedStream)).ConfigureAwait(false);

                    if (request != null)
                    {
						var response = new HttpResponse();

						// Qua va il request HAndler
						await Handler(request, response).ConfigureAwait(false);

                        await WriteResponse(request, response, limitedStream).ConfigureAwait(false);
                        await limitedStream.ExplicitFlushAsync().ConfigureAwait(false);

						String Connection = request.Headers["Connection"];							
						if (Connection == null || Connection.ToLower() != "keep-alive")
                            _client.Close();
                    }
                    else
                    {
                        _client.Close();
                    }
                }
            }
            catch (Exception Ex)
            {
                // Hate people who make bad calls.
                //Logger.WarnException(string.Format("Error while serving : {0}", _remoteEndPoint), e);
                _client.Close();
				LogHelper.Error(Ex);
            }

            //Logger.InfoFormat("Lost Client {0}", _remoteEndPoint);
        }

		private async Task WriteResponse(HttpRequest reqeust, HttpResponse response, Stream writer)
        {
			// Headers
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("HTTP/1.1 {0} {1}\r\n", (int)response.ResponseCode, response.ResponseCode);

			foreach (var header in response.Headers)
			{
				sb.AppendFormat("{0}: {1}\r\n", header.Key, header.Value);
			}

			sb.AppendFormat("Date: {0}\r\n", DateTime.UtcNow.ToString("R"));
			//sb.AppendFormat("connection: \r\n");
			sb.AppendFormat("content-length: {0}\r\n\r\n", response.Body.Length);

			byte[] vb = Encoding.UTF8.GetBytes(sb.ToString());
			await writer.WriteAsync(vb,0,vb.Length).ConfigureAwait(false);
			writer.Flush();

			// Body
			response.Body.Seek(0, System.IO.SeekOrigin.Begin);
			await response.Body.CopyToAsync(writer,512*1024).ConfigureAwait(false);
			writer.Flush();
		}

        public void ForceClose()
        {
            _client.Close();
        }

    }

    internal class NotFlushingStream : Stream
    {
        private readonly Stream _child;
        public NotFlushingStream(Stream child)
        {
            _child = child;
        }


        public void ExplicitFlush()
        {
            _child.Flush();
        }

        public Task ExplicitFlushAsync()
        {
            return _child.FlushAsync();
        }

        public override void Flush()
        {
            // _child.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _child.Seek(offset, origin);
        }
        public override void SetLength(long value)
        {
            _child.SetLength(value);
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            return _child.Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
            return _child.ReadByte();
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            _child.Write(buffer, offset, count);
        }
        public override void WriteByte(byte value)
        {
            _child.WriteByte(value);
        }
        public override bool CanRead
        {
            get { return _child.CanRead; }
        }
        public override bool CanSeek
        {
            get { return _child.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return _child.CanWrite; }
        }
        public override long Length
        {
            get { return _child.Length; }
        }
        public override long Position
        {
            get { return _child.Position; }
            set { _child.Position = value; }
        }
        public override int ReadTimeout
        {
            get { return _child.ReadTimeout; }
            set { _child.ReadTimeout = value; }
        }
        public override int WriteTimeout
        {
            get { return _child.WriteTimeout; }
            set { _child.WriteTimeout = value; }
        }
    }
}
