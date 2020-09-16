using System;
using System.Net;

namespace HttpdLib
{
    public class HttpRequest 
	{
		public KeyValueArray Headers = new KeyValueArray();
        public String Method = "";
		public String Protocol ="";
        public Uri Uri;
        public TcpClientAdapter TcpClient;
        public byte[] Body = new byte[0];

        public HttpRequest(KeyValueArray headers, String method, string protocol, Uri uri, byte[] body)
        {
			Headers = headers;
			Method = method;
			Protocol = protocol;
			Uri = uri;
			Body = body;
        }
    }
}