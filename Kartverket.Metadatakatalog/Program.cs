using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Threading;
using log4net;
using System.Reflection;
using System.IO;
using Castle.Windsor;
using Castle.Facilities.SolrNetIntegration;
using SolrNet;
using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Models.SearchIndex;
using Kartverket.Metadatakatalog.Service;

namespace Kartverket.Metadatakatalog
{
    public class Program
    {
        // Global static container for SolrNet (maintaining compatibility with existing code)
        public static WindsorContainer IndexContainer = new WindsorContainer();

        public static void Main(string[] args)
        {
            // Configure log4net early in the application lifecycle
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            log4net.Config.XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            // Set default culture for the application
            var culture = new CultureInfo("nb-NO");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            // Initialize SolrNet configuration
            InitializeSolrNet();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureServices((context, services) =>
                {
                    // BRIDGE SOLRNET SERVICES FROM WINDSORCONTAINER TO ASP.NET CORE DI
                    services.AddSingleton<ISolrOperations<MetadataIndexDoc>>(provider => 
                        IndexContainer.Resolve<ISolrOperations<MetadataIndexDoc>>(SolrCores.Metadata));
                    services.AddSingleton<ISolrOperations<MetadataIndexAllDoc>>(provider => 
                        IndexContainer.Resolve<ISolrOperations<MetadataIndexAllDoc>>(SolrCores.MetadataAll));
                    services.AddSingleton<ISolrOperations<ServiceIndexDoc>>(provider => 
                        IndexContainer.Resolve<ISolrOperations<ServiceIndexDoc>>(SolrCores.Services));
                    services.AddSingleton<ISolrOperations<ApplicationIndexDoc>>(provider => 
                        IndexContainer.Resolve<ISolrOperations<ApplicationIndexDoc>>(SolrCores.Applications));
                    services.AddSingleton<ISolrOperations<ArticleIndexDoc>>(provider => 
                        IndexContainer.Resolve<ISolrOperations<ArticleIndexDoc>>(SolrCores.Articles));

                    // ADD MISSING HTTPCLIENT SERVICES
                    services.AddHttpClient();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddDebug();
                });

        private static void InitializeSolrNet()
        {
            // Initialize SolrNet with multiple cores (from Application_Start)
            var configuration = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var solrServerUrl = configuration["SolrServerUrl"];
            
            var solrFacility = new SolrNetFacility(solrServerUrl);
            solrFacility.AddCore(SolrCores.Metadata, typeof(MetadataIndexDoc), solrServerUrl + "/solr/metadata");
            solrFacility.AddCore(SolrCores.MetadataEnglish, typeof(MetadataIndexDoc), solrServerUrl + "/solr/metadata_en");
            solrFacility.AddCore(SolrCores.MetadataAll, typeof(MetadataIndexAllDoc), solrServerUrl + "/solr/metadata_all");
            solrFacility.AddCore(SolrCores.MetadataAllEnglish, typeof(MetadataIndexAllDoc), solrServerUrl + "/solr/metadata_all_en");
            solrFacility.AddCore(SolrCores.Services, typeof(ServiceIndexDoc), solrServerUrl + "/solr/services");
            solrFacility.AddCore(SolrCores.ServicesEnglish, typeof(ServiceIndexDoc), solrServerUrl + "/solr/services_en");
            solrFacility.AddCore(SolrCores.Applications, typeof(ApplicationIndexDoc), solrServerUrl + "/solr/applications");
            solrFacility.AddCore(SolrCores.ApplicationsEnglish, typeof(ApplicationIndexDoc), solrServerUrl + "/solr/applications_en");
            solrFacility.AddCore(SolrCores.Articles, typeof(ArticleIndexDoc), solrServerUrl + "/solr/articles");
            solrFacility.AddCore(SolrCores.ArticlesEnglish, typeof(ArticleIndexDoc), solrServerUrl + "/solr/articles_en");
            
            IndexContainer.AddFacility(solrFacility);
        }
    }
}