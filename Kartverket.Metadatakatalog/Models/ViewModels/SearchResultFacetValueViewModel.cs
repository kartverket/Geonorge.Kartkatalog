using System;
using System.Collections.Generic;
using System.Linq;
using Resources;
using Kartverket.Metadatakatalog.Service;

namespace Kartverket.Metadatakatalog.Models.ViewModels
{
    public class SearchResultFacetValueViewModel
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public string ShortName { get; set; }

        private SearchResultFacetValueViewModel(Facet.FacetValue facetValue, string facetField, RegisterFetcher registerFetcher = null)
        {
            Name = facetValue.Name;
            Count = facetValue.Count;
            if(facetField == "organization" && registerFetcher != null)
                ShortName = GetShortName(facetValue.Name, registerFetcher);
        }

        private string GetShortName(string name, RegisterFetcher register)
        {
            string newValue = name;
            register.OrganizationShortNames.TryGetValue(name, out newValue);
            if (!string.IsNullOrEmpty(newValue))
                name = newValue;
            return name;
        }

        public static List<SearchResultFacetValueViewModel> CreateFromList(Facet facet, RegisterFetcher registerFetcher = null)
        {
            return facet.FacetResults.Select(item => new SearchResultFacetValueViewModel(item, facet.FacetField, registerFetcher)).ToList();
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