using System.Collections.Generic;
using System.Linq;
using GeoNorgeAPI;
using Kartverket.Metadatakatalog.Models.Api;
using www.opengis.net;
using System.Net;
using System.Net.Http;
using System;
using System.Net.Http.Headers;

namespace Kartverket.Metadatakatalog.Service
{
    public class Register
    {
        public string id { get; set; }
        public string label { get; set; }

        public string contentsummary { get; set; }
        public string owner { get; set; }
        public string manager { get; set; }
        public string controlbody { get; set; }
        public List<Registeritem> containeditems { get; set; }
        public List<Register> containedSubRegisters { get; set; }
        public DateTime lastUpdated { get; set; }
    }
    
    public class Registeritem
    {
        public string id { get; set; }
        public string label { get; set; }
        public string itemclass { get; set; }
        public string status { get; set; }
        public string description { get; set; }
        public string seoname { get; set; }
        public string owner { get; set; }

        public string logo { get; set; }
        public string documentreference { get; set; }

        public string inspireRequirement { get; set; }
        public string nationalRequirement { get; set; }
        public string nationalSeasRequirement { get; set; }

        public string verticalReferenceSystem { get; set; }
        public string horizontalReferenceSystem { get; set; }
        public string dimension { get; set; }

        public string codevalue { get; set; }

        public string serviceUrl { get; set; }


    }

    public  class PlaceResolver
    {
        public const string PlaceNorge = "Norge";
        public const string PlaceHavomraader = "Havområder";
        public const string PlaceSvalbard = "Svalbard";
        public const string PlaceJanMayen = "Jan Mayen";

        private Dictionary<string, string> _areas;

        private void populateAreas() {
            if (_areas == null || _areas.Count == 0)
            {
                _areas = new Dictionary<string, string>();
                //call register fylker og kommuner
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://register.dev.geonorge.no/"); 
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var result = client.GetAsync("api/subregister/sosi-kodelister/kartverket/fylkesnummer").Result;
                if (result.IsSuccessStatusCode)
                {
                    var register = result.Content.ReadAsAsync<Register>().Result;
                    
                    foreach (var item in register.containeditems)
                    {
                        _areas.Add("0/" + item.codevalue, item.label);
                    }
                }
                var result2 = client.GetAsync("api/subregister/sosi-kodelister/kartverket/kommunenummer").Result;
                if (result2.IsSuccessStatusCode)
                {
                    var register = result2.Content.ReadAsAsync<Register>().Result;
                    foreach (var item in register.containeditems)
                    {
                        _areas.Add("0/" + item.codevalue.Substring(0,2) + "/" + item.codevalue, item.label);
                    }
                }
               
            }
            
        }

        // note use of lowercase keywords - comparison is also done with lower casing of input
        private  readonly Dictionary<string, string> _placeToHavomraader = new Dictionary<string, string>
        {
            {"norskehavet", PlaceHavomraader},
            {"barentshavet", PlaceHavomraader},
            {"nordsjøen", PlaceHavomraader},
            {"tromsøflaket", PlaceHavomraader},
            {"norsk kontinentalsokkel", PlaceHavomraader},
            {"finnmarkskysten", PlaceHavomraader},
            {"eggakanten", PlaceHavomraader},
            {"hav", PlaceHavomraader},
            {"kyst", PlaceHavomraader},
            {"kystnære områder", PlaceHavomraader},
            {"sjø", PlaceHavomraader},
            {"grønlandshavet", PlaceHavomraader},
            {"norske sjøområder", PlaceHavomraader},
            {"kystnær", PlaceHavomraader},
            {"norsk økonomisk sone", PlaceHavomraader},
            {"skagerak", PlaceHavomraader}
            
        };

        private  readonly Dictionary<string, string> _placeToSvalbard = new Dictionary<string, string>
        {
            {"norge og svalbard", PlaceSvalbard},
            {"svalbard", PlaceSvalbard}
            
           
        };

        private  readonly Dictionary<string, string> _placeToJanMayen = new Dictionary<string, string>
        {
            {"jan mayen", PlaceJanMayen},
            
        };

        public  List<string> Resolve(SimpleMetadata metadata)
        {
            List<string> placegroup = new List<string>();
            placegroup.Add("Norge");
            foreach (var keyword in metadata.Keywords)
            {
                string lowerCaseKeyword = keyword.Keyword.ToLower();
                string placeHav;
                if (_placeToHavomraader.TryGetValue(lowerCaseKeyword, out placeHav))
                {
                    placegroup.Add(placeHav);
                }
                string placeSval;
                if (_placeToSvalbard.TryGetValue(lowerCaseKeyword, out placeSval))
                {
                    placegroup.Add(placeSval);
                }
                string placeJm;
                if (_placeToJanMayen.TryGetValue(lowerCaseKeyword, out placeJm))
                {
                    placegroup.Add(placeJm);
                }
            }

            return placegroup;
        }

        public List<string> ResolveArea(SimpleMetadata metadata)
        {
            populateAreas();

            List<string> placegroup = new List<string>();
            
            foreach (var keyword in metadata.Keywords)
            {
                var myValue = _areas.FirstOrDefault(x => x.Value.ToLower() == keyword.Keyword.ToLower()).Key;
                if (myValue != null) placegroup.Add(myValue);
            }

            return placegroup;
        }
       
    }
}