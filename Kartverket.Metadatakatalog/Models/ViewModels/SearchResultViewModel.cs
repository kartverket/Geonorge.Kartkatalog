using System.Collections.Generic;

namespace Kartverket.Metadatakatalog.Models.ViewModels
{
    public class SearchResultViewModel
    {
        public List<SearchResultItemViewModel> Items { get; set; }
        public List<SearchResultFacetViewModel> Facets { get; set; }

        public SearchResultViewModel(SearchResult searchResult)
        {
            Items = SearchResultItemViewModel.CreateFromList(searchResult.Items);
            Facets = SearchResultFacetViewModel.CreateFromList(searchResult.Facets);
        }

    }
}