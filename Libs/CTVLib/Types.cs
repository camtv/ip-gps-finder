using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Helpers
{
	// Types class contains functions to handle null and DBNull database values
	public class Types
	{
		public static bool IsNull(object o) { return (o == null || o == DBNull.Value); }
		public static object NaN(object o, object oNoNull) { return (o == null || o == DBNull.Value) ? oNoNull : o; }
		public static TNum ToNum<TNum>(object o, TNum oNoNull)
		{
			try { return (o == null || o == DBNull.Value) ? oNoNull : (TNum)Convert.ChangeType(o.ToString(), typeof(TNum)); }
			catch { }
			return oNoNull;
		}

		//TK: IVAN - 20181025_06 - START
		public static T ToType<T>(object o, T oNoNull=default(T))		//TK: IVAN - 20181106_28
		{
			try { return (o == null || o == DBNull.Value) ? oNoNull : (T)Convert.ChangeType(o.ToString(), typeof(T)); } //TK: IVAN - 20181106_28
			catch { }
			return oNoNull;
		}
		//TK: IVAN - 20181025_06 - END

		public static int ToInt(object o, int oNoNull = 0) { return ToNum<int>(o, oNoNull); }
		public static Int64 ToInt64(object o, Int64 oNoNull = 0) { return ToNum<Int64>(o, oNoNull); }
		public static UInt64 ToUInt64(object o, UInt64 oNoNull = 0) { return ToNum<UInt64>(o, oNoNull); }
		public static float ToFloat(object o, float oNoNull = 0) { return ToNum<float>(o, oNoNull); }
		public static double ToDouble(object o, double oNoNull = 0) { return ToNum<double>(o, oNoNull); }
		public static decimal ToDecimal(object o, decimal oNoNull = 0) { return ToNum<decimal>(o, oNoNull); }
		public static bool ToBool(object o, bool oNoNull = false)
		{
			//TK: Cristian - 20181203_01 START
			//TK: IVAN - 20181106_28 - START
			try
			{
				if (o == null || o == DBNull.Value)
					return oNoNull;

				if (o.GetType() == typeof(bool))	
					return (bool)o;

				String s = o.ToString();
				if (ToInt(s, 0) > 0)
					return true;

				if (s.StartsWith("t") || s.StartsWith("T"))
					return true;


				return false;
			}
			catch { }
			return oNoNull;
			//TK: IVAN - 20181106_28 - END
			//TK: Cristian - 20181203_01 END
		}
		public static string ToString(object o, string oNoNull = "") { return (o == null || o == DBNull.Value) ? oNoNull : o.ToString(); }

		public static DateTime ToDateTime(object obtDt)
		{
			try { return (obtDt == null || obtDt == DBNull.Value) ? DateTime.MinValue : (DateTime)Convert.ChangeType(obtDt, typeof(DateTime)); }
			catch { }
			return DateTime.MinValue;
		}

		//TK: IVAN - 20181025_06 - START
		public static T ToEnum<T>(object value, T defaultValue)
		{
			try
			{
				if (Enum.IsDefined(typeof(T), Types.ToString(value)) == true || Enum.IsDefined(typeof(T), Types.ToInt(value)) == true)
					return (T)Enum.Parse(typeof(T), Types.ToString(value));
			}
			catch { }
			return defaultValue;
		}
		//TK: IVAN - 20181025_06 - END		
	}
}
