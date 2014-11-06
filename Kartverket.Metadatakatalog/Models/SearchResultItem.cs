using System.Web.Routing;

namespace Kartverket.Metadatakatalog.Models
{
    public class SearchResultItem
    {
        public string Uuid { get; set; }
        public string Title { get; set; }
        public string @Abstract { get; set; }
        public string Type { get; set; }
        public string Theme { get; set; }
        public string Organization { get; set; }
        public string OrganizationLogoUrl { get; set; }
        public string ThumbnailUrl { get; set; }

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