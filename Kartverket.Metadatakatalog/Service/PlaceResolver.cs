using System.Collections.Generic;
using System.Linq;
using GeoNorgeAPI;
using Kartverket.Metadatakatalog.Models.Api;
using www.opengis.net;

namespace Kartverket.Metadatakatalog.Service
{
    public  class PlaceResolver
    {
        public const string PlaceNorge = "Norge";
        public const string PlaceHavomraader = "Havområder";
        public const string PlaceSvalbard = "Svalbard";
        public const string PlaceJanMayen = "Jan Mayen";
        

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
            List<string> placegroup = new List<string>();
            
            foreach (var keyword in metadata.Keywords)
            {
                string lowerCaseKeyword = keyword.Keyword.ToLower();
                
                if (lowerCaseKeyword.Contains("telemark"))
                {
                    placegroup.Add("0/08");
                }
                else
                {
                    placegroup.Add("0/07");
                    placegroup.Add("0/02");
                }
                if (lowerCaseKeyword == "sauherad")
                {
                    placegroup.Add("0/08");
                    placegroup.Add("0/08/0822");
                }
                if (lowerCaseKeyword == "kragerø")
                {
                    placegroup.Add("0/08");
                    placegroup.Add("0/08/0815");
                }
                
            }

            return placegroup;
        }
       
    }
}