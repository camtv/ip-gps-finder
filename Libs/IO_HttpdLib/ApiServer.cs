using System;
using System.Collections.Generic;
using Helpers;
using System.Reflection;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;

namespace HttpdLib
{
	public class CApiServer : CRequestHandler
	{
		static KeyValueHelperBase<CHandlerFunction> kvHandlerMap = new KeyValueHelperBase<CHandlerFunction>();

		public HTTPStatusCode StatusCode { get { return Response.StatusCode; } set { Response.StatusCode = value; } }
		public HTTPContentType ContentType { get { return Response.Header.ContentType; } set { Response.Header.ContentType = value; } }
		public HTTPContentType DefaultResponseContentType = HTTPContentType.JSON;

		public object Body { get { return Response.Body; } set { Response.Body = value; } }

		public DatabaseHelper Db = null;
		static String DatabaseHost;
		static int DatabasePort;
		static String DatabaseUser;
		static String DatabasePassword;
		static String DatabaseName;
		static int DatabaseMaxPoolConnections;

		public override void Process()
		{
			try
			{
				Params = Request.Params.Cookies;
				foreach (KeyValuePair<string, string> pair in Request.Params.Get)
					Params.Add(pair.Key, pair.Value);
				foreach (KeyValuePair<string, string> pair in Request.Params.Post)
					Params.Add(pair.Key, pair.Value);

				if (Types.ToInt(Params["DEBUG"], 0) == 1 || Types.ToInt(Params["debug"], 0) == 1)
					Response.SetCookie("DEBUG", "1", 365 * 1400 * 60);

				Response.Header.ContentType = DefaultResponseContentType;
				if (Request.HttpAccept != null && Request.HttpAccept.Contains("text/html"))
					Response.Header.ContentType = HTTPContentType.HTML;

				if (Request.Params.Cookies["Lang"] != null)  // altrimenti vince il cookie
					Response.Language = Types.ToString(Request.Params.Cookies["Lang"], Ln.csDefaultLang);

				if (ValidateURI() == false)
					throw new CHandlerException(HTTPStatusCode.NotFound_404, "ValidateURI failed - URI not valid: " + Types.ToString(Request.RequestURI));

				String EndpointURL = Types.ToString(Request.URIPath);
				CHandlerFunction HandlerFunction = kvHandlerMap[EndpointURL];
				if (HandlerFunction == null)
					throw new CHandlerException(HTTPStatusCode.NotFound_404, "Endpoint not found: " + EndpointURL);

				Db = null;
				using (DatabaseHelper DbHinstance = new DatabaseHelper())
				{
					try
					{
						if (DatabaseHost != null && DatabaseHost != "")
						{
							DbHinstance.SetDbConnectionData(DatabaseHost, DatabasePort, DatabaseUser, DatabasePassword, DatabaseName, DatabaseMaxPoolConnections);
							if (DbHinstance.OpenConnection() == false)
								throw new CHandlerException(HTTPStatusCode.InternalServerError_500, "DatabaseHelper.OpenConnection failed");
							Db = DbHinstance;
						}

						this.LoggedUserID = ValidateSession();

						if (HandlerFunction.Public == false && this.LoggedUserID == -1)
							throw new CHandlerException(HTTPStatusCode.Unauthorized_401, "HandlerFunction.Public == false && this.LoggedUserID == -1");

						if (HandlerFunction.HTTPRequestType != CRequest.Method.ALL && HandlerFunction.HTTPRequestType != Request.HttpMethod.ToUpper())
							throw new CHandlerException(HTTPStatusCode.NotFound_404, "Wrong request type - accepted: " + HandlerFunction.HTTPRequestType + ", used:" + Request.HttpMethod.ToUpper());

						Action action = (Action)Delegate.CreateDelegate(typeof(Action), this, HandlerFunction.MethodInfo);

						action();

						if (Db != null)
							Db.CloseConnection();
						Db = null;
					}
					catch (Exception Ex)
					{
						if (Db != null)
							Db.CloseConnection();
						Db = null;

						throw Ex;
					}

				}
			}
			catch (CHandlerException Ex)
			{
				// Caso di eccezione gestita
				LogHelper.Error("Exception detected - ErrorToken: " + Ex.StatusCode + " " + CHTTPHeader.StatusCodeStrings[Ex.StatusCode] + " - Message: " + Ex.Message + " - User IP Addr: " + Request.RemoteAddr);
				Response.Header.StatusCode = Ex.StatusCode;
				if (Response.StatusCode == HTTPStatusCode.NotFound_404)
					Response.Body = null;
			}
			catch (Exception Ex)
			{
				// In caso di eccezione generica
				LogHelper.Error("Exception detected - Message: " + Ex.Message + " - User IP Addr: " + Request.RemoteAddr + " - Stack: " + Ex.StackTrace);
				Response.Header.StatusCode = HTTPStatusCode.InternalServerError_500;
				Response.Body = null;
			}
			finally
			{
				Db = null;
			}
		}

		public void SetEndPointHandler(String EndpointURL, Action Handler)
		{
			Type t = this.GetType();
			System.Reflection.MethodInfo methodInfo = Handler.GetMethodInfo();

			if (methodInfo == null)
				return;

			// Verifica che il metodo sia identificato con l'attributo HTTPMethod
			String HTTPRequestType = null;
			bool MethodIsPublic = false;
			object[] MethodAttributes = methodInfo.GetCustomAttributes(true);
			if (MethodAttributes == null)
				return;

			foreach (object attr in MethodAttributes)
			{
				if (attr.GetType() == typeof(HTTPMethod))
				{
					HTTPRequestType = ((HTTPMethod)attr).Type;
					MethodIsPublic = ((HTTPMethod)attr).Public;
					break;
				}
			}

			if (HTTPRequestType == null)
				return;

			CHandlerFunction hf = new CHandlerFunction();
			hf.Name = methodInfo.Name;
			hf.Public = MethodIsPublic;
			hf.HTTPRequestType = HTTPRequestType;
			hf.MethodInfo = methodInfo;

			kvHandlerMap[EndpointURL] = hf;
		}

		public virtual void InitEndpoints() { }

		async Task HandlerFunc(HttpRequest request, HttpResponse response)
		{
			// Handling request
			try
			{
				var Handler = (CRequestHandler)Activator.CreateInstance(GetType());
				Handler.Request = new CRequest(request);
			
				Handler.Process();
				await Handler.Response.Finalize(response);			
			}
			catch (Exception Ex)
			{
				LogHelper.Error(Ex);
			}
		}

		public bool Run(String sIP, int Port, String DbHost, int DbPort, String DbUser, String DbPsw, String DbName, int DbMaxPoolConn, String CertificateCrt = null)
		{
			try
			{
				System.Globalization.CultureInfo cl = new System.Globalization.CultureInfo("en-US");
				cl.DateTimeFormat.FullDateTimePattern = cl.DateTimeFormat.UniversalSortableDateTimePattern;
				cl.DateTimeFormat.DateSeparator = "-";
				cl.DateTimeFormat.TimeSeparator = ":";
				cl.DateTimeFormat.ShortDatePattern = cl.DateTimeFormat.LongDatePattern = "yyyy-MM-dd";
				cl.DateTimeFormat.ShortTimePattern = cl.DateTimeFormat.LongTimePattern = "HH:mm:ss";
				System.Globalization.CultureInfo.DefaultThreadCurrentCulture = cl;

				DatabaseHost = DbHost;
				DatabasePort = DbPort;
				DatabaseUser = DbUser;
				DatabasePassword = DbPsw;
				DatabaseName = DbName;
				DatabaseMaxPoolConnections = DbMaxPoolConn;

				InitEndpoints();

				// Determina l'IP
				// Cerca la prima porta libera per connettersi
				for (int i = 0; i < 10; i++)
				{
					if (Utilities.CheckAvailableTcpPort(sIP, Port) == true)
						break;
					LogHelper.Info("Endpoint http://{0}:{1} not available, try to connect to port {2}", sIP, Port, Port + 1);
					Port++;
				}
				LogHelper.Info("Starting Http Server on: {0}:{1} with {2} max Db connections", sIP, Port, DbMaxPoolConn);

				TcpListener Listener = new TcpListener(IPAddress.Parse(sIP), Port);
				TcpListenerAdapter ListenerAdapter;
				if (CertificateCrt != null)
				{
					var serverCertificate = X509Certificate.CreateFromCertFile(CertificateCrt);
					ListenerAdapter = new SSlTcpListenerAdapter(Listener, serverCertificate);
				}
				else
					ListenerAdapter = new TcpListenerAdapter(Listener);

				var RequestParser = new HttpRequestParser();

				EventWaitHandle EvtTerminate = new EventWaitHandle(false, EventResetMode.ManualReset);
				System.Console.WriteLine("Press CTRL-C to exit");
				Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
				{
					e.Cancel = true;
					EvtTerminate.Set();
				};
				while (EvtTerminate.WaitOne(0) == false)
				{
					try
					{
						// starting new client to serve request
						new HttpClientHandler(ListenerAdapter.GetClient().Result, RequestParser, HandlerFunc);
					}
					catch (Exception Ex)
					{
						LogHelper.Error(Ex);
					}
				}

				EvtTerminate.WaitOne();

				return true;
			}
			catch (Exception Ex)
			{
				LogHelper.Error(Ex);
			}

			return false;
		}

		public bool Run(String ListenURL, String DbHost, int DbPort, String DbUser, String DbPsw, String DbName, int DbMaxPoolConn, String CertificateCrt = null)
		{
			try
			{
				Uri uri = new Uri(ListenURL);

				return Run(uri.Host, uri.Port, DbHost, DbPort, DbUser, DbPsw, DbName, DbMaxPoolConn, CertificateCrt);
			}
			catch (Exception Ex)
			{
				LogHelper.Error(Ex);
			}

			return false;
		}
	}
}
