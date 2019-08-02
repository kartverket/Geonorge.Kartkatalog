using Kartverket.Metadatakatalog.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Xml;
using Kartverket.Metadatakatalog.Helpers;
using Kartverket.Metadatakatalog.Models.Translations;

namespace Kartverket.Metadatakatalog.Models.Api
{
    public class Metadata
    {
        /// <summary>
        /// The uniqueidentifier
        /// </summary>
        public string Uuid { get; set; }
        /// <summary>
        /// The title
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// The abstract
        /// </summary>
        public string @Abstract { get; set; }
        /// <summary>
        /// The type of metadata
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// The type of metadata translated
        /// </summary>
        public string TypeTranslated { get; set; }
        /// <summary>
        /// The theme
        /// </summary>
        public string Theme { get; set; }
        /// <summary>
        /// The owner of the metadata
        /// </summary>
        public string Organization { get; set; }
        /// <summary>
        /// The logo for the organization
        /// </summary>
        public string OrganizationLogo { get; set; }
        /// <summary>
        /// Illustrative image of the metadata
        /// </summary>
        public string ThumbnailUrl { get; set; }
        /// <summary>
        /// Url for downloading dataset/service
        /// </summary>
        public string DistributionUrl { get; set; }
        /// <summary>
        /// The protocol used for downloading
        /// </summary>
        public string DistributionProtocol { get; set; }
        /// <summary>
        /// Url to metadata details page
        /// </summary>
        public string ShowDetailsUrl { get; set; }
        /// <summary>
        /// Url to metadata owner organization details page
        /// </summary>
        public string OrganizationUrl { get; set; }
        /// <summary>
        /// The layer for services
        /// </summary>
        public string DistributionName { get; set; }
        /// <summary>
        /// True if one of the nationalinitiativs(Samarbeid og lover) is "Åpne data"
        /// </summary>
        public bool? IsOpenData { get; set; }
        public bool? AccessIsOpendata { get; set; }
        public bool AccessIsRestricted { get; set; }
        public bool AccessIsProtected { get; set; }
        /// <summary>
        /// <summary>
        /// True if one of the nationalinitiativs(Samarbeid og lover) is "Det offentlige kartgrunnlaget"
        /// </summary>
        public bool? IsDokData { get; set; }
        /// <summary>
        /// Url for legend/drawing rules
        /// </summary>
        public string LegendDescriptionUrl { get; set; }
        /// <summary>
        /// Url for productsheet
        /// </summary>
        public string ProductSheetUrl { get; set; }
        /// <summary>
        /// Url for detailed spesifications
        /// </summary>
        public string ProductSpecificationUrl { get; set; }
        /// <summary>
        /// Services for dataset
        /// </summary>
        public List<string> DatasetServices { get; set; }
        /// <summary>
        /// Services for dataset with ShowMapLink true
        /// </summary>
        public List<DatasetService> DatasetServicesWithShowMapLink { get; set; }
        /// <summary>
        ///  Datasets for service
        /// </summary>
        public List<string> ServiceDatasets { get; set; }
        /// <summary>
        /// Bundles for dataset
        /// </summary>
        public List<string> Bundles { get; set; }
        /// <summary>
        /// ServiceLayers
        /// </summary>
        public List<string> ServiceLayers { get; set; }
        /// <summary>
        /// AccessConstraint
        /// </summary>
        public string AccessConstraint { get; set; }
        /// <summary>
        /// OtherConstraintsAccess
        /// </summary>
        public string OtherConstraintsAccess { get; set; }
        /// <summary>
        /// DataAccess
        /// </summary>
        public string DataAccess { get; set; }
        /// <summary>
        /// Url for service mapped to dataset (wms)
        /// </summary>
        public string ServiceDistributionUrlForDataset { get; set; }
        /// <summary>
        /// Url for service mapped to dataset (wfs)
        /// </summary>
        public string ServiceWfsDistributionUrlForDataset { get; set; }

        /// <summary>
        /// DistributionType
        /// </summary>
        //public string DistributionType { get; set; } 
        public string DistributionType { get; set; }
        /// <summary>
        /// URL for Get Capabilities
        /// </summary>
        public string GetCapabilitiesUrl { get; set; }

        /// <summary>
        /// The full dataset name
        /// </summary>
        public string DatasetName { get; set; }

        public DateTime? Date { get; set; }
        private string MapUrl { get; set; }
        public bool? ShowMapLink { get; set; }
        private bool ShowServiceMapLink { get; set; }
        private string DownloadUrl { get; set; }
        private string ServiceUrl { get; set; }


        public Metadata() { 
        }
        public Metadata(SearchResultItem item, UrlHelper urlHelper)
        {
            Uuid = item.Uuid;
            Title = item.Title;
            Abstract = item.Abstract;
            Type = item.Type;
            TypeTranslated = TranslateType();
            Theme = item.Theme;
            Organization = item.Organization;
            OrganizationLogo = item.OrganizationLogoUrl;
            ThumbnailUrl = item.ThumbnailUrl;
            DistributionUrl = item.DistributionUrl;
            DistributionProtocol = item.DistributionProtocol;
            DistributionName = item.DistributionName;
            DistributionType = item.DistributionType;
            GetCapabilitiesUrl = GetGetCapabilitiesUrl(item);
            if (urlHelper != null)
            {
                ShowDetailsUrl = WebConfigurationManager.AppSettings["KartkatalogenUrl"] + "metadata/uuid/" + item.Uuid;
                string s = new SeoUrl(item.Organization, "").Organization;
                OrganizationUrl = WebConfigurationManager.AppSettings["KartkatalogenUrl"] + "metadata/" + s;
            }

            if (item.NationalInitiative != null && item.NationalInitiative.Contains("Åpne data") || item.DataAccess == "Åpne data")
                IsOpenData = true;
            else IsOpenData = false;

            AccessIsOpendata = IsOpenData;
            AccessIsRestricted = SimpleMetadataUtil.IsRestricted(item.OtherConstraintsAccess);
            AccessIsProtected = SimpleMetadataUtil.IsProtected(item.AccessConstraint); 

            if (item.NationalInitiative != null && item.NationalInitiative.Contains("Det offentlige kartgrunnlaget"))
                IsDokData = true;
            else IsDokData = false;


            LegendDescriptionUrl = item.LegendDescriptionUrl;
            ProductSheetUrl = item.ProductSheetUrl;
            ProductSpecificationUrl = item.ProductSpecificationUrl;
            DatasetServices = item.DatasetServices;
            DatasetServicesWithShowMapLink = AddDatasetServicesWithShowMapLink(DatasetServices);
            ServiceDatasets = item.ServiceDatasets;
            Bundles = item.Bundles;
            ServiceLayers = item.ServiceLayers;
            AccessConstraint = item.AccessConstraint;
            OtherConstraintsAccess = item.OtherConstraintsAccess;
            DataAccess = item.DataAccess;
            ServiceDistributionUrlForDataset = item.ServiceDistributionUrlForDataset;
            ServiceWfsDistributionUrlForDataset = item.ServiceWfsDistributionUrlForDataset != null ? item.ServiceWfsDistributionUrlForDataset : WfsServiceUrl();
            Date = item.Date;
            DatasetName = item.DatasetName;

            if (Type == "dataset")
            {
                if (!string.IsNullOrWhiteSpace(item.ServiceDistributionProtocolForDataset))
                {
                    string commonPart = SimpleMetadataUtil.GetCommonPartOfNorgeskartUrl(item.ServiceDistributionProtocolForDataset, true);

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
                    string commonPart = $"{SimpleMetadataUtil.GetCommonPartOfNorgeskartUrl(item.DistributionProtocol, true)}{RemoveQueryString(item.DistributionUrl)}";

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
            else
            {
                DownloadUrl = item.DistributionUrl;
                if (!DownloadLink())
                    DownloadUrl = MakeDownloadUrlRelative();
            }

            MapUrl = GetMapUrl();
        }

        private List<DatasetService> AddDatasetServicesWithShowMapLink(List<string> datasetServicesString)
        {
            var datasetServices = new List<DatasetService>();
            if (datasetServicesString != null)
            {
                foreach (var serviceString in datasetServicesString)
                {
                    var serviceArray = serviceString.Split('|');
                    try
                    {
                        if (ShowMaplink(serviceArray[6]) && IsServiceOrServiceLayer(serviceArray[3]) && serviceArray[6] != null)
                        {
                            datasetServices.Add(new DatasetService
                            {
                                Uuid = serviceArray[0],
                                DistributionProtocol = serviceArray[6],
                                GetCapabilitiesUrl = serviceArray[7],
                                Title = serviceArray[1]
                            });
                        }
                    }
                    catch (Exception e)
                    {
                        
                    }
                }
            }

            return datasetServices;
        }

        private bool IsServiceOrServiceLayer(string type)
        {
            return type == "service" || type == "servicelayer";
        }

        public string TranslateType()
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

        private string MakeDownloadUrlRelative()
        {
            if (!string.IsNullOrWhiteSpace(DownloadUrl))
            {
                if (Uri.IsWellFormedUriString(DownloadUrl, UriKind.Absolute))
                {
                    Uri downloadUrl = new Uri(DownloadUrl);
                    return "//" + downloadUrl.Host + downloadUrl.PathAndQuery;
                }
            }
            return null;
        }
        public bool DownloadLink()
        {
            if (!string.IsNullOrWhiteSpace(DistributionProtocol) && (DistributionProtocol.Contains("WWW:DOWNLOAD") || DistributionProtocol.Contains("GEONORGE:FILEDOWNLOAD")) && (Type == "dataset" || Type == "series") && !string.IsNullOrWhiteSpace(DownloadUrl)) return true;
            else return false;
        }
        private string GetGetCapabilitiesUrl(SearchResultItem item)
        {
            DistributionDetails distributionDetails = new DistributionDetails(null, item.DistributionProtocol, null, item.DistributionUrl);
            return distributionDetails.DistributionDetailsGetCapabilitiesUrl();
        }
        private string GetMapUrl()
        {
            if (ShowMaplink())
            {
                ShowMapLink = true;
                return SimpleMetadataUtil.NorgeskartUrl + DownloadUrl;
            }
            if (ShowServiceMaplink())
            {
                ShowServiceMapLink = true;
                return SimpleMetadataUtil.NorgeskartUrl + ServiceUrl;
            }
            return "";
        }
        public bool ShowMaplink()
        {
            if (!string.IsNullOrWhiteSpace(DistributionProtocol) && (DistributionProtocol.Contains("OGC:WMS") || DistributionProtocol.Contains("OGC:WFS") || DistributionProtocol.Contains("OGC:WCS")) && (Type == "service" || Type == "servicelayer") && !string.IsNullOrWhiteSpace(DownloadUrl) || DatasetServicesWithShowMapLink.Any()) return true;
            else return false;
        }

        public bool ShowMaplink(string protocol)
        {
            if (!string.IsNullOrWhiteSpace(protocol) && (protocol.Contains("OGC:WMS") || protocol.Contains("OGC:WFS") || protocol.Contains("OGC:WCS")))
            {
                return true;
            };
            return false;
        }

        string RemoveQueryString(string URL)
        {
            int startQueryString = URL.IndexOf("?");

            if (startQueryString != -1)
                URL = URL.Substring(0, startQueryString);

            return URL;
        }

        public bool ShowServiceMaplink()
        {
            if (!string.IsNullOrWhiteSpace(ServiceUrl)) return true;
            else return false;
        }

        public static List<Metadata> CreateFromList(IEnumerable<SearchResultItem> items, UrlHelper urlHelper)
        {
            return items.Select(item => new Metadata(item, urlHelper)).ToList();
        }

        public string WfsServiceUrl()
        {
            string wfsServiceUrl = "";

            if(DatasetServices != null)
            { 
                foreach (var service in DatasetServices)
                {
                    var data = service.Split('|');
                    if (data.Count() >= 7 && data[6].ToString() == "OGC:WFS")
                        wfsServiceUrl = data[7].ToString();
                }
            }

            return wfsServiceUrl;
        }

        private System.Xml.XmlDocument AtomFeedDoc;
        XmlNamespaceManager nsmgr;

        public string AtomFeed()
        {
            string atomFeed = "";
            MemoryCache memoryCache = MemoryCache.Default;
            AtomFeedDoc = memoryCache.Get("AtomFeedDoc") as System.Xml.XmlDocument;
            if (AtomFeedDoc == null)
                SetAtomFeed();

            atomFeed = GetAtomFeed();

            return atomFeed;
        }

        private string GetAtomFeed()
        {
            nsmgr = new XmlNamespaceManager(AtomFeedDoc.NameTable);
            nsmgr.AddNamespace("ns", "http://www.w3.org/2005/Atom");
            nsmgr.AddNamespace("georss", "http://www.georss.org/georss");
            nsmgr.AddNamespace("inspire_dls", "http://inspire.ec.europa.eu/schemas/inspire_dls/1.0");

            string feed = "";
            XmlNode entry = AtomFeedDoc.SelectSingleNode("//ns:feed/ns:entry[inspire_dls:spatial_dataset_identifier_code='" + Uuid + "']/ns:link", nsmgr);
            if (entry != null) {
                feed = entry.InnerText;
            }

            return feed;
        }

        private void SetAtomFeed()
        {
            AtomFeedDoc = new XmlDocument();
            AtomFeedDoc.Load("https://nedlasting.geonorge.no/geonorge/Tjenestefeed.xml");

            MemoryCache memoryCache = MemoryCache.Default;
            memoryCache.Add("AtomFeedDoc", AtomFeedDoc, new DateTimeOffset(DateTime.Now.AddDays(1)));
        }
    }

    public class DatasetService
    {
        public string Uuid { get; set; }
        public string Title { get; set; }
        public string DistributionProtocol { get; set; }
        public string GetCapabilitiesUrl { get; set; }
    }
}