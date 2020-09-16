using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Helpers
{
	public class CIniFile
	{
		KeyValueHelper kvData = new KeyValueHelper();
		Dictionary<String, String> OriginalFile = new Dictionary<string, string>();

		public delegate void TLogger(String message, params object[] values);
		public TLogger Logger = null;

		public String FileName = "";

		public static String RandomString(int size)
		{
			Random _rng = new Random(Environment.TickCount);
			string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

			char[] buffer = new char[size];

			for (int i = 0; i < size; i++)
			{
				buffer[i] = _chars[_rng.Next(_chars.Length)];
			}
			return new String(buffer);
		}

		public bool LoadFromFile(String File)
		{
			try
			{
				System.IO.StreamReader file = new System.IO.StreamReader(File);

				String line = "";
				OriginalFile = new Dictionary<string, string>();
				kvData.Clear();
				while ((line = file.ReadLine()) != null)
				{
					//TK: IVAN - 20181113_INI

                    String s = line.Trim();
                    String RValue = null;
                    String LValue = null;

                    if (s.StartsWith("#") == true || s == "")
                        continue;

                    if (s.EndsWith(";") == true)
                        s = s.Remove(s.Length - 1, 1);

                    // Trova il primo = 
                    int EqPos = s.IndexOf('=');
                    if (EqPos == -1)
                        throw new Exception("Sintax error missing =");

                    LValue = s.Substring(0, EqPos).Trim();
                    if (LValue == "")
                        throw new Exception("Sintax error missing l-value");

                    RValue = s.Substring(EqPos + 1).Trim();
                    if (RValue == "")
                        throw new Exception("Sintax error missing r-value");

                    if ((RValue.StartsWith("\"") == true && RValue.EndsWith("\"") == false))
                        throw new Exception("Sintax error missing closing \"");

                    if ((RValue.StartsWith("\"") == false && RValue.EndsWith("\"") == true))
                        throw new Exception("Sintax error missing starting \"");

                    if (RValue.StartsWith("\"") == true)
                    {
                        RValue = RValue.Substring(1);
                        RValue = RValue.Substring(0, RValue.Length - 1);
                    }

                    kvData.Add(LValue, RValue);


                    //String s = line.Trim();
                    //String[] vs = s.Split(new char[] {'='});
					
                    //if (s.StartsWith("#") || vs.Length<2)
                    //{
                    //    int n = 10;
                    //    String k = RandomString(n);
                    //    while (OriginalFile.Keys.Contains(k))
                    //        k = RandomString(n++);
                    //    OriginalFile.Add(k,line);
                    //    continue;
                    //}

                    //if (vs.Length > 2)
                    //{
                    //    for (int i = 2; i < vs.Length;i++)
                    //        vs[1] += vs[i];
                    //}
					
                    //String LValue = vs[1].Trim();
                    //if (LValue.EndsWith(";") == true)
                    //    LValue = LValue.Remove(LValue.Length - 1, 1);
                    //kvData.Add(vs[0].Trim(), LValue);
                    ////OriginalFile.Add(vs[0].Trim(), vs[1]);
                    //TK: IVAN - 20181113_INI
				}

				file.Close();

				FileName = File;

				return true;
			}
			catch (Exception Ex)
			{
				if (Logger != null)
					Logger("IniFile.LoadFromFile " + Ex.Message);
			}

			return false;
		}

		public bool SaveToFile(String File = null)
		{
			if (File == null)
				File = FileName;
			try
			{
				System.IO.StreamWriter file = new System.IO.StreamWriter(File);

				// Sostituisce le stringhe che non erano valide
				foreach(String key in kvData.Keys)
				{
					if (OriginalFile.Keys.Contains(key) == true)
						OriginalFile[key] =  key + " = " + kvData[key];
					else
						OriginalFile.Add(key,  key + " = " + kvData[key]);  
				}

				foreach (String key in OriginalFile.Keys)
				{
					file.WriteLine(OriginalFile[key]);
				}

				file.Close();

				FileName = File;

				return true;
			}
			catch (Exception Ex)
			{
				if (Logger != null)
					Logger("IniFile.SaveToFile " + Ex.Message);
			}

			return false;
		}

		public bool Get(String key, bool DefaultValue = false)
		{
			if (kvData[key] == null)
				return DefaultValue;

			return Types.ToBool(kvData[key],DefaultValue);
		}

		public Int64 Get(String key, Int64 DefaultValue = 0)
		{
			if (kvData[key] == null)
				return DefaultValue;

			return Types.ToInt64(kvData[key], DefaultValue);
		}

		public String Get(String key, String DefaultValue = "")
		{
			if (kvData[key] == null)
				return DefaultValue;

			return Types.ToString(kvData[key], DefaultValue);
		}

		public String GetMustExist(String key)
		{
			if (kvData[key] == null)
				throw new Exception("CIniFile.GetMustExist - Key not found: " + key);

			return Types.ToString(kvData[key]);
		}

		public String this[String key, String d = ""]
		{
			get
			{
				return Get(key, d);
			}
			set
			{
				kvData[key] = value;
			}
		}
		/*
		public bool this[String key, bool d = false]
		{
			get
			{
				return Get(key,d);
			}
			set
			{
				kvData[key] = value.ToString();
			}
		}

		public Int64 this[String key, Int64 d = 0]
		{
			get
			{
				return Get(key,d);
			}
			set
			{
				kvData[key] = value.ToString();
			}
		}
		 * */
	}
}
