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
using Resources;

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
        /// The grouping name of dataset series
        /// </summary>
        public string TypeName { get; set; }
        /// <summary>
        /// The theme
        /// </summary>
        public string Theme { get; set; }
        /// <summary>
        /// The owner of the metadata
        /// </summary>
        public string Organization { get; set; }
        /// <summary>
        /// The list of owners of the metadata
        /// </summary>
        public List<string> Organizations { get; set; }
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
        /// Distributions for dataset
        /// </summary>
        public List<string> Distributions { get; set; }
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
        /// Datasets in series
        /// </summary>
        public List<Dataset> SerieDatasets { get; set; }
        /// <summary>
        /// Serie for datasett
        /// </summary>
        public Serie Serie { get; set; }
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
        public string ServiceUuid { get; set; }

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
        public string SpatialScope { get; set; }


        public Metadata() { 
        }
        public Metadata(SearchResultItem item, UrlHelper urlHelper)
        {
            Uuid = item.Uuid;
            Title = item.Title;
            Abstract = item.Abstract;
            Type = item.Type;
            TypeName = item.TypeName;
            TypeTranslated = TranslateType();
            Theme = item.Theme;
            Organization = item.Organization;
            Organizations = item.Organizations;

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
                OrganizationUrl = WebConfigurationManager.AppSettings["KartkatalogenUrl"] + "?organizations=" + item?.Organization;
            }

            if (item.DataAccess != null && item.DataAccess == "Åpne data" && UI.OpenData == "Open data")
                item.DataAccess = "Open data";

            if (item.NationalInitiative != null && item.NationalInitiative.Contains(UI.OpenData)
                || (!string.IsNullOrEmpty(item.DataAccess) && (item.DataAccess == UI.OpenData
                || item.DataAccess.Contains("ingen begrensninger") || item.DataAccess.Contains("no limitations"))))
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
            Distributions = item.Distributions;
            Bundles = item.Bundles;
            ServiceLayers = item.ServiceLayers;
            AccessConstraint = item.AccessConstraint;
            OtherConstraintsAccess = item.OtherConstraintsAccess;
            DataAccess = item.DataAccess;
            ServiceDistributionUrlForDataset = item.ServiceDistributionUrlForDataset;
            ServiceUuid = item.ServiceDistributionUuidForDataset;
            ServiceWfsDistributionUrlForDataset = item.ServiceWfsDistributionUrlForDataset != null ? item.ServiceWfsDistributionUrlForDataset : WfsServiceUrl();
            Date = item.Date;
            DatasetName = item.DatasetName;

            if (Type == "dataset" || Type == "series")
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

                if(item.Serie != null)
                {
                    Serie = AddSerie(item.Serie);
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

                if (string.IsNullOrEmpty(ServiceUuid) && string.IsNullOrWhiteSpace(item.DistributionName))
                    ServiceUuid = Uuid;
            }
             
            if(Type == "series" && item.SerieDatasets != null && item.SerieDatasets.Count > 0)
            {
                SerieDatasets = AddSerieDatasets(item.SerieDatasets);
            }

            else
            {
                DownloadUrl = item.DistributionUrl;
                if (!DownloadLink())
                    DownloadUrl = MakeDownloadUrlRelative();
            }

            MapUrl = GetMapUrl();

            SpatialScope = item.SpatialScope;
        }

        public static Serie AddSerie(string serieString)
        {
            var serie = new Serie();
           
            var datasetArray = serieString.Split('|');
            try
            {
                serie.Uuid = datasetArray[0];
                serie.DistributionProtocol = datasetArray[6];
                serie.GetCapabilitiesUrl = datasetArray[7];
                serie.Title = datasetArray[1];
                serie.TypeName = datasetArray[11];
                serie.Theme = datasetArray[8];
                serie.Organization = datasetArray[4];
                serie.DistributionUrl = datasetArray[7];
                serie.AccessIsOpendata = Convert.ToBoolean(datasetArray[12]);
                serie.AccessIsRestricted = Convert.ToBoolean(datasetArray[13]);
            }
            catch (Exception e)
            {

            }
            return serie;
        }

        public static List<Dataset> AddSerieDatasets(List<string> serieDatasets)
        {
            var datasets = new List<Dataset>();
            if (serieDatasets != null)
            {
                foreach (var datasetString in serieDatasets)
                {
                    var datasetArray = datasetString.Split('|');
                    try
                    {
                        datasets.Add(new Dataset
                        {
                            Uuid = datasetArray[0],
                            DistributionProtocol = datasetArray[6],
                            GetCapabilitiesUrl = datasetArray[7],
                            Title = datasetArray[1],
                            Type = "dataset",
                            Theme = datasetArray[8],
                            Organization = datasetArray[4],
                            DistributionUrl = datasetArray[7],
                            AccessIsOpendata = Convert.ToBoolean(datasetArray[14]),
                            AccessIsRestricted = Convert.ToBoolean(datasetArray[15])

                    });
                    }
                    catch (Exception e)
                    {

                    }
                }
            }

            return datasets;
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
                        if (ShowMaplink(serviceArray[6]) && IsServiceOrServiceLayer(serviceArray[3]) 
                            && serviceArray[6] != null && serviceArray[6] == "OGC:WMS"
                            && serviceArray[5] != null /*&& serviceArray[5] ==""*/)
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
            ShowMapLink = false; 
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
            if (!string.IsNullOrWhiteSpace(DistributionProtocol) && (DistributionProtocol.Contains("OGC:WMS") /*|| DistributionProtocol.Contains("OGC:WFS") || DistributionProtocol.Contains("OGC:WCS")*/) && (Type == "service" || Type == "servicelayer") && !string.IsNullOrWhiteSpace(DownloadUrl) || DatasetServicesWithShowMapLink.Any() || (ServiceDistributionUrlForDataset != null && ServiceDistributionUrlForDataset.ToLower().EndsWith("service=wms"))) return true;
            else return false;
        }

        public bool ShowMaplink(string protocol)
        {
            if (!string.IsNullOrWhiteSpace(protocol) && (protocol.Contains("OGC:WMS") /*|| protocol.Contains("OGC:WFS") || protocol.Contains("OGC:WCS")*/))
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

        public string GetAtomFeed(List<string> services)
        {
            string atomFeed = "";

            if(services != null) { 

                foreach(var service in services)
                { 
                    var data = service.Split('|');

                    if(data.Length >= 3) {

                        var protocol = data[1];
                        if(protocol == "W3C:AtomFeed")
                        {
                            var url = data[2];
                            if(!string.IsNullOrEmpty(url))
                            {
                                if (service.ToLower().Contains("gml"))
                                    return url;
                                else if (service.ToLower().Contains("sosi"))
                                    return url;
                                else
                                    atomFeed = url;
                            }
                        }
                    }
                }

            }

            return atomFeed;
        }
    }

    public class DatasetService
    {
        public string Uuid { get; set; }
        public string Title { get; set; }
        public string DistributionProtocol { get; set; }
        public string GetCapabilitiesUrl { get; set; }
    }

    public class Dataset
    {
        public string Uuid { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string DistributionProtocol { get; set; }
        public string GetCapabilitiesUrl { get; set; }
        public string Theme { get; set; }
        public string Organization { get; set; }
        public string DistributionUrl { get; set; }
        public bool? AccessIsOpendata { get; set; }
        public bool? AccessIsRestricted { get; set; }
    }

    public class Serie
    {
        public string Uuid { get; set; }
        public string Title { get; set; }
        public string DistributionProtocol { get; set; }
        public string GetCapabilitiesUrl { get; set; }
        public string TypeName { get; set; }
        public string Theme { get; set; }
        public string Organization { get; set; }
        public string DistributionUrl { get; set; }
        public bool? AccessIsRestricted { get; set; }
        public bool? AccessIsOpendata { get; set; }


    }
}