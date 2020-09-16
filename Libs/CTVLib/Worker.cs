using Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Helpers
{
	public class Worker
	{
		public delegate Task TJob(DatabaseHelper Db, int nCicles = 1);

		public static void RunForever(String DbHost, int DbPort, String DbUser, String DbPsw, String DbName, int DbMaxPoolSize, int CicleTimeMs, TJob Job)
		{
			Task.Run(async () =>
			{
				int nCicles = 0;
				while (true)
				{
					try
					{
						using (DatabaseHelper Db = new DatabaseHelper())
						{
							Db.SetDbConnectionData(DbHost, DbPort, DbUser, DbPsw, DbName, 5);
							if (Db.OpenConnection() == false)
								LogHelper.Throw("Db.OpenConnection() == false");

							await Job(Db, nCicles);

							Db.CloseConnection();
						}

						nCicles++;
					}
					catch (Exception Ex)
					{
						LogHelper.Error("Exception: {0}", Ex.Message);
					}

					await Task.Delay(CicleTimeMs);
				}
			}).Wait();
		}
	}
}
