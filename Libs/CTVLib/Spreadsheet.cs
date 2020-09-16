using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Threading.Tasks;

namespace Helpers
{
	public class GoogleSheet
	{
		string SpreadsheetId;
		SheetsService service;

		public delegate void TErrorLogger(string message, params object[] values);
		public TErrorLogger Logger = null;

		bool LogError(string message, params object[] values)
		{
			if (Logger != null)
				Logger(message, values);

			return false;
		}

		public static GoogleSheet Create(String Cert_P12, String Cert_Psw,String ServiceAccountEmail, String ApplicationName, string SpreadsheetId, TErrorLogger ErrLogger = null)
		{
			try
			{
				var certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(Cert_P12,Cert_Psw, System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.Exportable);

				GoogleSheet sh = new GoogleSheet();
				sh.Logger = ErrLogger;

				string[] Scopes = new string[] { SheetsService.Scope.Spreadsheets };
				sh.SpreadsheetId = SpreadsheetId;

				ServiceAccountCredential credential = new ServiceAccountCredential(
				new ServiceAccountCredential.Initializer(ServiceAccountEmail)
				{
					Scopes = new [] { SheetsService.Scope.Spreadsheets }
				}.FromCertificate(certificate));

				sh.service = new SheetsService(new BaseClientService.Initializer
				{
					ApplicationName = ApplicationName,
					HttpClientInitializer = credential,
				});
				
				return sh;
			}
			catch (Exception Ex)
			{
				if (ErrLogger != null)
					ErrLogger("Exception: {0}", Ex.Message);
			}

			return null;
		}

		public bool AddRow(KeyValueHelper kv)
		{
			try
			{
				var oblist = new List<object>();
				foreach (String Key in kv.Keys)
					oblist.Add(kv[Key]);

				var valueRange = new ValueRange();
				valueRange.Values = new List<IList<object>> { oblist };

				String range = "A:" + (char)('A' + kv.Keys.Count);
				var appendRequest = service.Spreadsheets.Values.Append(valueRange, SpreadsheetId, range);
				appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
				var appendReponse = appendRequest.Execute();

				return true;
			}
			catch (Exception Ex)
			{
				LogError("Exception: {0}", Ex.Message);
			}

			return false;
		}

		public bool UpdateCell(String Value, String Coordinates)
		{
			try
			{
				var valueRange = new ValueRange();

				var oblist = new List<object>() { Value };
				valueRange.Values = new List<IList<object>> { oblist };

				var updateRequest = service.Spreadsheets.Values.Update(valueRange, SpreadsheetId, Coordinates);
				updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
				var appendReponse = updateRequest.Execute();

				return true;
			}
			catch (Exception Ex)
			{
				LogError("Exception: {0}", Ex.Message);
			}

			return false;
		}
	}
}