using Microsoft.Extensions.Configuration;
using System;

namespace Kartverket.Metadatakatalog.Helpers
{
    /// <summary>
    /// Configuration service to provide easy access to configuration values in views
    /// </summary>
    public interface IConfigurationService
    {
        string GetApplicationVersionNumber();
        string GetGeonorgeUrl();
        string GetEnvironmentName();
        string GetStatusApiUrl();
        string GetAccessibilitystatementurl();
        string GetDownloadUrl();
        string GetKartkatalogenUrl();
        bool IsProduction();
        bool IsDevelopment();
        T GetValue<T>(string key, T defaultValue = default(T));
    }

    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfiguration _configuration;

        public ConfigurationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetApplicationVersionNumber()
        {
            return _configuration["AppSettings:BuildVersionNumber"] ?? "1.0";
        }

        public string GetGeonorgeUrl()
        {
            return _configuration["GeonorgeUrl"] ?? _configuration["AppSettings:GeonorgeUrl"];
        }

        public string GetEnvironmentName()
        {
            return _configuration["AppSettings:EnvironmentName"] ?? string.Empty;
        }

        public string GetStatusApiUrl()
        {
            return _configuration["AppSettings:StatusApiUrl"];
        }

        public string GetAccessibilitystatementurl()
        {
            return _configuration["Accessibilitystatementurl"] ?? _configuration["AppSettings:Accessibilitystatementurl"];
        }

        public string GetDownloadUrl()
        {
            return _configuration["DownloadUrl"] ?? _configuration["AppSettings:DownloadUrl"];
        }

        public string GetKartkatalogenUrl()
        {
            return _configuration["KartkatalogenUrl"] ?? _configuration["AppSettings:KartkatalogenUrl"];
        }

        public bool IsProduction()
        {
            var environmentName = GetEnvironmentName();
            return string.IsNullOrEmpty(environmentName) || 
                   environmentName.Equals("production", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsDevelopment()
        {
            var environmentName = GetEnvironmentName();
            return environmentName.Equals("dev", StringComparison.OrdinalIgnoreCase) || 
                   environmentName.Equals("development", StringComparison.OrdinalIgnoreCase);
        }

        public T GetValue<T>(string key, T defaultValue = default(T))
        {
            return _configuration.GetValue<T>(key, defaultValue);
        }
    }
}