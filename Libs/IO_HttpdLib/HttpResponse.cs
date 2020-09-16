using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace HttpdLib
{
 	public sealed class HttpResponse
	{
		public HTTPStatusCode ResponseCode = HTTPStatusCode.InternalServerError_500;
		public KeyValuePair<string, string>[] Headers = new KeyValuePair<string, string>[0];
		public MemoryStream Body = new MemoryStream();
    }

}