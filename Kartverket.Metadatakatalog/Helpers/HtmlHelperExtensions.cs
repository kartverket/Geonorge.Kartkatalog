using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Models.Translations;
using Microsoft.AspNetCore.WebUtilities;

namespace Kartverket.Metadatakatalog.Helpers
{
    public static class HtmlHelperExtensions
    {
        static readonly SeoFriendly Seo = new SeoFriendly();

        public static string ApplicationVersionNumber(this IHtmlHelper helper)
        {
            var configuration = GetConfiguration(helper);
            string versionNumber = configuration["AppSettings:BuildVersionNumber"];
            return versionNumber ?? "1.0";
        }

        public static string GeonorgeUrl(this IHtmlHelper helper)
        {
            var configuration = GetConfiguration(helper);
            // Use the top-level GeonorgeUrl first, fallback to AppSettings
            var url = configuration["GeonorgeUrl"] ?? configuration["AppSettings:GeonorgeUrl"];
            var culture = CultureHelper.GetCurrentCulture();
            if (culture != Culture.NorwegianCode)
                url = url + Culture.EnglishCode;

            return url;
        }

        public static string GeonorgeArtiklerUrl(this IHtmlHelper helper)
        {
            var configuration = GetConfiguration(helper);
            return configuration["AppSettings:GeonorgeArtiklerUrl"];
        }

        public static string NorgeskartUrl(this IHtmlHelper helper)
        {
            var configuration = GetConfiguration(helper);
            return configuration["AppSettings:NorgeskartUrl"];
        }

        public static string SecureNorgeskartUrl(this IHtmlHelper helper)
        {
            var configuration = GetConfiguration(helper);
            return configuration["AppSettings:SecureNorgeskartUrl"];
        }

        public static string RegistryUrl(this IHtmlHelper helper)
        {
            var configuration = GetConfiguration(helper);
            return configuration["AppSettings:RegistryUrl"];
        }

        public static string ObjektkatalogUrl(this IHtmlHelper helper)
        {
            var configuration = GetConfiguration(helper);
            return configuration["AppSettings:ObjektkatalogUrl"];
        }

        public static bool DownloadServiceEnabled(this IHtmlHelper helper)
        {
            var configuration = GetConfiguration(helper);
            return configuration.GetValue<bool>("AppSettings:DownloadServiceEnabled", true);
        }

        public static string WebmasterEmail(this IHtmlHelper helper)
        {
            var configuration = GetConfiguration(helper);
            return configuration["AppSettings:WebmasterEmail"];
        }

        public static string EnvironmentName(this IHtmlHelper helper)
        {
            var configuration = GetConfiguration(helper);
            return configuration["AppSettings:EnvironmentName"] ?? string.Empty;
        }

        public static string Accessibilitystatementurl(this IHtmlHelper helper)
        {
            var configuration = GetConfiguration(helper);
            // Use the top-level Accessibilitystatementurl first, fallback to AppSettings
            return configuration["Accessibilitystatementurl"] ?? configuration["AppSettings:Accessibilitystatementurl"];
        }

        public static string KartkatalogenUrl(this IHtmlHelper helper)
        {
            var configuration = GetConfiguration(helper);
            // Use the top-level KartkatalogenUrl first, fallback to AppSettings
            return configuration["KartkatalogenUrl"] ?? configuration["AppSettings:KartkatalogenUrl"];
        }

        public static string DownloadUrl(this IHtmlHelper helper)
        {
            var configuration = GetConfiguration(helper);
            // Use the top-level DownloadUrl first, fallback to AppSettings
            return configuration["DownloadUrl"] ?? configuration["AppSettings:DownloadUrl"];
        }

        public static string StatusApiUrl(this IHtmlHelper helper)
        {
            var configuration = GetConfiguration(helper);
            return configuration["AppSettings:StatusApiUrl"];
        }

        public static string SolrServerUrl(this IHtmlHelper helper)
        {
            var configuration = GetConfiguration(helper);
            return configuration["SolrServerUrl"];
        }

        public static bool SupportsMultiCulture(this IHtmlHelper helper)
        {
            var configuration = GetConfiguration(helper);
            return configuration.GetValue<bool>("AppSettings:SupportsMultiCulture", false);
        }

        // Helper method to get IConfiguration from the DI container
        private static IConfiguration GetConfiguration(IHtmlHelper helper)
        {
            return helper.ViewContext.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        }

        public static string RemoveQueryStringByKey(string url, string key)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return url;

            // Use ASP.NET Core's QueryHelpers instead of HttpUtility
            var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
            query.Remove(key);

            var pagePathWithoutQueryString = uri.GetLeftPart(UriPartial.Path);
            var queryString = Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString("", query);

            return queryString.Length > 1
                ? pagePathWithoutQueryString + queryString
                : pagePathWithoutQueryString;
        }

        public static string GetOrganizationNameOrDefault(string organizationName)
        {
            if (organizationName == null)
            {
                return "Etatsvis oversikt over data";
            }
            else
            {
                return organizationName;
            }
        }

        public static string HierarchyLevelLabel(string hierarchyLevelLabelText)
        {
            return "label-" + hierarchyLevelLabelText.Replace(" ", "").ToLower();
        }

        public static string SeoFriendlyString(string text)
        {
            return Seo.MakeSeoFriendlyString(text);
        }

        public static List<Theme> Parents(Theme theme)
        {
            var parentsTheme = new List<Theme>();
            if (theme.Parent != null)
            {
                var parent = theme.Parent;
                while (parent != null)
                {
                    parentsTheme.Add(parent);
                    parent = parent.Parent;
                }
            }
            return parentsTheme;
        }

        /// <summary>
        /// Get configuration value with environment-specific override support
        /// </summary>
        /// <typeparam name="T">Type of the configuration value</typeparam>
        /// <param name="helper">HTML helper instance</param>
        /// <param name="key">Configuration key</param>
        /// <param name="defaultValue">Default value if not found</param>
        /// <returns>Configuration value</returns>
        public static T GetConfigurationValue<T>(this IHtmlHelper helper, string key, T defaultValue = default(T))
        {
            var configuration = GetConfiguration(helper);
            return configuration.GetValue<T>(key, defaultValue);
        }

        /// <summary>
        /// Check if the application is running in production environment
        /// </summary>
        /// <param name="helper">HTML helper instance</param>
        /// <returns>True if production environment</returns>
        public static bool IsProduction(this IHtmlHelper helper)
        {
            var environmentName = helper.EnvironmentName();
            return string.IsNullOrEmpty(environmentName) || environmentName.Equals("production", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Check if the application is running in development environment
        /// </summary>
        /// <param name="helper">HTML helper instance</param>
        /// <returns>True if development environment</returns>
        public static bool IsDevelopment(this IHtmlHelper helper)
        {
            var environmentName = helper.EnvironmentName();
            return environmentName.Equals("dev", StringComparison.OrdinalIgnoreCase) || 
                   environmentName.Equals("development", StringComparison.OrdinalIgnoreCase);
        }
    }
}