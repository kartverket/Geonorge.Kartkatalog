using System.Collections.Generic;
using System.Linq;

namespace Kartverket.Metadatakatalog.Models.ViewModels
{
    public class SearchResultFacetViewModel
    {
        public string FacetField { get; set; }
        public List<SearchResultFacetValueViewModel> FacetResults { get; set; }

        private SearchResultFacetViewModel(Facet facet)
        {
            FacetField = facet.FacetField;
            FacetResults = SearchResultFacetValueViewModel.CreateFromList(facet);
        }

        public static List<SearchResultFacetViewModel> CreateFromList(IEnumerable<Facet> facets)
        {
            return facets.Select(item => new SearchResultFacetViewModel(item)).ToList();
        }
        
    }
}