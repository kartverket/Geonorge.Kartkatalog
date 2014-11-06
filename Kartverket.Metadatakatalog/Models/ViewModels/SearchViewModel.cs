using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;

namespace Kartverket.Metadatakatalog.Models.ViewModels
{
    public class SearchViewModel
    {
        public string Text { get; set; }
        public List<FacetParameter> FacetParameters { get; set; }
        public SearchResultViewModel Result { get; set; }

        public SearchViewModel(SearchParameters parameters, SearchResult searchResult)
        {
            Text = parameters.Text;
            FacetParameters = parameters.Facets;
            Result = new SearchResultViewModel(searchResult);
        }

        public RouteValueDictionary LinkForFacetValue(string name, string value)
        {
            var routeValues = new RouteValueDictionary();
            int index = 0;
            routeValues["Facets[" + index + "]" + ".name"] = name;
            routeValues["Facets[" + index++ + "]" + ".value"] = value;
            routeValues = CreateRoutesForFacetFieldsExcept(name, routeValues, index);
            return routeValues;
        }

        public RouteValueDictionary CreateRoutesForFacetFieldsExcept(string field, RouteValueDictionary routeValues, int index = 0)
        {
            IEnumerable<FacetParameter> filteredFacets = FacetParameters.Where(f => f.Name != field);
            foreach (var facetParameter in filteredFacets)
            {
                routeValues["Facets[" + index + "]" + ".name"] = facetParameter.Name;
                routeValues["Facets[" + index++ + "]" + ".value"] = facetParameter.Value;
            }
            return routeValues;
        }

        public RouteValueDictionary CreateRoutesForFacetFieldsExcept(string field)
        {
            var routeValues = new RouteValueDictionary();
            CreateRoutesForFacetFieldsExcept(field, routeValues);
            return routeValues;
        }

        public bool HasFilterForFacetField(string facetField)
        {
            return FacetParameters != null && FacetParameters.Any(f => f.Name == facetField);
        }
    }
}