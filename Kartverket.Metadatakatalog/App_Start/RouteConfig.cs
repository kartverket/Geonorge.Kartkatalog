using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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

            routes.MapRoute("DisplayMetadataUuidAndSlug", "{controller}/{organization}/{title}/{uuid}",
                new { controller = "Metadata", action = "Index" },
                new { uuid = @"[a-zA-Z0-9-]+$" }
            );

            routes.MapRoute("DisplayMetadataUuidAndTitle", "{controller}/{title}/{uuid}",
                new { controller = "Metadata", action = "Index" },
                new { uuid = @"[a-zA-Z0-9-]+$" }
            );

            routes.MapRoute("DisplayMetadataUuid", "{controller}/uuid/{uuid}",
                new { controller = "Metadata", action = "Index" },
                new { uuid = @"[a-zA-Z0-9-]+$" }
            );

            routes.MapRoute("DisplayOrganizationMetadata", "{controller}/{organization}",
                new { controller = "Metadata", action = "Organization" }
            );
            
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
