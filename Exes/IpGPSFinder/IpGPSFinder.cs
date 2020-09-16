using Helpers;
using HttpdLib;
using Newtonsoft.Json;
using System;
using System.Runtime.Remoting.Messaging;

namespace CamTV
{
	public class IpGPSFinderApi : CApiServer
	{
		public IpGPSFinderApi()
		{
			SetEndPointHandler("/api/ipgpsfinder/v1/health", Health);

			SetEndPointHandler("/api/ipgpsfinder/v1/getip", GetIp);
		}

		override protected Int64 ValidateSession()
		{
			try
			{
				String sAuthHeader = Types.ToString(Request.Params.Server["Authorization"], Types.ToString(Request.Params.Server["authorization"]));
				Int64 UserID = Types.ToInt64(sAuthHeader.Replace("Bearer ", ""), -1);

				return UserID;
			}
			catch (Exception Ex)
			{
				LogHelper.Error("Exception: {0}", Ex.Message);
			}

			return -1;
		}

		[HTTPMethod(Public = true, Type = CRequest.Method.GET)]
		void Health()
		{
			//String sAuthHeader = Types.ToString(Request.Params.Server["Authorization"], Types.ToString(Request.Params.Server["authorization"])).Replace("Bearer ", "");
			//if (sAuthHeader != Sets.API_KEY)
			//	ThrowError(HTTPStatusCode.Unauthorized_401, "Authorization Bearer != Sets.API_KEY - Remote IP:"+ Request.RemoteAddr);

			Body = "WORKING";   // Aggiungere dati di info sullo status del processo
			StatusCode = HTTPStatusCode.OK_200;
		}

		[HTTPMethod(Public = false, Type = CRequest.Method.POST)]
		void GetIp()
		{
			// Fase 1 - Verifica parametri ed acquisizione
			String MissingParameter = CheckMissingParams(new String[] { "Path" });
			if (MissingParameter != null)
				ThrowError(HTTPStatusCode.Bad_Request_400, MissingParameter);

			String sPath = Types.ToString(Params["Path"]);
			Int64 nLimitStart = Types.ToInt64(Params["LimitStart"],0);
			Int64 nLimitCounts = Types.ToInt64(Params["LimitCount"],30);

			// Fase 2 - Chiamata al modello dati


			// Fase 3 - [Optional] Condizionamento dei prametri di out

			// Fase 4 - Output
			Body = new
			{
				IP = "",
				GPS = new
				{
					lat = 0,
					lon = 0
				}
			};
			StatusCode = HTTPStatusCode.OK_200;
		}

	}
}