using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Models.Translations;

namespace Kartverket.Metadatakatalog.Helpers
{
    public static class HtmlHelperExtensions
    {
        static readonly SeoFriendly Seo = new SeoFriendly();

        public static string ApplicationVersionNumber(this HtmlHelper helper)
        {
            string versionNumber = WebConfigurationManager.AppSettings["BuildVersionNumber"];
            return versionNumber;
        }
        public static string GeonorgeUrl(this HtmlHelper helper)
        {
            var url = WebConfigurationManager.AppSettings["GeonorgeUrl"];
            var culture = CultureHelper.GetCurrentCulture();
            if (culture != Culture.NorwegianCode)
                url = url + Culture.EnglishCode;

            return url;
        }
        public static string GeonorgeArtiklerUrl(this HtmlHelper helper)
        {
            return WebConfigurationManager.AppSettings["GeonorgeArtiklerUrl"];
        }
        public static string NorgeskartUrl(this HtmlHelper helper)
        {
            return WebConfigurationManager.AppSettings["NorgeskartUrl"];
        }
        public static string SecureNorgeskartUrl(this HtmlHelper helper)
        {
            return WebConfigurationManager.AppSettings["SecureNorgeskartUrl"];
        }
        public static string RegistryUrl(this HtmlHelper helper)
        {
            return WebConfigurationManager.AppSettings["RegistryUrl"];
        }
        public static string ObjektkatalogUrl(this HtmlHelper helper)
        {
            return WebConfigurationManager.AppSettings["ObjektkatalogUrl"];
        }
        public static bool DownloadServiceEnabled(this HtmlHelper helper)
        {
            return WebConfigurationManager.AppSettings["DownloadServiceEnabled"] == "false" ? false : true;
        }
        public static string WebmasterEmail(this HtmlHelper helper)
        {
            return WebConfigurationManager.AppSettings["WebmasterEmail"];
        }
        public static string EnvironmentName(this HtmlHelper helper)
        {
            return WebConfigurationManager.AppSettings["EnvironmentName"];
        }

        public static string Accessibilitystatementurl(this HtmlHelper helper)
        {
            return WebConfigurationManager.AppSettings["Accessibilitystatementurl"];
        }

        public static string KartkatalogenUrl(this HtmlHelper helper)
        {
            return WebConfigurationManager.AppSettings["KartkatalogenUrl"];
        }

        public static string DownloadUrl(this HtmlHelper helper)
        {
            return WebConfigurationManager.AppSettings["DownloadUrl"];
        }

        public static string StatusApiUrl(this HtmlHelper helper)
        {
            return WebConfigurationManager.AppSettings["StatusApiUrl"];
        }

        public static bool SupportsMultiCulture(this HtmlHelper helper)
        {
            return Boolean.Parse(WebConfigurationManager.AppSettings["SupportsMultiCulture"]); ;
        }

        public static string RemoveQueryStringByKey(string url, string key)
        {
            var uri = new Uri(url);

            // this gets all the query string key value pairs as a collection
            var newQueryString = HttpUtility.ParseQueryString(uri.Query);

            // this removes the key if exists
            newQueryString.Remove(key);

            // this gets the page path from root without QueryString
            string pagePathWithoutQueryString = uri.GetLeftPart(UriPartial.Path);

            return newQueryString.Count > 0
                ? String.Format("{0}?{1}", pagePathWithoutQueryString, newQueryString)
                : pagePathWithoutQueryString;
        }

	public static string GetOrganizationNameOrDefault(string organizationName){
            if (organizationName == null)
            {
                return "Etatsvis oversikt over data";
            }
            else {
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
    }
}
