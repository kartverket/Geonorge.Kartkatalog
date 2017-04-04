using System.Web.Mvc;
using System.Web.Routing;

namespace Kartverket.Metadatakatalog
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.LowercaseUrls = true;

            routes.MapRoute("DisplayMetadataUuidAndSlug", "metadata/{organization}/{title}/{uuid}",
                new { controller = "Metadata", action = "Index" },
                new { uuid = @"[a-zA-Z0-9-]+$" }
            );

            routes.MapRoute("DisplayMetadataUuidAndTitle", "metadata/{title}/{uuid}",
                new { controller = "Metadata", action = "Index" },
                new { uuid = @"[a-zA-Z0-9-]+$" }
            );

            routes.MapRoute("DisplayMetadataUuid", "metadata/uuid/{uuid}",
                new { controller = "Metadata", action = "Index" },
                new { uuid = @"[a-zA-Z0-9-]+$" }
            );

            routes.MapRoute("DisplayOrganizationMetadata", "metadata/{OrganizationSeoName}",
                new { controller = "Metadata", action = "Organization" }
            );

            routes.MapRoute("SearchByOrganization", "search/organization",
                new { controller = "Search", action = "Organization" }
            );

            routes.MapRoute("SearchByApplication", "kartlosninger-i-norge",
               new { controller = "Application", action = "Index" }
           );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
