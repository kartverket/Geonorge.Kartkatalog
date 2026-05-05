using Kartverket.Metadatakatalog.Service;
using System.Collections.Generic;

namespace Kartverket.Metadatakatalog.Models.ViewModels
{
    public class SearchResultViewModel
    {
        public List<SearchResultItemViewModel> Items { get; set; }
        public List<SearchResultFacetViewModel> Facets { get; set; }

        public SearchResultViewModel(SearchResult searchResult, ISimpleMetadataUtil simpleMetadataUtil)
        {
            Items = SearchResultItemViewModel.CreateFromList(searchResult.Items, simpleMetadataUtil);
            Facets = SearchResultFacetViewModel.CreateFromList(searchResult.Facets);
        }

    }
}