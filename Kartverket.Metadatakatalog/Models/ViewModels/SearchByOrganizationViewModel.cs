using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;

namespace Kartverket.Metadatakatalog.Models.ViewModels
{
    public class SearchByOrganizationViewModel : SearchViewModel
    {
        public string OrganizationSeoName { get; set; }
        public string OrganizationName { get; set; }
        public string OrganizationLogoUrl { get; set; }
        public SelectList OrganizationSelectList { get; set; }

        public SearchByOrganizationViewModel(SearchByOrganizationParameters parameters, SearchResultForOrganization searchResult)
            : base(parameters, searchResult)
        {
            EnabledFacets = new List<string> { "themes", "types", "nationalinitiatives", "areas", "distributionProtocols", "dataAccesses" };

            if (parameters.OrganizationSeoName != null)
            {
                OrganizationSeoName = parameters.OrganizationSeoName;
            }
            if (searchResult.Organization != null)
            {
                OrganizationName = searchResult.Organization.Name;
                OrganizationLogoUrl = searchResult.Organization.LogoUrl;
            }

            var organizations = searchResult.Organizations();
            OrganizationSelectList = new SelectList(organizations, "key", "value", parameters.OrganizationSeoName);
        }

        public RouteValueDictionary ParamsForOrderByTitleLinkOrganization()
        {
            var routeValues = new RouteValueDictionary
            {
                ["orderby"] = "title"
            };

            return routeValues;
        }

        public RouteValueDictionary ParamsForOrderByTitleDescLinkOrganization()
        {
            var routeValues = new RouteValueDictionary
            {
                ["orderby"] = "title_desc",
            };
            return routeValues;
        }


        public RouteValueDictionary OrganizationRouteValues(RouteValueDictionary routeValues)
        {
            routeValues["OrganizationSeoName"] = GetOrganizationSeoName();
            return routeValues;
        }

        private string GetOrganizationSeoName()
        {
            return OrganizationSeoName ?? "organisasjon";
        }
    }
}