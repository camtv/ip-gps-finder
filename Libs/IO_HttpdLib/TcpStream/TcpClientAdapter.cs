using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace HttpdLib
{
    public class TcpClientAdapter 
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;

        public TcpClientAdapter(TcpClient client)
        {
            _client = client;
            _stream = _client.GetStream();

            // The next lines are commented out because they caused exceptions, 
            // They have been added because .net doesn't allow me to wait for data (ReadAsyncBlock).
            // Instead, I've added Task.Delay in MyStreamReader.ReadBuffer when
            // Read returns without data.
            
            // See https://github.com/Code-Sharp/uHttpSharp/issues/14
            
            // Read Timeout of one second.
            // _stream.ReadTimeout = 1000;
        }

        public Stream Stream
        {
            get { return _stream; }
        }

        public bool Connected
        {
            get { return _client.Connected; }
        }

        public void Close()
        {
            _client.Close();
        }

        public EndPoint RemoteEndPoint
        {
            get { return _client.Client.RemoteEndPoint; }
        }
    }

	public class SSlTcpClientAdapter : TcpClientAdapter
	{
		private readonly X509Certificate _certificate;
		private readonly SslStream _sslStream;

		public SSlTcpClientAdapter(TcpClient client, X509Certificate certificate) : base(client)
		{
			_certificate = certificate;
			_sslStream = new SslStream(base.Stream);
		}

		public async Task AuthenticateAsServer()
		{
			Task timeout = Task.Delay(TimeSpan.FromSeconds(10));
			if (timeout == await Task.WhenAny(_sslStream.AuthenticateAsServerAsync(_certificate, false, SslProtocols.Tls, true), timeout).ConfigureAwait(false))
			{
				throw new TimeoutException("SSL Authentication Timeout");
			}
		}

		public new Stream Stream
		{
			get { return _sslStream; }
		}
	}
}