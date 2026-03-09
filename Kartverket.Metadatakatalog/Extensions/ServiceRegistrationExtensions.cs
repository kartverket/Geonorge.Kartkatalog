using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Kartverket.Metadatakatalog.Service;
using Kartverket.Metadatakatalog.Service.Application;
using Kartverket.Metadatakatalog.Service.Article;
using Kartverket.Metadatakatalog.Service.Search;
using Kartverket.Metadatakatalog.Service.ServiceDirectory;
using GeoNorgeAPI;
using Kartverket.Geonorge.Utilities;
using Kartverket.Geonorge.Utilities.Organization;

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
            // GeoNorgeAPI
            services.AddScoped<IGeoNorge>(provider =>
                new GeoNorge(
                    geonetworkUsername: "",
                    geonetworkPassword: "",
                    geonetworkEndpoint: configuration["GeoNetworkUrl"]));

            // GeoNetworkUtil from Kartverket.Geonorge.Utilities
            services.AddScoped<GeoNetworkUtil>(provider =>
                new GeoNetworkUtil(configuration["GeoNetworkUrl"]));

            // Organization Service
            services.AddScoped<IOrganizationService>(provider =>
                new OrganizationService(
                    configuration["AppSettings:RegistryUrl"],
                    provider.GetRequiredService<IHttpClientFactory>()));

            // URL Resolver
            services.AddScoped<IGeonorgeUrlResolver>(provider =>
                new GeonorgeUrlResolver(configuration["EditorUrl"]));

            return services;
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
            services.AddSingleton<SimpleMetadataUtil>(provider => 
                new SimpleMetadataUtil(configuration));

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