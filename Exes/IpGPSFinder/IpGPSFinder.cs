using DAL;
using Helpers;
using HttpdLib;
using LocationLib;
using System;

namespace CamTV
{
    public class IpGPSFinderApi : CApiServer
    {
        public IpGPSFinderApi()
        {
            SetEndPointHandler("/api/ipgpsfinder/v1/health", Health);

            SetEndPointHandler("/api/ipgpsfinder/v1/getip", GetIp);

            SetEndPointHandler("/api/ipgpsfinder/v1/getdistance", GetDistance);

            SetEndPointHandler("/api/ipgpsfinder/v1/locations/new", NewLocation);
            SetEndPointHandler("/api/ipgpsfinder/v1/locations/list", List);
            SetEndPointHandler("/api/ipgpsfinder/v1/locations/edit", EditLocation);
            SetEndPointHandler("/api/ipgpsfinder/v1/locations/delete", DeleteLocation);
        }

        override protected Int64 ValidateSession()
        {
            try
            {
                // AUTH: Disabilitata per semplicità
                Int64 UserID = 1;

                //String sAuthHeader = Types.ToString(Request.Params.Server["Authorization"], Types.ToString(Request.Params.Server["authorization"]));
                //Int64 UserID = Types.ToInt64(sAuthHeader.Replace("Bearer ", ""), -1);

                return UserID;
            }
            catch (Exception Ex)
            {
                LogHelper.Error("Exception: {0}", Ex.Message);
            }

            return -1;
        }

        [HTTPMethod(Public = true, Type = CRequest.Method.GET)]
        void Health()
        {
            // AUTH: Disabilitata per semplicità
            //String sAuthHeader = Types.ToString(Request.Params.Server["Authorization"], Types.ToString(Request.Params.Server["authorization"])).Replace("Bearer ", "");
            //if (sAuthHeader != Sets.API_KEY)
            //	ThrowError(HTTPStatusCode.Unauthorized_401, "Authorization Bearer != Sets.API_KEY - Remote IP:"+ Request.RemoteAddr);
            String IpRemoteIP = "37.159.89.57";//this.Request.RemoteAddr;

            Body = "WORKING";   // Aggiungere dati di info sullo status del processo
            StatusCode = HTTPStatusCode.OK_200;
        }

        [HTTPMethod(Public = true, Type = CRequest.Method.POST)]
        void GetIp()
        {
            //todo solo per debugging, successivamente leggere il parametro tramite Params["HTTP_X_REMOTE_ADDR"]
            String ipRemoteIP = "87.9.232.109";
            var locationUtils = new LocationUtils();
            locationUtils.SetParameters(Sets.LOCATION_API_BASE_URL, Sets.LOCATION_API_KEY_SECRET);
            var ipCoordinates = locationUtils.GetCoordinates(ipRemoteIP);

            Body = new
            {
                ipCoordinates
            };
            StatusCode = HTTPStatusCode.OK_200;
        }

        [HTTPMethod(Public = true, Type = CRequest.Method.POST)]
        void GetDistance()
        {
            String MissingParameter = CheckMissingParams(new String[] { "LatA", "LonA", "LatB", "LonB" });
            if (MissingParameter != null)
                ThrowError(HTTPStatusCode.Bad_Request_400, MissingParameter);

            var latA = Types.ToDouble(Params["LatA"], 0);
            var lonA = Types.ToDouble(Params["LonA"], 0);
            var latB = Types.ToDouble(Params["LatB"], 0);
            var lonB = Types.ToDouble(Params["LonB"], 0);

            var locationUtils = new LocationUtils();
            var distance = locationUtils.GetDistance(latA, lonA, latB, lonB);

            Body = new
            {
                Distance = distance
            };
            StatusCode = HTTPStatusCode.OK_200;
        }

        [HTTPMethod(Public = true, Type = CRequest.Method.POST)]
        void NewLocation()
        {
            String MissingParameter = CheckMissingParams(new String[] { "Description", "Latitude", "Longitude" });
            if (MissingParameter != null)
                ThrowError(HTTPStatusCode.Bad_Request_400, MissingParameter);

            var desciption = Types.ToString(Params["Description"]);
            var latitude = Types.ToDouble(Params["Latitude"], 0);
            var longitude = Types.ToDouble(Params["Longitude"], 0);

            var newLocationId = Locations.New(desciption, latitude, longitude, Db);

            Body = new
            {
                Id = newLocationId
            };
            StatusCode = HTTPStatusCode.OK_200;
        }

        [HTTPMethod(Public = true, Type = CRequest.Method.POST)]
        void List()
        {
            var vLocations = Locations.List(Db);

            Body = vLocations;
            StatusCode = HTTPStatusCode.OK_200;
        }

        [HTTPMethod(Public = true, Type = CRequest.Method.POST)]
        void EditLocation()
        {
            String MissingParameter = CheckMissingParams(new String[] { "ID", "Description", "Latitude", "Longitude" });
            if (MissingParameter != null)
                ThrowError(HTTPStatusCode.Bad_Request_400, MissingParameter);

            var id = Types.ToInt(Params["ID"]);
            var desciption = Types.ToString(Params["Description"]);
            var latitude = Types.ToDouble(Params["Latitude"], 0);
            var longitude = Types.ToDouble(Params["Longitude"], 0);

            var rowAffected = Locations.Edit(id, desciption, latitude, longitude, Db);

            Body = new
            {
                Rows = rowAffected
            };
            StatusCode = HTTPStatusCode.OK_200;
        }

        [HTTPMethod(Public = true, Type = CRequest.Method.POST)]
        void DeleteLocation()
        {
            String MissingParameter = CheckMissingParams(new String[] { "ID" });
            if (MissingParameter != null)
                ThrowError(HTTPStatusCode.Bad_Request_400, MissingParameter);

            var id = Types.ToInt(Params["ID"]);

            var rowAffected = Locations.Delete(id, Db);

            Body = new
            {
                Rows = rowAffected
            };
            StatusCode = HTTPStatusCode.OK_200;
        }
    }
}