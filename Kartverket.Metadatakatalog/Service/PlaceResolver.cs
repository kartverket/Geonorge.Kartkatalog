using System.Collections.Generic;
using System.Linq;
using GeoNorgeAPI;
using Kartverket.Metadatakatalog.Models.Api;
using www.opengis.net;
using System.Net;
using System.Net.Http;
using System;
using System.Net.Http.Headers;
using System.Web.Configuration;
using Kartverket.Metadatakatalog.Models.Translations;

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

    public class PlaceResolver
    {
        public const string PlaceNorge = "Norge";
        public const string PlaceNorgeEnglish = "Norway";
        public const string PlaceHavomraader = "Havområder";
        public const string PlaceHavomraaderEnglish = "Sea areas";
        public const string PlaceSvalbard = "Svalbard";
        public const string PlaceJanMayen = "Jan Mayen";


        private Dictionary<string, string> _areas;
        private static readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri(WebConfigurationManager.AppSettings["RegistryUrl"]),
        };
        /// <summary>
        /// Gets fylke og kommuner fra register i et dictionary
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetAreas()
        {
            populateAreas();
            return _areas;
        }

        private void populateAreas()
        {
            MemoryCacher memCacher = new MemoryCacher();
            var cache = memCacher.GetValue("areas");

            if (cache != null)
            {
                _areas = cache as Dictionary<string, string>;
            }
            else
            {
                _areas = new Dictionary<string, string>();
                //call register fylker og kommuner
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var result = _httpClient.GetAsync("api/subregister/sosi-kodelister/kartverket/fylkesnummer-alle").Result;
                if (result.IsSuccessStatusCode)
                {
                    var register = result.Content.ReadAsAsync<Register>().Result;

                    foreach (var item in register.containeditems)
                    {
                        var codevalue = item.label;
                        var label = item.description;
                        var status = item.status;
                        if (status == "Gyldig")                        
                            _areas.Add("0/" + codevalue, label);
                    }
                }
                var result2 = _httpClient.GetAsync("api/subregister/sosi-kodelister/kartverket/kommunenummer-alle").Result;
                if (result2.IsSuccessStatusCode)
                {
                    var register = result2.Content.ReadAsAsync<Register>().Result;
                    foreach (var item in register.containeditems)
                    {
                        var codevalue = item.label;
                        var label = item.description;
                        var status = item.status;
                        if (status == "Gyldig")
                            _areas.Add("0/" + codevalue.Substring(0, 2) + "/" + codevalue, label);
                    }
                }

                memCacher.Add("areas", _areas, new DateTimeOffset(DateTime.Now.AddHours(12)));
            }

        }

        // note use of lowercase keywords - comparison is also done with lower casing of input
        private readonly Dictionary<string, string> _placeToHavomraader = new Dictionary<string, string>
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

        private readonly Dictionary<string, string> _placeToHavomraaderEnglish = new Dictionary<string, string>
        {
            {"norskehavet", PlaceHavomraaderEnglish},
            {"barentshavet", PlaceHavomraaderEnglish},
            {"nordsjøen", PlaceHavomraaderEnglish},
            {"tromsøflaket", PlaceHavomraaderEnglish},
            {"norsk kontinentalsokkel", PlaceHavomraaderEnglish},
            {"finnmarkskysten", PlaceHavomraaderEnglish},
            {"eggakanten", PlaceHavomraaderEnglish},
            {"hav", PlaceHavomraaderEnglish},
            {"kyst", PlaceHavomraaderEnglish},
            {"kystnære områder", PlaceHavomraaderEnglish},
            {"sjø", PlaceHavomraaderEnglish},
            {"grønlandshavet", PlaceHavomraaderEnglish},
            {"norske sjøområder", PlaceHavomraaderEnglish},
            {"kystnær", PlaceHavomraaderEnglish},
            {"norsk økonomisk sone", PlaceHavomraaderEnglish},
            {"skagerak", PlaceHavomraaderEnglish}

        };

        private readonly Dictionary<string, string> _placeToSvalbard = new Dictionary<string, string>
        {
            {"norge og svalbard", PlaceSvalbard},
            {"svalbard", PlaceSvalbard}


        };

        private readonly Dictionary<string, string> _placeToJanMayen = new Dictionary<string, string>
        {
            {"jan mayen", PlaceJanMayen},

        };

        public List<string> Resolve(SimpleMetadata metadata, string culture)
        {
            List<string> placegroup = new List<string>();
            if(culture == Culture.NorwegianCode)
                placegroup.Add(PlaceNorge);
            else
                placegroup.Add(PlaceNorgeEnglish);

            foreach (var keyword in metadata.Keywords)
            {
                string lowerCaseKeyword = keyword.Keyword.ToLower();
                string placeHav;
                if (culture == Culture.NorwegianCode)
                {
                    if (_placeToHavomraader.TryGetValue(lowerCaseKeyword, out placeHav))
                    {
                        placegroup.Add(placeHav);
                    }
                }
                else
                {
                    if (_placeToHavomraaderEnglish.TryGetValue(lowerCaseKeyword, out placeHav))
                    {
                        placegroup.Add(placeHav);
                    }
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
                var myValueList = _areas.Where(x => x.Value.ToLower() == keyword.Keyword.ToLower()).ToList();
                if(myValueList != null)
                {
                    foreach (var myValue in myValueList)
                    {
                        if (myValue.Key != null) placegroup.Add(myValue.Key);
                    }
                }
                
            }

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var result = _httpClient.GetAsync("https://ws.geonorge.no/dekningsApi/dekning?uuid=" + metadata.Uuid).Result;
            if (result.IsSuccessStatusCode)
            {
                var register = result.Content.ReadAsAsync<Coverage>().Result;

                for(int c = 0; c < register.kommuner.Count(); c ++)
                {
                    string kommune = register.kommuner[c].ToString("D4");
                    string fylke = kommune.Substring(0,2);
                    kommune = "0/" + kommune.Substring(0, 2) + "/" + kommune;
                    fylke = "0/" + fylke;
                    var municipality = _areas.FirstOrDefault(x => x.Key == kommune).Key;
                    if (municipality != null && !placegroup.Contains(kommune)) placegroup.Add(municipality);

                    var areaFylke = _areas.FirstOrDefault(x => x.Key == fylke).Key;
                    if (areaFylke != null && !placegroup.Contains(fylke)) placegroup.Add(areaFylke);
                }

            }

            List<string> placegroup2 = new List<string>();
            placegroup2.AddRange(placegroup);

            //Må sjekke at kommuner som er lagt inn også har fylke
            foreach (string placekey in placegroup2)
            {
                string[] parts = placekey.Split('/');
                if (parts.Count() > 2)
                {
                    if (!placegroup.Contains(parts[0] + "/" + parts[1]))
                    {
                        //Om kommunen ikke har fylke, så fjernes kommunen
                        placegroup.Remove(placekey);
                    }
                }
            }


            return placegroup;
        }

    }


    public class Coverage
    {
        public int[] kommuner { get; set; }
    }

}