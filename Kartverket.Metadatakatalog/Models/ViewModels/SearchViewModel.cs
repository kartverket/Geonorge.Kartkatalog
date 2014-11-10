using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;

namespace Kartverket.Metadatakatalog.Models.ViewModels
{
    public class SearchViewModel
    {
        public string Text { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public int NumFound { get; set; }
        public List<FacetParameter> FacetParameters { get; set; }
        public SearchResultViewModel Result { get; set; }

        public SearchViewModel(SearchParameters parameters, SearchResult searchResult)
        {
            Text = parameters.Text;
            FacetParameters = parameters.Facets;
            Result = new SearchResultViewModel(searchResult);
            Limit = searchResult.Limit;
            Offset = searchResult.Offset;
            NumFound = searchResult.NumFound;
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
            CreateLinkWithParameters(routeValues, filteredFacets, index);
            return routeValues;
        }

        private RouteValueDictionary CreateLinkWithParameters(RouteValueDictionary routeValues, IEnumerable<FacetParameter> filteredFacets, int index = 0)
        {
            foreach (var facetParameter in filteredFacets)
            {
                routeValues["Facets[" + index + "]" + ".name"] = facetParameter.Name;
                routeValues["Facets[" + index++ + "]" + ".value"] = facetParameter.Value;
            }
            routeValues["text"] = Text;

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
            return FacetParameters != null && FacetParameters.Any(f => f.Name == facetField && !string.IsNullOrWhiteSpace(f.Value));
        }


        public bool IsPreviousButtonActive()
        {
            return Offset > 1 && (Offset - Limit) >= 1;
        }

        public bool IsNextButtonActive()
        {
            return NumFound > (Offset + Limit);
        }


        public RouteValueDictionary ParamsForPreviousLink()
        {
            var routeValues = new RouteValueDictionary();
            routeValues = CreateLinkWithParameters(routeValues, FacetParameters);
            routeValues["Offset"] = (Offset - Limit);
            return routeValues;
        }

        public RouteValueDictionary ParamsForNextLink()
        {
            var routeValues = new RouteValueDictionary();
            routeValues = CreateLinkWithParameters(routeValues, FacetParameters);
            routeValues["Offset"] = (Offset + Limit);
            return routeValues;
        }

        public string ShowingFromAndTo()
        {
            int from = Offset;
            int to = (Offset + Limit - 1);

            if (to > NumFound)
            {
                to = NumFound;
            }

            return string.Format("{0} - {1}", from, to);
        }
    }

}