using System.Web.Http;
using Newtonsoft.Json;

namespace Kartverket.Metadatakatalog
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "SearchApi",
                routeTemplate: "api/search/{search}",
                defaults: new { controller = "ApiSearch", search = RouteParameter.Optional }
            );
/*
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{search}",
                defaults: new { search = RouteParameter.Optional }
            );
*/

            config.Formatters.JsonFormatter.SerializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

        }
    }

}
