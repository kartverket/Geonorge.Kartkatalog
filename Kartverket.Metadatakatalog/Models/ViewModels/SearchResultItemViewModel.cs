﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;

namespace Kartverket.Metadatakatalog.Models.ViewModels
{
    public class SearchResultItemViewModel
    {
        public string Uuid { get; set; }
        public string Title { get; set; }
        public string @Abstract { get; set; }
        public string Type { get; set; }
        public string Theme { get; set; }
        public string Organization { get; set; }
        public string OrganizationLogoUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public string MaintenanceFrequency { get; set; }
        public string DownloadUrl { get; set; }
        public string ServiceUrl { get; set; }
        public string DistributionProtocol { get; set; }
        public bool IsOpendata { get; set; }
        public string LegendDescriptionUrl { get; set; }
        public string ProductSheetUrl { get; set; }
        public string ProductSpecificationUrl { get; set; }

        public string GetInnholdstypeCSS()
        {
            string t = "label-default";
            if (Type=="dataset") t="label-success";
            else if (Type=="software") t="label-warning";
            else if (Type=="service") t="label-info";
            else if (Type == "servicelayer") t = "label-info";
            else if (Type=="series") t="label-primary";

            return t;
        }

        public string GetInnholdstype()
        {
            string t = Type;
            if (Type=="dataset") t="Datasett";
            else if (Type=="software") t="Applikasjon";
            else if (Type=="service") t="Tjeneste";
            else if (Type == "servicelayer") t = "Tjenestelag";
            else if (Type=="series") t="Datasettserie";

            return t;
        }

        public bool ShowDownloadLink()
        {
            if (!string.IsNullOrWhiteSpace(DistributionProtocol) && DistributionProtocol.Contains("WWW:DOWNLOAD") && (Type == "dataset" || Type == "series") && !string.IsNullOrWhiteSpace(DownloadUrl)) return true;
            else return false;
        }

        public bool ShowDownloadService()
        {
            if (System.Web.Configuration.WebConfigurationManager.AppSettings["DownloadServiceEnabled"] == "true")
            {
                if (DistributionProtocol != null && DistributionProtocol.Contains("GEONORGE:DOWNLOAD"))
                    return true;
            }

            return false;
        }

        public bool ShowMapLink()
        {
            if (!string.IsNullOrWhiteSpace(DistributionProtocol) && (DistributionProtocol.Contains("OGC:WMS") || DistributionProtocol.Contains("OGC:WFS") || DistributionProtocol.Contains("OGC:WCS")) && (Type == "service" || Type == "servicelayer") && !string.IsNullOrWhiteSpace(DownloadUrl)) return true;
            else return false;
        }
        public bool ShowServiceMapLink()
        {
            if  (!string.IsNullOrWhiteSpace(ServiceUrl)) return true;
            else return false;
        }
        public bool ShowWebsiteLink()
        {
            if (!string.IsNullOrWhiteSpace(DistributionProtocol) && !string.IsNullOrWhiteSpace(DownloadUrl) && DistributionProtocol.Contains("WWW:LINK") && Type == "software") return true;
            else return false;
        }

        private SearchResultItemViewModel(SearchResultItem item)
        {
            Uuid = item.Uuid;
            Title = item.Title;
            Abstract = item.Abstract;
            Type = item.Type;
            Theme = item.Theme;
            Organization = item.Organization;
            OrganizationLogoUrl = item.OrganizationLogoUrl;
            ThumbnailUrl = item.ThumbnailUrl;
            MaintenanceFrequency = item.MaintenanceFrequency;
            
            DistributionProtocol = item.DistributionProtocol;
            if (item.NationalInitiative != null && item.NationalInitiative.Contains("Åpne data")) IsOpendata = true;
            if (Type == "dataset")
            {
                if (!string.IsNullOrWhiteSpace(item.ServiceDistributionProtocolForDataset) && item.ServiceDistributionProtocolForDataset.Contains(("OGC:WMS")))
                {
                    if (!string.IsNullOrWhiteSpace(item.ServiceDistributionNameForDataset) && !string.IsNullOrWhiteSpace(item.ServiceDistributionUrlForDataset))
                        ServiceUrl = "#5/355422/6668909/*/l/wms/[" + item.ServiceDistributionUrlForDataset.Replace("request=GetCapabilities&service=WMS", "").Replace("service=WMS&request=GetCapabilities", "").Replace("request=getcapabilities&service=wms", "").Replace("service=wms&request=getcapabilities", "") + "]/+" + item.ServiceDistributionNameForDataset;
                    else if (!string.IsNullOrWhiteSpace(item.ServiceDistributionUrlForDataset))
                        ServiceUrl = "#5/355422/6668909/l/wms/[" + item.ServiceDistributionUrlForDataset.Replace("request=GetCapabilities&service=WMS", "").Replace("service=WMS&request=GetCapabilities", "").Replace("request=getcapabilities&service=wms", "").Replace("service=wms&request=getcapabilities", "") + "]";
                }
                else if (!string.IsNullOrWhiteSpace(item.ServiceDistributionProtocolForDataset) && item.ServiceDistributionProtocolForDataset.Contains(("OGC:WFS")))
                {
                    if (!string.IsNullOrWhiteSpace(item.ServiceDistributionNameForDataset) && !string.IsNullOrWhiteSpace(item.ServiceDistributionUrlForDataset))
                        ServiceUrl = "#5/355422/6668909/*/l/wfs/[" + item.ServiceDistributionUrlForDataset.Replace("request=GetCapabilities&service=WMS", "").Replace("service=WMS&request=GetCapabilities", "").Replace("request=getcapabilities&service=wms", "").Replace("service=wms&request=getcapabilities", "") + "]/+" + item.ServiceDistributionNameForDataset;
                    else if (!string.IsNullOrWhiteSpace(item.DistributionUrl))
                        ServiceUrl = "#5/355422/6668909/l/wfs/[" + item.ServiceDistributionUrlForDataset.Replace("request=GetCapabilities&service=WMS", "").Replace("service=WMS&request=GetCapabilities", "").Replace("request=getcapabilities&service=wms", "").Replace("service=wms&request=getcapabilities", "") + "]";
                }
               
            }

            if (Type == "service" || Type == "servicelayer")
            {
                if (!string.IsNullOrWhiteSpace(item.DistributionProtocol) && item.DistributionProtocol.Contains(("OGC:WMS")))
                {
                    if (!string.IsNullOrWhiteSpace(item.DistributionName) && !string.IsNullOrWhiteSpace(item.DistributionUrl))
                        DownloadUrl = "#5/355422/6668909/*/l/wms/[" + item.DistributionUrl.Replace("request=GetCapabilities&service=WMS", "").Replace("service=WMS&request=GetCapabilities", "").Replace("request=getcapabilities&service=wms", "").Replace("service=wms&request=getcapabilities", "") + "]/+" + item.DistributionName;
                    else if (!string.IsNullOrWhiteSpace(item.DistributionUrl))
                        DownloadUrl =  "#5/355422/6668909/l/wms/[" + item.DistributionUrl.Replace("request=GetCapabilities&service=WMS", "").Replace("service=WMS&request=GetCapabilities", "").Replace("request=getcapabilities&service=wms", "").Replace("service=wms&request=getcapabilities", "") + "]";
                }
                else if (!string.IsNullOrWhiteSpace(item.DistributionProtocol) && item.DistributionProtocol.Contains(("OGC:WFS")))
                {
                    if (!string.IsNullOrWhiteSpace(item.DistributionName) && !string.IsNullOrWhiteSpace(item.DistributionUrl))
                        DownloadUrl = "#5/355422/6668909/*/l/wfs/[" + item.DistributionUrl.Replace("request=GetCapabilities&service=WMS", "").Replace("service=WMS&request=GetCapabilities", "").Replace("request=getcapabilities&service=wms", "").Replace("service=wms&request=getcapabilities", "") + "]/+" + item.DistributionName;
                    else if (!string.IsNullOrWhiteSpace(item.DistributionUrl))
                        DownloadUrl = "#5/355422/6668909/l/wfs/[" + item.DistributionUrl.Replace("request=GetCapabilities&service=WMS", "").Replace("service=WMS&request=GetCapabilities", "").Replace("request=getcapabilities&service=wms", "").Replace("service=wms&request=getcapabilities", "") + "]";
                }
            }
            else DownloadUrl = item.DistributionUrl;

            LegendDescriptionUrl = item.LegendDescriptionUrl;
            ProductSheetUrl = item.ProductSheetUrl;
            ProductSpecificationUrl = item.ProductSpecificationUrl;
        }

        public static List<SearchResultItemViewModel> CreateFromList(IEnumerable<SearchResultItem> items)
        {
            return items.Select(item => new SearchResultItemViewModel(item)).ToList();
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

        public string OrganizationSeoName()
        {
            var seoUrl = new SeoUrl(Organization, Title);
            return seoUrl.Organization;
        }

        

        
    }
}