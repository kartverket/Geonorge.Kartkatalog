using System.Collections.Generic;
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
        public bool IsRestricted { get; set; }
        public bool IsOffline { get; set; }
        public string LegendDescriptionUrl { get; set; }
        public string ProductSheetUrl { get; set; }
        public string ProductSpecificationUrl { get; set; }
        public string ServiceUuid { get; set; }

        public string GetInnholdstypeCSS()
        {
            string t = "label-default";
            if (Type=="dataset") t="label-datasett";
            else if (Type=="software") t="label-applikasjon";
            else if (Type=="service") t="label-tjeneste";
            else if (Type == "servicelayer") t = "label-tjenestelag";
            else if (Type=="series") t="label-datasettserie";
            else if (Type == "dimensionGroup") t = "label-datasett";

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
            else if (Type == "dimensionGroup") t = "Datapakke";

            return t;
        }

        public bool ShowDownloadLink()
        {
            if (!string.IsNullOrWhiteSpace(DistributionProtocol) && (DistributionProtocol.Contains("WWW:DOWNLOAD") || DistributionProtocol.Contains("GEONORGE:FILEDOWNLOAD")) && (Type == "dataset" || Type == "series") && !string.IsNullOrWhiteSpace(DownloadUrl)) return true;
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
            if (!string.IsNullOrEmpty(item.OtherConstraintsAccess) && item.OtherConstraintsAccess.ToLower() == "no restrictions") IsOpendata = true;
            if (!string.IsNullOrEmpty(item.OtherConstraintsAccess) && item.OtherConstraintsAccess.ToLower() == "norway digital restricted") IsRestricted = true;
            if (item.AccessConstraint == "restricted") IsOffline = true; 


            if (Type == "dataset")
            {
                if (!string.IsNullOrWhiteSpace(item.ServiceDistributionProtocolForDataset) && item.ServiceDistributionProtocolForDataset.Contains(("OGC:WMS")))
                {
                    if (!string.IsNullOrWhiteSpace(item.ServiceDistributionNameForDataset) && !string.IsNullOrWhiteSpace(item.ServiceDistributionUrlForDataset))
                        ServiceUrl = "#5/355422/6668909/*/l/wms/[" + RemoveQueryString(item.ServiceDistributionUrlForDataset) + "]/+" + item.ServiceDistributionNameForDataset;
                    else if (!string.IsNullOrWhiteSpace(item.ServiceDistributionUrlForDataset))
                        ServiceUrl = "#5/355422/6668909/l/wms/[" + RemoveQueryString(item.ServiceDistributionUrlForDataset) + "]";
                }
                else if (!string.IsNullOrWhiteSpace(item.ServiceDistributionProtocolForDataset) && item.ServiceDistributionProtocolForDataset.Contains(("OGC:WFS")))
                {
                    if (!string.IsNullOrWhiteSpace(item.ServiceDistributionNameForDataset) && !string.IsNullOrWhiteSpace(item.ServiceDistributionUrlForDataset))
                        ServiceUrl = "#5/355422/6668909/*/l/wfs/[" + RemoveQueryString(item.ServiceDistributionUrlForDataset) + "]/+" + item.ServiceDistributionNameForDataset;
                    else if (!string.IsNullOrWhiteSpace(item.DistributionUrl))
                        ServiceUrl = "#5/355422/6668909/l/wfs/[" + RemoveQueryString(item.ServiceDistributionUrlForDataset) + "]";
                }
               
            }

            if (Type == "service" || Type == "servicelayer")
            {
                if (!string.IsNullOrWhiteSpace(item.DistributionProtocol) && item.DistributionProtocol.Contains(("OGC:WMS")))
                {
                    if (!string.IsNullOrWhiteSpace(item.DistributionName) && !string.IsNullOrWhiteSpace(item.DistributionUrl))
                        DownloadUrl = "#5/355422/6668909/*/l/wms/[" + RemoveQueryString(item.DistributionUrl) + "]/+" + item.DistributionName;
                    else if (!string.IsNullOrWhiteSpace(item.DistributionUrl))
                        DownloadUrl =  "#5/355422/6668909/l/wms/[" + RemoveQueryString(item.DistributionUrl) + "]";
                }
                else if (!string.IsNullOrWhiteSpace(item.DistributionProtocol) && item.DistributionProtocol.Contains(("OGC:WFS")))
                {
                    if (!string.IsNullOrWhiteSpace(item.DistributionName) && !string.IsNullOrWhiteSpace(item.DistributionUrl))
                        DownloadUrl = "#5/355422/6668909/*/l/wfs/[" + RemoveQueryString(item.DistributionUrl) + "]/+" + item.DistributionName;
                    else if (!string.IsNullOrWhiteSpace(item.DistributionUrl))
                        DownloadUrl = "#5/355422/6668909/l/wfs/[" + RemoveQueryString(item.DistributionUrl) + "]";
                }
            }
            else DownloadUrl = item.DistributionUrl;

            LegendDescriptionUrl = item.LegendDescriptionUrl;
            ProductSheetUrl = item.ProductSheetUrl;
            ProductSpecificationUrl = item.ProductSpecificationUrl;
            ServiceUuid = item.Uuid;
            if (item.Type == "dataset")
                ServiceUuid = item.ServiceDistributionUuidForDataset;
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

        string RemoveQueryString(string URL)
        {
            int startQueryString = URL.IndexOf("?");

            if (startQueryString != -1)
                URL = URL.Substring(0, startQueryString);

            return URL;
        }


    }
}