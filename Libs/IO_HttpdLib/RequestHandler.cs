using Helpers;
using System;
using System.Threading.Tasks;

namespace HttpdLib
{
	public class CHandlerFunction
	{
		public bool Public = false;
		public String Name = "";
		public String HTTPRequestType = CRequest.Method.ALL;

		public System.Reflection.MethodInfo MethodInfo = null;
	}

	public class CHandlerException : Exception
	{
		public HTTPStatusCode StatusCode;

		public CHandlerException(HTTPStatusCode _StatusCode, String _Message) : base(_Message)
		{
			StatusCode = _StatusCode;
		}
	}

	public class CRequestHandler
	{
		public CRequest Request = null;
		public CResponse Response = new CResponse();

		public KeyValueArray Params = new KeyValueArray();

		protected bool DebugMode { get { return CRequest.DebugMode; } }

		protected Int64 LoggedUserID = -1;

		public class HTTPMethod : Attribute
		{
			public const String ALL = CRequest.Method.ALL;
			public const String GET = CRequest.Method.GET;
			public const String POST = CRequest.Method.POST;
			public const String PUT = CRequest.Method.PUT;
			public const String DELETE = CRequest.Method.DELETE;

			private String _Type = ALL;
			public String Type { get { return _Type; } set { _Type = value; } }
			private bool _Public = false;
			public bool Public { get { return _Public; } set { _Public = value; } }
		}

		// Member function to be ovverided if session handling
		public virtual void Process() {  }
		protected virtual bool ValidateURI() { return true; }
		protected virtual Int64 ValidateSession() { return -1; }
		protected virtual void TraceCall(String InputParams, String Output) { }

		protected void Redirect(String Url)
		{
			Response.Header.Location = Url;
			Response.Body = null;
			Response.StatusCode = HTTPStatusCode.RedirectFound_302;
		}

		protected void Redirect301(String Url)
		{
			Response.Header.Location = Url;
			Response.Body = null;
			Response.StatusCode = HTTPStatusCode.MovedPermanently_301;
		}

		protected void ThrowError(HTTPStatusCode ErrorStatusCode, String ErrMsg = null, object ResponseBody = null)
		{
			Response.Body = ResponseBody;
			throw new CHandlerException(ErrorStatusCode, ErrMsg);
		}

		protected String CheckMissingParams(String[] vParamNames)
		{
			for (int i = 0; i < vParamNames.Length; i++)
				if (this.Params[vParamNames[i]] == null)
					return vParamNames[i];

			return null;
		}

		protected String CheckParamsForHtmlInjections(String[] vParamNames)
		{
			for (int i = 0; i < vParamNames.Length; i++)
				if (ValidationHelper.CheckNoHtml(this.Params[vParamNames[i]]) == false)
					return vParamNames[i];

			return null;
		}

		String CheckAndGetStringParam(String Key)
		{
			String Value = Types.ToString(Params[Key]).Trim();
			if (Value == "")
				ThrowError(HTTPStatusCode.Bad_Request_400, String.Format(Key + " value not valid: {0}", Types.ToString(Params[Key])));

			return Value;
		}

		Int64 CheckAndGetInt64Param(String Key, Int64 MinAcceptableValue = 0)
		{
			Int64 Value = Types.ToInt64(Params[Key], -1);
			if (Value < MinAcceptableValue)
				ThrowError(HTTPStatusCode.Bad_Request_400, String.Format(Key + " value not valid: {0}", Types.ToString(Params[Key])));

			return Value;
		}
	}
}
