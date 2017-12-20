using GeoNorgeAPI;
using Kartverket.Metadatakatalog.Helpers;
using Kartverket.Metadatakatalog.Models.Translations;
using Kartverket.Metadatakatalog.Service;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Device.Location;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Routing;
using Kartverket.Metadatakatalog.Models.Api;

namespace Kartverket.Metadatakatalog.Models
{
    public class MetadataViewModel
    {
        public void SetDistributionUrl()
        {
            if (DistributionDetails != null && !string.IsNullOrWhiteSpace(DistributionDetails.URL)){
                DownloadUrl = DistributionDetails.URL;
            }
        }

        public string Abstract { get; set; }
        public BoundingBox BoundingBox { get; set; }
        public Constraints Constraints { get; set; }
        public Contact ContactMetadata { get; set; }
        public Contact ContactOwner { get; set; }
        public Contact ContactPublisher { get; set; }
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? DateCreated { get; set; }
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? DateMetadataUpdated { get; set; }

        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? DateMetadataValidFrom { get; set; }
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? DateMetadataValidTo { get; set; }

        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? DatePublished { get; set; }
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? DateUpdated { get; set; }
        public DistributionDetails DistributionDetails { get; set; }
        public DistributionFormat DistributionFormat { get; set; }
        public List<SimpleDistributionFormat> DistributionFormats { get; set; }
        public string UnitsOfDistribution { get; set; }
        public List<ReferenceSystem> ReferenceSystems { get; set; }
        public string EnglishAbstract { get; set; }
        public string EnglishTitle { get; set; }
        //
        // Summary:
        //     Note: Only supporting one hierarchyLevel element. Array is overwritten with
        //     an array of one element when value is updated.
        public string HierarchyLevel { get; set; }
        public List<Keyword> Keywords { get; set; }

        public List<Keyword> KeywordsPlace { get; set; }
        public List<Keyword> KeywordsTheme { get; set; }
        public List<Keyword> KeywordsInspire { get; set; }
        public List<Keyword> KeywordsNationalInitiative { get; set; }
        public List<Keyword> KeywordsNationalTheme { get; set; }
        public List<Keyword> KeywordsOther { get; set; }
        public List<Keyword> KeywordsConcept { get; set; }

        public string LegendDescriptionUrl { get; set; }
        //
        // Summary:
        //     Values from codelist: http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/gmxCodelists.xml#MD_MaintenanceFrequencyCode
        public string MaintenanceFrequency { get; set; }
        public string MetadataLanguage { get; set; }
        public string MetadataStandard { get; set; }
        public string MetadataStandardVersion { get; set; }
        public List<string> OperatesOn { get; set; }
        public List<MetadataViewModel> Related { get; set; }
        public string ProcessHistory { get; set; }
        public string ProductPageUrl { get; set; }
        public string ProductSheetUrl { get; set; }
        public string ProductSpecificationUrl { get; set; }
        public string CoverageUrl { get; set; }
        public string DownloadUrl { get; set; }
        public string Purpose { get; set; }
        public List<QualitySpecification> QualitySpecifications { get; set; }
        public ReferenceSystem ReferenceSystem { get; set; }
        public string ResolutionScale { get; set; }
        public string SpatialRepresentation { get; set; }
        public string SpecificUsage { get; set; }
        //
        // Summary:
        //     Values from codelist: http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/gmxCodelists.xml#MD_ProgressCode
        public string Status { get; set; }
        public string SupplementalDescription { get; set; }
        public string HelpUrl { get; set; }
        public List<Thumbnail> Thumbnails { get; set; }
        public string Title { get; set; }
        public string TopicCategory { get; set; }
        public string Uuid { get; set; }

        public string ResourceReferenceCode { get; set; }
        public string ResourceReferenceCodespace { get; set; }

        public string MetadataXmlUrl { get; set; }
        public string MetadataEditUrl { get; set; }
        public string OrganizationLogoUrl { get; set; }

        public string ServiceDistributionNameForDataset { get; set; }
        public string ServiceDistributionUrlForDataset { get; set; }
        public string ServiceDistributionProtocolForDataset { get; set; }
        public string ServiceUuid { get; set; }
        public string ServiceWfsDistributionUrlForDataset { get; set; }
        public string ServiceDistributionAccessConstraint { get; set; }
        public string ServiceWfsDistributionAccessConstraint { get; set; }
        public bool AccessIsOpendata { get; set; }
        public bool AccessIsRestricted { get; set; }
        public bool AccessIsProtected { get; set; }
        public bool CanShowMapUrl { get; set; }
        public bool CanShowServiceMapUrl { get; set; }
        public bool CanShowDownloadService { get; set; }
        public bool CanShowDownloadUrl { get; set; }
        public bool CanShowWebsiteUrl { get; set; }
        public string MapLink { get; set; }
        public string ServiceLink { get; set; }
        public string DistributionUrl { get; set; }
        public Distributions Distributions { get; set; }


        public MetadataViewModel()
        {
            Related = new List<MetadataViewModel>();
            Thumbnails = new List<Thumbnail>();
            Distributions = new Distributions();
        }

        public SeoUrl CreateSeoUrl()
        {
            if (ContactOwner != null)
            {
                return new SeoUrl((ContactOwner.Organization != null ? ContactOwner.Organization : ""), Title);
            }
            else
                return new SeoUrl("", Title);
        }
        

        public String MapUrl()
        {
            var norgeskartUrl = WebConfigurationManager.AppSettings["NorgeskartUrl"];
            if (IsService() || IsServiceLayer())
            {
                if (DistributionDetails != null && !string.IsNullOrWhiteSpace(DistributionDetails.URL) && !string.IsNullOrWhiteSpace(DistributionDetails.Protocol) && DistributionDetails.Protocol.Contains(("OGC:WMS")))
                {
                    if (!string.IsNullOrWhiteSpace(DistributionDetails.Name))
                        return norgeskartUrl + "#5/355422/6668909/*/l/wms/[" + RemoveQueryString(DistributionDetails.URL) + "]/+" + DistributionDetails.Name + "/";
                    else
                        return norgeskartUrl + "#5/355422/6668909/l/wms/[" + RemoveQueryString(DistributionDetails.URL) + "]" + "/";
                }
                else if (DistributionDetails != null && !string.IsNullOrWhiteSpace(DistributionDetails.URL) && !string.IsNullOrWhiteSpace(DistributionDetails.Protocol) && DistributionDetails.Protocol.Contains(("OGC:WFS")))
                {
                    if (!string.IsNullOrWhiteSpace(DistributionDetails.Name))
                        return norgeskartUrl + "#5/355422/6668909/*/l/wfs/[" + RemoveQueryString(DistributionDetails.URL) + "]/+" + DistributionDetails.Name + "/";
                    else
                        return norgeskartUrl + "#5/355422/6668909/l/wfs/[" + RemoveQueryString(DistributionDetails.URL) + "]" + "/";
                }
                else if (DistributionDetails != null && !string.IsNullOrWhiteSpace(DistributionDetails.URL) && !string.IsNullOrWhiteSpace(DistributionDetails.Protocol) && DistributionDetails.Protocol.Contains(("OGC:WCS")))
                {
                    if (!string.IsNullOrWhiteSpace(DistributionDetails.Name))
                        return norgeskartUrl + "#5/355422/6668909/*/l/wcs/[" + RemoveQueryString(DistributionDetails.URL) + "]/+" + DistributionDetails.Name + "/";
                    else
                        return norgeskartUrl + "#5/355422/6668909/l/wcs/[" + RemoveQueryString(DistributionDetails.URL) + "]" + "/";
                }

                else return "";
            }
            else if (IsDataset())
            {
                return WebConfigurationManager.AppSettings["NorgeskartUrl"] + ServiceUrl() + "/";
            }
            else return "";
        }

        public string ServiceUrl()
        {
            string url = "";

            if (IsDataset())
            {
                if (!string.IsNullOrWhiteSpace(ServiceDistributionProtocolForDataset) && ServiceDistributionProtocolForDataset.Contains(("OGC:WMS")))
                {
                    if (!string.IsNullOrWhiteSpace(ServiceDistributionNameForDataset) && !string.IsNullOrWhiteSpace(ServiceDistributionUrlForDataset))
                        url = "#5/355422/6668909/*/l/wms/[" + RemoveQueryString(ServiceDistributionUrlForDataset) + "]/+" + ServiceDistributionNameForDataset;
                    else if (!string.IsNullOrWhiteSpace(ServiceDistributionUrlForDataset))
                        url = "#5/355422/6668909/l/wms/[" + RemoveQueryString(ServiceDistributionUrlForDataset) + "]";
                }
                else if (!string.IsNullOrWhiteSpace(ServiceDistributionProtocolForDataset) && ServiceDistributionProtocolForDataset.Contains(("OGC:WFS")))
                {
                    if (!string.IsNullOrWhiteSpace(ServiceDistributionNameForDataset) && !string.IsNullOrWhiteSpace(ServiceDistributionUrlForDataset))
                        url = "#5/355422/6668909/*/l/wfs/[" + RemoveQueryString(ServiceDistributionUrlForDataset) + "]/+" + ServiceDistributionNameForDataset;
                    else if (DistributionDetails != null && !string.IsNullOrWhiteSpace(DistributionDetails.URL))
                        url = "#5/355422/6668909/l/wfs/[" + RemoveQueryString(ServiceDistributionUrlForDataset) + "]";
                }

            }

            return url;
        }

        string RemoveQueryString(string URL) 
        {
            int startQueryString = URL.IndexOf("?");

            if (startQueryString != -1)
                URL = URL.Substring(0, startQueryString);

            return URL;
        }

        public String OrganizationSeoName()
        {
            return CreateSeoUrl().Organization;
        }

        public bool ShowDownloadLink()
        {
            if (DistributionDetails != null && !string.IsNullOrWhiteSpace(DistributionDetails.URL) && !string.IsNullOrWhiteSpace(DistributionDetails.Protocol) && (DistributionDetails.Protocol.Contains("WWW:DOWNLOAD") || DistributionDetails.Protocol.Contains("GEONORGE:FILEDOWNLOAD")) && (IsDataset() || IsDatasetSeries())) return true;
            else return false;
        }

        public bool ShowDownloadService()
        {
            if (System.Web.Configuration.WebConfigurationManager.AppSettings["DownloadServiceEnabled"] == "true") 
            {
                if (DistributionDetails != null && !string.IsNullOrWhiteSpace(DistributionDetails.Protocol) && DistributionDetails.Protocol.Contains("GEONORGE:DOWNLOAD"))
                    return true;
            }

            return false;
        }

        public bool ShowMapLink()
        {
            if (DistributionDetails != null && !string.IsNullOrWhiteSpace(DistributionDetails.URL) && !string.IsNullOrWhiteSpace(DistributionDetails.Protocol) && (DistributionDetails.Protocol.Contains("OGC:WMS") || DistributionDetails.Protocol.Contains("OGC:WFS")) && (IsService() || IsServiceLayer())) return true;
            else return false;
        }

        public bool ShowServiceMapLink()
        {
            if (!string.IsNullOrWhiteSpace(ServiceUrl())) return true;
            else return false;
        }

        public bool ShowWebsiteLink()
        {
            if (DistributionDetails != null && !string.IsNullOrWhiteSpace(DistributionDetails.URL) && !string.IsNullOrWhiteSpace(DistributionDetails.Protocol) && DistributionDetails.Protocol.Contains("WWW:LINK") ) return true;
            else return false;
        }

        public bool IsService()
        {
            return HierarchyLevel == "service" || HierarchyLevel == "Tjeneste";
        }

        public bool IsServiceLayer()
        {
            return (HierarchyLevel == "service" || HierarchyLevel == "Tjeneste") && (DistributionDetails != null && !string.IsNullOrWhiteSpace(DistributionDetails.Name));
        }

        public bool IsDataset()
        {
            return HierarchyLevel == "dataset" || HierarchyLevel == "Datasett";
        }

        public bool IsDatasetSeries()
        {
            return HierarchyLevel == "series";
        }

        public bool IsApplication()
        {
            return HierarchyLevel == "software" || HierarchyLevel == "Applikasjon";
        }

        public bool IsDatasetBundle()
        {
            return HierarchyLevel == "dimensionGroup" || HierarchyLevel == "Datapakke";
        }

        public bool IsOpendata()
        {
            if (Constraints != null && !string.IsNullOrEmpty(Constraints.OtherConstraintsAccess) && Constraints.OtherConstraintsAccess.ToLower() == "no restrictions")
                return true;
            else
                return false;
        }

        public bool IsRestricted()
        {
            if (Constraints != null &&  !string.IsNullOrEmpty(Constraints.OtherConstraintsAccess) && Constraints.OtherConstraintsAccess.ToLower() == "norway digital restricted")
            return true;
            else
                return false;
        }

        public bool IsOffline()
        {
            if (Constraints != null && (Constraints.AccessConstraints == "Beskyttet" || Constraints.AccessConstraints == "restricted"))
                return true;
            else
                return false;
        }

        public bool IsRestrictedService()
        {
            return ServiceDistributionAccessConstraint == "Beskyttet" || ServiceDistributionAccessConstraint == "restricted" || ServiceDistributionAccessConstraint == "norway digital restricted";
        }

        public string GetCoverageLink()
        {

            if (!string.IsNullOrEmpty(CoverageUrl))
            {
                if (CoverageUrl.IndexOf("TYPE:") != -1)
                {
                    string CoverageLink = "";
                    var coverageStr = CoverageUrl;
                    var startPos = 5;
                    var endPosType = coverageStr.IndexOf("@PATH");
                    var typeStr = coverageStr.Substring(startPos, endPosType - startPos);

                    var endPath = coverageStr.IndexOf("@LAYER");
                    var pathStr = coverageStr.Substring(endPosType + startPos + 1, endPath - (endPosType + startPos + 1));

                    var startLayer = endPath + 7;
                    var endLayer = coverageStr.Length - startLayer;
                    var layerStr = coverageStr.Substring(startLayer, endLayer);

                    int zoomLevel = ZoomLevel();

                    if (typeStr == "WMS")
                    {
                        CoverageLink = WebConfigurationManager.AppSettings["NorgeskartUrl"] + "#" + zoomLevel +
                                       "/269663/6802350/l/wms/[" + pathStr + "]/+" + layerStr;
                    }

                    else if (typeStr == "WFS")
                    {
                        CoverageLink = WebConfigurationManager.AppSettings["NorgeskartUrl"] + "#" + zoomLevel +
                                       "/255216/6653881/l/wfs/[" + RemoveQueryString(pathStr) + "]/+" + layerStr;
                    }

                    else if (typeStr == "GeoJSON")
                    {
                        CoverageLink = WebConfigurationManager.AppSettings["NorgeskartUrl"] + "#" + zoomLevel +
                                       "/355422/6668909/l/geojson/[" + RemoveQueryString(pathStr) + "]/+" + layerStr;
                    }

                    return CoverageLink;
                }
            }
            return CoverageUrl;
        }

        public string GetCoverageParams()
        {

            string CoverageParams = "";
            var coverageStr = CoverageUrl;
            var startPos = 5;
            if (!string.IsNullOrEmpty(coverageStr) && coverageStr.IndexOf("@PATH") > -1)
            { 
                var endPosType = coverageStr.IndexOf("@PATH");
                var typeStr = coverageStr.Substring(startPos, endPosType - startPos);

                var endPath = coverageStr.IndexOf("@LAYER");
                var pathStr = coverageStr.Substring(endPosType + startPos + 1, endPath - (endPosType + startPos + 1));

                var startLayer = endPath + 7;
                var endLayer = coverageStr.Length - startLayer;
                var layerStr = coverageStr.Substring(startLayer, endLayer);


                if (typeStr == "WMS")
                {
                    CoverageParams = "#4/355422/6668909/l/wms/[" + pathStr + "]/+" + layerStr;
                }

                else if (typeStr == "WFS")
                {
                    CoverageParams = "#5/255216/6653881/l/wfs/[" + RemoveQueryString(pathStr) + "]/+" + layerStr;
                }

                else if (typeStr == "GeoJSON")
                {
                    CoverageParams = "#5/355422/6668909/l/geojson/[" + RemoveQueryString(pathStr) + "]/+" + layerStr;
                }
            }

            return CoverageParams;

        }


        public string ParentIdentifier { get; set; }

        public int ZoomLevel() 
        {
            double zoomLevel = 7;

            if (string.IsNullOrEmpty(BoundingBox.WestBoundLongitude) || string.IsNullOrEmpty(BoundingBox.SouthBoundLatitude) ||
                string.IsNullOrEmpty(BoundingBox.EastBoundLongitude) || string.IsNullOrEmpty(BoundingBox.NorthBoundLatitude))
                return Convert.ToInt16(zoomLevel);

            GeoCoordinate[] locations = new GeoCoordinate[]
            {
                new GeoCoordinate(Convert.ToDouble(BoundingBox.SouthBoundLatitude), Convert.ToDouble(BoundingBox.WestBoundLongitude)),
                new GeoCoordinate(Convert.ToDouble(BoundingBox.NorthBoundLatitude), Convert.ToDouble(BoundingBox.EastBoundLongitude))
            };


            double maxLat = -85;
            double minLat = 85;
            double maxLon = -180;
            double minLon = 180;

            //calculate bounding rectangle
            for (int i = 0; i < locations.Count(); i++)
            {
                if (locations[i].Latitude > maxLat)
                {
                    maxLat = locations[i].Latitude;
                }

                if (locations[i].Latitude < minLat)
                {
                    minLat = locations[i].Latitude;
                }

                if (locations[i].Longitude > maxLon)
                {
                    maxLon = locations[i].Longitude;
                }

                if (locations[i].Longitude < minLon)
                {
                    minLon = locations[i].Longitude;
                }
            }

            double zoom1 = 0; double zoom2 = 0;
            double mapWidth = 1359; //Map width in pixels
            double mapHeight = 940; //Map height in pixels
            int buffer = 1; //Width in pixels to use to create a buffer around the map. This is to keep pushpins from being cut off on the edge
            //Determine the best zoom level based on the map scale and bounding coordinate information
            if (maxLon != minLon && maxLat != minLat)
            {
                //best zoom level based on map width
                zoom1 = Math.Log(360.0 / 256.0 * (mapWidth - 2*buffer) / (maxLon - minLon)) / Math.Log(2);
                //best zoom level based on map height
                zoom2 = Math.Log(180.0 / 256.0 * (mapHeight - 2*buffer) / (maxLat - minLat)) / Math.Log(2);
            }

            //use the most zoomed out of the two zoom levels
            zoomLevel = (zoom1 < zoom2) ? zoom1 : zoom2; 

            return Convert.ToInt16(zoomLevel);
        }

        public RouteValueDictionary ParamsForOrderByTitleLink()
        {
            var routeValues = new RouteValueDictionary();
            routeValues["orderby"] = "title";
            return routeValues;
        }

        public RouteValueDictionary ParamsForOrderByTitleDescLink()
        {
            var routeValues = new RouteValueDictionary();
            routeValues["orderby"] = "title_desc";
            return routeValues;
        }

        public RouteValueDictionary ParamsForOrderByOrganizationLink()
        {
            var routeValues = new RouteValueDictionary();
            routeValues["orderby"] = "organization";
            return routeValues;
        }

        public RouteValueDictionary ParamsForOrderByOrganizationDescLink()
        {
            var routeValues = new RouteValueDictionary();
            routeValues["orderby"] = "organization_desc";
            return routeValues;
        }

        public string GetHierarchyLevelTranslated()
        {
            var culture = CultureHelper.GetCurrentCulture();
            if (culture == Culture.NorwegianCode)
            {
                if (IsDataset()) return "Datasett";
                if (IsServiceLayer()) return "Tjenestelag";
                if (IsService()) return "Tjeneste";
                if (IsApplication()) return "Applikasjon";
                if (IsDatasetSeries()) return "Datasettserie";
                if (IsDatasetBundle()) return "Datapakke";
            }
            else
            {
                if (IsDataset()) return "Dataset";
                if (IsServiceLayer()) return "Service layer";
                if (IsService()) return "Service";
                if (IsApplication()) return "Application";
                if (IsDatasetSeries()) return "Dataset series";
                if (IsDatasetBundle()) return "Data package";
            }

            return HierarchyLevel;
        }

    }

    public class Distributions
    {
        public List<Distribution> SelfDistribution { get; set; }
        public List<Distribution> RelatedDataset { get; set; }
        public List<Distribution> RelatedApplications { get; set; }
        public List<Distribution> RelatedServices { get; set; }
        public List<Distribution> RelatedServiceLayer { get; set; }
        public List<Distribution> RelatedViewServices { get; set; }
        public List<Distribution> RelatedDownloadServices { get; set; }

        public bool ShowRelatedDataset { get; set; }
        public bool ShowRelatedApplications { get; set; }
        public bool ShowRelatedServices { get; set; }
        public bool ShowRelatedServiceLayer { get; set; }
        public bool ShowRelatedViewServices { get; set; }
        public bool ShowRelatedDownloadServices { get; set; }
        public bool ShowSelfDistributions { get; set; }

        public Distributions()
        {
            SelfDistribution = new List<Distribution>();
            RelatedDataset = new List<Distribution>();
            RelatedApplications = new List<Distribution>();
            RelatedServices = new List<Distribution>();
            RelatedServiceLayer = new List<Distribution>();
            RelatedViewServices = new List<Distribution>();
            RelatedDownloadServices = new List<Distribution>();

            ShowRelatedDataset = false;
            ShowRelatedApplications = false;
            ShowRelatedServices = false;
            ShowRelatedServiceLayer = false;
            ShowRelatedViewServices = false;
            ShowRelatedDownloadServices = false;
            ShowSelfDistributions = true;
        }

        public bool ShowApplications()
        {
            return RelatedApplications.Any();
        }

        public bool ShowServices()
        {
            return RelatedServices.Any();
        }

        public bool ShowViewServices()
        {
            return RelatedViewServices.Any();
        }

        public bool ShowDownloadServices()
        {
            return RelatedDownloadServices.Any();
        }

        public bool ShowDatasets()
        {
            return RelatedDataset.Any();
        }

        public bool ShowServicLayers()
        {
            return RelatedServiceLayer.Any();
        }
    }

    public class BoundingBox
    {
        public string EastBoundLongitude { get; set; }
        public string NorthBoundLatitude { get; set; }
        public string SouthBoundLatitude { get; set; }
        public string WestBoundLongitude { get; set; }
    }


    public class Constraints
    {
        public string AccessConstraints { get; set; }
        public string OtherConstraints { get; set; }
        public string OtherConstraintsLink { get; set; }
        public string OtherConstraintsLinkText { get; set; }
        public string SecurityConstraints { get; set; }
        public string SecurityConstraintsNote { get; set; }
        public string UseConstraints { get; set; }
        public string UseLimitations { get; set; }
        public string OtherConstraintsAccess { get; set; }
    }

    public class Contact
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string Organization { get; set; }
        public string OrganizationEnglish { get; set; }
        public string Role { get; set; }
    }

    public class DistributionDetails
    {
        public string Name { get; set; }
        public string Protocol { get; set; }
        public string ProtocolName { get; set; }
        public string URL { get; set; }

        public DistributionDetails() {

        }

        public DistributionDetails(string name, string protocol, string protocolName, string url)
        {
            Name = name;
            Protocol = protocol;
            ProtocolName = protocolName;
            URL = url;
        }

        public bool IsWmsUrl()
        {
            return !string.IsNullOrWhiteSpace(Protocol) && Protocol.Contains("OGC:WMS");
        }

        public bool IsWfsUrl()
        {
            return !string.IsNullOrWhiteSpace(Protocol) && Protocol.Contains("OGC:WFS");
        }


        public string DistributionDetailsGetCapabilitiesUrl()
        {
            return this != null ? SimpleMetadataUtil.GetCapabilitiesUrl(URL, Protocol) : "";
        }

        public bool IsWms()
        {
            return Protocol == "OGC:WMS";
        }

        public bool IsWfs()
        {
            return Protocol == "OGC:WFS";
        }

    }

    public class DistributionFormat
    {
        public string Name { get; set; }
        public string Version { get; set; }
    }

    public class Keyword
    {
        public string EnglishKeyword { get; set; }
        public string KeywordValue { get; set; }
        public string Thesaurus { get; set; }
        public string Type { get; set; }
        public string KeywordLink { get; set; }
}

    public class QualitySpecification
    {
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? Date { get; set; }
        public string DateType { get; set; }
        public string Explanation { get; set; }
        public bool Result { get; set; }
        public string Title { get; set; }
    }

    public class ReferenceSystem
    {
        public string CoordinateSystem { get; set; }
        public string CoordinateSystemUrl { get; set; }
        public string Namespace { get; set; }
    }

    public class Thumbnail
    {
        public string Type { get; set; }
        public string URL { get; set; }
    }
}