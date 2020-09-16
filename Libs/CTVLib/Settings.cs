using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Helpers
{
	public class SettingsBase
	{
		public class INIProperty : Attribute
		{
			private bool _AllowMissing = false;
			public bool AllowMissing { get { return _AllowMissing; } set { _AllowMissing = value; } }
		}
	}

	public class Settings<TSets> : SettingsBase
	{
		public static String IniFileName = VersionHelper.csSoftwareName + ".ini";
        public static String sLoadedIniFilePath = "";

        public static KeyValueHelper AsKeyValueHelper()
		{
			Type t = typeof(TSets);
			KeyValueHelper kv = new KeyValueHelper();
			foreach (FieldInfo fi in t.GetFields(BindingFlags.Public | BindingFlags.Static))
			{
				kv[fi.Name] = fi.GetValue(null);
			}

			return kv;
		}

		public static String FindIniFile(String _IniFileName = null)
		{
			if (_IniFileName != null)
				IniFileName = _IniFileName;
			String sDir = Directory.GetCurrentDirectory();
			sLoadedIniFilePath = Path.Combine(sDir, IniFileName);

			if (File.Exists(sLoadedIniFilePath) == false)
			{
				sDir = Path.GetDirectoryName(sDir);
				sLoadedIniFilePath = Path.Combine(sDir, IniFileName);
				if (File.Exists(sLoadedIniFilePath) == false)
				{
					sDir = Path.GetDirectoryName(sDir);
					sLoadedIniFilePath = Path.Combine(sDir, IniFileName);
					if (File.Exists(sLoadedIniFilePath) == false)
						return "not found";
				}
			}

			return sLoadedIniFilePath;
		}

		public static bool Load(String _IniFileName = null)
		{
			CIniFile IniFile = new CIniFile();

			if (_IniFileName != null)
				IniFileName = _IniFileName;

            String sDir = Directory.GetCurrentDirectory();
            sLoadedIniFilePath = Path.Combine(sDir, IniFileName);

            if (File.Exists(sLoadedIniFilePath) == false)
            {
                sDir = Path.GetDirectoryName(sDir);
                sLoadedIniFilePath = Path.Combine(sDir, IniFileName);
                if (File.Exists(sLoadedIniFilePath) == false)
                {
                    sDir = Path.GetDirectoryName(sDir);
                    sLoadedIniFilePath = Path.Combine(sDir, IniFileName);
                    if (File.Exists(sLoadedIniFilePath) == false)
                        return false;
                }
            }

			if (IniFile.LoadFromFile(sLoadedIniFilePath) == false)
				return false;

			if (LoadINIProperties(IniFile) == false)
				return false;			

			return true;
		}

		static public bool LoadINIProperties(CIniFile IniFile)
		{
			try
			{
				Type t = typeof(TSets);
				System.Reflection.FieldInfo[] vFields = t.GetFields
					(
						System.Reflection.BindingFlags.DeclaredOnly |
						System.Reflection.BindingFlags.Static |
						System.Reflection.BindingFlags.Public
					);

				foreach (System.Reflection.FieldInfo field in vFields)
				{
					if (field == null)
						continue;

					// Verifica che il metodo sia identificato con l'attributo HTTPMethod				
					object[] PropertyAttributes = field.GetCustomAttributes(true);
					if (PropertyAttributes == null)
						continue;

					foreach (object attr in PropertyAttributes)
					{
						if (attr.GetType() == typeof(INIProperty))
						{
							String sVal = "";
							if (((INIProperty)attr).AllowMissing == true)
							{
								sVal = IniFile.Get(field.Name, "MISSING");
								if (sVal == "MISSING")
									break;
							}
							else
								sVal = IniFile.GetMustExist(field.Name);
							String s = field.Name;
							Debug.WriteLine("field.Name=" + field.Name + " - field.FieldType.Name=" + field.FieldType.Name.ToString());

							switch (field.FieldType.Name.ToString().ToLower())
							{
								case "int":
								case "int32":
									field.SetValue(null, Types.ToInt(sVal));
									break;
								case "int64":
									field.SetValue(null, Types.ToInt64(sVal));
									break;
								case "uint64":
									field.SetValue(null, Types.ToUInt64(sVal));
									break;
								case "float":
									field.SetValue(null, Types.ToFloat(sVal));
									break;
								case "double":
									field.SetValue(null, Types.ToDouble(sVal));
									break;
								case "decimal":
									field.SetValue(null, Types.ToDecimal(sVal));
									break;
								case "bool":
								case "boolean":
									field.SetValue(null, Types.ToBool(sVal));
									break;
								case "datetime":
									field.SetValue(null, Types.ToDateTime(sVal));
									break;
								case "string":
								default:
									field.SetValue(null, Types.ToString(sVal));
									break;
							}

							break;
						}
					}


				}

				return true;
			}
			catch (Exception Ex)
			{
				LogHelper.Error("Exception: " + Ex.Message);
			}

			return false;
		}
	}
}

