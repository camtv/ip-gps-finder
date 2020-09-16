using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Helpers;
using System.Dynamic;
using Newtonsoft.Json;

namespace Helpers
{
	public class KeyValueHelperBase<T> : Dictionary<string, T>
	{
		public KeyValueHelperBase() : base() { } 

		public KeyValueHelperBase(String s)
		{
			this.FromString(s);
		}

		virtual public bool FromString(String s_data,String Separator="&",String Assignement="=")
		{
			return false;
		}

		public String ToString(String Separator="&",String Assignement="=",String NullValue="null")
		{
			String s = "";
			foreach (KeyValuePair<string,T> pair in this)
			{
				if (s != "")
					s += Separator;
				string val = NullValue;
				if (pair.Value != null)
					val = pair.Value.ToString();
				s += pair.Key + Assignement + val;
			}

			return s;
		}

		public byte[] ToByteArray(String Separator = "&", String Assignement = "=")
		{
			return System.Text.Encoding.UTF8.GetBytes(ToString(Separator, Assignement,""));
		}

		public new T this[String key]
		{
			get
			{
				if (base.ContainsKey(key) == false)
					return default(T);
				return base[key];
			}
			set
			{
				base[key] = value;
			}
		}

		public new void Add(String key, T value)
		{
			if (this.Keys.Contains(key) == false)
				base.Add(key, value);
			else
				base[key] = value;
		}

		public dynamic GetDynamicObject()
		{
			var dynamicObject = new ExpandoObject() as IDictionary<string,object>;
			foreach (KeyValuePair<string, T> pair in this)
			{
				dynamicObject.Add(pair.Key, pair.Value);
			}
			return dynamicObject;
		}

	}

	public class KeyValueHelper : KeyValueHelperBase<object>
	{
		public KeyValueHelper() : base() { } 

		public KeyValueHelper(DataRow row)
		{
			this.FromDataRow(row);
		}

		public KeyValueHelper(DataTable dt, String Table,String sCheckSum)
		{
			this.FromDataTable("",dt, Table, sCheckSum,true);
		}

		public KeyValueHelper(DataTable[] dt, String[] Tables, String sCheckSum)
		{
			int i;
			bool doclear = true;
			for (i=0;i<Tables.Length;i++)
			{
				this.FromDataTable(Tables[i]+":", dt[i], Tables[i], sCheckSum,doclear);
				doclear = false;
			}
			
		}

		public void FromDataRow(DataRow row)
		{
			try
			{
				Clear();
				for (int i = 0;i<row.Table.Columns.Count;i++)
					Add(row.Table.Columns[i].ColumnName, row[i]);

			}
			catch { }
		}

		public void FromDataTable(String TablePrefix,DataTable dt,String sTableName,String sCheckSum,bool doclear)
		{
			try
			{
				String str="";
				if (doclear)
					Clear();

				// Table Name
				Add(TablePrefix+"TABLENAME", sTableName);

				// Rows Number
				Add(TablePrefix + "ROWS", dt.Rows.Count);

				if (dt.Rows.Count > 0)
				{
					// Fields names and types (sqlite format)
					str = "(";
					for (int i = 0; i < dt.Rows[0].Table.Columns.Count; i++)
					{
						str += dt.Rows[0].Table.Columns[i].ColumnName;
						if (i != (dt.Rows[0].Table.Columns.Count - 1))
							str += ",";
					}
					str += ")";
				}
				Add(TablePrefix + "HEADERSCREATE", str);

				if (dt.Rows.Count > 0)
				{
					// Field Names
					str = "(";
					for (int i = 0; i < dt.Rows[0].Table.Columns.Count; i++)
					{
						str += dt.Rows[0].Table.Columns[i].ColumnName;
						if (i != (dt.Rows[0].Table.Columns.Count - 1))
							str += ",";
					}
					str += ")";
				   
				}
				Add(TablePrefix + "HEADERS", str);
				// Records
				for (int j = 0; j < dt.Rows.Count; j++)
				{
					str = "(";
					for (int i = 0; i < dt.Rows[j].Table.Columns.Count; i++)
					{
						str += "'" + Types.ToString(dt.Rows[j][i], "") + "'";
						if (i != (dt.Rows[j].Table.Columns.Count - 1))
							str += ",";
					}
					str += ")";
					Add(TablePrefix + j.ToString(), str);
				}
				Add(TablePrefix + "CHANGES", "TRUE");
				Add(TablePrefix + "CHECKSUM", sCheckSum);
			}
			catch { }
		}

		override public bool FromString(String s_data, String Separator = "&", String Assignement = "=")
		{
			try
			{
				String[] vs = s_data.Split(new String[] { Separator }, StringSplitOptions.RemoveEmptyEntries);

				Clear();
				foreach (string s in vs)
				{
					String[] vs1 = s.Split(new String[] { Assignement }, StringSplitOptions.RemoveEmptyEntries);
					if (vs1.Length == 1)
						Add(vs1[0].Trim(), null);
					else if (vs1.Length == 2)
						Add(vs1[0].Trim(), vs1[1].Trim());
					else
						return false;
				}
				return true;
			}
			catch { }

			return false;
		}

		static public object FromJSON(String sJson)
		{
			try
			{
				if (sJson == null || sJson.Length == 0)
					return sJson;
				KeyStringHelper kv = JsonConvert.DeserializeObject<KeyStringHelper>(sJson);

				if (kv.Keys.Count == 1 && kv[(string)kv.Keys.ToArray()[0]] == null)
					return (String)sJson;

				return KeyValueHelper.FromJSON(sJson);
			}
			catch { }

			return null;
		}
	}

	public class KeyStringHelper : KeyValueHelperBase<String>
	{
		public KeyStringHelper() : base() { } 

		override public bool FromString(String s_data, String Separator = "&", String Assignement = "=")
		{
			try
			{
				String[] vs = s_data.Split(new String[] { Separator }, StringSplitOptions.RemoveEmptyEntries);

				Clear();
				foreach (string s in vs)
				{
					String[] vs1 = s.Split(new String[] { Assignement }, StringSplitOptions.RemoveEmptyEntries);
					if (vs1.Length == 1)
						Add(vs1[0].Trim(), null);
					else if (vs1.Length == 2)
						Add(vs1[0].Trim(), vs1[1].Trim());
					else
						return false;
				}
				return true;
			}
			catch { }

			return false;
		}
	}

	public class DynObjHelper : DynamicObject
	{
		private readonly KeyValueHelper Properties;

		public DynObjHelper(KeyValueHelper _properties)
		{
			Properties = _properties;
		}

		public override IEnumerable<string> GetDynamicMemberNames()
		{
			return Properties.Keys;
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			if (Properties.ContainsKey(binder.Name))
			{
				result = Properties[binder.Name];
				return true;
			}
			else
			{
				result = null;
				return false;
			}
		}

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			Properties[binder.Name] = value;
			return true;
		}
	}
}
