using Helpers;
using System;

namespace DAL
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
                TLocation[] locations = Db.DoSelectQueryArray<TLocation>("SELECT ID,Description,Latitude,Longitude FROM locations");

                return locations;
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

        public static Int64 Edit(int id, String description, Double latitude, Double longitude, DatabaseHelper db)
        {
            try
            {
                var helper = new KeyValueHelper();
                helper.Add("ID", id);
                helper.Add("Description", description);
                helper.Add("Latitude", latitude);
                helper.Add("Longitude", longitude);

                return db.DoExecUpdateQuery("locations", helper);
            }
            catch (Exception Ex)
            {
                LogHelper.Error(Ex);
            }

            return -1;
        }

        public static Int64 Delete(int locationId, DatabaseHelper Db)
        {
            try
            {
                return Db.DoExecQuery("DELETE FROM locations WHERE ID=@0", locationId);
            }
            catch (Exception Ex)
            {
                LogHelper.Error(Ex);
            }

            return -1;
        }
    }
}
