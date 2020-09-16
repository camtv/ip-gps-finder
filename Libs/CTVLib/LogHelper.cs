// TK: IVAN - 20181227_06 - FILE
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Runtime.CompilerServices;

namespace Helpers
{
	public class LogHelper
	{
		public static String sLogFile = null;

		public static String RemoteLogger_Address = null;
		public static int RemoteLogger_Port = 0;

		private enum TLogLevel { ERROR, WARNING, INFO };

		static System.Object Locker = new System.Object();

		public static void Init(String LogFilePath)
		{
			sLogFile = LogFilePath;
			if (sLogFile.ToLower().EndsWith(".log") == false)
				sLogFile += ".log";

			LogHelper.Info("Log File: {0}", LogHelper.sLogFile);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static void Write(TLogLevel LogLevel, String message, params object[] values)
		{
			lock (Locker)
			{
				DateTime datet = DateTime.UtcNow;
				bool bNewLineLast = false;
				bool bNewLineFirst = false;
				if (message.First() == '\b')
					bNewLineFirst = true;
				if (message.Last() == '\b')
					bNewLineLast = true;

				message = message.Replace("\b", "");

				try
				{
					String sLevel = LogLevel.ToString();
					if (values != null && values.Length > 0)
						message = String.Format(message, values);

					String SourceReference = "";
					if (LogLevel != TLogLevel.INFO)
					{
						try
						{
							var stack = new StackFrame(2, true);
							var Method = stack.GetMethod();
							var Class = Method.DeclaringType;
							var FileName = stack.GetFileName();
							var FileLine = stack.GetFileLineNumber();
							if (FileName != null)
								SourceReference = String.Format(" {0}.{1} [{2}@Line:{3}] - ", Class.Name, Method.Name, FileName, FileLine);
							else
								SourceReference = String.Format(" {0}.{1} - ", Class.Name, Method.Name);
						}
						catch { }
					}

					String sConsoleLog = String.Format(" <{0}> {1}{2}", sLevel, SourceReference, message);
					if (bNewLineFirst == true)
						sConsoleLog = message;

					String sLog = String.Format("{0} <{1}> {2}{3}", datet.ToString("yyyy/MM/dd-HH:mm:ss"), sLevel, SourceReference, message);

					String sFilePath = sLogFile;

					if (sLogFile != null)
					{
						if (!File.Exists(sFilePath))
						{
							FileStream files = File.Create(sFilePath);
							files.Close();
						}

						StreamWriter sw = File.AppendText(sFilePath);
						sw.WriteLine(sLog);
						sw.Flush();
						sw.Close();
					}

					ConsoleColor cc = Console.ForegroundColor;
					if (LogLevel == TLogLevel.ERROR)
						Console.ForegroundColor = ConsoleColor.Red;
					else if (LogLevel == TLogLevel.WARNING)
						Console.ForegroundColor = ConsoleColor.Yellow;

					if (bNewLineLast == true)
						System.Console.Write(sConsoleLog);
					else
						System.Console.WriteLine(sConsoleLog);

					Console.ForegroundColor = cc;

					SendToRemoteLogger(sLog);

				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message.ToString());
				}
			}
		}

		public static void Info(String message, params object[] values) { Write(TLogLevel.INFO, message, values); }
		public static void Warning(String message, params object[] values) { Write(TLogLevel.WARNING, message, values); }
		public static void Error(String message, params object[] values) { Write(TLogLevel.ERROR, message, values); }
		public static void Error(Exception Ex) { Write(TLogLevel.ERROR, "Exception: "+Ex.Message); }

		private static void SendToRemoteLogger(String sLog)
		{
			if (RemoteLogger_Address == null)
				return;
			UdpClient udpClient = new UdpClient(RemoteLogger_Address, RemoteLogger_Port);
			udpClient.Client.SendTimeout = 500;
			udpClient.Client.ReceiveTimeout = 500;
			Byte[] sendBytes = Encoding.UTF8.GetBytes(sLog);
			try
			{
				udpClient.Send(sendBytes, sendBytes.Length);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}
		}

		public static void Throw(String message, params object[] values)
		{
			throw new Exception(String.Format(message, values));
		}

	}
}
