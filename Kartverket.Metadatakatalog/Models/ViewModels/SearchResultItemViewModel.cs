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
        
    }
}