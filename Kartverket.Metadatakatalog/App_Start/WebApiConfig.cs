using System.Web.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Kartverket.Metadatakatalog.Formatter;
using WebApi.BasicAuth;
using Kartverket.Metadatakatalog.Models.Api;
using System.Web.Http.ModelBinding;
using System.Web.Http.ModelBinding.Binders;

namespace Kartverket.Metadatakatalog
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            var provider = new SimpleModelBinderProvider(typeof(SearchParameters), new SearchParameterModelBuilder());
            config.Services.Insert(typeof(ModelBinderProvider), 0, provider);

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
