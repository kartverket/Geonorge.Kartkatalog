using Kartverket.Metadatakatalog.Helpers;
using Kartverket.Metadatakatalog.Models.Translations;
using System;
using System.Collections.Generic;
using System.Linq;
using Kartverket.Metadatakatalog.Service;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using System.Web;

namespace Kartverket.Metadatakatalog.Models.ViewModels
{
    public class SearchResultItemViewModel
    {
        private static IConfiguration _configuration;
        
        public static void SetConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string Uuid { get; set; }
        public string Title { get; set; }
        public string @Abstract { get; set; }
        public string Type { get; set; }
        public string TypeName { get; set; }
        public string Theme { get; set; }
        public string Organization { get; set; }
        public string OrganizationShortName { get; set; }
        public string OrganizationLogoUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public string MaintenanceFrequency { get; set; }
        public string DownloadUrl { get; set; }
        public string ServiceUrl { get; set; }
        public string DistributionProtocol { get; set; }
        public string DistributionType { get; set; }
        public bool IsOpendata { get; set; }
        public bool IsRestricted { get; set; }
        public bool IsOffline { get; set; }
        public string LegendDescriptionUrl { get; set; }
        public string ProductSheetUrl { get; set; }
        public string ProductSpecificationUrl { get; set; }
        public string ServiceUuid { get; set; }
        public string ServiceDistributionAccessConstraint { get; set; }
        public string DistributionUrl { get; set; }
        public string GetCapabilitiesUrl { get; set; }
        public string TypeCssClass { get; set; }
        public string TypeTranslated { get; set; }
        public RouteValueDictionary MetadataLinkRouteValueDictionary { get; set; }
        public string OrganizationSeoName { get; set; }
        public string TitleSeo { get; set; }
        public string MapTitleTag { get; set; }
        public bool ShowDownloadService { get; set; }
        public bool ShowDownloadLink { get; set; }
        public string AddToCartUrl { get; set; }
        public string MapUrl { get; set; }
        public bool ShowMapLink { get; set; }
        public bool ShowServiceMapLink { get; set; }



        public string GetInnholdstypeCSS()
        {
            string t = "label-default";
            if (Type == "dataset") t = "label-datasett";
            else if (Type == "software") t = "label-applikasjon";
            else if (Type == "service") t = "label-tjeneste";
            else if (Type == "servicelayer") t = "label-tjenestelag";
            else if (Type == "series") t = "label-datasettserie";
            else if (Type == "dimensionGroup") t = "label-datasett";

            return t;
        }

        public string GetInnholdstype()
        {
            string t = Type;

            if (CultureHelper.GetCurrentCulture() == Culture.NorwegianCode)
            {
                if (Type == "dataset") t = "Datasett";
                else if (Type == "software") t = "Applikasjon";
                else if (Type == "service") t = "Tjeneste";
                else if (Type == "servicelayer") t = "Tjenestelag";
                else if (Type == "series") t = "Datasettserie";
                else if (Type == "dimensionGroup") t = "Datapakke";
            }
            else
            {
                if (Type == "dataset") t = "Dataset";
                else if (Type == "software") t = "Application";
                else if (Type == "service") t = "Service";
                else if (Type == "servicelayer") t = "Service layer";
                else if (Type == "series") t = "Dataset series";
                else if (Type == "dimensionGroup") t = "Data package";
            }

            return t;
        }

        public bool DownloadLink()
        {
            if (!string.IsNullOrWhiteSpace(DistributionProtocol) && (DistributionProtocol.Contains("WWW:DOWNLOAD") || DistributionProtocol.Contains("GEONORGE:FILEDOWNLOAD")) && (Type == "dataset" || Type == "series") && !string.IsNullOrWhiteSpace(DownloadUrl)) return true;
            else return false;
        }

        public bool DownloadService()
        {
            if (_configuration != null && Convert.ToBoolean(_configuration["DownloadServiceEnabled"] ?? "false"))
            {
                if (DistributionProtocol != null && DistributionProtocol.Contains("GEONORGE:DOWNLOAD"))
                    return true;
            }

            return false;
        }

        public bool ShowMaplink()
        {
            if (!string.IsNullOrWhiteSpace(DistributionProtocol) && (DistributionProtocol.Contains("OGC:WMS") || DistributionProtocol.Contains("OGC:WFS") || DistributionProtocol.Contains("OGC:WCS")) && (Type == "service" || Type == "servicelayer") && !string.IsNullOrWhiteSpace(DownloadUrl)) return true;
            else return false;
        }
        public bool ShowServiceMaplink()
        {
            if (!string.IsNullOrWhiteSpace(ServiceUrl)) return true;
            else return false;
        }
        public bool ShowWebsiteLink()
        {
            if (!string.IsNullOrWhiteSpace(DistributionProtocol) && !string.IsNullOrWhiteSpace(DownloadUrl) && DistributionProtocol.Contains("WWW:LINK") && Type == "software") return true;
            else return false;
        }

        public SearchResultItemViewModel(SearchResultItem item, ISimpleMetadataUtil simpleMetadataUtil)
        {
            Uuid = item.Uuid;
            Title = item.Title;
            Abstract = item.Abstract;
            Type = item.Type;
            Theme = item.Theme;
            Organization = item.Organization;
            OrganizationShortName = !string.IsNullOrEmpty(item.OrganizationShortName) ? item.OrganizationShortName : item.Organization;
            OrganizationLogoUrl = GetOrganizationLogoUrl(item.OrganizationLogoUrl);
            ThumbnailUrl = item.ThumbnailUrl;
            MaintenanceFrequency = item.MaintenanceFrequency;
            DistributionType = item.DistributionType;
            DistributionUrl = item.DistributionUrl;
            GetCapabilitiesUrl = item.DistributionDetails.DistributionDetailsGetCapabilitiesUrl();
            TypeCssClass = GetInnholdstypeCSS();
            TypeTranslated = GetInnholdstype();
            DistributionProtocol = item.DistributionProtocol;
            var seoUrl = new SeoUrl(Organization, Title);
            OrganizationSeoName = seoUrl.Organization;
            TitleSeo = seoUrl.Title;

            if (!string.IsNullOrEmpty(item.OtherConstraintsAccess) && item.OtherConstraintsAccess.ToLower() == "no restrictions") IsOpendata = true;
            if (!string.IsNullOrEmpty(item.OtherConstraintsAccess) && item.OtherConstraintsAccess.ToLower() == "norway digital restricted") IsRestricted = true;
            if (item.AccessConstraint == "restricted") IsOffline = true;

            if (Type == "dataset")
            {
                if (!string.IsNullOrWhiteSpace(item.ServiceDistributionProtocolForDataset))
                {
                    string commonPart = simpleMetadataUtil.GetCommonPartOfNorgeskartUrl(item.ServiceDistributionProtocolForDataset, true);

                    if (item.ServiceDistributionProtocolForDataset.Contains(SimpleMetadataUtil.OgcWms))
                    {
                        if (!string.IsNullOrWhiteSpace(item.ServiceDistributionUrlForDataset))
                        {
                            ServiceUrl = $"{commonPart}{RemoveQueryString(item.ServiceDistributionUrlForDataset)}";

                            if (!string.IsNullOrWhiteSpace(item.ServiceDistributionNameForDataset))
                            {
                                ServiceUrl += $"&addLayers={item.ServiceDistributionNameForDataset}";
                            }
                        }
                    }
                    else if (item.ServiceDistributionProtocolForDataset.Contains(SimpleMetadataUtil.OgcWfs))
                    {
                        if (!string.IsNullOrWhiteSpace(item.ServiceDistributionNameForDataset) && !string.IsNullOrWhiteSpace(item.ServiceDistributionUrlForDataset))
                        {
                            ServiceUrl = $"{commonPart}{RemoveQueryString(item.ServiceDistributionUrlForDataset)}&addLayers={item.ServiceDistributionNameForDataset}";
                        }
                        else if (!string.IsNullOrWhiteSpace(item.DistributionUrl))
                        {
                            ServiceUrl = $"{commonPart}{RemoveQueryString(item.DistributionUrl)}";
                        }
                    }
                }
            }

            if (Type == "service" || Type == "servicelayer")
            {
                if (!string.IsNullOrWhiteSpace(item.DistributionProtocol) && !string.IsNullOrWhiteSpace(item.DistributionUrl))
                {
                    string commonPart = $"{simpleMetadataUtil.GetCommonPartOfNorgeskartUrl(item.DistributionProtocol, true)}{RemoveQueryString(item.DistributionUrl)}";

                    if (item.DistributionProtocol.Contains(SimpleMetadataUtil.OgcWms) || item.DistributionProtocol.Contains(SimpleMetadataUtil.OgcWfs))
                    {
                        DownloadUrl = commonPart;

                        if (!string.IsNullOrWhiteSpace(item.DistributionName))
                        {
                            DownloadUrl += $"&addLayers={item.DistributionName}";
                        }
                    }
                }
            }
            else {
                DownloadUrl = item.DistributionUrl;
                if (!DownloadLink())
                    DownloadUrl = MakeDownloadUrlRelative();
            }

            LegendDescriptionUrl = item.LegendDescriptionUrl;
            ProductSheetUrl = item.ProductSheetUrl;
            ProductSpecificationUrl = item.ProductSpecificationUrl;
            ServiceUuid = item.Uuid;
            if (item.Type == "dataset")
                ServiceUuid = item.ServiceDistributionUuidForDataset;

            if (!string.IsNullOrEmpty(item.ParentIdentifier) && Type == "servicelayer")
                ServiceUuid = item.ParentIdentifier;

            ServiceDistributionAccessConstraint = item.ServiceDistributionAccessConstraint;
            MetadataLinkRouteValueDictionary = ShowMetadataLinkRouteValueDictionary();
            MapTitleTag = GetMapTitleTag();
            ShowDownloadService = DownloadService();
            ShowDownloadLink = DownloadLink();
            AddToCartUrl = GetAddToCartUrl();
            MapUrl = GetMapUrl(simpleMetadataUtil);
        }

        private string GetMapUrl(ISimpleMetadataUtil simpleMetadataUtil)
        {
            if (ShowMaplink())
            {
                ShowMapLink = true;
                return simpleMetadataUtil.NorgeskartUrl + DownloadUrl;
            }
            if (ShowServiceMaplink())
            {
                ShowServiceMapLink = true; 
                return simpleMetadataUtil.NorgeskartUrl + ServiceUrl;
            }
            return "";
        }


        private string GetAddToCartUrl()
        {
            string addToCartUrl = "";
            if (DownloadService())
            {
                if (IsRestricted || IsOffline)
                {
                    string addToCartEventParamater = "?addtocart_event_id=addToCart-" + Uuid;
                    // Note: In ASP.NET Core, this would need to be passed from the controller/page
                    // For now, we'll use the simpler parameter
                    
                    if (_configuration?["BaseUrl"]?.Contains("?") == true)
                    {
                        addToCartEventParamater = "&addtocart_event_id=addToCart-" + Uuid;
                    }

                    string downloadSignInUrl = _configuration?["DownloadUrl"]
                                             + "AuthServices/SignIn?ReturnUrl="
                                             + _configuration?["BaseUrl"] // Simplified for ASP.NET Core
                                             + addToCartEventParamater;

                    addToCartUrl = _configuration?["KartkatalogenUrl"]
                                 + "AuthServices/SignIn?ReturnUrl="
                                 + downloadSignInUrl;
                }
            }
            return addToCartUrl;
        }

        private string MakeDownloadUrlRelative()
        {
            if (!string.IsNullOrWhiteSpace(DownloadUrl))
            {
                if (Uri.IsWellFormedUriString(DownloadUrl, UriKind.Absolute)){ 
                    Uri downloadUrl = new Uri(DownloadUrl);
                    return "//" + downloadUrl.Host + downloadUrl.PathAndQuery;
                }
            }
            return null;
        }

        private string GetOrganizationLogoUrl(string organizationLogoUrl)
        {
            if (!string.IsNullOrWhiteSpace(organizationLogoUrl))
            {
                Uri uri = new Uri(organizationLogoUrl);
                string relativeOrganizationLogoUrl = "//" + uri.Host + uri.PathAndQuery;
                return relativeOrganizationLogoUrl;
            }
            return OrganizationLogoUrl;

        }

        public static List<SearchResultItemViewModel> CreateFromList(IEnumerable<SearchResultItem> items, ISimpleMetadataUtil simpleMetadataUtil)
        {
            return items.Select(item => new SearchResultItemViewModel(item, simpleMetadataUtil)).ToList();
        }

        public RouteValueDictionary ShowMetadataLinkRouteValueDictionary()
        {
            var routeValueDictionary = new RouteValueDictionary();

            var seoUrl = new SeoUrl(Organization, Title);

            routeValueDictionary["uuid"] = Uuid;
            routeValueDictionary["organization"] = seoUrl.Organization;
            routeValueDictionary["title"] = seoUrl.Title;

            return routeValueDictionary;
        }

        public string GetOrganizationSeoName()
        {
            var seoUrl = new SeoUrl(Organization, Title);
            return seoUrl.Organization;
        }

        public bool IsRestrictedService()
        {
            return ServiceDistributionAccessConstraint == "Beskyttet" || ServiceDistributionAccessConstraint == "restricted" || ServiceDistributionAccessConstraint == "norway digital restricted";
        }

        public string GetMapTitleTag()
        {
            return IsRestrictedService() ? "Tjenesten krever spesiell tilgang for å kunne vises - kontakt dataeier" : "";
        }

        string RemoveQueryString(string URL)
        {
            int startQueryString = URL.IndexOf("?");

            if (startQueryString != -1)
                URL = URL.Substring(0, startQueryString);

            return URL;
        }
    }
}