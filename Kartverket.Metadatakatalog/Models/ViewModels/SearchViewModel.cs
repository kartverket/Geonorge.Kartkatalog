using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace Kartverket.Metadatakatalog.Models.ViewModels
{
    public class SearchViewModel
    {
        public string Text { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public int NumFound { get; set; }
        public string orderby { get; set; }
        public List<FacetParameter> FacetParameters { get; set; }
        public SearchResultViewModel Result { get; set; }
        public int pages { get; set; }
        public int page { get; set; }
        public int startPage { get; set; }
        public int endPage { get; set; }
        public Dictionary<string, string> areaDictionary { get; set; }
        public List<string> EnabledFacets { get; set; }

        public SearchViewModel(SearchParameters parameters, SearchResult searchResult)
        {
            Text = parameters.Text;
            FacetParameters = parameters.Facets;
            Result = new SearchResultViewModel(searchResult);
            Limit = searchResult.Limit;
            Offset = searchResult.Offset;
            NumFound = searchResult.NumFound;
            orderby = parameters.orderby.ToString();
            page = 1;
            var placeResolver = new Service.PlaceResolver();
            areaDictionary = placeResolver.GetAreas();

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

            startPage = page;
            endPage = page;
        }

        public bool IsActivePage(int i) {

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
            if (value.Split('/').Length - 1 > 1)
            {
                int lastSlash = value.LastIndexOf("/");
                if (lastSlash > 0)
                    value = value.Substring(0, lastSlash);
                routeValues["Facets[" + index + "]" + ".name"] = name;
                routeValues["Facets[" + index++ + "]" + ".value"] = value;
            };
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

        public bool HasFacetFieldValue(string facetField, string facetValue)
        {
            return FacetParameters != null && FacetParameters.Any(f => f.Name == facetField && f.Value == facetValue);
        }

        public double GetRelativeHierarchyLevel(string facetField, string previousFacetField)
        {
            int previousFacetHierarchyLevel = previousFacetField.Split('/').Length - 1;
            int facetHierarchyLevel = facetField.Split('/').Length - 1;
            double relativeHierarchyLevel = facetHierarchyLevel - previousFacetHierarchyLevel;
            return relativeHierarchyLevel;
        }


        public bool IsPreviousButtonActive()
        {
            return Offset > 1 && (Offset - Limit) >= 1;
        }

        public bool IsNextButtonActive()
        {
            return NumFound > (Offset + Limit-1);
        }
        public bool IsLastButtonActive()
        {
            return NumFound > (Offset + Limit - 1);
        }
        public bool IsFirstButtonActive()
        {
            return Offset > 1 && (Offset - Limit) >= 1;
        }

        public RouteValueDictionary ParamsForPageNumber(int page)
        {
            page = page - 1;
            
            var routeValues = new RouteValueDictionary();
            routeValues = CreateLinkWithParameters(routeValues, FacetParameters);
            routeValues["Offset"] = page * Limit + 1;
            routeValues["orderby"] = orderby;
            return routeValues;
        }

        public RouteValueDictionary ParamsForPreviousLink()
        {
            var routeValues = new RouteValueDictionary();
            routeValues = CreateLinkWithParameters(routeValues, FacetParameters);
            routeValues["Offset"] = (Offset - Limit);
            routeValues["orderby"] = orderby;
            return routeValues;
        }
        public RouteValueDictionary ParamsForFirstLink()
        {
            return ParamsForPageNumber(1);
        }
        public RouteValueDictionary ParamsForLastLink()
        {
            return ParamsForPageNumber(pages);
        }

        public RouteValueDictionary ParamsForNextLink()
        {
            var routeValues = new RouteValueDictionary();
            routeValues = CreateLinkWithParameters(routeValues, FacetParameters);
            routeValues["Offset"] = (Offset + Limit);
            routeValues["orderby"] = orderby;
            return routeValues;
        }
        
        public RouteValueDictionary ParamsForOrderByScoreLink()
        {
            var routeValues = new RouteValueDictionary();
            routeValues = CreateLinkWithParameters(routeValues, FacetParameters);
            routeValues["orderby"] = "score";
            return routeValues;
        }

        public RouteValueDictionary ParamsForOrderByLink(string name)
        {
            var routeValues = new RouteValueDictionary();
            routeValues = CreateLinkWithParameters(routeValues, FacetParameters);

            if (orderby.Contains(name))
            {
                if (orderby.Contains("_desc"))
                {
                    routeValues["orderby"] = name;
                }
                else {
                    routeValues["orderby"] = name + "_desc";
                }
            }
            else {
                routeValues["orderby"] = name;
            }
            return routeValues;
        }

        public RouteValueDictionary ParamsForOrderByTitleLink()
        {
            var routeValues = new RouteValueDictionary();
            routeValues = CreateLinkWithParameters(routeValues, FacetParameters);
            routeValues["orderby"] = "title";
            return routeValues;
        }

        public RouteValueDictionary ParamsForOrderByTitleDescLink()
        {
            var routeValues = new RouteValueDictionary();
            routeValues = CreateLinkWithParameters(routeValues, FacetParameters);
            routeValues["orderby"] = "title_desc";
            return routeValues;
        }

        public RouteValueDictionary ParamsForOrderByOrganizationLink()
        {
            var routeValues = new RouteValueDictionary();
            routeValues = CreateLinkWithParameters(routeValues, FacetParameters);
            routeValues["orderby"] = "organization";
            return routeValues;
        }

        public RouteValueDictionary ParamsForOrderByTitle()
        {
            var routeValues = new RouteValueDictionary();
            routeValues = CreateLinkWithParameters(routeValues, FacetParameters);
            if (orderby == "title")
            {
                routeValues["orderby"] = "title_desc";
            }
            else
            {
                routeValues["orderby"] = "title";
            }
            return routeValues;
        }

        public RouteValueDictionary ParamsForOrderByOrganization()
        {
            var routeValues = new RouteValueDictionary();
            routeValues = CreateLinkWithParameters(routeValues, FacetParameters);
            if (orderby == "organization")
            {
                routeValues["orderby"] = "organization_desc";
            }
            else
            {
                routeValues["orderby"] = "organization";
            }
            return routeValues;
        }


        public RouteValueDictionary ParamsForOrderByOrganizationDescLink()
        {
            var routeValues = new RouteValueDictionary();
            routeValues = CreateLinkWithParameters(routeValues, FacetParameters);
            routeValues["orderby"] = "organization_desc";
            return routeValues;
        }

        public RouteValueDictionary ParamsForOrderByMetadataUpdateLink()
        {
            var routeValues = new RouteValueDictionary();
            routeValues = CreateLinkWithParameters(routeValues, FacetParameters);
            routeValues["orderby"] = "newest";
            return routeValues;
        }
        public RouteValueDictionary ParamsForOrderByResourceUpdateLink()
        {
            var routeValues = new RouteValueDictionary();
            routeValues = CreateLinkWithParameters(routeValues, FacetParameters);
            routeValues["orderby"] = "updated";
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

        public List<string> FacetsServiceDirectory()
        {
            return new List<string> { "themes", "organizations", "nationalinitiatives", "areas", "distributionProtocols", "dataAccesses" };
        }

        public List<string> FacetApplications()
        {
            return new List<string> { "themes", "organizations", "nationalinitiatives", "areas", "distributionProtocols", "dataAccesses" };
        }
    }

}