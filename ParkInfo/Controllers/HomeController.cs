using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc;
using ParkInfo.Models;

namespace ParkInfo.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult Index(string PostalCode)
        {
            var model = new Location();
            var postalcode = PostalCode;
            string URL = "https://www.toronto.ca/ext/open_data/catalog/data_set_files/locations-20110725.xml";
            string urlParameters = "";
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/xml"));

            // List data response.
            HttpResponseMessage response = client.GetAsync(urlParameters).Result;  // Blocking call! Program will wait here until a response is received or a timeout occurs.
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body.
                var dataObjects = response.Content.ReadAsStringAsync().Result;  //Make sure to add a reference to System.Net.Http.Formatting.dll
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(dataObjects);
                XmlNode root = xmlDocument.DocumentElement;

                // Add the namespace.  
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
                nsmgr.AddNamespace("lc", "http://www.example.org/PFRMapData");


                string xpath = "Locations";
                var nodes = xmlDocument.SelectSingleNode("/lc:Locations", nsmgr);
                Dictionary<String, Location> locationsById = new Dictionary<String, Location>();

                foreach (XmlNode locationNode in nodes)
                {
                    //XmlSerializer serial = new XmlSerializer(typeof(Location));
                    //Location parkLocation = (Location)serial.Deserialize(new XmlNodeReader(locationNode));
                    //locationsById[parkLocation.PostalCode] = parkLocation;
                    if (locationNode.Name == "Location")
                    {
                        Location location = new Location();

                        foreach (XmlNode lcn in locationNode.ChildNodes)
                        {
                            if (lcn.Name.Equals("LocationID"))
                            {
                                location.LocationId = Convert.ToInt32(lcn.InnerText);
                            }

                            if (lcn.Name.Equals("LocationName"))
                            {
                                location.LocationName = (lcn.InnerText);
                            }

                            if (lcn.Name.Equals("Address"))
                            {
                                location.Address = (lcn.InnerText);
                            }

                            if (lcn.Name.Equals("PostalCode"))
                            {
                                location.PostalCode = (lcn.InnerText);
                            }
                        }
                        if(location.PostalCode != null)
                        {
                            locationsById[location.PostalCode] = location;
                        }else
                        {
                            locationsById[location.LocationName] = location;
                        }

                    }
                    Console.WriteLine(locationNode.InnerText);
                }

                if (locationsById.ContainsKey(postalcode))
                {
                    Location coolLocation = locationsById[postalcode];
                    Console.WriteLine("**** Found Location: " + coolLocation.LocationName);
                    model = coolLocation;
                }
                else
                {
                    model = null;
                    // get lat/lng of postal code
                    const String gmeClientID = "538898332741-u032k2pc46v7d2k3nrqlejc8vve5175p.apps.googleusercontent.com";
                    const String key = "AIzaSyBSr7bbumMbtztH_YoN7jsibs9P90S0Hkc";
                    string address = "160 Yorkland St";
                    string requestUri = string.Format("/maps/api/geocode/json?address={0}&sensor=false&client={1}", Uri.EscapeDataString(address), gmeClientID);

                    HMACSHA1 myhmacsha1 = new HMACSHA1();
                    myhmacsha1.Key = Convert.FromBase64String(key);
                    var hash = myhmacsha1.ComputeHash(Encoding.ASCII.GetBytes(requestUri));

                    var url = String.Format("http://maps.googleapis.com{0}&signature={1}", requestUri, Convert.ToBase64String(hash).Replace("+", "-").Replace("/", "_"));


                    WebRequest request = WebRequest.Create(requestUri);
                    WebResponse response1 = request.GetResponse();
                    XDocument xdoc = XDocument.Load(response1.GetResponseStream());

                    XElement result = xdoc.Element("GeocodeResponse").Element("result");
                    XElement locationElement = result.Element("geometry").Element("location");
                    XElement lat = locationElement.Element("lat");
                    XElement lng = locationElement.Element("lng");

                    // get lat lng of all locations by postal code or address
                    // find nearest location to postal code
                }

            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }

            //Make any other calls using HttpClient here.

            //Dispose once all HttpClient calls are complete. This is not necessary if the containing object will be disposed of; for example in this case the HttpClient instance will be disposed automatically when the application terminates so the following call is superfluous.
            client.Dispose();

            return View(model);
        }

    }
}
