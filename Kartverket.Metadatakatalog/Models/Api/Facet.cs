using System;
using System.Collections.Generic;
using System.Linq;
using Resources;

namespace Kartverket.Metadatakatalog.Models.Api
{
    public class Facet
    {
        /// <summary>
        /// The name of the facet field
        /// </summary>
        public string FacetField { get; set; }
        /// <summary>
        /// The name of the facet field
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The facet result
        /// </summary>
        public List<FacetValue> FacetResults { get; set; }
        /// <summary>
        /// The translated name of the facet field
        /// </summary>
        public string NameTranslated { get; set; }

        public Facet() { }
        private Facet(Models.Facet item)
        {
            FacetField = item.FacetField;
            Name = item.FacetField;
            if (FacetField == "area") 
              FacetResults = FacetValue.OrganizationCreateFromList(item.FacetResults);
            else if (FacetField == "nationalinitiative")
                FacetResults = FacetValue.NationalInitiativeCreateFromList(item.FacetResults);
            else
                FacetResults = FacetValue.CreateFromList(item.FacetResults);

            NameTranslated = UI.ResourceManager.GetString("Facet_" + item.FacetField);
        }

        public static List<Facet> CreateFromList(IEnumerable<Models.Facet> facets)
        {
            return facets.Select(item => new Facet(item)).ToList();
        }

        public class FacetValue
        {
            /// <summary>
            /// The name of the facet result
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// The number of items that has this facet
            /// </summary>
            public int Count { get; set; }
            /// <summary>
            /// The translated name of the facet field
            /// </summary>
            public string NameTranslated { get; set; }
            /// <summary>
            /// The facet result
            /// </summary>
            public List<FacetValue> FacetResults { get; set; }



            public FacetValue() { }

            private FacetValue(Models.Facet.FacetValue item)
            {
                Name = item.Name;
                Count = item.Count;
                NameTranslated = LinkName();

            }

            private FacetValue(Models.Facet.FacetValue facet, Dictionary<string, string> dictionary)
            {
                Name = facet.Name;
                Count = facet.Count;
                NameTranslated = AreaLinkName(dictionary);
            }

            private FacetValue(Models.Facet.FacetValue facet, List<Models.Facet.FacetValue> facetValues, Dictionary<string, string> dictionary)
            {
                Name = facet.Name;
                Count = facet.Count;
                NameTranslated = AreaLinkName(dictionary);
                FacetResults = facetValues.Select(item => new FacetValue(item, dictionary)).ToList();
            }

            public static List<FacetValue> CreateFromList(IEnumerable<Models.Facet.FacetValue> facetResults)
            {
                return facetResults.Select(item => new FacetValue(item)).ToList();
            }

            public static List<FacetValue> OrganizationCreateFromList(IEnumerable<Models.Facet.FacetValue> facetResults)
            {
                List<FacetValue> facets = new List<FacetValue>();

                var placeResolver = new Service.PlaceResolver();
                var areaDictionary = placeResolver.GetAreas();
                var areas = facetResults.Where(fy => fy.Name.Length == 4 && fy.Name != "0/21" && fy.Name != "0/22").Select(fy => fy).Distinct().OrderBy(fo => fo.Name).ToList();
                var places = facetResults.Where(p => !p.Name.Contains("/")).Select(p => p).Distinct().OrderBy(po => po.Name).ToList();

                foreach (var place in places)
                {
                    facets.Add(new FacetValue(place));
                }

                foreach (var area in areas)
                {
                    var municipalities = facetResults.Where(k => k.Name.Length > 4 && k.Name.StartsWith(area.Name)).Select(ko => ko).OrderBy(ko => ko.Name).ToList();
                    facets.Add(new FacetValue(area, municipalities, areaDictionary));
                }

                return facets;
            }
            public static List<FacetValue> NationalInitiativeCreateFromList(IEnumerable<Models.Facet.FacetValue> facetResults)
            {
                List<FacetValue> facets = new List<FacetValue>();

                var categoriesDictionary = new Dictionary<string, string>
                {
                    { "Geodata", "Geodata" },
                    { "Jordobservasjon og miljø", "Jordobservasjon og miljø" },
                    { "Meteorologi", "Meteorologi" },
                    { "Mobilitet", "Mobilitet" },
                    { "Selskaps- og eierskapsdata", "Selskaps- og eierskapsdata" },
                    { "Statistikk", "Statistikk" },
                    { "Geospatial", "Geospatial" },
                    { "Earth observation and environment", "Earth observation and environment" },
                    { "Meteorological", "Meteorological" },
                    { "Mobility", "Mobility" },
                    { "Companies and company ownership", "Sompanies and company ownership" },
                    { "Statistics", "Statistics" },

                };

                IEnumerable<Models.Facet.FacetValue> facetsGeneral = new List<Models.Facet.FacetValue>();
                IEnumerable<Models.Facet.FacetValue> facetsHVD = new List<Models.Facet.FacetValue>();

                foreach (var initiative in facetResults)
                {
                    foreach (var category in categoriesDictionary.Keys)
                    {
                        if (initiative.Name.Contains(category))
                        {
                            if(!facetsHVD.Any(f => f.Name == initiative.Name))
                                facetsHVD = facetsHVD.Append(initiative);
                        }
                        else
                        {
                            if (!facetsGeneral.Any(f => f.Name == initiative.Name) && initiative.Name != "High value dataset")
                                facetsGeneral = facetsGeneral.Append(initiative);
                        }
                    }
                }

                foreach (var initiative in facetsGeneral)
                {
                    facets.Add(new FacetValue(initiative));
                }

                if(facetsHVD.Any())
                {
                    var categories = new List<Models.Facet.FacetValue>();
                    foreach (var category in categoriesDictionary.Keys)
                    {
                        var count = facetsHVD.Where(f => f.Name.Contains(category)).Distinct().Sum(f => f.Count);
                        if (count > 0)
                        {
                            categories.Add(new Models.Facet.FacetValue { Name = category, Count = count });
                        }
                    }

                    var highValueDataset = facetResults.FirstOrDefault(f => f.Name == "High value dataset");
                    if(highValueDataset != null)
                        facets.Add(new FacetValue(new Models.Facet.FacetValue { Count = highValueDataset.Count, Name = "High value dataset" }, categories, categoriesDictionary));
                }

                return facets;
            }


            public string LinkName()
            {
                string link = Name;

                if (Name == "GEONORGE:OFFLINE")
                {
                    link = UI.ResourceManager.GetString("Facet_type_GEONORGE_OFFLINE");
                }
                else { 
                    var translatedName = UI.ResourceManager.GetString("Facet_type_" + Name);
                    link = !string.IsNullOrWhiteSpace(translatedName) ? translatedName : Name;
                    if (link.Length > 50) link = link.Substring(0, 50) + "...";
                }
                return link;
            }

            public string AreaLinkName(Dictionary<string, string> dictionary)
            {
                var translatedName = UI.ResourceManager.GetString("Facet_type_" + Name);
                string link = !string.IsNullOrWhiteSpace(translatedName) ? translatedName : Name;
                if (dictionary.ContainsKey(link))
                {
                    link = dictionary[link];
                }
                return link;
            }
        }

    }
}