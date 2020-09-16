using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CamTV
{
	public class Locations
	{
		// TODO: 
		//  - Creare un database
		//  - Creare una tabella delle Locations
		//  - Creare una interfaccia CRUD su Locations
		//  - Esporre gli endpoit per l'interfaccia CRUD sulla API
		//  - Testarla e documentarla su Postman

		public class TLocation
		{
			public Int64 ID;
			public String Description;
			public Double Latitude;
			public Double Longitude;
		}

		public static TLocation[] List(DatabaseHelper Db)
		{
			try
			{
				TLocation[] vL = Db.DoSelectQueryArray<TLocation>("SELECT * FROM locations");

				return vL;
			}
			catch (Exception Ex)
			{
				LogHelper.Error(Ex);
			}

			return null;
		}

		public static Int64 New(String Description, Double Latitude, Double Longitude, DatabaseHelper Db)
		{
			try
			{
				return Db.DoExecInsertQuery("INSERT INTO locations (Description, Latitude, Longitude) VALUES (@0,@1,@2)",
					Description, Latitude, Longitude);
			}
			catch (Exception Ex)
			{
				LogHelper.Error(Ex);
			}

			return -1;
		}


		// TODO: Edit

		// TODO: Delete

	}
}
