using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Kartverket.Metadatakatalog.Service;
using Kartverket.Metadatakatalog.Service.Application;
using Kartverket.Metadatakatalog.Service.Article;
using Kartverket.Metadatakatalog.Service.Search;
using Kartverket.Metadatakatalog.Service.ServiceDirectory;
using GeoNorgeAPI;
using Kartverket.Geonorge.Utilities;
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;

namespace Kartverket.Metadatakatalog.Extensions
{
    /// <summary>
    /// Extension methods for registering application services in ASP.NET Core DI
    /// Migrated from Autofac DependencyConfig
    /// </summary>
    public static class ServiceRegistrationExtensions
    {
        /// <summary>
        /// Register external API services
        /// </summary>
        public static IServiceCollection AddExternalServices(this IServiceCollection services, IConfiguration configuration)
        {
            // GeoNorge is a stateless HTTP/CSW client wrapper. Its constructor performs
            // remote service discovery against GeoNetworkUrl, which takes ~900 ms. Registering
            // it as a singleton pays that cost once at startup instead of on every request.
            services.AddSingleton<IGeoNorge>(provider =>
            {
                var httpClientFactory = provider.GetService<System.Net.Http.IHttpClientFactory>();
                return CreateOptimizedGeoNorge(configuration, httpClientFactory);
            });

            // GeoNetworkUtil from Kartverket.Geonorge.Utilities
            services.AddScoped<GeoNetworkUtil>(provider =>
                new GeoNetworkUtil(configuration["GeoNetworkUrl"]));

            // Organization Service with optimized HttpClient
            services.AddScoped<Kartverket.Geonorge.Utilities.Organization.IOrganizationService>(provider =>
                new Kartverket.Geonorge.Utilities.Organization.OrganizationService(
                    configuration["RegistryUrl"],
                    provider.GetRequiredService<Kartverket.Geonorge.Utilities.Organization.IHttpClientFactory>()));

            // URL Resolver
            services.AddScoped<Kartverket.Geonorge.Utilities.IGeonorgeUrlResolver>(provider =>
                new Kartverket.Geonorge.Utilities.GeonorgeUrlResolver(configuration["EditorUrl"]));

            return services;
        }

        /// <summary>
        /// Create an optimized GeoNorge instance using the performance-optimized HttpClient
        /// </summary>
        private static IGeoNorge CreateOptimizedGeoNorge(IConfiguration configuration, System.Net.Http.IHttpClientFactory httpClientFactory)
        {
            try
            {
                // Try to use named HttpClient if GeoNorge constructor supports it
                var geoNorgeClient = httpClientFactory?.CreateClient("GeoNorge");
                
                // Use reflection to check if GeoNorge constructor accepts HttpClient
                var geoNorgeType = typeof(GeoNorge);
                var constructors = geoNorgeType.GetConstructors();
                
                // Look for constructor with HttpClient parameter
                var httpClientConstructor = constructors.FirstOrDefault(c => 
                    c.GetParameters().Any(p => p.ParameterType == typeof(HttpClient)));

                if (httpClientConstructor != null && geoNorgeClient != null)
                {
                    System.Console.WriteLine("?? Using optimized HttpClient for GeoNorge API");
                    // If HttpClient constructor exists, use it
                    var parameters = httpClientConstructor.GetParameters();
                    var args = new object[parameters.Length];
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (parameters[i].ParameterType == typeof(HttpClient))
                            args[i] = geoNorgeClient;
                        else if (parameters[i].ParameterType == typeof(string))
                        {
                            if (parameters[i].Name.Contains("endpoint", StringComparison.OrdinalIgnoreCase))
                                args[i] = configuration["GeoNetworkUrl"];
                            else
                                args[i] = ""; // username/password
                        }
                    }
                    return (IGeoNorge)Activator.CreateInstance(geoNorgeType, args);
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"?? Could not create optimized GeoNorge instance: {ex.Message}");
            }

            // Fallback to standard constructor - this is likely the 6-7 second bottleneck
            System.Console.WriteLine("?? Using standard GeoNorge constructor (performance bottleneck identified here)");
            return new GeoNorge(
                geonetworkUsername: "",
                geonetworkPassword: "",
                geonetworkEndpoint: configuration["GeoNetworkUrl"]);
        }

        /// <summary>
        /// <summary>
        /// Register core application services
        /// </summary>
        public static IServiceCollection AddKartverketApplicationServices(this IServiceCollection services)
        {
            // Core services
            services.AddScoped<IErrorService, ErrorService>();
            services.AddScoped<IMetadataService, MetadataService>();
            services.AddScoped<ISearchService, SearchService>();
            services.AddScoped<ISearchServiceAll, SearchServiceAll>();
            services.AddScoped<IServiceDirectoryService, ServiceDirectoryService>();
            services.AddScoped<IApplicationService, ApplicationService>();
            // services.AddScoped<IThemeService, ThemeService>(); // ThemeService not found, commented out
            services.AddScoped<IArticleService, ArticleService>();
            services.AddScoped<IAiService, AiService>();

            return services;
        }

        /// <summary>
        /// Register indexing and search services
        /// </summary>
        public static IServiceCollection AddIndexingServices(this IServiceCollection services)
        {
            // Note: SolrNet ISolrOperations registration is handled elsewhere or was working through different means
            // For now, just register the service classes without the SolrNet dependencies
            
            // Metadata indexers
            services.AddScoped<MetadataIndexer, SolrMetadataIndexer>();
            services.AddScoped<Indexer, SolrIndexer>();
            services.AddScoped<IndexerAll, SolrIndexerAll>();
            services.AddScoped<IndexerService, SolrIndexerServices>();
            services.AddScoped<IndexerApplication, SolrIndexerApplication>();
            services.AddScoped<IndexDocumentCreator, SolrIndexDocumentCreator>();

            // Article indexers
            services.AddScoped<ArticleIndexer, SolrArticleIndexer>();
            services.AddScoped<IndexerArticle, SolrIndexerArticle>();
            services.AddScoped<IndexArticleDocumentCreator, SolrIndexArticleDocumentCreator>();

            // Resolvers and fetchers
            services.AddScoped<ThemeResolver>();
            services.AddScoped<PlaceResolver>();
            services.AddScoped<IArticleFetcher, ArticleFetcher>();

            return services;
        }

        /// <summary>
        /// Register all application services (equivalent to old DependencyConfig)
        /// </summary>
        public static IServiceCollection AddKartverketServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddExternalServices(configuration);
            services.AddKartverketApplicationServices();
            services.AddIndexingServices();
            services.AddAuthenticationServices();

            // Register SimpleMetadataUtil with configuration
            services.AddSingleton<ISimpleMetadataUtil, SimpleMetadataUtil>();

            return services;
        }

        /// <summary>
        /// Register authentication services (migrated from GeonorgeAuthenticationModule)
        /// </summary>
        public static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
        {
            // Register Geonorge authentication services
            services.AddScoped<Kartverket.Metadatakatalog.Services.Authentication.IGeonorgeAuthenticationService, 
                               Kartverket.Metadatakatalog.Services.Authentication.GeonorgeAuthenticationService>();

            // Register claims transformation
            services.AddScoped<Microsoft.AspNetCore.Authentication.IClaimsTransformation, 
                               Kartverket.Metadatakatalog.Services.Authentication.GeonorgeClaimsTransformation>();

            return services;
        }
    }
}