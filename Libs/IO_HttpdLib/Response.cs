using Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace HttpdLib
{
	public class CResponse
	{
		public class CCookie
		{
			public String Name = "";
			public String Value = "";
			public Int64 MaxAge = 0;
			public String Path = null;
			public String Domain = null;
			public bool Secure = false;

			public new String ToString()
			{
				String ret = String.Format("{0}={1}", Name, Types.ToString(Value));
				if (MaxAge != 0)
					ret += String.Format(";Expires={0}", DateTime.UtcNow.AddSeconds(MaxAge).ToString("ddd, dd MMM yyyy HH:mm:ss 'UTC'"));

				if (Path != null)
					ret += ";Path=" + Path;

				if (Domain != null)
					ret += ";Domain=" + Domain;

				if (Secure == true)
					ret += ";Secure";
				return ret;
			}

			public String GetUID() { return Name; }
		}

		public CHTTPHeader Header = new CHTTPHeader();
		public HTTPStatusCode StatusCode { get { return Header.StatusCode; } set { Header.StatusCode = value; } }
		public String ContentType { get { return Types.ToString(CHTTPHeader.ContentTypeStrings[Header.ContentType]); } }
		private KeyValueHelperBase<CCookie> Cookies = new KeyValueHelperBase<CCookie>();

        public String Language = Ln.csDefaultLang;

		public String JSONPCallback = null;

		public Object Body = null;

		public CResponse()
		{
		}

		public KeyValuePair<string, string>[] GetHeaders()
		{
			List<KeyValuePair<string, string>> lsHeaders = new List<KeyValuePair<string, string>>();

			lsHeaders.Add(new KeyValuePair<string, string>("Content-Type", CHTTPHeader.ContentTypeStrings[Header.ContentType]));

			if (Header.Location != null)
				lsHeaders.Add(new KeyValuePair<string, string>("Location", Header.Location));

			if (Header.ETag != null)
				lsHeaders.Add(new KeyValuePair<string, string>("ETag", Header.Location));

			if (Header.CustomHeaders!=null)
			{
				foreach(String key in Header.CustomHeaders.Keys)
					lsHeaders.Add(new KeyValuePair<string, string>(key, Header.CustomHeaders[key]));
			}

			if (Cookies.Count > 0)
			{
				foreach (CCookie cookie in Cookies.Values)
					lsHeaders.Add(new KeyValuePair<string, string>("Set-Cookie",cookie.ToString()));
			}

			return lsHeaders.ToArray();
		}

		public async Task Finalize(HttpResponse LowLevelResponse)
		{
			LowLevelResponse.Headers = GetHeaders();
			LowLevelResponse.ResponseCode = this.Header.StatusCode;

			Object ResBody = Body;
			if (Header.ContentType == HTTPContentType.JSON)
			{
				if (ResBody == null)
					ResBody = new Object();
				ResBody = JsonConvert.SerializeObject(Body);
			}

			// Gestisce il tipo di data
			if (ResBody != null && ResBody.GetType() == typeof(String))
			{
				byte[] vb = Encoding.UTF8.GetBytes((String)ResBody);
				await LowLevelResponse.Body.WriteAsync(vb, 0, vb.Length).ConfigureAwait(false);
			}
			else if (ResBody != null && ResBody.GetType() != typeof(byte[]))
			{
				BinaryFormatter bf = new BinaryFormatter();
				using (MemoryStream ms = new MemoryStream())
				{
					bf.Serialize(ms, ResBody);
					byte[] vb = ms.ToArray();
					await LowLevelResponse.Body.WriteAsync(vb, 0, vb.Length).ConfigureAwait(false);
				}
			}
			else if (ResBody != null && ResBody.GetType() == typeof(byte[]))
			{
				byte[] vb = (byte[])ResBody;
				await LowLevelResponse.Body.WriteAsync(vb, 0, vb.Length).ConfigureAwait(false);
			}
		}

		public void SetCookie(String Name, String Value, Int64 MaxAge = 0, bool Secure = false, String Domain = null, String Path = null)
		{
			CCookie cookie = new CCookie();
			cookie.Name = Name;
			cookie.Value = Value;
			cookie.MaxAge = MaxAge;
			cookie.Secure = Secure;
			if (Domain != null)
				cookie.Domain = Domain;
			if (Path != null)
				cookie.Path = Path;

			Cookies[cookie.GetUID()] = cookie;
		}
	}
}
