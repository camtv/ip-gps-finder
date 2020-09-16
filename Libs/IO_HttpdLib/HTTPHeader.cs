using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HttpdLib
{
	public enum HTTPStatusCode
	{
		OK_200 = 200, Created_201 = 201, Accepted_202 = 202,
		MovedPermanently_301=301, RedirectFound_302 = 302, NotModified_304 = 304,
		Bad_Request_400 = 400, Unauthorized_401 = 401, Forbidden_403 = 403, NotFound_404 = 404, NotAcceptable_406=406, Conflict_409=409, AuthenticationTimeout_419 = 419,
		InternalServerError_500 = 500, NotImplemented_501 = 501
	}

	public enum HTTPContentType { HTML, JSON, PDF, GIF, JPG, PNG, BINARY}; // TK: Abramo - 20181119_04

	public class CHTTPHeader
	{
		static public Dictionary<HTTPStatusCode, String> StatusCodeStrings = new Dictionary<HTTPStatusCode, String>()
		{ 
			{HTTPStatusCode.OK_200,"OK"}, {HTTPStatusCode.Created_201, "Created" },  {HTTPStatusCode.Accepted_202, "Accepted" },
			{HTTPStatusCode.MovedPermanently_301,"MovedPermanently"},{HTTPStatusCode.RedirectFound_302,"Found"},{HTTPStatusCode.NotModified_304,"Not Modified"},
			{HTTPStatusCode.Bad_Request_400,"Bad Request"},{HTTPStatusCode.Forbidden_403, "Forbidden"},{HTTPStatusCode.NotFound_404,"Not Found"},{HTTPStatusCode.Unauthorized_401,"Unauthorized"},{HTTPStatusCode.NotAcceptable_406,"Not Acceptable"},{HTTPStatusCode.Conflict_409,"Conflict"},{HTTPStatusCode.AuthenticationTimeout_419,"Authentication Timeout"},
			{HTTPStatusCode.InternalServerError_500,"Internal Server Error"},{HTTPStatusCode.NotImplemented_501,"Not Implemented"}
		};

		
		static public Dictionary<HTTPContentType, String> ContentTypeStrings = new Dictionary<HTTPContentType, String>()
		{ 
			{HTTPContentType.HTML,"text/html"},
			{HTTPContentType.JSON,"application/json"},
			{HTTPContentType.PDF,"application/pdf"},
			{HTTPContentType.GIF,"image/gif"},
			{HTTPContentType.JPG,"image/jpeg"},
			{HTTPContentType.PNG,"image/png"},
			{HTTPContentType.BINARY,"application/octet-stream"}, // TK: Abramo - 20181119_04
		};

		public HTTPStatusCode StatusCode = HTTPStatusCode.InternalServerError_500;
		public HTTPContentType ContentType = HTTPContentType.HTML;
		public String Charset = "UTF-8";
		public String Location = null;

		public String ETag = null;

		public KeyValueArray CustomHeaders = null;
		public void Add(String Header, String Value)
		{
			if (CustomHeaders == null)
				CustomHeaders = new KeyValueArray();
			CustomHeaders.Add(Header, Value);
		}
	}
}
