using Newtonsoft.Json;
using System;
using System.Device.Location;
using System.Net.Http;

namespace LocationLib
{
    public class LocationUtils
    {
        private string _baseUrl;
        private string _accessKey;

        public void SetParameters(string baseUrl, string accessKey)
        {
            _baseUrl = baseUrl;
            _accessKey = accessKey;
        }

        public IpCoordinates GetCoordinates(string ip)
        {
            var ipCoordinates = new IpCoordinates();
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_baseUrl);
                    var response = client.GetAsync($"{ip}?access_key={_accessKey}&format=1").Result;
                    var result = response.Content.ReadAsStringAsync().Result;
                    ipCoordinates = JsonConvert.DeserializeObject<IpCoordinates>(result);
                }
            }
            catch (Exception)
            {
                //todo log exception
            }

            return ipCoordinates;
        }

        public double GetDistance(double latA, double lonA, double latB, double lonB)
        {
            var a = new GeoCoordinate(latA, lonA);
            var b = new GeoCoordinate(latB, lonB);
            return Math.Round(a.GetDistanceTo(b) / 1000, 2);
        }
    }
}
