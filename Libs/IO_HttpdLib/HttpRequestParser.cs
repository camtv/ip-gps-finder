using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HttpdLib
{
	public class HttpRequestParser
	{
		private static readonly char[] Separators = { '/' };

		public async Task<HttpRequest> Parse(IStreamReader reader)
		{
			// parse the http request
			var request = await reader.ReadLine().ConfigureAwait(false);

			if (request == null)
				return null;

			var firstSpace = request.IndexOf(' ');
			var lastSpace = request.LastIndexOf(' ');

			var tokens = new[]
			{
				request.Substring(0, firstSpace),
				request.Substring(firstSpace + 1, lastSpace - firstSpace - 1),
				request.Substring(lastSpace + 1)
			};

			if (tokens.Length != 3)
			{
				return null;
			}

			var Method = tokens[0];
			var url = tokens[1];
			var httpProtocol = tokens[2];

			var uri = new Uri(url, UriKind.Relative);

			var Headers = new KeyValueArray();

			// get the headers
			string line;
			int ContentLength = 0;
			while (!string.IsNullOrEmpty((line = await reader.ReadLine().ConfigureAwait(false))))
			{
				string currentLine = line;

				var headerKvp = SplitHeader(currentLine);
				if (headerKvp.Key.ToLower() == "content-length")
					ContentLength = int.Parse(headerKvp.Value);

				Headers[headerKvp.Key] = headerKvp.Value;
			}

			byte[] vbBodyRawData = await ReadBody(reader, ContentLength).ConfigureAwait(false);

			return new HttpRequest(Headers, Method, httpProtocol, uri, vbBodyRawData);
		}

		private async Task<byte[]> ReadBody(IStreamReader streamReader, int postContentLength)
		{
			byte[] raw = new byte[0];
			if (postContentLength > 0)
				raw = await streamReader.ReadBytes(postContentLength).ConfigureAwait(false);

			return raw;
		}

		private KeyValuePair<string, string> SplitHeader(string header)
		{
			var index = header.IndexOf(": ", StringComparison.InvariantCultureIgnoreCase);
			return new KeyValuePair<string, string>(header.Substring(0, index), header.Substring(index + 2));
		}

	}
}