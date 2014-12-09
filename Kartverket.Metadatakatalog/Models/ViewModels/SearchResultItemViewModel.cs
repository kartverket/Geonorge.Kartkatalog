using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;

namespace Kartverket.Metadatakatalog.Models.ViewModels
{
    public class SearchResultItemViewModel
    {
        public string Uuid { get; set; }
        public string Title { get; set; }
        public string @Abstract { get; set; }
        public string Type { get; set; }
        public string Theme { get; set; }
        public string Organization { get; set; }
        public string OrganizationLogoUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public string MaintenanceFrequency { get; set; }

        public string GetInnholdstypeCSS()
        {
            string t = "label-default";
            if (Type=="dataset") t="label-success";
            else if (Type=="software") t="label-warning";
            else if (Type=="service") t="label-info";
            else if (Type=="series") t="label-primary";

            return t;
        }

        public string GetInnholdstype()
        {
            string t = Type;
            if (Type=="dataset") t="Datasett";
            else if (Type=="software") t="Applikasjon";
            else if (Type=="service") t="Tjeneste";
            else if (Type=="series") t="Datasettserie";

            return t;
        }

        private SearchResultItemViewModel(SearchResultItem item)
        {
            Uuid = item.Uuid;
            Title = item.Title;
            Abstract = item.Abstract;
            Type = item.Type;
            Theme = item.Theme;
            Organization = item.Organization;
            OrganizationLogoUrl = item.OrganizationLogoUrl;
            ThumbnailUrl = item.ThumbnailUrl;
            MaintenanceFrequency = item.MaintenanceFrequency;
        }

        public static List<SearchResultItemViewModel> CreateFromList(IEnumerable<SearchResultItem> items)
        {
            return items.Select(item => new SearchResultItemViewModel(item)).ToList();
        }

        public RouteValueDictionary ShowMetadataLinkRouteValueDictionary()
        {
            var routeValueDictionary = new RouteValueDictionary();

            var seoUrl = new SeoUrl(Organization, Title);

            routeValueDictionary["uuid"] = Uuid;
            routeValueDictionary["organization"] = seoUrl.Organization;
            routeValueDictionary["title"] = seoUrl.Title;

            return routeValueDictionary;
        }

        public string OrganizationSeoName()
        {
            var seoUrl = new SeoUrl(Organization, Title);
            return seoUrl.Organization;
        }
    }
}