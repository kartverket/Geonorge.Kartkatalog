using System.Collections.Generic;
using System.Linq;
using GeoNorgeAPI;
using Kartverket.Metadatakatalog.Models.Api;
using www.opengis.net;
using System.Net;
using System.Net.Http;
using System;
using System.Net.Http.Headers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http.Json;
using Kartverket.Metadatakatalog.Models.Translations;
using Kartverket.Metadatakatalog.Models;

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

        private const string AreasCacheKey = "PlaceResolver.Areas";
        private static readonly TimeSpan AreasCacheLifetime = TimeSpan.FromHours(12);
        private static readonly SemaphoreSlim AreasFetchLock = new SemaphoreSlim(1, 1);

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;

        public PlaceResolver(HttpClient httpClient, IConfiguration configuration, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _cache = cache;
            _httpClient.BaseAddress = new Uri(_configuration["RegistryUrl"]);
        }

        /// <summary>
        /// Gets fylke og kommuner fra register i et dictionary, cached for 12 hours.
        /// </summary>
        public async Task<Dictionary<string, string>> GetAreasAsync(CancellationToken cancellationToken = default)
        {
            if (_cache.TryGetValue(AreasCacheKey, out Dictionary<string, string> cached))
                return cached;

            await AreasFetchLock.WaitAsync(cancellationToken);
            try
            {
                if (_cache.TryGetValue(AreasCacheKey, out cached))
                    return cached;

                var areas = await FetchAreasAsync(cancellationToken);
                _cache.Set(AreasCacheKey, areas, AreasCacheLifetime);
                return areas;
            }
            finally
            {
                AreasFetchLock.Release();
            }
        }

        // Sync wrapper retained for legacy callers. Cache hits are non-blocking
        // dictionary lookups; only the very first call after process start (or
        // cache expiry) actually blocks on I/O.
        public Dictionary<string, string> GetAreas() => GetAreasAsync().GetAwaiter().GetResult();

        private async Task<Dictionary<string, string>> FetchAreasAsync(CancellationToken cancellationToken)
        {
            var areas = new Dictionary<string, string>();

            await AddCodeListAsync(
                areas,
                "api/sosi-kodelister/inndelinger/inndelingsbase/fylkesnummer",
                code => "0/" + code,
                cancellationToken);

            await AddCodeListAsync(
                areas,
                "api/sosi-kodelister/inndelinger/inndelingsbase/kommunenummer",
                code => "0/" + code.Substring(0, 2) + "/" + code,
                cancellationToken);

            return areas;
        }

        private async Task AddCodeListAsync(Dictionary<string, string> target, string requestUri, Func<string, string> keyBuilder, CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode) return;

            var register = await response.Content.ReadFromJsonAsync<Register>(cancellationToken: cancellationToken);
            if (register?.containeditems == null) return;

            foreach (var item in register.containeditems)
            {
                if (item.status != "Gyldig" || string.IsNullOrEmpty(item.codevalue)) continue;
                target[keyBuilder(item.codevalue)] = RemoveSamiTranslation(item.description);
            }
        }

        private string RemoveSamiTranslation(string description)
        {
            if(description.Contains(" – ")) 
            {
                var names = description.Split('–');
                description = names[0].Trim();
            }

            return description;
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
            var areas = GetAreasAsync().GetAwaiter().GetResult();

            List<string> placegroup = new List<string>();

            foreach (var keyword in metadata.Keywords)
            {
                string[] keyWords = FixKeyWord(keyword.Keyword.ToLower());
                foreach (var keyWord in keyWords)
                {
                    var myValueList = areas.Where(x => x.Value.ToLower() == keyWord).ToList();
                    if(myValueList != null)
                    {
                        foreach (var myValue in myValueList)
                        {
                            if (myValue.Key != null && !placegroup.Contains(myValue.Key) ) placegroup.Add(myValue.Key);
                        }
                    }
                }

            }

            if(metadata.HierarchyLevel != "software")
            {
                using var coverageRequest = new HttpRequestMessage(HttpMethod.Get, "https://ws.geonorge.no/dekningsApi/dekning?uuid=" + metadata.Uuid);
                coverageRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var result = _httpClient.SendAsync(coverageRequest).GetAwaiter().GetResult();
                if (result.IsSuccessStatusCode)
                {
                    var register = result.Content.ReadFromJsonAsync<Coverage>().GetAwaiter().GetResult();

                    for(int c = 0; c < register.kommuner.Count(); c ++)
                    {
                        string kommune = register.kommuner[c].ToString("D4");
                        string fylke = kommune.Substring(0,2);
                        kommune = "0/" + kommune.Substring(0, 2) + "/" + kommune;
                        fylke = "0/" + fylke;
                        var municipality = areas.FirstOrDefault(x => x.Key == kommune).Key;
                        if (municipality != null && !placegroup.Contains(kommune)) placegroup.Add(municipality);

                        var areaFylke = areas.FirstOrDefault(x => x.Key == fylke).Key;
                        if (areaFylke != null && !placegroup.Contains(fylke)) placegroup.Add(areaFylke);
                    }

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

        private string[] FixKeyWord(string keyword)
        {
            List<string> keywords = new List<string>();

            if (keyword.Contains("fylke")) { 
                keyword = keyword.Replace(" fylke", "");
                keywords.Add(keyword);
            }
            else if (keyword.Contains("kommune"))
            {
                var area = keyword.Split(',');
                if(area != null && area.Length > 1) 
                {
                    var municipality = area[0].Replace("kommune","").Trim();
                    var county = area[1].Trim();

                    keywords.Add(municipality);
                    keywords.Add(county);
                }
            }
            else
                keywords.Add(keyword);

            return keywords.ToArray();
        }

        internal List<string> ResolveSpatialScope(List<Models.Keyword> keywords, string culture)
        {
            List<string> spatialscopes = new List<string>();

            foreach(var keyword in keywords)
            {
                if (culture == Culture.NorwegianCode)
                {
                    spatialscopes.Add(GetNorwegianSpatialScope(keyword.KeywordValue));
                }   
                else
                    spatialscopes.Add(keyword.KeywordValue);
            }

            return spatialscopes;
        }

        private readonly Dictionary<string, string> _norwegianSpatialScope = new Dictionary<string, string>
        {
            {"European", "Europeisk"},
            {"Global", "Global"},
            {"Local", "Lokal"},
            {"National", "Nasjonal"},
            {"Regional", "Regional"}
           

        };

        private string GetNorwegianSpatialScope(string keywordValue)
        {
            if (_norwegianSpatialScope.ContainsKey(keywordValue))
                return _norwegianSpatialScope[keywordValue];
            else
                return keywordValue;
        }
    }


    public class Coverage
    {
        public int[] kommuner { get; set; }
    }

}