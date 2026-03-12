using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Threading;
using System.Reflection;
using System.IO;
using Castle.Windsor;
using Castle.Facilities.SolrNetIntegration;
using SolrNet;
using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Models.SearchIndex;
using Kartverket.Metadatakatalog.Service;
using System;
using Serilog;

namespace Kartverket.Metadatakatalog
{
    // Factory interface for accessing different Solr cores
    public interface ISolrOperationsFactory
    {
        ISolrOperations<T> GetOperations<T>(string coreId);
    }

    // Factory implementation that uses the WindsorContainer
    public class SolrOperationsFactory : ISolrOperationsFactory
    {
        public ISolrOperations<T> GetOperations<T>(string coreId)
        {
            return Program.IndexContainer.Resolve<ISolrOperations<T>>(coreId);
        }
    }

    public class Program
    {
        // Global static container for SolrNet (maintaining compatibility with existing code)
        public static WindsorContainer IndexContainer = new WindsorContainer();

        public static void Main(string[] args)
        {
            // Configure Serilog early in the application lifecycle
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            try
            {
                Log.Information("Starting Kartverket Metadatakatalog application");

                // Set default culture for the application
                var culture = new CultureInfo("nb-NO");
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;

                // Initialize SolrNet configuration
                InitializeSolrNet();

                // Initialize SimpleMetadataUtil with configuration early
                var simpleMetadataUtil = new SimpleMetadataUtil(configuration);

                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog() // Use Serilog instead of default logging
                .ConfigureAppConfiguration((context, config) =>
                {
                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        config.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);
                    }
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureServices((context, services) =>
                {
                    // BRIDGE SOLRNET SERVICES FROM WINDSORCONTAINER TO ASP.NET CORE DI
                    // Register default cores (for backward compatibility)
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

                    // REGISTER SOLR FACTORY FOR MULTI-CORE ACCESS
                    // This enables our optimized indexers to access different cores dynamically
                    services.AddSingleton<ISolrOperationsFactory>(provider => new SolrOperationsFactory());

                    // ADD MISSING HTTPCLIENT SERVICES
                    services.AddHttpClient();
                });

        private static void InitializeSolrNet()
        {
            // Initialize SolrNet with multiple cores (from Application_Start)
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true);
            
            // Only load local settings in development
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            if (environment == "Development")
            {
                configBuilder.AddJsonFile("appsettings.local.json", optional: true);
            }
            
            var configuration = configBuilder
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