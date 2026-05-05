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
            // ?? PERFORMANCE FIX: Configure HTTP settings for .NET 10 to match .NET Framework 4.8 behavior
            ConfigureHttpPerformanceSettings();

            // ?? CRITICAL PERFORMANCE FIX: Pre-warm Castle Windsor container to prevent JIT delays
            PrewarmWindsorContainer();

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
            
            // ?? PERFORMANCE FIX: Create SolrNetFacility with optimized settings
            var solrFacility = new SolrNetFacility(solrServerUrl);

            // ?? PERFORMANCE OPTIMIZATION: Add cores in optimized order (most used first)
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            solrFacility.AddCore(SolrCores.Metadata, typeof(MetadataIndexDoc), solrServerUrl + "/solr/metadata", true);
            solrFacility.AddCore(SolrCores.MetadataAll, typeof(MetadataIndexAllDoc), solrServerUrl + "/solr/metadata_all", true);
            solrFacility.AddCore(SolrCores.MetadataEnglish, typeof(MetadataIndexDoc), solrServerUrl + "/solr/metadata_en", true);
            solrFacility.AddCore(SolrCores.MetadataAllEnglish, typeof(MetadataIndexAllDoc), solrServerUrl + "/solr/metadata_all_en", true);
            solrFacility.AddCore(SolrCores.Services, typeof(ServiceIndexDoc), solrServerUrl + "/solr/services", true);
            solrFacility.AddCore(SolrCores.ServicesEnglish, typeof(ServiceIndexDoc), solrServerUrl + "/solr/services_en", true);
            solrFacility.AddCore(SolrCores.Applications, typeof(ApplicationIndexDoc), solrServerUrl + "/solr/applications", true);
            solrFacility.AddCore(SolrCores.ApplicationsEnglish, typeof(ApplicationIndexDoc), solrServerUrl + "/solr/applications_en", true);
            solrFacility.AddCore(SolrCores.Articles, typeof(ArticleIndexDoc), solrServerUrl + "/solr/articles", true);
            solrFacility.AddCore(SolrCores.ArticlesEnglish, typeof(ArticleIndexDoc), solrServerUrl + "/solr/articles_en", true);
            
            // ?? CRITICAL PERFORMANCE MEASUREMENT: Track Windsor container initialization time
            IndexContainer.AddFacility(solrFacility);
            
            stopwatch.Stop();
            Console.WriteLine($"?? SolrNet Windsor Container initialized in {stopwatch.ElapsedMilliseconds}ms");
            
            // ?? PERFORMANCE OPTIMIZATION: Pre-resolve commonly used services to warm up container
            WarmupSolrOperations();
        }

        /// <summary>
        /// Pre-warm Castle Windsor container to prevent first-time JIT compilation delays
        /// </summary>
        private static void PrewarmWindsorContainer()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Force Windsor container initialization and JIT compilation
            var dummy = new Castle.Windsor.WindsorContainer();
            dummy.Dispose();
            
            stopwatch.Stop();
            Console.WriteLine($"?? Castle Windsor pre-warmed in {stopwatch.ElapsedMilliseconds}ms");
        }

        /// <summary>
        /// Warm up SolrNet operations to prevent first-call delays
        /// </summary>
        private static void WarmupSolrOperations()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                // Pre-resolve the most commonly used SolrOperations to warm up the container
                var metadataOps = IndexContainer.Resolve<ISolrOperations<MetadataIndexDoc>>(SolrCores.Metadata);
                var metadataAllOps = IndexContainer.Resolve<ISolrOperations<MetadataIndexAllDoc>>(SolrCores.MetadataAll);
                
                stopwatch.Stop();
                Console.WriteLine($"?? SolrNet operations warmed up in {stopwatch.ElapsedMilliseconds}ms");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Console.WriteLine($"?? SolrNet warmup failed in {stopwatch.ElapsedMilliseconds}ms: {ex.Message}");
            }
        }

        /// <summary>
        /// Configure HTTP performance settings to address .NET 10 vs .NET Framework 4.8 performance differences
        /// </summary>
        private static void ConfigureHttpPerformanceSettings()
        {
            // ?? CRITICAL PERFORMANCE FIX: Configure ServicePointManager settings for .NET 10
            // These settings are crucial for HTTP performance, especially with external APIs like GeoNorge
            System.Net.ServicePointManager.DefaultConnectionLimit = 100;
            System.Net.ServicePointManager.MaxServicePointIdleTime = 30000; // 30 seconds
            System.Net.ServicePointManager.DnsRefreshTimeout = 120000; // 2 minutes
            System.Net.ServicePointManager.EnableDnsRoundRobin = false;
            System.Net.ServicePointManager.UseNagleAlgorithm = false;
            System.Net.ServicePointManager.Expect100Continue = false;
            
            // ?? CRITICAL THREADING PERFORMANCE: Configure ThreadPool for better async performance
            System.Threading.ThreadPool.SetMinThreads(50, 50); // Increase minimum threads for better responsiveness
            
            // ?? CRITICAL DNS PERFORMANCE FIX: Configure DNS caching behavior
            AppContext.SetSwitch("System.Net.SocketsHttpHandler.Http2UnencryptedSupport", true);
            
            // ?? CRITICAL CONNECTION PERFORMANCE: Configure connection behavior  
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2Support", true);
            AppContext.SetSwitch("System.Net.Http.UseSocketsHttpHandler", true); // Force use of SocketsHttpHandler
            
            // ?? TCP PERFORMANCE: Configure TCP behavior for faster connections
            AppContext.SetSwitch("System.Net.Sockets.DisableIPProtectionLevel", true);
            
            // ?? GARBAGE COLLECTION: Configure for better performance with many HTTP requests
            System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.SustainedLowLatency;
            
            Console.WriteLine("?? Applied comprehensive .NET 10 performance optimizations for HTTP/GeoNorge/Solr");
        }
    }
}