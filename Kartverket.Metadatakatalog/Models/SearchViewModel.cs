using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;

namespace Kartverket.Metadatakatalog.Models
{
    public class SearchViewModel
    {
        public SearchParameters Parameters { get; set; }
        public SearchResult Result { get; set; }


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
            IEnumerable<FacetParameter> filteredFacets = Parameters.Facets.Where(f => f.Name != field);
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

    }
}