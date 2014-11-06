﻿using System.Collections.Generic;
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
            return !string.IsNullOrWhiteSpace(translatedName) ? translatedName : Name;
        }
    }
}