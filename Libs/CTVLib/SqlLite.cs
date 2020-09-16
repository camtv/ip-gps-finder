using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FastCgi.Api.Helpers
{
	class SqlLite
	{
		public static String GetSqlLiteType(String CSharpType)
		{
			if (CSharpType.Contains(".Int"))
				return "INT";
			if (CSharpType.Contains(".String"))
				return "TEXT";
			if (CSharpType.Contains(".DateTime"))
				return "TEXT";
			return "";
		}

	}
}
