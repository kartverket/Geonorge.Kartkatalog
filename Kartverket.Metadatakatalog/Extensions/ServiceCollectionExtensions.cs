using Microsoft.Extensions.DependencyInjection;
using Kartverket.Metadatakatalog.Service;
using Kartverket.Metadatakatalog.Service.Application;
using Kartverket.Metadatakatalog.Service.Article;
using Kartverket.Metadatakatalog.Service.Search;
using Kartverket.Metadatakatalog.Service.ServiceDirectory;

namespace Kartverket.Metadatakatalog.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register services - these may need actual implementations
            // For now, we'll add them as placeholders to resolve compilation errors
            
            // Uncomment when you have the actual service implementations:
            // services.AddScoped<ISearchService, SearchService>();
            // services.AddScoped<ISearchServiceAll, SearchServiceAll>();
            // services.AddScoped<IMetadataService, MetadataService>();
            // services.AddScoped<IApplicationService, ApplicationService>();
            // services.AddScoped<IServiceDirectoryService, ServiceDirectoryService>();
            // services.AddScoped<IArticleService, ArticleService>();
            // services.AddScoped<IAiService, AiService>();
            
            return services;
        }
    }
}