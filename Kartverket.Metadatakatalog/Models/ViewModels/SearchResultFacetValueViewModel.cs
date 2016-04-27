using System.Collections.Generic;
using System.Linq;
using Resources;

namespace Kartverket.Metadatakatalog.Models.ViewModels
{
    public class SearchResultFacetValueViewModel
    {
        public string Name { get; set; }
        public int Count { get; set; }

        private SearchResultFacetValueViewModel(Facet.FacetValue facetValue)
        {
            Name = facetValue.Name;
            Count = facetValue.Count;
        }

        public static List<SearchResultFacetValueViewModel> CreateFromList(IEnumerable<Facet.FacetValue> facetResults)
        {
            return facetResults.Select(item => new SearchResultFacetValueViewModel(item)).ToList();
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