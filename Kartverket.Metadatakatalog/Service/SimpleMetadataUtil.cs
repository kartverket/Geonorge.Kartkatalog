using GeoNorgeAPI;
using Kartverket.Metadatakatalog.Helpers;
using System;
using System.Text;
using System.Web.Configuration;

namespace Kartverket.Metadatakatalog.Service
{
    public static class SimpleMetadataUtil
    {
        public const string ZoomLevel = "3";
        public const string Longitude = "306722";
        public const string Latitude = "7197864";
        public const string OgcWms = "OGC:WMS";
        public const string OgcWfs = "OGC:WFS";
        public const string OgcWcs = "OGC:WCS";
        public const string OgcCsw = "OGC:CSW";
        public const string Wms = "wms";
        public const string Wfs = "wfs";
        public const string Wcs = "wcs";

        public static readonly string NorgeskartUrl = WebConfigurationManager.AppSettings["NorgeskartUrl"];
        public static readonly bool MapOnlyWms = Convert.ToBoolean(WebConfigurationManager.AppSettings["MapOnlyWms"]);

        public static string ConvertHierarchyLevelToType(string hierarchyLevel)
        {
            var res = "default";
            switch (hierarchyLevel)
            {
                case "dataset":
                    res = "Datasett";
                    break;
                case "software":
                    res = "Applikasjon";
                    break;
                case "service":
                    res = "Tjeneste";
                    break;
                case "servicelayer":
                    res = "Tjenestelag";
                    break;
                case "datasetserie":
                    res = "Datasettserie";
                    break;
                case "dimensionGroup":
                    res = "Datapakke";
                    break;
            }
            return res;
        }
        public static bool IsOpendata(SimpleMetadata simpleMetadata)
        {
            return simpleMetadata.Constraints != null && IsOpendata(simpleMetadata.Constraints.OtherConstraintsAccess);
        }
        public static bool IsOpendata(string otherConstraintsAccess)
        {
            return !string.IsNullOrEmpty(otherConstraintsAccess) && otherConstraintsAccess.ToLower() == "no restrictions";
        }
        public static bool IsRestricted(SimpleMetadata simpleMetadata)
        {
            return simpleMetadata.Constraints != null && IsRestricted(simpleMetadata.Constraints.OtherConstraintsAccess);
        }
        public static bool IsRestricted(string otherConstraintsAccess)
        {
            if (string.IsNullOrEmpty(otherConstraintsAccess)) return false;
            return otherConstraintsAccess.ToLower() == "norway digital restricted" ||
                   otherConstraintsAccess.ToLower() == "Beskyttet" || otherConstraintsAccess.ToLower() == "restricted";
        }
        public static bool IsProtected(SimpleMetadata simpleMetadata)
        {
            return simpleMetadata.Constraints != null && IsProtected(simpleMetadata.Constraints.AccessConstraints);
        }
        public static bool IsProtected(string accessConstraints)
        {
            return (accessConstraints == "Beskyttet" || accessConstraints == "restricted");
        }

        internal static bool ShowMapLink(SimpleDistribution simpleMetadataDistribution, string hierarchyLevel)
        {
            return !string.IsNullOrWhiteSpace(simpleMetadataDistribution?.URL) && !string.IsNullOrWhiteSpace(simpleMetadataDistribution.Protocol) && (simpleMetadataDistribution.Protocol.Contains("OGC:WMS") || (!MapOnlyWms && simpleMetadataDistribution.Protocol.Contains("OGC:WFS"))) && (hierarchyLevel == "service" || hierarchyLevel == "servicelayer");
        }

        public static string GetCapabilitiesUrl(string url, string protocol)
        {
            if (string.IsNullOrWhiteSpace(url)) return "";
            var tmp = url;
            var startQueryString = tmp.IndexOf("?", StringComparison.Ordinal);

            if (startQueryString != -1)
                tmp = tmp.Substring(0, startQueryString + 1);
            else
                tmp = tmp + "?";

            if (!string.IsNullOrWhiteSpace(protocol) && protocol.Contains(("OGC:WMS")))
                return tmp + "request=GetCapabilities&service=WMS";
            if (!string.IsNullOrWhiteSpace(protocol) && protocol.Contains(("OGC:WFS")))
                return tmp + "request=GetCapabilities&service=WFS";
            if (!string.IsNullOrWhiteSpace(protocol) && protocol.Contains(("OGC:WCS")))
                return tmp + "request=GetCapabilities&service=WCS";
            if (!string.IsNullOrWhiteSpace(protocol) && protocol.Contains(("OGC:CSW")))
                return tmp + "request=GetCapabilities&service=CSW";
            if (!string.IsNullOrWhiteSpace(protocol) && protocol.Contains(("OGC:WMTS")))
                return tmp + "request=GetCapabilities&service=wmts";
            return tmp;
        }

        internal static string MapUrl(SimpleDistribution simpleMetadataDistribution, string hierarchyLevel)
        {
            return simpleMetadataDistribution != null ? MapUrl(simpleMetadataDistribution.URL, hierarchyLevel, simpleMetadataDistribution.Protocol, simpleMetadataDistribution.Name) : "";
        }

        internal static bool ShowDownloadService(SimpleDistribution simpleMetadataDistribution)
        {
            if (System.Web.Configuration.WebConfigurationManager.AppSettings["DownloadServiceEnabled"] != "true") return false;
            return !string.IsNullOrWhiteSpace(simpleMetadataDistribution?.Protocol) && simpleMetadataDistribution.Protocol.Contains("GEONORGE:DOWNLOAD");
        }

        internal static bool ShowDownloadLink(SimpleDistribution simpleMetadataDistribution, string hierarchyLevel)
        {
            return !string.IsNullOrWhiteSpace(simpleMetadataDistribution?.URL) && !string.IsNullOrWhiteSpace(simpleMetadataDistribution.Protocol) && (simpleMetadataDistribution.Protocol.Contains("WWW:DOWNLOAD") || simpleMetadataDistribution.Protocol.Contains("GEONORGE:FILEDOWNLOAD")) && (hierarchyLevel == "dataset" || hierarchyLevel == "series");
        }

        // @TODO doNotCheckHierarchyLevel: check if multiple different methods are really necessary to check hierarchy level in this class and MetadataViewModel.
        public static string MapUrl(string url, string hierarchyLevel, string protocol, string name, bool doNotCheckHierarchyLevel = false)
        {
            StringBuilder mappedUrl = new StringBuilder();

            if (AreUrlParamsValid(url, hierarchyLevel, protocol, doNotCheckHierarchyLevel))
            {
                string commonPart = GetCommonPartOfNorgeskartUrl(protocol);
                string urlWithoutQueryString = RemoveQueryString(url);

                if (!String.IsNullOrWhiteSpace(commonPart) && !String.IsNullOrWhiteSpace(urlWithoutQueryString))
                {
                    mappedUrl.Append($"{commonPart}{urlWithoutQueryString}");

                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        mappedUrl.Append($"&addLayers={name}");
                    }
                }
            }
            return mappedUrl.ToString();
        }

        public static string GetCommonPartOfNorgeskartUrl(string protocol, bool relativePath = false)
        {
            StringBuilder url = new StringBuilder();
            string protocolName = null;

            if (protocol.Contains(OgcWms))
            {
                protocolName = Wms;
            }
            else if (protocol.Contains(OgcWfs))
            {
                protocolName = Wfs;
            }
            else if (protocol.Contains(OgcWcs))
            {
                protocolName = Wcs;
            }

            if (protocolName != null)
            {
                if (!relativePath)
                {
                    url.Append(NorgeskartUrl);
                }
                url.Append($"#!?zoom={ZoomLevel}&lon={Longitude}&lat={Latitude}&{protocolName}=");
            }
            return url.ToString();
        }

        private static bool AreUrlParamsValid(string url, string hierarchyLevel, string protocol, bool doNotCheckHierarchyLevel)
        {
            return
                (doNotCheckHierarchyLevel || hierarchyLevel == "service" || hierarchyLevel == "servicelayer")
                && !string.IsNullOrWhiteSpace(url)
                && !string.IsNullOrWhiteSpace(protocol);
        }

        private static string RemoveQueryString(string url)
        {
            var startQueryString = url.IndexOf("?", StringComparison.Ordinal);

            if (startQueryString != -1)
                url = url.Substring(0, startQueryString);

            return url;
        }

        internal static string GetDataAccess(SimpleMetadata simpleMetadata, string culture)
        {
            if (IsRestricted(simpleMetadata))
                if (CultureHelper.IsNorwegian(culture))
                    return "Tilgangsbegrensede data";
                else
                    return "Protected data";
            else if (IsProtected(simpleMetadata))
                if (CultureHelper.IsNorwegian(culture))
                    return "Skjermede data";
                else
                    return "Restricted data";
            else
            if (CultureHelper.IsNorwegian(culture))
                return "Åpne data";
            else
                return "Open data";

        }
    }
}