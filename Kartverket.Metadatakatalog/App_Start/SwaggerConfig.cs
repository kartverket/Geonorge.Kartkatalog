using Kartverket.Metadatakatalog;
using Swashbuckle.Application;
using System.Net.Http;
using System.Web.Configuration;
using System.Web.Http;
using WebActivatorEx;

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

                        //set the host for the Swagger document
                        c.RootUrl(req =>
                        {                  
                            return WebConfigurationManager.AppSettings["KartkatalogenUrl"];
                        });
                    })
                .EnableSwaggerUi(c => { });
        }
    }
}
