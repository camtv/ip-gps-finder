using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Collections;
using System.Data;
using System.Reflection;
using System.Data.Common;
using System.Threading.Tasks;

namespace Helpers
{
	// DatabaseHelper class contains SQL database commands and handling
	public class DatabaseHelper : IDisposable       //TK: IVAN - 20181120_01
	{
		public MySqlConnection sqlConnection = null;
		public string sConnectionString = "";

		public string sDbServerName = "";
		public int sDbServerPort = 3306;
		public string sDbServerLogin = "";
		public string sDbServerPassword = "";
		public string sDbArchiveName = "";

		public int MaxSecondsToForceReconnection = 30;
		DateTime ServerConnectionStartTimestamp = DateTime.MinValue;

		volatile bool TransactionAlreadyOpen = false;

		public class Debug
		{
			static object oLock = new object();
			static String sPath = null;

			public static bool Active { get { return sLogRepository != null; } }
			public static String sLogRepository = null;

			public static void Activate(String LogRepository) { sLogRepository = LogRepository; }

			private static void Log(String Txt, params object[] vParams)
			{
				if (Active == false)
					return;

				lock (oLock)
				{
					try
					{
						if (sPath == null)
						{
							String FileName = "Db.Debug-" + VersionHelper.csSoftwareName + ".log";
							sPath = Path.Combine(sLogRepository, FileName);
							if (File.Exists(sPath) == true)
							{
								File.Move(sPath, sPath.Replace(FileName, (DateTime.Now.ToString("yyyyMMdd@HHmmss") + "-" + FileName)));
								File.Delete(sPath);
							}
						}

						using (StreamWriter w = File.AppendText(sPath))
						{
							w.WriteLine(String.Format(Txt, vParams));
						}
					}
					catch { };
				}
			}

			public static void LogError(String Txt, params object[] vParams)
			{
				string s = String.Format("{0} - {1} - ", DateTime.Now, "ERROR");
				Log(s + Txt, vParams);
			}

			public static void LogInfo(String Txt, params object[] vParams)
			{
				string s = String.Format("{0} - {1} - ", DateTime.Now, "INFO");
				Log(s + Txt, vParams);
			}

			public class Query
			{
				public class Desc
				{
					public Int32 nDbHinstance;
					public Int32 QID;
					public Int64 Ts;
					public String Qry;

					public void Close()
					{
						if (Active == false)
							return;

						Debug.Query.Log("DbHinstance: {0} - QueryID: {1} - ELAPSED: {2} - QRY: {3}", nDbHinstance, QID, Environment.TickCount - Ts, Qry);
					}
				}

				public static Desc New(String Qry, Int32 nDbHinstance)
				{
					Desc d = new Desc();
					d.nDbHinstance = nDbHinstance;
					d.QID = Query.QueryID++;
					d.Ts = Environment.TickCount;

					d.Qry = Qry;

					if (Active == false)
						return d;

					Debug.Query.Log("DbHinstance: {0} - QueryID: {1} - START - QRY: {2}", d.nDbHinstance, d.QID, d.Qry);

					return d;
				}

				static String sQueryPath = null;
				static object oQueryLock = new object();

				static volatile Int32 QueryID = 0;

				public static void Error(Exception Ex, Int32 nDbHinstance)
				{
					Debug.Query.Log("ERROR - DbHinstance: {0} - Exception: {1}", nDbHinstance, Ex.Message);
				}

				public static void Log(String Txt, params object[] vParams)
				{
					if (Active == false)
						return;

					lock (oQueryLock)
					{
						try
						{
							if (sQueryPath == null)
							{
								String FileName = "Db.Query-" + VersionHelper.csSoftwareName + ".log";
								sQueryPath = Path.Combine(sLogRepository, FileName);
								if (File.Exists(sQueryPath) == true)
								{
									File.Move(sQueryPath, sQueryPath.Replace(FileName, (DateTime.Now.ToString("yyyyMMdd@HHmmss") + "-" + FileName)));
									File.Delete(sQueryPath);
								}
							}

							using (StreamWriter w = File.AppendText(sQueryPath))
							{
								string st = String.Format("{0} - ", DateTime.Now);
								w.WriteLine(String.Format(st + Txt, vParams));
							}
						}
						catch { };
					}
				}
			}
		}

		static volatile Int32 NConnections = 0;
		static Task tsk = null;

		static volatile Int32 nHinstanceCounter = 0;

		Int32 nHinstance = 0;

		public DatabaseHelper()
		{
			nHinstance = nHinstanceCounter++;

			Debug.LogInfo("DatabaseHelper -> CONSTRUCTOR nHinstance: " + nHinstance);

			if (tsk == null && Debug.Active == true)
			{
				tsk = Task.Run(() =>
				{
					String FileName = "Db.ConnectionsCount-" + VersionHelper.csSoftwareName + ".log";
					String sPath = Path.Combine(Debug.sLogRepository, FileName);
					if (File.Exists(sPath) == true)
					{
						File.Move(sPath, sPath.Replace(FileName, (DateTime.Now.ToString("yyyyMMdd@HHmmss") + "-" + FileName)));
						File.Delete(sPath);
					}
					while (true)
					{
						using (StreamWriter w = File.AppendText(sPath))
						{
							w.WriteLine(String.Format("{0} - NConnections: {1}", DateTime.Now, NConnections));
						}
						Task.Delay(5000).Wait();
					}
				});
			}
		}

		~DatabaseHelper()
		{
			CloseConnection();

			Debug.LogInfo("DatabaseHelper -> DESTRUCTOR nHinstance: " + nHinstance);
		}

		//TK: IVAN - 20181120_01 - START
		public void Dispose()
		{
			CloseConnection();
		}
		//TK: IVAN - 20181120_01 - END

		public String EscapeString(String s)
		{
			return MySqlHelper.EscapeString(s);
		}

		public void SetDbConnectionData(string sDbServerName, int sDbServerPort, string sDbServerLogin, string sDbServerPassword, string sDbArchiveName, int nConnectionPoolSize = 5) // TK: Cristian - CTBT-1534
		{
			sConnectionString = String.Format("server={0};port={1};user={2};password={3};database={4};charset=utf8;convert zero datetime=True;pooling=true;MaximumPoolsize={5}; respect binary flags=false;",   //TK: IVAN - 20181120_01
				sDbServerName, sDbServerPort, sDbServerLogin, sDbServerPassword, sDbArchiveName, nConnectionPoolSize);

			this.sDbServerName = sDbServerName;
			this.sDbServerPort = sDbServerPort;
			this.sDbServerLogin = sDbServerLogin;
			this.sDbServerPassword = sDbServerPassword;
			this.sDbArchiveName = sDbArchiveName;
		}

		public bool IsSetDbConnectionData()
		{
			return (this.sDbServerName != "");
		}

		public void CloneConnectionData(DatabaseHelper DbHToClone)
		{
			sDbServerName = DbHToClone.sDbServerName;
			sDbServerPort = DbHToClone.sDbServerPort;
			sDbServerLogin = DbHToClone.sDbServerLogin;
			sDbServerPassword = DbHToClone.sDbServerPassword;
			sDbArchiveName = DbHToClone.sDbArchiveName;

			sConnectionString = DbHToClone.sConnectionString;
		}

		public void CheckDbConnection()
		{
			using (MySqlCommand comm = new MySqlCommand() { Connection = sqlConnection })
			{
				try
				{
					comm.CommandText = "SELECT 1";

					var qdbg = Debug.Query.New(comm.CommandText, nHinstance);

					comm.ExecuteScalar();

					qdbg.Close();
				}
				catch (Exception ex)
				{
					Debug.Query.Error(ex, nHinstance);
					if (sqlConnection != null)
					{
						try
						{
							sqlConnection.Close();
						}
						catch (Exception Ex) { Debug.LogError("{0} - {1}", "sqlConnection.Close() FAILED in CheckDbConnection", Ex.Message); }
						NConnections--;
					}

					sqlConnection = null;
					OpenConnection();
				}
			}
		}

		public bool EnsureConnectionIsOpenAndReady()
		{
			// Se non � connesso o se l'ultima verifica della connessione � stata pi� di 2 ore fa chiude e riapre la connessione
			if (IsConnected() == false || (DateTime.Now - ServerConnectionStartTimestamp).TotalSeconds >= MaxSecondsToForceReconnection)
			{
				if (OpenConnection(true) == false)
					return false;

				ServerConnectionStartTimestamp = DateTime.Now;
			}

			return true;
		}

		public bool IsConnected()
		{
			if (sqlConnection != null && sqlConnection.State == ConnectionState.Open)
				return true;

			return false;
		}

		public bool OpenConnection(bool ForceReconnection = false)
		{
			try
			{
				// if already connected to database then returns true
				if (ForceReconnection == true)
				{
					if (sqlConnection != null)
					{
						try
						{
							sqlConnection.Close();
						}
						catch (Exception Ex) { Debug.LogError("{0} - {1}", "sqlConnection.Close() FAILED in OpenConnection", Ex.Message); }
						NConnections--;
					}
				}
				else
					if (IsConnected())
					return true;

				TransactionAlreadyOpen = false;

				// connects to database
				Debug.LogInfo("OpenConnection -> new MySqlConnection sConnectionString: " + sConnectionString);
				sqlConnection = new MySqlConnection(sConnectionString);
				if (sqlConnection != null)
				{
					if (sqlConnection.State == ConnectionState.Broken)
					{
						Debug.LogInfo("OpenConnection -> sqlConnection.State == ConnectionState.Broken");
						try
						{
							sqlConnection.Close();
						}
						catch (Exception Ex) { Debug.LogError("{0} - {1}", "sqlConnection.Close() FAILED in OpenConnection2", Ex.Message); }
						NConnections--;
					}

					bool WasClosed = false;
					if (sqlConnection.State == ConnectionState.Closed)
					{
						Debug.LogInfo("OpenConnection -> sqlConnection.State == ConnectionState.Closed");

						sqlConnection.Open();
						WasClosed = true;
						Debug.LogInfo("OpenConnection -> sqlConnection.Open() and WasClosed = true done");
					}

					while (sqlConnection.State == ConnectionState.Connecting)
						Thread.Sleep(500);

					if (sqlConnection.State == ConnectionState.Open)
					{
						// TK: IVAN - CTBT-1534 - Rimosso
						//CheckDbConnection();
						if (WasClosed == true)
							NConnections++;
						Debug.LogInfo("OpenConnection -> SUCCESS");
						return true;
					}
					else
						Debug.LogError("OpenConnection -> FAILED because sqlConnection.State != ConnectionState.Open after timeout");
				}
				else
					Debug.LogError("OpenConnection -> FAILED because new MySqlConnection failed");
			}
			catch (Exception ex)
			{
				LogHelper.Error("Exception: " + ex.Message);
				Debug.LogError("OpenConnection -> FAILED because of exception: " + ex.Message);
			}

			// Se arriva qua deve tornare false, in ogni caso se sqlconnection non � null deve chiuderla
			if (sqlConnection != null)
			{
				try
				{
					sqlConnection.Close();
				}
				catch (Exception Ex) { Debug.LogError("{0} - {1}", "sqlConnection.Close() FAILED in exting with false", Ex.Message); }
				sqlConnection = null;
			}

			return false;
		}

		public void CloseConnection()
		{
			if (sqlConnection != null)
			{
				try
				{
					sqlConnection.Close();
				}
				catch (Exception Ex) { Debug.LogError("{0} - {1}", "sqlConnection.Close() FAILED in CloseConnection", Ex.Message); }
				NConnections--;
				sqlConnection.Dispose();
			}

			sqlConnection = null;

			TransactionAlreadyOpen = false;

			Debug.LogInfo("DatabaseHelper.CloseConnection nHinstance: " + nHinstance);
		}

		public bool BeginTransaction()
		{
			try
			{
				if (TransactionAlreadyOpen == true)
					throw new Exception("Transaction alrteady opened");

				using (MySqlCommand comm = new MySqlCommand() { Connection = sqlConnection })
				{
					comm.CommandText = "START TRANSACTION";

					var qdbg = Debug.Query.New("START TRANSACTION", nHinstance);

					int nRet = comm.ExecuteNonQuery();

					qdbg.Close();
				}
				TransactionAlreadyOpen = true;
				return true;
			}
			catch (Exception Ex)
			{
				Debug.Query.Error(Ex, nHinstance);
				LogHelper.Error("Exception type: " + Ex.GetType().ToString() + " - Exception message: " + Ex.Message);

				// Forza la riapertura della connessione.
				OpenConnection(true);
			}

			return false;
		}

		public bool Commit()
		{
			try
			{
				using (MySqlCommand comm = new MySqlCommand() { Connection = sqlConnection })
				{
					comm.CommandText = "COMMIT";

					var qdbg = Debug.Query.New("COMMIT", nHinstance);

					comm.ExecuteNonQuery();

					qdbg.Close();
				}
				TransactionAlreadyOpen = false;
				return true;
			}
			catch (Exception Ex)
			{
				LogHelper.Error("Exception type: " + Ex.GetType().ToString() + " - Exception message: " + Ex.Message);
			}

			return false;
		}

		public bool RollBack()
		{
			try
			{
				using (MySqlCommand comm = new MySqlCommand() { Connection = sqlConnection })
				{
					comm.CommandText = "ROLLBACK";

					var qdbg = Debug.Query.New("ROLLBACK", nHinstance);

					comm.ExecuteNonQuery();

					qdbg.Close();
				}
				TransactionAlreadyOpen = false;
				return true;
			}
			catch (Exception Ex)
			{
				LogHelper.Error("Exception type: " + Ex.GetType().ToString() + " - Exception message: " + Ex.Message);
			}

			return false;
		}

		public object DoOneFieldQuery(string sQuery, params object[] QryParams)
		{
			try
			{
				// executes a query and returns the first row
				using (MySqlCommand comm = new MySqlCommand() { Connection = sqlConnection })
				{
					comm.CommandText = sQuery;

					for (int i = 0; i < QryParams.Length; i++)
						comm.Parameters.Add(new MySqlParameter(i.ToString(), QryParams[i]));

					var qdbg = Debug.Query.New(sQuery, nHinstance);

					var o = comm.ExecuteScalar();

					qdbg.Close();

					return o;
				}
			}
			catch (Exception Ex)
			{
				Debug.Query.Error(Ex, nHinstance);
				throw Ex;
			}
		}


		public T DoOneFieldQuery<T>(string sQuery, params object[] QryParams)
		{
			// executes a query and returns the first row
			using (MySqlCommand comm = new MySqlCommand() { Connection = sqlConnection })
			{
				comm.CommandText = sQuery;

				for (int i = 0; i < QryParams.Length; i++)
					comm.Parameters.Add(new MySqlParameter(i.ToString(), QryParams[i]));

				var qdbg = Debug.Query.New(sQuery, nHinstance);

				var o =  Types.ToType<T>(comm.ExecuteScalar());

				qdbg.Close();

				return o;
			}
		}

		public DataTable DoSelectQuery(string sQuery, params object[] QryParams)
		{
			DataSet oDataSet = new DataSet();

			try
			{
				// executes a query and returns the first row
				MySqlCommand comm = new MySqlCommand() { Connection = sqlConnection };

				comm.CommandText = sQuery;

				for (int i = 0; i < QryParams.Length; i++)
					comm.Parameters.Add(new MySqlParameter(i.ToString(), QryParams[i]));

				var qdbg = Debug.Query.New(sQuery, nHinstance);

				MySqlDataAdapter adapter = new MySqlDataAdapter(comm);

				// creates a dataset object and fill the values from Employees table	
				adapter.Fill(oDataSet);

				qdbg.Close();

				if (oDataSet.Tables.Count == 0)
					return null;
				return oDataSet.Tables[0];
			}
			catch (Exception ex)
			{
				Debug.Query.Error(ex, nHinstance);
				LogHelper.Error("Exception: " + ex.Message);
			}

			return null;
		}

		public KeyValueHelper[] DoSelectQueryKVArray(string sQuery, params object[] QryParams)
		{
			try
			{
				List<KeyValueHelper> lskv = new List<KeyValueHelper>();

				DataTable dt = DoSelectQuery(sQuery, QryParams);
				if (dt == null)
					return null;

				foreach (DataRow r in dt.Rows)
					lskv.Add(new KeyValueHelper(r));

				return lskv.ToArray();
			}
			catch (Exception Ex)
			{
				Debug.Query.Error(Ex, nHinstance);
				throw Ex;
			}
		}

		public KeyValueHelper DoSelectQueryKV(string sQuery, params object[] QryParams)
		{
			DataTable dt = DoSelectQuery(sQuery, QryParams);
			if (dt == null || dt.Rows.Count == 0)
				return null;

			return new KeyValueHelper(dt.Rows[0]);
		}

		//TK: IVAN - 20181025_06 - START
		public T[] DoSelectOneFieldQueryArray<T>(T NullValue, string sQuery, params object[] QryParams)
		{
			List<T> lskv = new List<T>();

			DataTable dt = DoSelectQuery(sQuery, QryParams);
			if (dt == null)
				return null;

			foreach (DataRow r in dt.Rows)
				lskv.Add(Types.ToType<T>(r[0], NullValue));

			return lskv.ToArray();
		}
		//TK: IVAN - 20181025_06 - END

		public TableArray DoSelectQueryTableArray(string sQuery, params object[] QryParams)
		{
			return new TableArray(DoSelectQuery(sQuery, QryParams));
		}

		public int DoExecQuery(string sQuery, params object[] QryParams)
		{
			try
			{
				using (MySqlCommand comm = new MySqlCommand() { Connection = sqlConnection })
				{
					comm.CommandText = sQuery;

					for (int i = 0; i < QryParams.Length; i++)
						comm.Parameters.Add(new MySqlParameter(i.ToString(), QryParams[i]));

					var qdbg = Debug.Query.New(sQuery, nHinstance);

					var o = comm.ExecuteNonQuery();

					qdbg.Close();

					return o;
				}
			}
			catch (Exception Ex)
			{
				Debug.Query.Error(Ex, nHinstance);
				throw Ex;
			}
		}

		public Int64 DoExecInsertQuery(string sInsertQuery, params object[] QryParams)
		{
			try
			{
				using (MySqlCommand comm = new MySqlCommand() { Connection = sqlConnection })
				{
					comm.CommandText = sInsertQuery;

					for (int i = 0; i < QryParams.Length; i++)
						comm.Parameters.Add(new MySqlParameter(i.ToString(), QryParams[i]));

					var qdbg = Debug.Query.New(sInsertQuery, nHinstance);

					comm.ExecuteNonQuery();

					qdbg.Close();

					return comm.LastInsertedId;
				}
			}
			catch (Exception Ex)
			{
				Debug.Query.Error(Ex, nHinstance);
				throw Ex;
			}
		}

		public Int64 DoExecInsertQuery(String Table, KeyValueHelper kv)
		{
			String PlaceHolders = "";
			for (int i = 0; i < kv.Keys.Count; i++)
			{
				if (PlaceHolders != "")
					PlaceHolders += ",";
				PlaceHolders += String.Format("@{0}", i);
			}
			String sInsertQuery = String.Format("INSERT INTO {0} ({1}) VALUES ({2})", Table, String.Join(",", kv.Keys), PlaceHolders);

			return DoExecInsertQuery(sInsertQuery, kv.Values.ToArray());
		}

		public Int64 DoExecUpdateQuery(string sTable, DataRow r, String sWhereField = "ID", String[] vFieldsToExcude = null)
		{
			ArrayList sFieldNames = new ArrayList();
			ArrayList sFieldValues = new ArrayList();
			string sValues = "";
			int nValues = 0;

			for (int i = 0; i < r.Table.Columns.Count; i++)
			{
				if (vFieldsToExcude != null && Array.IndexOf(vFieldsToExcude, r.Table.Columns[i].ColumnName) != -1)
					continue;

				if (r.Table.Columns[i].ColumnName != sWhereField)
				{
					sFieldNames.Add(r.Table.Columns[i].ColumnName);
					sFieldValues.Add(r[i]);

					if (sValues != "")
						sValues += ",";

					sValues += String.Format("{0}=@{1}", r.Table.Columns[i].ColumnName, nValues++);
				}
			}

			String sWhereCondition = String.Format("{0}=@{1}", sWhereField, nValues);

			string sInsertQuery = String.Format("UPDATE {0} SET {1} WHERE {2};",
										sTable, sValues, sWhereCondition);

			try
			{
				using (MySqlCommand comm = new MySqlCommand() { Connection = sqlConnection })
				{
					comm.CommandText = sInsertQuery;

					for (int i = 0; i < sFieldValues.Count; i++)
						comm.Parameters.Add(new MySqlParameter(i.ToString(), sFieldValues[i]));

					comm.Parameters.Add(new MySqlParameter(nValues.ToString(), r[sWhereField]));

					var qdbg = Debug.Query.New(sInsertQuery, nHinstance);

					var o = comm.ExecuteNonQuery();

					qdbg.Close();

					return o;
				}
			}
			catch (Exception Ex)
			{
				Debug.Query.Error(Ex, nHinstance);
				throw Ex;
			}
		}

		public Int64 DoExecInsertOrUpdateQuery(String Table, KeyValueHelper kv, String sWhereField = "ID")
		{
			String InsPlaceHolders = "";
			for (int i = 0; i < kv.Keys.Count; i++)
			{
				if (InsPlaceHolders != "")
					InsPlaceHolders += ",";
				InsPlaceHolders += String.Format("@{0}", i);
			}
			String sInsertQuery = String.Format("INSERT INTO {0} ({1}) VALUES ({2})", Table, String.Join(",", kv.Keys), InsPlaceHolders);

			KeyValueHelper kvVals = new KeyValueHelper();
			String PlaceHolders = "";
			int j = 0;
			foreach (string k in kv.Keys)
			{
				if (k == sWhereField)
					continue;
				if (PlaceHolders != "")
					PlaceHolders += ",";
				PlaceHolders += String.Format("{0}=@{1}", k, j++);
				kvVals[k] = kv[k];
			}
			String sUpdateQuery = String.Format("UPDATE {0}", PlaceHolders);
			kvVals[sWhereField] = kv[sWhereField];

			return DoExecQuery(sInsertQuery + " ON DUPLICATE KEY " + sUpdateQuery, kvVals.Values.ToArray());
		}

		public Int64 DoExecUpdateQuery(String Table, KeyValueHelper kv, String sWhereField = "ID")
		{
			KeyValueHelper kvVals = new KeyValueHelper();
			String PlaceHolders = "";
			int i = 0;
			foreach (string k in kv.Keys)
			{
				if (k == sWhereField)
					continue;
				if (PlaceHolders != "")
					PlaceHolders += ",";
				PlaceHolders += String.Format("{0}=@{1}", k, i++);
				kvVals[k] = kv[k];
			}
			String sUpdateQuery = String.Format("UPDATE {0} SET {1} WHERE {2}=@{3}", Table, PlaceHolders, sWhereField, i);
			kvVals[sWhereField] = kv[sWhereField];

			return DoExecQuery(sUpdateQuery, kvVals.Values.ToArray());
		}

		public void DoExecQueryScriptFromFile(string sFileName, BackgroundWorker bw = null)
		{
			StreamReader f = new StreamReader(sFileName);
			string qryFile = f.ReadToEnd();
			f.Close();

			DoExecQueryScript(qryFile, bw);
		}

		public void DoExecQueryScript(string sScript, BackgroundWorker bw = null)
		{
			Regex _sqlScriptSplitRegEx = new Regex(@"^\s*GO\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

			var scripts = _sqlScriptSplitRegEx.Split(sScript);

			if (scripts.Count<String>() < 2)
			{
				scripts = sScript.Split(new string[] { ";\r\n" }, StringSplitOptions.None);
			}

			if (bw != null)
				bw.ReportProgress(0);

			float i = 0;

			foreach (var scriptLet in scripts)
			{
				if (scriptLet.Trim().Length == 0)
					continue;
				try
				{
					DoExecQuery(scriptLet, false);
				}
				catch (Exception ex)
				{
					LogHelper.Error("Exception: " + ex.Message + " - Error in query: {0}", scriptLet);
					throw ex;
				}

				if (bw != null)
				{
					bw.ReportProgress(Convert.ToInt32(100.0 * (i / scripts.Count<String>())));
					if (bw.CancellationPending)
						return;
				}

				i += 1;
			}
		}

		public bool TableExist(String tablename)
		{
			bool result = false;

			try
			{

				using (MySqlCommand comm = new MySqlCommand() { Connection = sqlConnection })
				{
					comm.CommandText = "SHOW TABLES;";

					var qdbg = Debug.Query.New("SHOW TABLES;", nHinstance);

					MySqlDataReader Reader = comm.ExecuteReader();

					qdbg.Close();

					while ((Reader.Read()) && (result == false))
					{
						String currtable;
						for (int i = 0; i < Reader.FieldCount; i++)
						{
							currtable = Reader.GetValue(i).ToString();
							if ((currtable == tablename))
							{
								result = true;
								break;
							}
						}

					}
					Reader.Close();
				}
				return result;
			}
			catch (Exception ex)
			{
				Debug.Query.Error(ex, nHinstance);
				LogHelper.Error("Exception: " + ex.Message);
			}

			return false;
		}

		public bool DatabaseExist(String dbname)
		{
			bool result = false;

			try
			{

				using (MySqlCommand comm = new MySqlCommand() { Connection = sqlConnection })
				{
					comm.CommandText = "SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '" + dbname + "'";

					var qdbg = Debug.Query.New(comm.CommandText, nHinstance);

					MySqlDataReader Reader = comm.ExecuteReader();

					qdbg.Close();

					while ((Reader.Read()) && (result == false))
					{
						if (Reader.FieldCount > 0)
						{
							Reader.Close();
							return true;
						}

					}

					Reader.Close();
				}
				return result;
			}
			catch (Exception ex)
			{
				Debug.Query.Error(ex, nHinstance);
				LogHelper.Error("Exception: " + ex.Message);
			}

			return false;
		}

		public bool CreateNewDatabase(String databasename, String sDatabaseStructSQLFile = null, String sDatabaseIntiDataSQLFile = null)
		{
			// recreate database
			try
			{
				DoExecQuery("CREATE DATABASE " + databasename + " DEFAULT CHARACTER SET utf8 DEFAULT COLLATE utf8_general_ci;");
				DoExecQuery("USE " + databasename);

				if (sDatabaseStructSQLFile != null)
				{
					DoExecQueryScriptFromFile(sDatabaseStructSQLFile);

					if (sDatabaseIntiDataSQLFile != null)
					{
						DoExecQueryScriptFromFile(sDatabaseIntiDataSQLFile);
					}
				}

			}
			catch (Exception ex)
			{
				LogHelper.Error("Exception: " + ex.Message);
				return false;
			}

			return true;
		}

		public List<String> EnumDatabases()
		{
			int i;
			String sTemp;
			List<String> DatabaseList = new List<String>();

			try
			{
				using (MySqlCommand comm = new MySqlCommand() { Connection = sqlConnection })
				{
					comm.CommandText = "SHOW DATABASES";

					var qdbg = Debug.Query.New(comm.CommandText, nHinstance);

					MySqlDataReader Reader = comm.ExecuteReader();

					qdbg.Close();

					i = 0;
					while (Reader.Read())
					{
						for (i = 0; i < Reader.FieldCount; i++)
						{
							sTemp = Reader.GetValue(i).ToString();
							DatabaseList.Add(sTemp);
						}
					}

					Reader.Close();
				}

				return DatabaseList;
			}
			catch (Exception ex)
			{
				Debug.Query.Error(ex, nHinstance);
				LogHelper.Error("Exception: " + ex.Message);
				return null;
			}

		}

		public void DoTransaction(Action func)
		{
			if (TransactionAlreadyOpen == false)
			{
				if (BeginTransaction() == false)
					throw new Exception("BeginTransaction() failed");
				try
				{
					func();
					Commit();
					return;
				}
				catch
				{
					RollBack();
					throw;
				}
			}

			func();
		}

		public T DoTransaction<T>(Func<T> func)
		{
			// DA RIVEDERE -> Possibile che se non chiude una transaction poi non la apre pi� ??
			if (TransactionAlreadyOpen == false)
			{
				if (BeginTransaction() == false)
					throw new Exception("BeginTransaction() failed");
				try
				{
					T result = func();
					Commit();
					return result;
				}
				catch
				{
					RollBack();
					throw;
				}
			}

			return func();
		}

		public class DbFieldsMap<T>
		{
			public class FieldDescriptor
			{
				public int Index;
				public Type type;
				public FieldInfo Prop;

				public delegate void TSetRecordValue(ref T record, object value);

				public TSetRecordValue SetRecordValue = null;

				public void SetRecordValueDefault(ref T record, object value)
				{
					try
					{
						Prop.SetValue(record, Convert.ChangeType(value, type));
					}
					catch { }
				}

				public void SetRecordValueBool(ref T record, object value)
				{
					try
					{
						Prop.SetValue(record, Types.ToBool(Types.ToString(value), false));
					}
					catch { }
				}

				public void SetRecordValueEnum(ref T record, object value)
				{
					try
					{
						if (Enum.IsDefined(type, Types.ToString(value)) == true || Enum.IsDefined(type, Types.ToInt(value)) == true)
							Prop.SetValue(record, Enum.Parse(type, Types.ToString(value)));
					}
					catch { }
				}

			}

			protected Dictionary<string, FieldDescriptor> dColumns = null;

			public FieldDescriptor this[String key]
			{
				get
				{
					if (dColumns.ContainsKey(key) == false)
						return null;
					return dColumns[key];
				}
			}

			public DbFieldsMap()
			{
				dColumns = new Dictionary<string, FieldDescriptor>();
				FieldInfo[] vp = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);
				foreach (FieldInfo p in vp)
				{
					var fd = new FieldDescriptor() { Index = -1, type = p.FieldType, Prop = p };
					if (fd.type == typeof(bool))
						fd.SetRecordValue = fd.SetRecordValueBool;
					else if (fd.type.IsEnum == true)
						fd.SetRecordValue = fd.SetRecordValueEnum;
					else
						fd.SetRecordValue = fd.SetRecordValueDefault;

					dColumns[p.Name] = fd;
				}
			}
		}

		public T[] DoSelectQueryArray<T>(string sQuery, params object[] QryParams) where T : new()
		{
			List<T> lskv = new List<T>();

			try
			{
				// executes a query and returns the first row
				MySqlCommand comm = new MySqlCommand() { Connection = sqlConnection };

				comm.CommandText = sQuery;

				for (int i = 0; i < QryParams.Length; i++)
					comm.Parameters.Add(new MySqlParameter(i.ToString(), QryParams[i]));

				var qdbg = Debug.Query.New(comm.CommandText, nHinstance);

				DbFieldsMap<T> fm = new DbFieldsMap<T>();
				using (DbDataReader reader = comm.ExecuteReader())
				{
					if (reader.HasRows == false)
						return lskv.ToArray();

					while (reader.Read())
					{
						T record = new T();

						for (int i = 0; i <= (reader.FieldCount - 1); i++)
						{
							var fd = fm[reader.GetName(i)];
							if (fd == null)
								continue;

							fd.Index = i;
							if (reader[i] != DBNull.Value)
							{
								fd.SetRecordValue(ref record, reader[i]);
							}
						}

						lskv.Add(record);
					}
				}

				qdbg.Close();

				return lskv.ToArray();
			}
			catch (Exception ex)
			{
				Debug.Query.Error(ex, nHinstance);
				LogHelper.Error(ex);
			}

			return null;
		}

		public T DoSelectQuery<T>(string sQuery, params object[] QryParams) where T : new()
		{
			try
			{
				// executes a query and returns the first row
				MySqlCommand comm = new MySqlCommand() { Connection = sqlConnection };

				comm.CommandText = sQuery;

				for (int i = 0; i < QryParams.Length; i++)
					comm.Parameters.Add(new MySqlParameter(i.ToString(), QryParams[i]));

				var qdbg = Debug.Query.New(comm.CommandText, nHinstance);

				DbFieldsMap<T> fm = new DbFieldsMap<T>();
				T record = new T();
				using (DbDataReader reader = comm.ExecuteReader())
				{
					if (reader.HasRows == true)
					{
						while (reader.Read())
						{
							for (int i = 0; i <= (reader.FieldCount - 1); i++)
							{
								var fd = fm[reader.GetName(i)];
								if (fd == null)
									continue;

								fd.Index = i;
								if (reader[i] != DBNull.Value)
								{
									fd.SetRecordValue(ref record, reader[i]);
								}
							}

							break;
						}
					}
				}

				qdbg.Close();

				return record;
			}
			catch (Exception ex)
			{
				Debug.Query.Error(ex, nHinstance);
				LogHelper.Error(ex);
			}

			return default(T);
		}

		public bool DoExecBulkWhereInQueries(String QueryWhereIN, Int64[] vIDs, params object[] QryParams)
		{
			try
			{
				String sQry = "";
				int i = 0;
				foreach (Int64 ID in vIDs)
				{
					if (sQry == "")
						sQry = String.Format(QueryWhereIN + " ({1}", ID);
					else
						sQry += String.Format(", {0}", ID);

					i++;

					if (i >= 100)
					{
						DoExecQuery(sQry + ")", QryParams);
						i = 0;
						sQry = "";
					}
				}
				if (sQry != "")
					DoExecQuery(sQry + ")", QryParams);

				return true;
			}
			catch (Exception Ex)
			{
				LogHelper.Error(Ex);
			}

			return false;
		}

	}

	// TableArray Simplifies hadling of recordset vs JSON
	public class TableArray
	{
		public String[] Columns = new String[] { };
		public object[][] Rows = new Object[][] { };

		public TableArray() { }

		public TableArray(DataTable dt)
		{
			Columns = new String[dt.Columns.Count];
			for (int i = 0; i < dt.Columns.Count; i++)
				Columns[i] = dt.Columns[i].ColumnName;

			Rows = (from DataRow d in dt.Rows
					select (from i in Enumerable.Range(0, dt.Columns.Count)
							select d[i]).ToArray()).ToArray();
		}
	}

}



