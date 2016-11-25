using System.Web.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Kartverket.Metadatakatalog.Formatter;
using WebApi.BasicAuth;

namespace Kartverket.Metadatakatalog
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Formatters.Add(new CsvFormatter());

            config.MapHttpAttributeRoutes();

            config.EnableCors();

            config.EnableBasicAuth();

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
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
        }
    }

}
