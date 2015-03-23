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
        public int orderby { get; set; }
        public List<FacetParameter> FacetParameters { get; set; }
        public SearchResultViewModel Result { get; set; }
        public int pages { get; set; }
        public int page { get; set; }
        public int startPage { get; set; }
        public int endPage { get; set; }

        public SearchViewModel(SearchParameters parameters, SearchResult searchResult)
        {
            Text = parameters.Text;
            FacetParameters = parameters.Facets;
            Result = new SearchResultViewModel(searchResult);
            Limit = searchResult.Limit;
            Offset = searchResult.Offset;
            NumFound = searchResult.NumFound;
            orderby = (int)parameters.orderby;
            page = 1;
            if (Offset != 1)
            {
                page = (Offset / Limit) + 1;
            }

            //Finne totalt antall sider
            pages = NumFound / Limit;

            //Test om det er noe bak komma.... 
            if ((Limit*pages) != NumFound)
            {
                pages = pages + 1;
            }

            //Hvilke sider som skal være synlige
            if (pages > 10)
            {
                startPage = 1;
                endPage = 10;


                if (page > 5 && page <= (pages-5))
                {
                    startPage = page - 4;
                    endPage = page + 5;
                }
                if (page > (pages-5) && page > 5) {
                    startPage = pages - 9;
                    endPage = pages;
                }

                
            }
            else { 
                startPage = 1;
                endPage = pages;
            }
        }

        public bool IsActivePage(int i) {
            ////page = 1;

            ////Finne hvilke side en er på
            //if (Offset != 1)
            //{
            //    page = (Offset / Limit) + 1;
            //}

            if (i == page)
            {
                return true;
            }
            else return false;
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
            return NumFound > (Offset + Limit-1);
        }

        

        public RouteValueDictionary ParamsForPageNumber(int page)
        {
            page = page - 1;
            
            var routeValues = new RouteValueDictionary();
            routeValues = CreateLinkWithParameters(routeValues, FacetParameters);
            routeValues["Offset"] = page * Limit + 1;
            return routeValues;
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
        
        public RouteValueDictionary ParamsForOrderByScoreLink()
        {
            var routeValues = new RouteValueDictionary();
            routeValues = CreateLinkWithParameters(routeValues, FacetParameters);
            routeValues["orderby"] = 0;
            return routeValues;
        }

        public RouteValueDictionary ParamsForOrderByTitleLink()
        {
            var routeValues = new RouteValueDictionary();
            routeValues = CreateLinkWithParameters(routeValues, FacetParameters);
            routeValues["orderby"] = 1;
            return routeValues;
        }
        public RouteValueDictionary ParamsForOrderByOrganizationLink()
        {
            var routeValues = new RouteValueDictionary();
            routeValues = CreateLinkWithParameters(routeValues, FacetParameters);
            routeValues["orderby"] = 2;
            return routeValues;
        }
        public RouteValueDictionary ParamsForOrderByMetadataUpdateLink()
        {
            var routeValues = new RouteValueDictionary();
            routeValues = CreateLinkWithParameters(routeValues, FacetParameters);
            routeValues["orderby"] = 3;
            return routeValues;
        }
        public RouteValueDictionary ParamsForOrderByResourceUpdateLink()
        {
            var routeValues = new RouteValueDictionary();
            routeValues = CreateLinkWithParameters(routeValues, FacetParameters);
            routeValues["orderby"] = 4;
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