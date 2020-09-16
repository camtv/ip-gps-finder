using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Helpers
{
	public class VersionHelper
	{
		public static Int64 cnSoftwareVersion
		{
			get
			{
				Version version = Assembly.GetEntryAssembly().GetName().Version;
				Int64 DaysSince1Jan2000 = TimeSpan.TicksPerDay * version.Build;
				Int64 SecondsSinceMidnight = TimeSpan.TicksPerSecond * 2 * version.Revision;
				DateTime buildDateTime = new DateTime(2000, 1, 1).Add(new TimeSpan(DaysSince1Jan2000 + SecondsSinceMidnight));
				return buildDateTime.Year * 100 * 100 + buildDateTime.Month * 100 + buildDateTime.Day;
			}
		}

		public static Int64 cnSoftwareBuild
		{
			get
			{
				return Assembly.GetEntryAssembly().GetName().Version.Build;
			}
		}

		public static String csSoftwareName 
		{ 
			get 
			{ 
				var currentProc = System.Diagnostics.Process.GetCurrentProcess();
				return currentProc.ProcessName;
			} 
		}

		public static String csSoftwareFullNameAndVersion
		{
			get { return String.Format("{0} - {1}.{2}", csSoftwareName, cnSoftwareVersion, cnSoftwareBuild); }
		}
	}
}
