using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpdLib
{
	public class KeyValueArray : Dictionary<string, String>
	{
		public KeyValueArray() : base() { }

		public KeyValueArray(int ne, StringComparer sc ) : base(ne,sc) { }

		public static KeyValueArray FromUrlEncodedString(String query)
		{
			var splittedKeyValues = query.Split(new char[] { '&', '=' }, StringSplitOptions.RemoveEmptyEntries);

			var dicRet = new KeyValueArray(splittedKeyValues.Length / 2, StringComparer.InvariantCultureIgnoreCase);

			for (int i = 0; i < splittedKeyValues.Length; i += 2)
			{
				var key = Uri.UnescapeDataString(splittedKeyValues[i]);
				string value = "";
				if (splittedKeyValues.Length > i + 1)
					value = Uri.UnescapeDataString(splittedKeyValues[i + 1]).Replace('+', ' ');
				dicRet[key] = value;
			}

			return dicRet;
		}

		public new String this[String key]
		{
			get
			{
				if (base.ContainsKey(key) == false)
					return null;
				return base[key];
			}
			set
			{
				base[key] = value;
			}
		}

		public new void Add(String key, String value)
		{
			if (this.Keys.Contains(key) == false)
				base.Add(key, value);
			else
				base[key] = value;
		}
	}

}
