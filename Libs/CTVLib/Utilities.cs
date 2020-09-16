using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
//using VIESCheckVatService;	//TK: IVAN - PORTING
using System.Net.Http;
using System.ComponentModel.DataAnnotations;
using HtmlAgilityPack;
using Vereyon.Web;
using PhoneNumbers;
using RestSharp;
using System.Net.NetworkInformation;

namespace Helpers
{
	public static class Utilities
	{
		public static Dictionary<string, char> dAccentedLettersToNormalLetters = new Dictionary<string, char>
		{
			{"ÀÁÂÃÄàâãä", 'a'},
			{"ÈÉÊËèéêë", 'e'},
			{"ÌÍÎÏÑìíîïñ",'i'},
			{"ÒÓÔÕÖòóôõö",'o'},
			{"ÙÚÛÜùúûü",'u'},
			{"ÝŸýÿ",'y'}
		};

		public static string CleanURIName(string URIName)
		{
			List<string> lstKeys = new List<string>(Utilities.dAccentedLettersToNormalLetters.Keys);
			string sCleanedURIName = "";

			foreach (char letter in URIName)
			{
				char ch = letter;
				foreach (string sKey in lstKeys)
				{
					if (sKey.IndexOf(letter) > 0)
						ch = Utilities.dAccentedLettersToNormalLetters[sKey];

					if (letter > 127)
						ch = '#';
				}

				sCleanedURIName += ch;
			}

			return sCleanedURIName;
		}


		static Random _rng = new Random(Environment.TickCount);
		public static String RandomString(int size)
		{
			string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

			char[] buffer = new char[size];

			for (int i = 0; i < size; i++)
			{
				buffer[i] = _chars[_rng.Next(_chars.Length)];
			}
			return new String(buffer);
		}

		public static String RandomDigitsString(int size)
		{
			string _chars = "0123456789";

			char[] buffer = new char[size];

			for (int i = 0; i < size; i++)
			{
				buffer[i] = _chars[_rng.Next(_chars.Length)];
			}
			return new String(buffer);
		}

		public static String MakeUrl(String sUrl, params String[] sParams)
		{
			String sRet = "?";
			if (sUrl.Contains("?") == true)
			{
				sRet = "&";
			}

			foreach (String s in sParams)
			{
				sRet += s + "&";
			}

			if (sRet.EndsWith("&") == true)
				sRet = sRet.Remove(sRet.Length - 1);

			return sUrl + sRet;
		}

		public static String FormatCredits(Int64 value, Int64 CreditsDivisor, int ShowDecimals = 0)
		{
			String sFormat = "";
			if (ShowDecimals > 0)
				sFormat = String.Format("F{0}", ShowDecimals);

			return ((double)(value * 1.0 / CreditsDivisor)).ToString(sFormat);
		}

		public static String FormatCurrency(double value, int ShowDecimals = 2)
		{
			String sFormat = "";
			if (ShowDecimals > 0)
				sFormat = String.Format("F{0}", ShowDecimals);

			return ((double)(value * 1.0)).ToString(sFormat);
		}

		//TK: IVAN - 20181210_14 - START
		public static String FormatDateTimeToISO8601(DateTime UTCDateTime, bool ExtendedNotation = true)
		{
			// Il formato ISO8601, qui si suppone una data-ora in UTC, da cui la Z alla fine
			//http://support.sas.com/documentation/cdl/en/lrdict/64316/HTML/default/viewer.htm#a003169814.htm

			if (ExtendedNotation == true)
				return UTCDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");    // Extended notation: E8601DZw.d

			return UTCDateTime.ToString("yyyyMMddTHHmmssZ");    // Basic notation: B8601DZw.d
		}
		//TK: IVAN - 20181210_14 - END

		public static String SHA256(String data)
		{
			SHA256Managed crypt = new SHA256Managed();
			string hash = String.Empty;
			byte[] bytes = Encoding.UTF8.GetBytes(data);
			byte[] crypto = crypt.ComputeHash(bytes, 0, bytes.Length);

			return HexStringFromBytes(crypto);
		}

		public static String HMACSHA256(String data, String key)
		{
			HMACSHA256 crypt = new HMACSHA256(Encoding.UTF8.GetBytes(key));
			string hash = String.Empty;
			byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(data));

			return HexStringFromBytes(crypto);
		}

		public static string SHA1(String data)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(data);

			var sha1 = System.Security.Cryptography.SHA1.Create();
			byte[] hashBytes = sha1.ComputeHash(bytes);

			return HexStringFromBytes(hashBytes);
		}

		static public String MD5(String data)
		{
			System.Security.Cryptography.MD5 crypt = System.Security.Cryptography.MD5.Create();
			string hash = String.Empty;
			byte[] bytes = Encoding.UTF8.GetBytes(data);
			byte[] crypto = crypt.ComputeHash(bytes, 0, bytes.Length);

			return HexStringFromBytes(crypto);
		}

		static public String RIPDEM160(String data)
        {
			RIPEMD160 crypt = RIPEMD160Managed.Create();
			byte[] bytes = Encoding.UTF8.GetBytes(data);
			byte[] crypto = crypt.ComputeHash(bytes, 0, bytes.Length);

			return HexStringFromBytes(crypto);
		}

		public static string HexStringFromBytes(byte[] bytes)
		{
			var sb = new StringBuilder();
			foreach (byte b in bytes)
			{
				var hex = b.ToString("x2");
				sb.Append(hex);
			}
			return sb.ToString();
		}

		public static string EncodeJsString(object os)
		{
			String s = Types.ToString(os);
			StringBuilder sb = new StringBuilder();
			//sb.Append("\"");
			foreach (char c in s)
			{
				switch (c)
				{
					case '\'':
						sb.Append("\\\'");
						break;
					case '\"':
						sb.Append("\\\"");
						break;
					case '\\':
						sb.Append("\\\\");
						break;
					case '\b':
						sb.Append("\\b");
						break;
					case '\f':
						sb.Append("\\f");
						break;
					case '\n':
						sb.Append("\\n");
						break;
					case '\r':
						sb.Append("\\r");
						break;
					case '\t':
						sb.Append("\\t");
						break;
					default:
						int i = (int)c;
						if (i < 32 || i > 127)
						{
							sb.AppendFormat("\\u{0:X04}", i);
						}
						else
						{
							sb.Append(c);
						}
						break;
				}
			}
			//sb.Append("\"");

			return sb.ToString();
		}

		public static KeyValueHelper JsonToKeyValueHelper(string jo)
		{
			var values = JsonConvert.DeserializeObject<KeyValueHelper>(jo);
			var values2 = new KeyValueHelper();
			foreach (KeyValuePair<string, object> d in values)
			{
				// if (d.Value.GetType().FullName.Contains("Newtonsoft.Json.Linq.JObject"))
				if (d.Value is Newtonsoft.Json.Linq.JObject)
				{
					values2.Add(d.Key, JsonToKeyValueHelper(d.Value.ToString()));
				}
				else
				{
					values2.Add(d.Key, d.Value);
				}
			}
			return values2;
		}

		public static Int32 GetUnixTimestamp()
		{
			Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

			return unixTimestamp;
		}

		public static UInt64 GetIDsPairUniqueIndex(Int64 IDA, Int64 IDB)
		{
			if (IDA > 0x00000000FFFFFFFF)
				throw new OverflowException("GetIDPairUniqueIndex - Maximum ID reached");

			if (IDB > 0x00000000FFFFFFFF)
				throw new OverflowException("GetIDPairUniqueIndex - Maximum ID reached");

			UInt32 ida = (UInt32)(IDA);
			UInt32 idb = (UInt32)(IDB);
			if (ida > idb)
			{
				UInt32 idum = ida;
				ida = idb;
				idb = idum;
			}

			UInt64 IDX = idb;
			IDX <<= 32;
			IDX += ida;

			return IDX;
		}

		public class HtmlPageCrawler
		{
			public class Result
			{
				public String Url = "";
				public String Title = "";
				public String Description = "";
				public String ImageURL = "";
			}

			public static Result Analyze(String Url, int TimeoutMs)
			{
				try
				{
					using (WebClientWithTimeout clt = new WebClientWithTimeout())
					{
						clt.Timeout = TimeoutMs;
						clt.Encoding = Encoding.UTF8;
						String sResponse = clt.DownloadString(Url);

						Result r = new Result();
						r.Url = Url;

						HtmlDocument doc = new HtmlDocument();
						doc.LoadHtml(sResponse);

						// Prova con i tag standard dell'header html e la prima immagine che incontra
						HtmlNode metaHtmlNode = doc.DocumentNode.SelectSingleNode("//head/title");
						if (metaHtmlNode != null)
							r.Title = metaHtmlNode.InnerText;

						metaHtmlNode = doc.DocumentNode.SelectSingleNode($"//meta[@name='description']");
						if (metaHtmlNode != null)
							r.Description = metaHtmlNode.GetAttributeValue("content", "");

						metaHtmlNode = doc.DocumentNode.SelectSingleNode($"//img");
						if (metaHtmlNode != null)
							r.ImageURL = metaHtmlNode.GetAttributeValue("src", "");

						// Prova con i tag OpenGraph
						metaHtmlNode = doc.DocumentNode.SelectSingleNode($"//meta[@property='og:title']");
						if (metaHtmlNode != null)
							r.Title = metaHtmlNode.GetAttributeValue("content", "");

						metaHtmlNode = doc.DocumentNode.SelectSingleNode($"//meta[@property='og:description']");
						if (metaHtmlNode != null)
							r.Description = metaHtmlNode.GetAttributeValue("content", "");

						metaHtmlNode = doc.DocumentNode.SelectSingleNode($"//meta[@property='og:image']");
						if (metaHtmlNode != null)
							r.ImageURL = metaHtmlNode.GetAttributeValue("content", "");

						return r;
					}
				}
				catch (Exception Ex)
				{
					LogHelper.Error("Utility.HtmlPageCrawler.Analyze exception: {0}", Ex.Message);
				}

				return null;
			}
		}

		// Rimuove tutti i tag html dalla stringa passata e ritorna solo il testo
		public static string StripHTML(string html)
		{
			if (string.IsNullOrEmpty(html)) return "";

			HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
			doc.LoadHtml(html);
			string s = doc.DocumentNode.InnerText;

			return s;
		}

		// Rimuove tutti i tag html dalla stringa passata e ritorna solo il testo
		public class Sanitizer
		{
			static public String SanitizeHTML(String sHtml, String[] HtmlTagsWhiteList = null, String CssClassesWhiteList = null)
			{
				if (sHtml == null)
					return "";

				var sanitizer = HtmlSanitizer.SimpleHtml5Sanitizer();

				if (HtmlTagsWhiteList == null)
					HtmlTagsWhiteList = new string[] { "a" };

				foreach (String sk in HtmlTagsWhiteList)
				{
					if (sk != "br")
						sanitizer.Tag(sk).RemoveEmpty();

					if (sk == "a")
					{
						sanitizer.Tag(sk)
							.SetAttribute("target", "_blank")
							.SetAttribute("rel", "nofollow")
							.CheckAttributeUrl("href")
							.NoAttributes(SanitizerOperation.FlattenTag);
					}
				}
				// TK: FRANCESCO - CTBT-1322 START
				if (CssClassesWhiteList == null)
					CssClassesWhiteList = "text-left text-right text-center text-justify color1 color2 color3 color4 color5 color6 color7 color8 color9 color10 lh100 lh115 lh150 lh200 fs-11 fs-13 fs-15 fs-18 fs-22 fs-30 fs-36 fs-48 fs-60 typoFontOpenSans typoFontLora typoFontRoboto";
				sanitizer.AllowCss(CssClassesWhiteList.Split(' '));
				// TK: FRANCESCO - CTBT-1322 END				

				return sanitizer.Sanitize(sHtml);
			}
		}

		public class WebClientWithTimeout : System.Net.WebClient
		{
			public int Timeout { get; set; }

			protected override WebRequest GetWebRequest(Uri uri)
			{
				WebRequest lWebRequest = base.GetWebRequest(uri);
				lWebRequest.Timeout = Timeout;
				((HttpWebRequest)lWebRequest).ReadWriteTimeout = Timeout;
				return lWebRequest;
			}
		}

		public static String PostClient(Uri uri, KeyStringHelper kvRequest, Int32 Timeout = 15000)
		{
			try
			{
				String sResponse = null;
				using (HttpClient httpClient = new HttpClient())
				{
					httpClient.Timeout = TimeSpan.FromMilliseconds(Timeout);
					FormUrlEncodedContent content = new FormUrlEncodedContent(kvRequest);
					HttpResponseMessage httpResponse = httpClient.PostAsync(uri, content).Result;
					if (httpResponse != null)
					{
						sResponse = httpResponse.Content.ReadAsStringAsync().Result;
					}
				}
				return sResponse;
			}
			catch (WebException Ex)
			{
				if (Ex.Response != null)
				{
					var responseStream = Ex.Response.GetResponseStream();
					if (responseStream != null)
					{
						using (var reader = new System.IO.StreamReader(responseStream))
						{
							String Error = reader.ReadToEnd();
							LogHelper.Error("Utility.PostClient exception: {0}", Error);
						}
					}
				}

			}
			catch (Exception Ex)
			{
				LogHelper.Error("Utility.PostClient exception: {0}", Ex.Message);
			}

			return null;
		}

		public static bool SaveBitmapToJpg(Bitmap imgBitmap, String sFilePath)
		{
			try
			{
				ImageCodecInfo jpgEncoder = null;
				ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
				foreach (ImageCodecInfo codec in codecs)
				{
					if (codec.FormatID == ImageFormat.Jpeg.Guid)
					{
						jpgEncoder = codec;
					}
				}
				if (jpgEncoder != null)
				{
					//Forzo la risoluzione al massimo: 100L
					System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
					EncoderParameters myEncoderParameters = new EncoderParameters(1);
					EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 100L);
					myEncoderParameters.Param[0] = myEncoderParameter;
					imgBitmap.Save(sFilePath, jpgEncoder, myEncoderParameters);
					return true;
				}
			}
			catch (Exception Ex)
			{
				LogHelper.Error("Exception: " + Ex.Message);
			}
			return false;
		}

		public static bool IsFileLocked(String sFile)
		{
			FileInfo file = new FileInfo(sFile);
			FileStream stream = null;

			try
			{
				stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
			}
			catch (IOException)
			{
				return true;
			}
			finally
			{
				if (stream != null)
					stream.Close();
			}

			return false;
		}

		public static bool IsValidEmailAddress(string EmailAddress)
		{
			//TK: Cristian - CTBT-798 START
			bool retVal = false;

			try
			{
				//https://stackoverflow.com/questions/2049502/what-characters-are-allowed-in-an-email-address
				//https://blogs.msdn.microsoft.com/testing123/2009/02/06/email-address-test-cases/
				EmailAddressAttribute at = new EmailAddressAttribute();
				retVal = at.IsValid(EmailAddress);
				//Evita ò à è ù ì
				if (System.Text.Encoding.UTF8.GetByteCount(EmailAddress) == EmailAddress.Length == false)
					retVal = false;
				return retVal;
			}
			catch
			{
				return false;
			}
			//TK: Cristian - CTBT-798 END
		}

		static public string ConvertFromCharToASCII(String Text)
		{
			StringBuilder sb = new StringBuilder();
			foreach (char c in Text)
			{
				if (c > 127)
				{
					string encodedValue = "\\u" + ((int)c).ToString("x4");
					sb.Append(encodedValue);
				}
				else
				{
					sb.Append(c);
				}
			}
			return (sb.ToString());
		}

		static public string ConvertFromASCIIToChar(String Text)
		{
			try
			{
				Text = System.Text.RegularExpressions.Regex.Replace(Text, @"\\u(?<Value>[a-zA-Z0-9]{4})",
					m =>
					{
						return ((char)int.Parse(m.Groups["Value"].Value, System.Globalization.NumberStyles.HexNumber)).ToString();
					});
				return DecodeHTMLEntities(Text);
			}
			catch (Exception Ex)
			{
				LogHelper.Error("Error: " + Ex.Message);
			}
			return Text;
		}

		static public String DecodeHTMLEntities(String str)
		{
			String s = System.Net.WebUtility.HtmlEncode(str);
			return System.Net.WebUtility.HtmlDecode(str);
		}

		public static string GetFilterParamText(KeyValueHelper Filters, string paramName, string fieldName, string tab)
		{
			if (string.IsNullOrEmpty(Types.ToString(Filters[paramName])))
				return string.Empty;

			if (string.IsNullOrEmpty(fieldName))
				fieldName = paramName;

			//if (paramName == "Name") Filters["Name"] = RemoveAccents(Types.ToString(Filters["Name"]));

			//ALBERTO: BUG-52
			Filters[paramName] = Types.ToString(Filters[paramName]).Replace("'", "_");
			Filters[paramName] = Types.ToString(Filters[paramName]).Replace("’", "_");

			// TK: Cristian - CTBT-824 START
			if (Types.ToString(Filters[paramName]).ToLower().Equals("*null*"))
				return " AND " + tab + "." + fieldName + " IS NULL";
			else if (Types.ToString(Filters[paramName]).ToLower().Equals("*notnull*"))
				return " AND " + tab + "." + fieldName + " IS NOT NULL";
			else
				return " AND LOWER(" + tab + "." + fieldName + ") LIKE '%" + MySql.Data.MySqlClient.MySqlHelper.EscapeString(Types.ToString(Filters[paramName]).ToLower()) + "%'";  // TK: Cristian - CTBT-73
																																													// TK: Cristian - CTBT-824 END
		}

		public static string GetFilterParamJSONText(KeyValueHelper Filters, string paramName, string fieldName, string tab)
		{
			if (string.IsNullOrEmpty(Types.ToString(Filters[paramName])))
				return string.Empty;

			if (string.IsNullOrEmpty(fieldName))
				fieldName = paramName;

			//ALBERTO: BUG-52
			Filters[paramName] = Types.ToString(Filters[paramName]).Replace("'", "_");
			Filters[paramName] = Types.ToString(Filters[paramName]).Replace("’", "_");

			//if(paramName=="Name") Filters["Name"] = RemoveAccents(Types.ToString(Filters["Name"]));

			// TK: Cristian - CTBT-824 START
			if (Types.ToString(Filters[paramName]).ToLower().Equals("*null*"))
				return " AND " + tab + "." + fieldName + " IS NULL";
			else if (Types.ToString(Filters[paramName]).ToLower().Equals("*notnull*"))
				return " AND " + tab + "." + fieldName + " IS NOT NULL";
			else
				return " AND LOWER(JSON_EXTRACT(" + tab + "." + fieldName + ", '$." + paramName + "')) LIKE '%" + MySql.Data.MySqlClient.MySqlHelper.EscapeString(Types.ToString(Filters[paramName]).ToLower()) + "%'";
		}

		public static string GetFilterParamJSONMultiText(string Filter, string[] paramsName, string fieldName, string tab)
		{
			if (string.IsNullOrEmpty(Filter))
				return string.Empty;

			//ALBERTO: BUG-52
			Filter = Filter.Replace("'", "_");
			Filter = Filter.Replace("’", "_");

			//Filter = RemoveAccents(Filter);
			string[] paramsFilter = Filter.Split(' ');
			for (int i = 0; i < paramsFilter.Length; i++)
			{
				paramsFilter[i] = "'%" + paramsFilter[i] + "%'";
			}
			for (int i = 0; i < paramsName.Length; i++)
			{
				paramsName[i] = "'$." + paramsName[i] + "'";
			}

			return " AND LOWER(JSON_EXTRACT(" + tab + "." + fieldName + "," + String.Join(",", paramsName) + ")) LIKE (" + String.Join(" ", paramsFilter).ToLower() + ")";
		}

		public static string GetFilterParamTextArray(KeyValueHelper Filters, string paramName, string[] fieldNames, string tab)
		{
			if (string.IsNullOrEmpty(Types.ToString(Filters[paramName])))
				return string.Empty;

			if (fieldNames.Length == 0)
				fieldNames = new string[] { paramName };

			string sText = "";
			foreach (string fieldName in fieldNames)
			{
				if (sText == "")
					sText += " AND ";
				else
					sText += " OR ";

				// TK: Cristian - CTBT-824 START
				if (Types.ToString(Filters[paramName]).ToLower().Equals("*null*"))
					sText += " " + tab + "." + fieldName + " IS NULL";
				else if (Types.ToString(Filters[paramName]).ToLower().Equals("*notnull*"))
					sText += " " + tab + "." + fieldName + " IS NOT NULL";
				else
					sText += " LOWER(" + tab + "." + fieldName + ") LIKE '%" + MySql.Data.MySqlClient.MySqlHelper.EscapeString(Types.ToString(Filters[paramName]).ToLower()) + "%'";  // TK: Cristian - CTBT-73
			}

			return sText;// TK: Cristian - CTBT-824 END
		}

		// TK: Cristian - CTBT-824 START
		public static string GetFilterParamDate(KeyValueHelper Filters, string paramName, string fieldName, string tab)
		{
			if (string.IsNullOrEmpty(Types.ToString(Filters[paramName])))
				return string.Empty;

			if (string.IsNullOrEmpty(fieldName))
				fieldName = paramName;

			if (Types.ToString(Filters[paramName]).ToLower().Equals("*null*"))
				return " AND " + tab + "." + fieldName + " IS NULL";
			else if (Types.ToString(Filters[paramName]).ToLower().Equals("*notnull*"))
				return " AND " + tab + "." + fieldName + " IS NOT NULL";
			else
				return " AND " + tab + "." + fieldName + " BETWEEN '" + (Filters[paramName]) + "' AND '" + (Filters[paramName]) + "' + INTERVAL 1 DAY ";
		}

		public static string GetFilterParamNumber(KeyValueHelper Filters, string paramName, string fieldName, string tab)
		{
			if (string.IsNullOrEmpty(Types.ToString(Filters[paramName])))
				return string.Empty;

			if (string.IsNullOrEmpty(fieldName))
				fieldName = paramName;

			if (Types.ToString(Filters[paramName]).ToLower().Equals("*null*"))
				return " AND " + tab + "." + fieldName + " IS NULL";
			else if (Types.ToString(Filters[paramName]).ToLower().Equals("*notnull*"))
				return " AND " + tab + "." + fieldName + " IS NOT NULL";
			else if (Types.ToInt64(Filters[paramName]) > 0)
				return " AND " + tab + "." + fieldName + " = " + Types.ToInt64(Filters[paramName]);
			else
				return string.Empty;
		}
		// TK: Cristian - CTBT-824 END

		public static string GetFilterParamNumberWithNegativeValues(KeyValueHelper Filters, string paramName, string fieldName, string tab)
		{
			if (string.IsNullOrEmpty(Types.ToString(Filters[paramName])))
				return string.Empty;

			if (string.IsNullOrEmpty(fieldName))
				fieldName = paramName;

			if (Types.ToString(Filters[paramName]).ToLower().Equals("*null*"))
				return " AND " + tab + "." + fieldName + " IS NULL";
			else if (Types.ToString(Filters[paramName]).ToLower().Equals("*notnull*"))
				return " AND " + tab + "." + fieldName + " IS NOT NULL";
			else
				return " AND " + tab + "." + fieldName + " = " + Types.ToInt64(Filters[paramName]);
		}
		// TK: Cris

		public static string GetFilterParamNotNull(KeyValueHelper Filters, string paramName, string fieldName, string tab)
		{
			if (string.IsNullOrEmpty(Types.ToString(Filters[paramName])))
				return string.Empty;

			if (string.IsNullOrEmpty(fieldName))
				fieldName = paramName;

			if (Types.ToBool(Filters[paramName]) == true)
				return " AND " + tab + "." + fieldName + " IS NOT NULL";
			else
				return string.Empty;
		}

		public static string UrlEncodeRFC3986(string str)
		{
			String sOut = "";
			byte[] v = Encoding.UTF8.GetBytes(str);
			foreach (byte b in v)
			{
				if (
					(b >= 0x30 && b <= 0x39) ||
					(b >= 0x41 && b <= 0x5A) ||
					(b >= 0x61 && b <= 0x7A) ||
					(b == 0x2D || b == 0x2E || b == 0x5F || b == 0x7E)
				   )
				{
					sOut += Encoding.ASCII.GetString(new[] { b });
				}
				else
				{
					sOut += String.Format("%{0:X}", b);
				}
			}

			return sOut;
		}

		public static class PhoneNumber
		{
			public static String FormatToE164(String PhoneNumber, string DefaultCountryISOCode)
			{
				try
				{
					string RegionCode = Ln.GetRegionCodeFromCountryISOCode(DefaultCountryISOCode).ToUpper();
					if (Types.ToString(RegionCode) == "")
						return null;

					var PhoneUtil = PhoneNumberUtil.GetInstance();
					var sNumberProto = PhoneUtil.Parse(PhoneNumber, RegionCode);
					var sFormattedPhoneNumber = PhoneUtil.Format(sNumberProto, PhoneNumberFormat.E164);

					return sFormattedPhoneNumber;
				}
				catch (Exception Ex)
				{
					LogHelper.Error("FormatPhoneNumberToE164 exception: {0}", Ex.Message);
				}

				return null;
			}

			public static bool IsMobile(String PhoneNumber, string DefaultCountryISOCode)
			{
				try
				{
					string RegionCode = Ln.GetRegionCodeFromCountryISOCode(DefaultCountryISOCode).ToUpper();
					if (Types.ToString(RegionCode) == "")
						return false;

					var PhoneUtil = PhoneNumberUtil.GetInstance();
					var sNumberProto = PhoneUtil.Parse(PhoneNumber, RegionCode.ToUpper());

					return PhoneUtil.GetNumberType(sNumberProto) == PhoneNumberType.MOBILE;
				}
				catch (Exception Ex)
				{
					LogHelper.Error("IsMobileNumber exception: {0}", Ex.Message);
				}

				return false;
			}

			public static bool IsValid(String PhoneNumber, string DefaultCountryISOCode)
			{
				try
				{
					string RegionCode = Ln.GetRegionCodeFromCountryISOCode(DefaultCountryISOCode).ToUpper();
					if (Types.ToString(RegionCode) == "")
						return false;

					var PhoneUtil = PhoneNumberUtil.GetInstance();
					var sNumberProto = PhoneUtil.Parse(PhoneNumber, RegionCode.ToUpper());

					return PhoneUtil.IsValidNumberForRegion(sNumberProto, RegionCode);
				}
				catch (Exception Ex)
				{
					LogHelper.Error("IsValidNumber exception: {0}", Ex.Message);
				}

				return false;
			}

			public static string GetRegionCodeForNumber(String PhoneNumber, string DefaultCountryISOCode)
			{
				try
				{
					string RegionCode = Ln.GetRegionCodeFromCountryISOCode(DefaultCountryISOCode).ToUpper();
					if (Types.ToString(RegionCode) == "")
						return "";

					var PhoneUtil = PhoneNumberUtil.GetInstance();
					var sNumberProto = PhoneUtil.Parse(PhoneNumber, RegionCode.ToUpper());

					//prendo il codice solo se il numero� mobile
					if (PhoneUtil.GetNumberType(sNumberProto) != PhoneNumberType.MOBILE)
						return "";

					return PhoneUtil.GetRegionCodeForNumber(sNumberProto);
				}
				catch (Exception Ex)
				{
					LogHelper.Error("GetRegionCodeForNumber exception: {0}", Ex.Message);
				}

				return null;
			}

			public static string GetPrefix(String PhoneNumber, string DefaultCountryISOCode)
			{
				try
				{
					if (PhoneNumber == "" || PhoneNumber == null)
						return "";

					string RegionCode = Ln.GetRegionCodeFromCountryISOCode(DefaultCountryISOCode).ToUpper();
					if (Types.ToString(RegionCode) == "")
						return "";

					var PhoneUtil = PhoneNumberUtil.GetInstance();
					var sNumberProto = PhoneUtil.Parse(PhoneNumber, RegionCode.ToUpper());


					return sNumberProto.CountryCode.ToString();
				}
				catch (Exception Ex)
				{
					LogHelper.Error("GetRegionCodeForNumber exception: {0}", Ex.Message);
				}

				return null;
			}

			public static string GetMobileNumberWithoutPrefix(String PhoneNumber, string DefaultCountryISOCode)
			{
				try
				{
					if (PhoneNumber == null || PhoneNumber == "")
						return "";

					String Prefix = "+" + Utilities.PhoneNumber.GetPrefix(PhoneNumber, DefaultCountryISOCode);

					return PhoneNumber.Replace(Prefix, "");
				}
				catch (Exception Ex)
				{
					LogHelper.Error("GetRegionCodeForNumber exception: {0}", Ex.Message);
				}

				return null;
			}
		}

		public delegate void TSliceIterator<T>(List<T> vSlice);
		public static void IterateSlices<T>(T[] vIds, int SliceSize, TSliceIterator<T> SliceIterator)
		{
			List<T> lsIds = new List<T>(vIds);
			while (lsIds.Count > 0)
			{
				List<T> ls = lsIds.GetRange(0, Math.Min(SliceSize, lsIds.Count));
				lsIds.RemoveRange(0, Math.Min(SliceSize, lsIds.Count));
				SliceIterator(ls);
			}
		}

		static public bool LogPayWithCard(String Text, String Path, bool StartNewPayment = false)
		{
			try
			{
				using (StreamWriter sw = File.AppendText(Path))
				{
					if (StartNewPayment)
						sw.WriteLineAsync(Text);
					sw.WriteLineAsync(Text);
				}
				return true;
			}
			catch (Exception Ex)
			{
				LogHelper.Error("Utilities.LogPayWithCard.Exception: {0}", Ex.Message);
			}
			return false;
		}

		// https://docs.microsoft.com/it-it/dotnet/api/system.security.cryptography.md5?view=netframework-4.8
		static public string GetMd5Hash(MD5 md5Hash, string input)
		{

			// Convert the input string to a byte array and compute the hash.
			byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

			// Create a new Stringbuilder to collect the bytes
			// and create a string.
			StringBuilder sBuilder = new StringBuilder();

			// Loop through each byte of the hashed data 
			// and format each one as a hexadecimal string.
			for (int i = 0; i < data.Length; i++)
			{
				sBuilder.Append(data[i].ToString("x2"));
			}

			// Return the hexadecimal string.
			return sBuilder.ToString();
		}

		static public bool CheckAvailableTcpPort(String Ip, int port)
		{
			IPAddress ip = IPAddress.Parse(Ip);
			IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
			IPEndPoint[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpListeners();

			foreach (IPEndPoint endpoint in tcpConnInfoArray)
			{
				if (endpoint.Address.Equals(ip) == true && endpoint.Port == port)
					return false;
			}

			return true;
		}

		static string RemoveAccents(string text)
		{
			if (text == null)
				return "";
			var normalizedString = text.Normalize(NormalizationForm.FormD);
			var stringBuilder = new StringBuilder();

			foreach (var c in normalizedString)
			{
				var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
				if (unicodeCategory != UnicodeCategory.NonSpacingMark)
				{
					stringBuilder.Append(c);
				}
			}

			return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
		}

	}
}
