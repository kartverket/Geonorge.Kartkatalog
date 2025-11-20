using System.Web.Http;
using WebActivatorEx;
using Kartverket.Metadatakatalog;
using Swashbuckle.Application;
using System.Net.Http;

[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace Kartverket.Metadatakatalog
{
    public class SwaggerConfig
    {
        public static void Register()
        {
            var thisAssembly = typeof(SwaggerConfig).Assembly;

            GlobalConfiguration.Configuration
                .EnableSwagger(c =>
                    {
                        c.SingleApiVersion("v1", "Kartverket.Metadatakatalog");

                        // Force HTTPS and correct port for Swagger JSON
                        c.RootUrl(req =>
                        {
                            var uri = req.RequestUri;
                            var builder = new System.UriBuilder(uri)
                            {
                                Scheme = "https"
                            };
                            return builder.Uri.GetLeftPart(System.UriPartial.Authority) + req.GetRequestContext().VirtualPathRoot.TrimEnd('/');
                        });
                    })
                .EnableSwaggerUi( c => { });
        }
    }
}
