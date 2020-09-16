using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Helpers
{
	public class Sets : Settings<Sets>
	{
		//:: Static settings section
		[INIProperty()]
		public static bool DEBUG = false;

		[INIProperty(AllowMissing = true)]
		public static String LOG_REPOSITORY;

		[INIProperty()]
		public static String DATABASE_HOST;
		[INIProperty()]
		public static int DATABASE_PORT;
		[INIProperty()]
		public static String DATABASE_USER;
		[INIProperty()]
		public static String DATABASE_PASSWORD;
		[INIProperty()]
		public static String DATABASE_NAME;

		[INIProperty()]
		public static String API_KEY;

		[INIProperty()]
		public static String IPGPS_API_HTTP_LISTEN_URL;
		[INIProperty()]
		public static int IPGPS_API_MAX_CONN_POOL;
		[INIProperty()]
		public static String IPGPS_API_KEY_SECRET;
	}
}