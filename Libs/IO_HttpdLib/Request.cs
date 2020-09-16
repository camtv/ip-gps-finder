using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Helpers;
using System.Web;
using System.Collections.Specialized;
using System.Net;

namespace HttpdLib
{
	public class CRequest
	{
		public class CInputParams
		{
			private readonly HttpRequest LowLevelRequest;

			public CInputParams(HttpRequest llr) { LowLevelRequest = llr; }

			public KeyValueArray Post = new KeyValueArray();
			public byte[] Body { get { return LowLevelRequest.Body; } }
			public KeyValueArray Get = new KeyValueArray();
			public KeyValueArray Cookies = new KeyValueArray();
			public KeyValueArray Server { get { return LowLevelRequest.Headers; } }
		}

		static public bool DebugMode = false;

		// HTTP method definition (GET, PUT, POST,..)
		public class Method
		{
			public const String ALL = "ALL";
			public const String GET = "GET";
			public const String POST = "POST";
			public const String PUT = "PUT";
			public const String DELETE = "DELETE";
		}

		public class BrowserDescriptor
		{
			public enum TName { UNKNOWN, IE, CHROME, SAFARI, FIREFOX, OPERA, EDGE };
			public enum TPlatform { UNKNOWN, ANDROID, IPHONE, IPAD, IPOD, BLACKBERRY, WEBOS, MACINTOSH, WINDOWS, LINUX };

			public TName Name = TName.UNKNOWN;
			public String Version = "";
			public bool IsMobile = false;
			public TPlatform Platform = TPlatform.UNKNOWN;

			public BrowserDescriptor(String UserAgent)
			{
				try
				{
					int nPos = 0;
					if (UserAgent.IndexOf("MSIE 6.0") > 0)
					{
						Name = TName.IE;
						Version = "6.0";
					}
					else if (UserAgent.IndexOf("MSIE 7.0") > 0)
					{
						Name = TName.IE;
						Version = "7.0";
					}
					else if (UserAgent.IndexOf("MSIE 8.0") > 0 || UserAgent.IndexOf("Trident/4.0") > 0)
					{
						Name = TName.IE;
						Version = "8.0";
					}
					else if (UserAgent.IndexOf("MSIE 9.0") > 0 || UserAgent.IndexOf("Trident/5.0") > 0)
					{
						Name = TName.IE;
						Version = "9.0";
					}
					else if (UserAgent.IndexOf("MSIE 10.0") > 0 || UserAgent.IndexOf("Trident/6.0") > 0)
					{
						Name = TName.IE;
						Version = "10.0";
					}
					else if (UserAgent.IndexOf("MSIE 11.0") > 0 || UserAgent.IndexOf("Trident/7.0") > 0)
					{
						Name = TName.IE;
						Version = "11.0";
					}
					else if ((nPos = UserAgent.LastIndexOf("OPR/")) > 0)
					{
						Name = TName.OPERA;
						String[] vs = UserAgent.Split(new String[] { "OPR/" }, StringSplitOptions.RemoveEmptyEntries);
						if (vs != null && vs.Length > 0)
						{
							Version = vs[1];
							nPos = Version.IndexOf(' ');
							if (nPos > 0)
								Version = Version.Substring(0, nPos);
						}
					}
					else if ((nPos = UserAgent.LastIndexOf("Opera/")) > 0)
					{
						Name = TName.OPERA;
						String[] vs = UserAgent.Split(new String[] { "Version/" }, StringSplitOptions.RemoveEmptyEntries);
						if (vs != null && vs.Length > 0)
						{
							Version = vs[1];
							nPos = Version.IndexOf(' ');
							if (nPos > 0)
								Version = Version.Substring(0, nPos);
						}
					}
					else if ((nPos = UserAgent.LastIndexOf("Opera ")) > 0)
					{
						Name = TName.OPERA;
						String[] vs = UserAgent.Split(new String[] { "Opera " }, StringSplitOptions.RemoveEmptyEntries);
						if (vs != null && vs.Length > 0)
						{
							Version = vs[1];
							nPos = Version.IndexOf(' ');
							if (nPos > 0)
								Version = Version.Substring(0, nPos);
						}
					}
					else if ((nPos = UserAgent.LastIndexOf("Firefox/")) > 0 || (nPos = UserAgent.LastIndexOf("FxiOS/")) > 0)    // TK: Cristian - CTBT-1042 Identifica FF anche con FxiOS
					{
						Name = TName.FIREFOX;
						Version = UserAgent.Substring(nPos);

						// TK: Cristian - CTBT-1042 START
						//Individiua correttamente la versione 
						int tmpnPos = Version.IndexOf(' ');
						if (tmpnPos > 0)
							Version = Version.Substring(0, tmpnPos);
						// TK: Cristian - CTBT-1042 END
					}
					else if ((nPos = UserAgent.LastIndexOf("Edge/")) > 0)
					{
						Name = TName.EDGE;
						Version = UserAgent.Substring(nPos);
					}
					else if ((nPos = UserAgent.LastIndexOf("Chrome/")) > 0 || (nPos = UserAgent.LastIndexOf("CriOS/")) > 0)
					{
						Name = TName.CHROME;
						Version = UserAgent.Substring(nPos);
						String[] vs = Version.Split(' ');
						if (vs != null && vs.Length > 0)
							Version = vs[0];
					}
					else if ((nPos = UserAgent.LastIndexOf("Safari/")) > 0)
					{
						Name = TName.SAFARI;
						String[] vs = UserAgent.Split(new String[] { "Version/" }, StringSplitOptions.RemoveEmptyEntries);
						if (vs != null && vs.Length > 0)
						{
							Version = vs[1];
							nPos = Version.IndexOf(' ');
							if (nPos > 0)
								Version = Version.Substring(0, nPos);
						}
					}
				}
				catch (Exception Ex)
				{
					LogHelper.Error("Exception in BrowserDescriptor creation, message: " + Ex.Message);
					Name = TName.UNKNOWN;
					IsMobile = false;
				}

				try
				{
					int nPos = 0;
					if ((nPos = UserAgent.IndexOf("Android")) > 0)
					{
						IsMobile = true;
						Platform = TPlatform.ANDROID;
					}
					else if ((nPos = UserAgent.IndexOf("iPhone")) > 0)
					{
						IsMobile = true;
						Platform = TPlatform.IPHONE;
					}
					else if ((nPos = UserAgent.IndexOf("iPad")) > 0)
					{
						IsMobile = true;
						Platform = TPlatform.IPAD;
					}
					else if ((nPos = UserAgent.IndexOf("iPod")) > 0)
					{
						IsMobile = true;
						Platform = TPlatform.IPOD;
					}
					else if (UserAgent.IndexOf("BlackBerry") > 0 || UserAgent.IndexOf("BB") > 0 || UserAgent.IndexOf("RIM") > 0)
					{
						IsMobile = true;
						Platform = TPlatform.BLACKBERRY;
					}
					else if ((nPos = UserAgent.IndexOf("webOS")) > 0)
					{
						IsMobile = true;
						Platform = TPlatform.WEBOS;
					}
					else if ((nPos = UserAgent.IndexOf("Mobile")) > 0)
					{
						IsMobile = true;
						Platform = TPlatform.UNKNOWN;
					}
					else if ((nPos = UserAgent.IndexOf("Macintosh")) > 0)
					{
						IsMobile = false;
						Platform = TPlatform.MACINTOSH;
					}
					else if ((nPos = UserAgent.IndexOf("Windows")) > 0)
					{
						IsMobile = false;
						Platform = TPlatform.WINDOWS;
					}
					else if ((nPos = UserAgent.IndexOf("Linux")) > 0)
					{
						IsMobile = false;
						Platform = TPlatform.LINUX;
					}
				}
				catch (Exception Ex)
				{
					LogHelper.Error("Exception in BrowserDescriptor creation, message: " + Ex.Message + " - Browser Agent=" + UserAgent);
					Name = TName.UNKNOWN;
					IsMobile = false;
				}
			}

			public override string ToString()
			{
				return String.Format("{0}-{1}-{2}-{3}", Name.ToString(), Version, Platform, (IsMobile ? "Mobile" : "Desktop"));
			}

			public bool FromString(String s)
			{
				try
				{
					String[] vs = s.Split('-');
					if (vs != null && vs.Length == 4)
					{
						try
						{
							Name = (TName)Enum.Parse(typeof(TName), vs[0]);
						}
						catch { return false; }
						Version = vs[1];
						try
						{
							Platform = (TPlatform)Enum.Parse(typeof(TPlatform), vs[2]);
						}
						catch { return false; }
						IsMobile = (vs[3] == "Mobile");

						return true;
					}
				}
				catch { }
				return false;
			}
		}

		// 
		public CInputParams Params;

		public BrowserDescriptor Browser;

		public String HttpAccept = "";
		public String HttpMethod;
		public String UserAgent = "";
		public String AcceptLanguage = "";
		public String Host = "";
		public String RemoteAddr = "";
		public String RequestURI = "";
		public String ContentType = "";

		/*
			 foo://example.com:8042/over/there?name=ferret#nose
			 \_/   \______________/\_________/ \_________/ \__/
			  |           |            |            |        |
			scheme     authority       path        query   fragment
			  |   _____________________|__
			 / \ /                        \
			 urn:example:animal:ferret:nose>		 		 
		*/
		public String URIScheme = "";
		public String URIAuthority = "";
		public String URIAuthorityDomain = "";
		public String URIAuthorityPort = "";
		public String URIPath = "";
		public String URIQueryString = "";
		public String[] URIPathParts;

		public CRequest(HttpRequest LowLevelContext)
		{
			ParseInputStreams(LowLevelContext);
		}

		String GetServerInputParam(String Key)
		{
			String val = Params.Server[Key];
			if (val != null)
				return Params.Server[Key];

			return Params.Server[Key.ToLower()];
		}

		public void ParseInputStreams(HttpRequest LowLevelRequest)
		{
			Params = new CInputParams(LowLevelRequest);

			// Header parameters
			try
			{
				HttpMethod = LowLevelRequest.Method.ToString().ToUpper();
				HttpAccept = Types.ToString(GetServerInputParam("Accept"),"text/html");				
				UserAgent = Types.ToString(GetServerInputParam("User-Agent"));
				AcceptLanguage = Types.ToString(GetServerInputParam("Accept-Language"));
				Host = Types.ToString(GetServerInputParam("Host"));
				RequestURI = LowLevelRequest.Uri.ToString();
				ContentType = Types.ToString(GetServerInputParam("Content-Type"));
				var ipe = (IPEndPoint)(LowLevelRequest.TcpClient.RemoteEndPoint);
				RemoteAddr = IPAddress.Parse((ipe).Address.ToString()).ToString();

				// Gestisce il caso del redirect da dominio esterno per canale o cobrand
				if (Params.Server["HTTP_X_DOMAIN"] != null)
					Host = Params.Server["HTTP_X_DOMAIN"];

				if (Params.Server["HTTP_X_REMOTE_ADDR"] != null)
					RemoteAddr = Params.Server["HTTP_X_REMOTE_ADDR"];
			}
			catch (Exception Ex)
			{
				LogHelper.Error("Exception in parsing Server parameters, message: " + Ex.Message);
			}

			ParseURI();

			// Cookie
			try
			{
				if (GetServerInputParam("Cookie") != null)
				{
					//Uses cursom parsing 
					String[] vs = GetServerInputParam("Cookie").ToString().Split(new String[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

					Params.Cookies.Clear();
					foreach (string s in vs)
					{
						String[] vs1 = s.Split(new String[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
						if (vs1.Length == 1)
							Params.Cookies.Add(vs1[0].Trim(), null);
						else
						{
							String sValue = "";
							for (int i = 1; i < vs1.Length; i++)
								sValue += vs1[i].Trim();
							Params.Cookies.Add(vs1[0].Trim(), sValue);
						}
					}
				}
			}
			catch (Exception Ex)
			{
				LogHelper.Error("Exception in parsing Cookies parameters, message: " + Ex.Message);
			}

			// Get
			try
			{
				NameValueCollection nvc = HttpUtility.ParseQueryString(Types.ToString(this.URIQueryString));
				foreach (String key in nvc.Keys)
					if (key != null)
						Params.Get.Add(key, nvc[key]);
			}
			catch (Exception Ex)
			{
				LogHelper.Error("Exception in parsing Server parameters, message: " + Ex.Message);
			}

			// Post
			try
			{
				Params.Post.Clear();
				if (HttpMethod == Method.POST && this.ContentType.Contains("application/x-www-form-urlencoded")==true && Params.Body!=null)
				{
					String sPostBody = HttpUtility.UrlDecode(Encoding.UTF8.GetString(Params.Body));
					if (sPostBody != null)
					{						
						NameValueCollection nvc = HttpUtility.ParseQueryString(sPostBody);
						foreach (String key in nvc.Keys)
							if (key != null)
								Params.Post.Add(key, nvc[key]);
					}
				}
			}
			catch (Exception Ex)
			{
				LogHelper.Error("Exception in parsing Post parameters, message: " + Ex.Message);
			}

			// Detect Browser Type
			try
			{
				Browser = new BrowserDescriptor(UserAgent);
			}
			catch (Exception Ex)
			{
				LogHelper.Error("Exception in detecting user agent, message: " + Ex.Message);
			}
		}

		public void ParseURI()
		{
			try
			{
				URIQueryString = "";
				URIPath = Types.ToString(RequestURI);
				int nQuestion = RequestURI.IndexOf('?');
				if (nQuestion > 0)
				{
					if (RequestURI.Length >= nQuestion + 1)
						URIQueryString = RequestURI.Substring(nQuestion+1);
					URIPath = RequestURI.Substring(0, nQuestion);
				}

				URIPathParts = URIPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
			}
			catch (Exception Ex)
			{
				LogHelper.Error("Exception processing uri path from server variable SCRIPT_NAME: " + Ex.Message);
				URIPathParts = new String[0];
			}

			try
			{
				URIScheme = "http";
				if (Params.Server["HTTPS"] != null)
					URIScheme = "https";

				int nPort = Types.ToInt(Params.Server["SERVER_PORT"], 80);
				if (URIScheme == "http" && nPort != 80)
					URIAuthorityPort = ":" + nPort.ToString();
				else if (URIScheme == "https" && nPort != 443)
					URIAuthorityPort = ":" + nPort.ToString();

				URIAuthorityDomain = Host;
				URIAuthority = URIAuthorityDomain + URIAuthorityPort;
			}
			catch (Exception Ex)
			{
				LogHelper.Error("Exception in processing URI Scheme and Autority : " + Ex.Message);

			}
		}

		public String ParamsToString()
		{
			// TODO: completare a string
			return "";
		}

		public String GetBrowserLanguage()
		{
			String sLng = AcceptLanguage;
			if (sLng == null || sLng=="")
				return Ln.csDefaultLang;

			String[] vs = sLng.Split(new char[] { ',' });
			List<String> AcceptedLanguages = new List<string>();
			foreach (String s in vs)
			{
				if (s != null)
				{
					String[] vs1 = s.Split(new char[] { ';' });
					if (vs1 != null && vs1.Length > 0)
						AcceptedLanguages.Add(vs1[0].Substring(0, 2));
				}
			}

			return Ln.GetNearestSupportedLanguage(AcceptedLanguages.ToArray());
		}

		public String GetBrowserIsoCode()
		{
			String sLng = AcceptLanguage;
			if (sLng == null || sLng == "")
				return Ln.csDefaultISOCode;

			sLng = sLng.Replace("-", "_");
			String[] vs = sLng.Split(new char[] { ',' });
			List<String> AcceptedLanguages = new List<string>();
			foreach (String s in vs)
			{
				if (s != null)
				{
					String[] vs1 = s.Split(new char[] { ';' });
					if (vs1 != null && vs1.Length > 0)
					{
						String sCode = vs1[0].Substring(0, 2);
						if (vs1[0].Length > 2)
							sCode = vs1[0].Substring(3, 2);

						AcceptedLanguages.Add(sCode.ToLower());
					}
				}
			}

			return Ln.GetNearestCountyISOCode(AcceptedLanguages.ToArray());
		}

		public String GetDomainNameWithProtocolNoTermSlash()
		{
			return URIScheme + "://" + URIAuthority;
		}

	}
}
