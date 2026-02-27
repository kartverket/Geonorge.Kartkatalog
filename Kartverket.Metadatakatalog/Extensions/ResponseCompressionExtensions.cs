using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Kartverket.Metadatakatalog.Extensions
{
    public static class ResponseCompressionExtensions
    {
        public static IServiceCollection AddCustomResponseCompression(this IServiceCollection services, IConfiguration configuration)
        {
            bool compressionEnabled = configuration.GetValue<bool>("AppSettings:CompressionFilterEnabled", false);

            if (compressionEnabled)
            {
                // ResponseCompression functionality is built into .NET 10
                // For now, we'll skip this feature during migration
                // TODO: Implement compression configuration if needed
            }

            return services;
        }
    }
}