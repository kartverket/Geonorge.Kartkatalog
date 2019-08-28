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

            routes.MapRoute("OIDC-callback-signout", "signout-callback-oidc", new { controller = "Search", action = "SignOutCallback" });

            routes.MapRoute("DisplayMetadataUuidAndSlug", "metadata/{organization}/{title}/{uuid}",
                new { controller = "Metadata", action = "Index" },
                new { uuid = @"[a-zA-Z0-9-_]+$" }
            );

            routes.MapRoute("DisplayMetadataUuidAndTitle", "metadata/{title}/{uuid}",
                new { controller = "Metadata", action = "Index" },
                new { uuid = @"[a-zA-Z0-9-_]+$" }
            );

            routes.MapRoute("DisplayMetadataUuid", "metadata/uuid/{uuid}",
                new { controller = "Metadata", action = "Index" },
                new { uuid = @"[a-zA-Z0-9-_]+$" }
            );

            routes.MapRoute("DisplayOrganizationMetadata", "metadata/{OrganizationSeoName}",
                new { controller = "Metadata", action = "Organization" }
            );

            // apier-og-tjenester
            routes.MapRoute("DisplayServiceDirectory", "apier-og-tjenester",
                new { controller = "ServiceDirectory", action = "Index" }
            );

            // Hva finnes i kommunen/Fylke
            routes.MapRoute("SearchByArea", "hva-finnes-i-kommunen-eller-fylket",
                new { controller = "Search", action = "Area" }
            );


            // Etatvis oversikt
            routes.MapRoute("SearchByOrganizationMetadata", "etatvis-oversikt/{OrganizationSeoName}",
                new { controller = "Metadata", action = "Organization" }
            );

            // Kartløsninger
            routes.MapRoute("SearchByApplication", "kartlosninger",
               new { controller = "Application", action = "Index" }
            );

            // Tema detaljside 
            routes.MapRoute("Tema", "tema/{ThemeSeoName}/{id}",
               new { controller = "Themes", action = "Details", id = UrlParameter.Optional }
            );

            // åpne-data
            routes.MapRoute("DisplayOpenData", "aapne-data",
                new { controller = "OpenData", action = "Index" }
            );

            routes.MapRoute(
            name: "ThemesRoute",
            url: "temaer/{action}/{id}",
            defaults: new
            {
                controller = "Themes",
                action = "Index",
                id = UrlParameter.Optional
            }
            );

            routes.MapRoute(
                name: "DownloadRoute",
                url: "nedlasting",
                defaults: new
                {
                    controller = "Download",
                    action = "Index",
                    id = UrlParameter.Optional
                }
             );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
