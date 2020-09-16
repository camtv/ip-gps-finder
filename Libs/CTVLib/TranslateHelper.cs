using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;


// LA LANG VIENE TRATTATA SEMPRE IN MINUSCOLO
namespace Helpers
{
    // Ln class contains languages translations related matters
    public class Ln
    {
		public class TranslatorEntry
		{
			public bool Used;
			public String Key;
			public String Translation;
			public String SourceReference;
			public String Notes;
		}

		public class TranslationNeedle
		{
			public bool Used = false;
			public String SourceReference="";
			public Int64 UnixTimeLastModify = -1;

			public List<String> Translations = new List<String>();
			public List<String> TranslatorNotes = new List<String>();

			public event Action<String, String> OnTranslationChangeEvent;

			public String GetTranslation(String Lang,bool UpdateUsed=false)
			{
				try
				{
					int iLang = Ln.SupportedLanguages.IndexOf(Lang.ToLower());
					if (Translations==null || iLang < 0 || iLang > Translations.Count)
						return null;

					String sTranslation = Translations[iLang];
					if (sTranslation != null)
					{
						sTranslation = sTranslation.Replace("\n", "\r\n");

						return sTranslation;
					}
				}
				catch {  }
                return null;
			}

			public void SetTranslation(String Lang, String Translation, String TranslatorNote=null)
			{
				try
				{
					int iLang = Ln.SupportedLanguages.IndexOf(Lang.ToLower());
					if (Translations == null || iLang < 0 || iLang > Translations.Count)
						return;

					String sOld = Translations[iLang];
					Translations[iLang] = Translation;
					//TranslatorNotes[iLang] = TranslatorNote;
					UnixTimeLastModify = Utilities.GetUnixTimestamp();
					if (sOld != Translation && OnTranslationChangeEvent != null)
						OnTranslationChangeEvent(Lang, Translation);
				}
				catch { }
			}
		}

		public static String TranslationFile = "";

        // will contain specified language translated strings loaded from translations database
		public static KeyValueHelperBase<TranslationNeedle> Translations = new KeyValueHelperBase<TranslationNeedle>();

        public const string csDefaultISOCode = "USA";
        public const string csDefaultLocale = "en_US";

        public static string csDefaultLang { get { return csDefaultLocale.Substring(0, 2).ToLower(); } }        

		// Provvisorio da migliorare (magari caricare un DB o file esterno per i locale)
		public static KeyStringHelper SupportedLocales = new KeyStringHelper();
		public static List<String> SupportedLanguages = new List<String>();

		public static String[] GetSupportedLanguages()
		{
			// Gestisce il caso che non è stato caricato alcun db di traduzioni, usa i default
			if (SupportedLanguages == null || SupportedLanguages.Count==0)
			{
				SupportedLocales.Add(csDefaultLang,csDefaultLocale);
				SupportedLanguages.Add(csDefaultLang);
			}

			return SupportedLanguages.ToArray();
		}

		public static bool IsLanguageSupported(String ln)
		{
			if (ln == null || ln.Length != 2)
				return false;

			String Lang = ln.ToLower();
			return (SupportedLanguages.IndexOf(Lang) != -1);
		}

		public static String LangToLocale(String Lang)
		{
			String loc = SupportedLocales[Lang.ToLower()];
			if (loc == null)
				return SupportedLocales[csDefaultLang.ToLower()];

			return loc;
		}

        public static bool Init(String sFile)
        {
            SupportedLanguages.Clear();

			Ln.TranslationFile = sFile;

			if (sFile == null || sFile == "")
				return false;
			
            string sPath = Path.GetFullPath(sFile);

			return LoadTranslationsFromXLS(sPath);
        }

		public static string Ts(string sString, String Lang = null, Action<String, String> OnTranslateChangeHanlder = null, String _SourceReference=null)
        {
            try
            {
                if (sString == null || sString == "")
                    return "";

				if (Lang == null)
					Lang = csDefaultLang;

				// Sistema il lang che deve essere di due caratteri ed uppercase
				Lang = Lang.Substring(0,2).ToLower();

                // "\r\n" could create problems if used in Excel files
                // therefore, it's necessary to transform it into "\n" before writing into Excel file
                // and then to change it back after reading from Excel file
                string sKey = sString.Replace("\r\n", "\n");
				TranslationNeedle tnl = Translations[sKey];
				if (tnl != null)
                {
					if (_SourceReference != null)
						tnl.SourceReference = _SourceReference;

					String sTranslation = tnl.GetTranslation(Lang);
					if (sTranslation == null || sTranslation == "")
						sTranslation = RemoveTags(sString);

					tnl.Used = true;
					if (OnTranslateChangeHanlder != null)
						tnl.OnTranslationChangeEvent += OnTranslateChangeHanlder;

					return sTranslation;
                }

                // if in debug mode then adds missing strings to translations database 
                // else 
				tnl = new TranslationNeedle();
				tnl.Used = false;
				if (_SourceReference != null)
					tnl.SourceReference = _SourceReference;
				Translations.Add(sKey, tnl);                
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
            }

            return sString;
        }

		// Libreria non compatibile netcore
		//public static bool LoadTranslationsFromXLS(String sFile)
		//{
		//	try
		//	{
		//		SpreadsheetInfo.SetLicense("EFY0-NQA2-LETM-TJ54");
		//		ExcelFile book = ExcelFile.Load(sFile);
		//		ExcelWorksheet sheet = book.Worksheets[0];

		//		List<string> columns = new List<string>();
		//		string sKey = "";

		//		// traverse rows by Index				
		//		foreach (ExcelRow row in sheet.Rows)
		//		{
		//			// the first excel row is assumed to be columns names
		//			if (row.Index == 0)
		//			{
		//				// inizializzazione dei Locales in base alle lingue presenti nel file di traduzione
		//				int i = 1;
		//				while (true)
		//				{
		//					ExcelCell cell = row.Cells[i];

		//					if (cell.ValueType != CellValueType.String)
		//						break;

		//					String Locale = cell.StringValue;
		//					String Lang = Locale.Substring(0, 2).ToLower();
		//					if (Locale.Length == 5)
		//						Locale = Locale.Substring(0, 2).ToLower() + "_" + Locale.Substring(3, 2).ToUpper();
		//					else
		//						Locale = Lang + "_" + Lang.ToUpper();

		//					SupportedLocales.Add(Lang, Locale);
		//					SupportedLanguages.Add(Lang);
		//					columns.Add(Lang);
		//					i++;
		//				}
		//			}
		//			else
		//			{
		//				ExcelCell KeyCell = row.Cells[1];
		//				sKey = Types.ToString(KeyCell.Value).Replace("\r\n", "\n");

		//				TranslationNeedle tnl = new TranslationNeedle();
		//				tnl.Used = false;
		//				for (int i = 0; i < columns.Count; i++)
		//				{
		//					int iCellIndex = i + 1;
		//					ExcelCell TnsCell = row.Cells[iCellIndex];
		//					tnl.Translations.Add(Types.ToString(TnsCell.Value).Replace("\r\n", "\n"));
		//				}
		//				Translations[sKey] = tnl;
		//			}
		//		}

		//		return true;
		//	}
		//	catch (Exception ex)
		//	{
		//		LogHelper.Error(ex.Message);
		//	}

		//	return false;
		//}

		//public static void SaveTranslationsToXLS(String SFile)
		//{
		//	ExcelFile book = ExcelFile.Load(SFile);
		//	ExcelWorksheet sheet = book.Worksheets[0];

		//	int nRow = 1;
		//	foreach (String sKey in Translations.Keys)
		//	{
		//		TranslationNeedle tnl = (TranslationNeedle)Translations[sKey];
		//		if (tnl.Used)
		//			sheet.Cells[nRow, 0].Value ="*";
		//		else
		//			sheet.Cells[nRow, 0].Value = "";

		//		sheet.Cells[nRow, 1].Value = sKey;
			
		//		for (int i = 0; i < tnl.Translations.Count;i++ )
		//		{
		//			int iCellIndex = i + 1;
		//			sheet.Cells[nRow, iCellIndex].Value = tnl.Translations[i];
		//		}

		//		nRow++;
		//	}

		//	book.Save(SFile);
		//}

		public static bool LoadTranslationsFromXLS(String sFile)
		{
			try
			{
				FileInfo xlsFile = new FileInfo(sFile);
				using (OfficeOpenXml.ExcelPackage package = new OfficeOpenXml.ExcelPackage(xlsFile))
				{
					//Open worksheet 1
					OfficeOpenXml.ExcelWorksheet sheet = package.Workbook.Worksheets[1];

					List<string> columns = new List<string>();
					string sKey = "";

					var rowCount = sheet.Dimension.End.Row;
					var colCount = sheet.Dimension.End.Column + 1;

					// traverse rows by Index				
					for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
					{
						// the first excel row is assumed to be columns names
						if (rowIndex == 0)
						{
							// inizializzazione dei Locales in base alle lingue presenti nel file di traduzione
							int i = 1;
							while (true)
							{
								OfficeOpenXml.ExcelRange cell = sheet.Cells[rowIndex + 1, i + 1];

								if (cell.Text == "")
									break;

								String Locale = cell.Text;
								String Lang = Locale.Substring(0, 2).ToLower();
								if (Locale.Length == 5)
									Locale = Locale.Substring(0, 2).ToLower() + "_" + Locale.Substring(3, 2).ToUpper();
								else
									Locale = Lang + "_" + Lang.ToUpper();

								SupportedLocales.Add(Lang, Locale);
								SupportedLanguages.Add(Lang);
								columns.Add(Lang);
								i++;
							}
						}
						else
						{
							OfficeOpenXml.ExcelRange KeyCell = sheet.Cells[rowIndex + 1, 1 + 1];
							sKey = Types.ToString(KeyCell.Value).Replace("\r\n", "\n");

							TranslationNeedle tnl = new TranslationNeedle();
							tnl.Used = false;
							for (int i = 0; i < columns.Count; i++)
							{
								int iCellIndex = i + 1;
								OfficeOpenXml.ExcelRange TnsCell = sheet.Cells[rowIndex + 1, iCellIndex + 1];
								tnl.Translations.Add(Types.ToString(TnsCell.Text).Replace("\r\n", "\n"));
							}
							Translations[sKey] = tnl;
						}
					}

				}

				return true;
			}
			catch (Exception ex)
			{
				LogHelper.Error(ex.Message);
			}

			return false;
		}

		public static bool SaveTranslationsToXLS(String sFile)
		{

			try
			{
				FileInfo xlsFile = new FileInfo(sFile);
				using (OfficeOpenXml.ExcelPackage package = new OfficeOpenXml.ExcelPackage(xlsFile))
				{
					//Open worksheet 1
					OfficeOpenXml.ExcelWorksheet sheet = package.Workbook.Worksheets[1];

					int nRow = 1;
					foreach (String sKey in Translations.Keys)
					{
						TranslationNeedle tnl = (TranslationNeedle)Translations[sKey];
						if (tnl.Used)
							sheet.Cells[nRow+1, 0+1].Value = "*";
						else
							sheet.Cells[nRow+1, 0+1].Value = "";

						sheet.Cells[nRow+1, 1+1].Value = sKey;

						for (int i = 0; i < tnl.Translations.Count; i++)
						{
							int iCellIndex = i + 1;
							sheet.Cells[nRow+1, iCellIndex+1].Value = tnl.Translations[i];
						}

						nRow++;
					}
					package.SaveAs(xlsFile);
				}				

				return true;
			}
			catch (Exception ex)
			{
				LogHelper.Error(ex.Message);
			}

			return false;

		}

		public static TranslatorEntry[] GetTranslatorEntries(String Lang,Int64 Filter_UnixTimeLastModify=-1)
		{
			List<TranslatorEntry> lsTe = new List<TranslatorEntry>();
			foreach (String sKey in Translations.Keys)
			{
				TranslationNeedle tnl = (TranslationNeedle)Translations[sKey];
				if (Filter_UnixTimeLastModify != -1 && tnl.UnixTimeLastModify <= Filter_UnixTimeLastModify)
					continue;

				TranslatorEntry te = new TranslatorEntry();
				te.Key = sKey;
				te.Translation = tnl.GetTranslation(Lang);
				te.Used = tnl.Used;
				te.SourceReference = tnl.SourceReference;

				lsTe.Add(te);
			}

			return lsTe.OrderBy(o => o.Used == false).ToArray();
		}

        static string RemoveTags(string s)
        {
            int n = s.LastIndexOf('§');
            if (n != -1)
                s = s.Substring(0, n);

            return s;
        }

        static public string GetNearestSupportedLanguage(string[] sLngOptions)
        {
            if (sLngOptions == null || sLngOptions.Length == 0)
                return csDefaultLang;

            if (Translations == null)
				Init(Ln.TranslationFile);

            if (Translations == null)
                return csDefaultLang;

            foreach (String sLn in sLngOptions)
            {
                String sLnOpt = char.ToUpper(sLn[0]) + sLn.Substring(1).ToLower();
				if (SupportedLanguages.Contains(sLnOpt.ToLower()) == true)
                    return sLnOpt;
            }

            return csDefaultLang;
        }

		public static String CountyISOCode(String Lang)
		{
			String ISOCode = vdCountryISOCodes[Lang.ToLower()];
			if (ISOCode == null)
				return vdCountryISOCodes["us"]; // Di default prende l'iso-code USA

			return ISOCode;
		}

        public static String GetNearestCountyISOCode(string[] sLngCodeOptions)
        {
            for (int i = 0; i < sLngCodeOptions.Length; i++)
            {
                String ISOCode = vdCountryISOCodes[sLngCodeOptions[i].ToLower()];
                if (ISOCode != null)
                    return ISOCode;
            }

            return vdCountryISOCodes["us"];  // Di default prende l'iso-code USA
        }

		public static String GetRegionCodeFromCountryISOCode(String CountryISOCode)
		{
			String Prefix = vdCountryISOCodeToRegionCode[CountryISOCode];
			return Prefix;
		}

		public static bool HasEuroCurrency(String sLang)
		{
			foreach(string kLang in vdCountryISOCodesWithEuroCurrency.Keys)
			{
				if (kLang == sLang)
					return true;
			}

			return false;
		}

		public static string GetTwoLettersCountryISOCode(String sLang)
		{
			foreach (string lang in vdCountryISOCodes.Values)
			{
				if (lang == sLang)
				{
					var sCountryISOCode = vdCountryISOCodes.FirstOrDefault(x => x.Value.Contains(sLang));
					return sCountryISOCode.Key;
				}
			}

			return null;
		}

		static KeyStringHelper vdCountryISOCodesWithEuroCurrency = new KeyStringHelper() 
		{ 
			{"at","AUT"},
			{"be","BEL"},
			{"cy","CYP"},
			{"ee","EST"},
			{"fi","FIN"},
			{"fr","FRA"},
			{"de","DEU"},
			{"gr","GRC"},
			{"ie","IRL"},
			{"it","ITA"},
			{"lv","LVA"},
			{"lt","LTU"},
			{"lu","LUX"},
			{"mt","MLT"},
			{"nl","NLD"},
			{"pt","PRT"},
			{"sk","SVK"},
			{"si","SVN"},
			{"es","ESP"},
		};

        // Dati presi da: https://countrycode.org
        static KeyStringHelper vdCountryISOCodes = new KeyStringHelper()
		{
			{"af","AFG"},
			{"al","ALB"},
			{"dz","DZA"},
			{"as","ASM"},
			{"ad","AND"},
			{"ao","AGO"},
			{"ai","AIA"},
			{"aq","ATA"},
			{"ag","ATG"},
			{"ar","ARG"},
			{"am","ARM"},
			{"aw","ARM"},
			{"aw","ABW"},
			{"au","AUS"},
			{"at","AUT"},
			{"az","AZE"},
			{"bb","BRB"},
			{"by","BLR"},
			{"be","BEL"},
			{"bz","BLZ"},
			{"bj","BEN"},
			{"bm","BMU"},
			{"bt","BTN"},
			{"bo","BOL"},
			{"ba","BIH"},
			{"bw","BWA"},
			{"br","BRA"},
			{"io","IOT"},
			{"vg","VGR"},
			{"bn","BNR"},
			{"bg","BGR"},
			{"bf","BFA"},
			{"bi","BDI"},
			{"kh","KHM"},
			{"cm","CMR"},
			{"ca","CAN"},
			{"cv","CPV"},
			{"ky","CYM"},
			{"cf","CAF"},
			{"td","TCD"},
			{"cl","CHL"},
			{"cn","CHN"},
			{"CX","CXR"},
			{"cc","CCK"},
			{"co","COL"},
			{"km","COM"},
			{"ck","COK"},
			{"cr","CRI"},
			{"hr","HRV"},
			{"cu","CUB"},
			{"cw","CUW"},
			{"cy","CYP"},
			{"cz","CZE"},
			{"cd","COD"},
			{"dk","DNK"},
			{"dj","DJI"},
			{"dm","DMA"},
			{"do","DOM"},
			{"tl","TLS"},
			{"ec","ECU"},
			{"eg","EGY"},
			{"sv","SLV"},
			{"gq","GNQ"},
			{"er","ERI"},
			{"ee","EST"},
			{"et","ETH"},
			{"fk","FLK"},
			{"fo","FRO"},
			{"fj","FJI"},
			{"fi","FIN"},
			{"fr","FRA"},
			{"pf","PYF"},
			{"ga","GAB"},
			{"gm","GMB"},
			{"ge","GEO"},
			{"de","DEU"},
			{"gh","GHA"},
			{"gi","GIB"},
			{"gr","GRC"},
			{"gl","GRL"},
			{"gd","GRD"},
			{"gu","GUM"},
			{"gt","GTM"},
			{"gg","GGY"},
			{"gn","GIN"},
			{"gw","GNB"},
			{"gy","GUY"},
			{"ht","HTI"},
			{"hn","HND"},
			{"hk","HKG"},
			{"hu","HUN"},
			{"is","ISL"},
			{"in","IND"},
			{"id","IDN"},
			{"ir","IRN"},
			{"iq","IRQ"},
			{"ie","IRL"},
			{"im","IMN"},
			{"il","ISR"},
			{"it","ITA"},
			{"ci","CIV"},
			{"jm","JAM"},
			{"jp","JPN"},
			{"je","JEY"},
			{"jo","JOR"},
			{"kz","KAZ"},
			{"ke","KEN"},
			{"ki","KIR"},
			{"xk","XKX"},
			{"kw","KWT"},
			{"kg","KGZ"},
			{"la","LAO"},
			{"lv","LVA"},
			{"lb","LBN"},
			{"ls","LSO"},
			{"lr","LBR"},
			{"ly","LBY"},
			{"li","LIE"},
			{"lt","LTU"},
			{"lu","LUX"},
			{"mo","MAC"},
			{"mk","MKD"},
			{"mg","MDG"},
			{"mw","MWI"},
			{"mv","MDV"},
			{"ml","MLI"},
			{"mt","MLT"},
			{"mh","MHL"},
			{"mr","MRT"},
			{"mu","MUS"},
			{"yt","MYT"},
			{"mx","MEX"},
			{"fm","FSM"},
			{"md","MDA"},
			{"mc","MCO"},
			{"mn","MNG"},
			{"me","MNE"},
			{"ms","MSR"},
			{"ma","MAR"},
			{"mz","MOZ"},
			{"mm","MMR"},
			{"na","NAM"},
			{"nr","NRU"},
			{"np","NPL"},
			{"nl","NLD"},
			{"an","ANT"},
			{"nc","NCL"},
			{"nz","NZL"},
			{"ni","NIC"},
			{"ne","NER"},
			{"ng","NGA"},
			{"nu","NIU"},
			{"KP","PRK"},
			{"mp","MNP"},
			{"no","NOR"},
			{"om","OMN"},
			{"pk","PAK"},
			{"pw","PLW"},
			{"ps","PSE"},
			{"pa","PAN"},
			{"pg","PNG"},
			{"py","PRY"},
			{"pe","PER"},
			{"ph","PHL"},
			{"pn","PCN"},
			{"pl","POL"},
			{"pt","PRT"},
			{"pr","PRI"},
			{"qa","QAT"},
			{"cg","COG"},
			{"re","REU"},
			{"ro","ROU"},
			{"ru","RUS"},
			{"rw","RWA"},
			{"bl","BLM"},
			{"sh","SHN"},
			{"kn","KNA"},
			{"lc","LCA"},
			{"mf","MAF"},
			{"pm","SPM"},
			{"vc","VCT"},
			{"ws","WSM"},
			{"sm","SMR"},
			{"st","STR"},
			{"sa","SAU"},
			{"sn","SEN"},
			{"rs","SRB"},
			{"sc","SYC"},
			{"sl","SLE"},
			{"sg","SGP"},
			{"sx","SXM"},
			{"sk","SVK"},
			{"si","SVN"},
			{"sb","SLB"},
			{"so","SOM"},
			{"za","ZAF"},
			{"kr","KOR"},
			{"ss","SSD"},
			{"es","ESP"},
			{"lk","LKA"},
			{"sd","SDN"},
			{"sr","SUR"},
			{"sj","SJM"},
			{"sz","SWZ"},
			{"se","SWE"},
			{"ch","CHE"},
			{"sy","SYR"},
			{"tw","TWN"},
			{"tj","TJK"},
			{"tz","TZA"},
			{"th","THA"},
			{"tg","TGO"},
			{"tk","TKL"},
			{"to","TON"},
			{"tt","TTO"},
			{"tn","TUN"},
			{"tr","TUR"},
			{"tm","TKM"},
			{"tc","TCA"},
			{"tv","TUV"},
			{"vi","VIR"},
			{"ug","UGA"},
			{"ua","UKR"},
			{"ae","AER"},
			{"gb","GBR"},
			{"us","USA"},
			{"uy","URY"},
			{"uz","UZB"},
			{"vu","VUT"},
			{"va","VAT"},
			{"ve","VEN"},
			{"vn","VNM"},
			{"wf","WLF"},
			{"eh","ESH"},
			{"ye","YEM"},
			{"zm","ZMB"},
			{"zw","ZWE"},            
        };
		
		static KeyStringHelper vdCountryISOCodeToRegionCode = new KeyStringHelper()
		{
			{"AFG","af"},
			{"ALB","al"},
			{"DZA","dz"},
			{"ASM","as"},
			{"AND","ad"},
			{"AGO","ao"},
			{"AIA","ai"},
			{"ATA","aq"},
			{"ATG","ag"},
			{"ARG","ar"},
			{"ARM","am"},
			{"ARM","aw"},
			{"ABW","aw"},
			{"AUS","au"},
			{"AUT","at"},
			{"AZE","az"},
			{"BRB","bb"},
			{"BLR","by"},
			{"BEL","be"},
			{"BLZ","bz"},
			{"BEN","bj"},
			{"BMU","bm"},
			{"BTN","bt"},
			{"BOL","bo"},
			{"BIH","ba"},
			{"BWA","bw"},
			{"BRA","br"},
			{"IOT","io"},
			{"VGR","vg"},
			{"BNR","bn"},
			{"BGR","bg"},
			{"BFA","bf"},
			{"BDI","bi"},
			{"KHM","kh"},
			{"CMR","cm"},
			{"CAN","ca"},
			{"CPV","cv"},
			{"CYM","ky"},
			{"CAF","cf"},
			{"TCD","td"},
			{"CHL","cl"},
			{"CHN","cn"},
			{"CXR","CX"},
			{"CCK","cc"},
			{"COL","co"},
			{"COM","km"},
			{"COK","ck"},
			{"CRI","cr"},
			{"HRV","hr"},
			{"CUB","cu"},
			{"CUW","cw"},
			{"CYP","cy"},
			{"CZE","cz"},
			{"COD","cd"},
			{"DNK","dk"},
			{"DJI","dj"},
			{"DMA","dm"},
			{"DOM","do"},
			{"TLS","tl"},
			{"ECU","ec"},
			{"EGY","eg"},
			{"SLV","sv"},
			{"GNQ","gq"},
			{"ERI","er"},
			{"EST","ee"},
			{"ETH","et"},
			{"FLK","fk"},
			{"FRO","fo"},
			{"FJI","fj"},
			{"FIN","fi"},
			{"FRA","fr"},
			{"PYF","pf"},
			{"GAB","ga"},
			{"GMB","gm"},
			{"GEO","ge"},
			{"DEU","de"},
			{"GHA","gh"},
			{"GIB","gi"},
			{"GRC","gr"},
			{"GRL","gl"},
			{"GRD","gd"},
			{"GUM","gu"},
			{"GTM","gt"},
			{"GGY","gg"},
			{"GIN","gn"},
			{"GNB","gw"},
			{"GUY","gy"},
			{"HTI","ht"},
			{"HND","hn"},
			{"HKG","hk"},
			{"HUN","hu"},
			{"ISL","is"},
			{"IND","in"},
			{"IDN","id"},
			{"IRN","ir"},
			{"IRQ","iq"},
			{"IRL","ie"},
			{"IMN","im"},
			{"ISR","il"},
			{"ITA","it"},
			{"CIV","ci"},
			{"JAM","jm"},
			{"JPN","jp"},
			{"JEY","je"},
			{"JOR","jo"},
			{"KAZ","kz"},
			{"KEN","ke"},
			{"KIR","ki"},
			{"XKX","xk"},
			{"KWT","kw"},
			{"KGZ","kg"},
			{"LAO","la"},
			{"LVA","lv"},
			{"LBN","lb"},
			{"LSO","ls"},
			{"LBR","lr"},
			{"LBY","ly"},
			{"LIE","li"},
			{"LTU","lt"},
			{"LUX","lu"},
			{"MAC","mo"},
			{"MKD","mk"},
			{"MDG","mg"},
			{"MWI","mw"},
			{"MDV","mv"},
			{"MLI","ml"},
			{"MLT","mt"},
			{"MHL","mh"},
			{"MRT","mr"},
			{"MUS","mu"},
			{"MYT","yt"},
			{"MEX","mx"},
			{"FSM","fm"},
			{"MDA","md"},
			{"MCO","mc"},
			{"MNG","mn"},
			{"MNE","me"},
			{"MSR","ms"},
			{"MAR","ma"},
			{"MOZ","mz"},
			{"MMR","mm"},
			{"NAM","na"},
			{"NRU","nr"},
			{"NPL","np"},
			{"NLD","nl"},
			{"ANT","an"},
			{"NCL","nc"},
			{"NZL","nz"},
			{"NIC","ni"},
			{"NER","ne"},
			{"NGA","ng"},
			{"NIU","nu"},
			{"PRK","KP"},
			{"MNP","mp"},
			{"NOR","no"},
			{"OMN","om"},
			{"PAK","pk"},
			{"PLW","pw"},
			{"PSE","ps"},
			{"PAN","pa"},
			{"PNG","pg"},
			{"PRY","py"},
			{"PER","pe"},
			{"PHL","ph"},
			{"PCN","pn"},
			{"POL","pl"},
			{"PRT","pt"},
			{"PRI","pr"},
			{"QAT","qa"},
			{"COG","cg"},
			{"REU","re"},
			{"ROU","ro"},
			{"RUS","ru"},
			{"RWA","rw"},
			{"BLM","bl"},
			{"SHN","sh"},
			{"KNA","kn"},
			{"LCA","lc"},
			{"MAF","mf"},
			{"SPM","pm"},
			{"VCT","vc"},
			{"WSM","ws"},
			{"SMR","sm"},
			{"STR","st"},
			{"SAU","sa"},
			{"SEN","sn"},
			{"SRB","rs"},
			{"SYC","sc"},
			{"SLE","sl"},
			{"SGP","sg"},
			{"SXM","sx"},
			{"SVK","sk"},
			{"SVN","si"},
			{"SLB","sb"},
			{"SOM","so"},
			{"ZAF","za"},
			{"KOR","kr"},
			{"SSD","ss"},
			{"ESP","es"},
			{"LKA","lk"},
			{"SDN","sd"},
			{"SUR","sr"},
			{"SJM","sj"},
			{"SWZ","sz"},
			{"SWE","se"},
			{"CHE","ch"},
			{"SYR","sy"},
			{"TWN","tw"},
			{"TJK","tj"},
			{"TZA","tz"},
			{"THA","th"},
			{"TGO","tg"},
			{"TKL","tk"},
			{"TON","to"},
			{"TTO","tt"},
			{"TUN","tn"},
			{"TUR","tr"},
			{"TKM","tm"},
			{"TCA","tc"},
			{"TUV","tv"},
			{"VIR","vi"},
			{"UGA","ug"},
			{"UKR","ua"},
			{"AER","ae"},
			{"GBR","gb"},
			{"USA","us"},
			{"URY","uy"},
			{"UZB","uz"},
			{"VUT","vu"},
			{"VAT","va"},
			{"VEN","ve"},
			{"VNM","vn"},
			{"WLF","wf"},
			{"ESH","eh"},
			{"YEM","ye"},
			{"ZMB","zm"},
			{"ZWE","zw"},
		};
	}
}