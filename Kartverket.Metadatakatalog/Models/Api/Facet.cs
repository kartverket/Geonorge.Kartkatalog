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
            FacetResults = FacetField == "area" ? FacetValue.OrganizationCreateFromList(item.FacetResults) : FacetValue.CreateFromList(item.FacetResults);
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

                foreach (var area in areas)
                {
                    var municipalities = facetResults.Where(k => k.Name.Length > 4 && k.Name.StartsWith(area.Name)).Select(ko => ko).OrderBy(ko => ko.Name).ToList();
                    facets.Add(new FacetValue(area, municipalities, areaDictionary));
                }

                return facets;
            }


            public string LinkName()
            {
                var translatedName = UI.ResourceManager.GetString("Facet_type_" + Name);
                string link = !string.IsNullOrWhiteSpace(translatedName) ? translatedName : Name;
                if (link == "GEONORGE:OFFLINE")
                    link = "";
                if (link.Length > 50) link = link.Substring(0, 50) + "...";
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