﻿using GeoNorgeAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Metadatakatalog.Service
{
    public static class SimpleMetadataUtil
    {
        public static string ConvertHierarchyLevelToType(string HierarchyLevel)
        {
            string res = "default";
            switch (HierarchyLevel)
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
                    res = "Tjeneste";
                    break;
                case "datasetserie":
                    res = "Datasettserie";
                    break;
            }
            return res;
        }
        public static bool IsOpendata(SimpleMetadata simpleMetadata)
        {
            if (simpleMetadata.Constraints != null && IsOpendata(simpleMetadata.Constraints.OtherConstraintsAccess))
                return true;
            else
                return false;
        }
        public static bool IsOpendata(string OtherConstraintsAccess)
        {
            if (!string.IsNullOrEmpty(OtherConstraintsAccess) && OtherConstraintsAccess.ToLower() == "no restrictions")
                return true;
            else
                return false;
        }
        public static bool IsRestricted(SimpleMetadata simpleMetadata)
        {
            if (simpleMetadata.Constraints != null && IsRestricted(simpleMetadata.Constraints.OtherConstraintsAccess))
                return true;
            else
                return false;
        }
        public static bool IsRestricted(string OtherConstraintsAccess)
        {
            if (!string.IsNullOrEmpty(OtherConstraintsAccess) && OtherConstraintsAccess.ToLower() == "norway digital restricted")
                return true;
            else
                return false;
        }
        public static bool IsProtected(SimpleMetadata simpleMetadata)
        {
            if (simpleMetadata.Constraints != null && IsProtected(simpleMetadata.Constraints.AccessConstraints))
                return true;
            else
                return false;
        }
        public static bool IsProtected(string AccessConstraints)
        {
            if ((AccessConstraints == "Beskyttet" || AccessConstraints == "restricted"))
                return true;
            else
                return false;
        }
        public static bool ShowDownloadLink(SimpleMetadata simpleMetadata)
        {
            if (simpleMetadata.DistributionDetails != null && !string.IsNullOrWhiteSpace(simpleMetadata.DistributionDetails.URL) && !string.IsNullOrWhiteSpace(simpleMetadata.DistributionDetails.Protocol) && (simpleMetadata.DistributionDetails.Protocol.Contains("WWW:DOWNLOAD") || simpleMetadata.DistributionDetails.Protocol.Contains("GEONORGE:FILEDOWNLOAD")) && (simpleMetadata.HierarchyLevel == "dataset" || simpleMetadata.HierarchyLevel == "series")) return true;
            else return false;
        }

        public static bool ShowDownloadService(SimpleMetadata simpleMetadata)
        {
            if (System.Web.Configuration.WebConfigurationManager.AppSettings["DownloadServiceEnabled"] == "true")
            {
                if (simpleMetadata.DistributionDetails != null && !string.IsNullOrWhiteSpace(simpleMetadata.DistributionDetails.Protocol) && simpleMetadata.DistributionDetails.Protocol.Contains("GEONORGE:DOWNLOAD"))
                    return true;
            }

            return false;
        }

        public static bool ShowMapLink(SimpleMetadata simpleMetadata)
        {
            if (simpleMetadata.DistributionDetails != null && !string.IsNullOrWhiteSpace(simpleMetadata.DistributionDetails.URL) && !string.IsNullOrWhiteSpace(simpleMetadata.DistributionDetails.Protocol) && (simpleMetadata.DistributionDetails.Protocol.Contains("OGC:WMS") || simpleMetadata.DistributionDetails.Protocol.Contains("OGC:WFS")) && (simpleMetadata.HierarchyLevel == "service" || simpleMetadata.HierarchyLevel == "servicelayer")) return true;
            else return false;
        }


        public static String MapUrl(SimpleMetadata simpleMetadata)
        {
            if (simpleMetadata.HierarchyLevel == "service" || simpleMetadata.HierarchyLevel == "servicelayer")
            {
                if (simpleMetadata.DistributionDetails != null && !string.IsNullOrWhiteSpace(simpleMetadata.DistributionDetails.URL) && !string.IsNullOrWhiteSpace(simpleMetadata.DistributionDetails.Protocol) && simpleMetadata.DistributionDetails.Protocol.Contains(("OGC:WMS")))
                {
                    if (!string.IsNullOrWhiteSpace(simpleMetadata.DistributionDetails.Name))
                        return "#5/355422/6668909/*/l/wms/[" + RemoveQueryString(simpleMetadata.DistributionDetails.URL) + "]/+" + simpleMetadata.DistributionDetails.Name;
                    else
                        return "#5/355422/6668909/l/wms/[" + RemoveQueryString(simpleMetadata.DistributionDetails.URL) + "]";
                }
                else if (simpleMetadata.DistributionDetails != null && !string.IsNullOrWhiteSpace(simpleMetadata.DistributionDetails.URL) && !string.IsNullOrWhiteSpace(simpleMetadata.DistributionDetails.Protocol) && simpleMetadata.DistributionDetails.Protocol.Contains(("OGC:WFS")))
                {
                    if (!string.IsNullOrWhiteSpace(simpleMetadata.DistributionDetails.Name))
                        return "#5/355422/6668909/*/l/wfs/[" + RemoveQueryString(simpleMetadata.DistributionDetails.URL) + "]/+" + simpleMetadata.DistributionDetails.Name;
                    else
                        return "#5/355422/6668909/l/wfs/[" + RemoveQueryString(simpleMetadata.DistributionDetails.URL) + "]";
                }
                else if (simpleMetadata.DistributionDetails != null && !string.IsNullOrWhiteSpace(simpleMetadata.DistributionDetails.URL) && !string.IsNullOrWhiteSpace(simpleMetadata.DistributionDetails.Protocol) && simpleMetadata.DistributionDetails.Protocol.Contains(("OGC:WCS")))
                {
                    if (!string.IsNullOrWhiteSpace(simpleMetadata.DistributionDetails.Name))
                        return "#5/355422/6668909/*/l/wcs/[" + RemoveQueryString(simpleMetadata.DistributionDetails.URL) + "]/+" + simpleMetadata.DistributionDetails.Name;
                    else
                        return "#5/355422/6668909/l/wcs/[" + RemoveQueryString(simpleMetadata.DistributionDetails.URL) + "]";
                }

                else return "";
            }
            else return "";
        }
        private static string RemoveQueryString(string URL)
        {
            int startQueryString = URL.IndexOf("?");

            if (startQueryString != -1)
                URL = URL.Substring(0, startQueryString);

            return URL;
        }
    }
}