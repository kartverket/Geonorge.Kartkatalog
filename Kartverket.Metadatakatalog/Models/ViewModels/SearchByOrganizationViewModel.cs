using System.Web.Routing;

namespace Kartverket.Metadatakatalog.Models.ViewModels
{
    public class SearchByOrganizationViewModel : SearchViewModel
    {
        public string OrganizationSeoName { get; set; }
        public string OrganizationName { get; set; }
        public string OrganizationLogoUrl { get; set; }

        public SearchByOrganizationViewModel(SearchByOrganizationParameters parameters, SearchResultForOrganization searchResult)
            : base(parameters, searchResult)
        {
            if (parameters.OrganizationSeoName == null) return;
            OrganizationSeoName = parameters.OrganizationSeoName;
            if (searchResult.Organization != null)
            {
                OrganizationName = searchResult.Organization.Name;
                OrganizationLogoUrl = searchResult.Organization.LogoUrl;
            }
            else
            {
                OrganizationName = searchResult.GetOrganizationNameFromFirstItem();
            }
        }

        public RouteValueDictionary ParamsForOrderByTitleLinkOrganization()
        {
            var routeValues = new RouteValueDictionary();
            routeValues["orderby"] = "title";
            routeValues["organizationSeoName"] = OrganizationSeoName;
            return routeValues;
        }

        public RouteValueDictionary ParamsForOrderByTitleDescLinkOrganization()
        {
            var routeValues = new RouteValueDictionary();
            routeValues["orderby"] = "title_desc";
            routeValues["organizationSeoName"] = OrganizationSeoName;
            return routeValues;
        }
    }
}