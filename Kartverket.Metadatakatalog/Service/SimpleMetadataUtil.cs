using GeoNorgeAPI;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.AIPlatform.V1;
using Kartverket.Metadatakatalog.Helpers;
using Kartverket.Metadatakatalog.Models.Translations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Configuration;
using Value = Google.Protobuf.WellKnownTypes.Value;
using System.Linq;

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
        public static readonly bool UseVectorSearch = System.Convert.ToBoolean(WebConfigurationManager.AppSettings["AI:UseVectorSearch"]);
        static log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
                case "series":
                    res = "series";
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
            return !string.IsNullOrEmpty(otherConstraintsAccess) && (otherConstraintsAccess.ToLower() == "no restrictions" || otherConstraintsAccess.Contains("noLimitations"));
        }
        public static bool IsRestricted(SimpleMetadata simpleMetadata)
        {
            return simpleMetadata.Constraints != null && IsRestricted(!string.IsNullOrEmpty(simpleMetadata.Constraints.AccessConstraintsLink) ? simpleMetadata.Constraints.AccessConstraintsLink : simpleMetadata.Constraints.OtherConstraintsAccess);
        }
        public static bool IsRestricted(string accessConstraint)
        {
            if (string.IsNullOrEmpty(accessConstraint)) return false;
            return accessConstraint.ToLower() == "norway digital restricted" ||
                   accessConstraint.ToLower() == "Beskyttet" || accessConstraint.ToLower() == "restricted" || accessConstraint.Contains("INSPIRE_Directive_Article13_1d");
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
            return !string.IsNullOrWhiteSpace(simpleMetadataDistribution?.URL) /*&& !string.IsNullOrWhiteSpace(simpleMetadataDistribution.Protocol)*/ && (simpleMetadataDistribution.Protocol.Contains("OGC:WMS") || (!MapOnlyWms && simpleMetadataDistribution.Protocol.Contains("OGC:WFS"))) && (hierarchyLevel == "service" || hierarchyLevel == "servicelayer");
        }

        public static string GetCapabilitiesUrl(string url, string protocol)
        {
            return url;

            //if (string.IsNullOrWhiteSpace(url)) return "";
            //var tmp = url;
            //var startQueryString = tmp.IndexOf("?request=", StringComparison.Ordinal);

            //if (startQueryString != -1)
            //    tmp = tmp.Substring(0, startQueryString + 1);
            //else
            //    tmp = tmp + "?";

            //if (!string.IsNullOrWhiteSpace(protocol) && protocol.Contains(("OGC:WMS")))
            //    return tmp + "request=GetCapabilities&service=WMS";
            //if (!string.IsNullOrWhiteSpace(protocol) && protocol.Contains(("OGC:WFS")))
            //    return tmp + "request=GetCapabilities&service=WFS";
            //if (!string.IsNullOrWhiteSpace(protocol) && protocol.Contains(("OGC:WCS")))
            //    return tmp + "request=GetCapabilities&service=WCS";
            //if (!string.IsNullOrWhiteSpace(protocol) && protocol.Contains(("OGC:CSW")))
            //    return tmp + "request=GetCapabilities&service=CSW";
            //if (!string.IsNullOrWhiteSpace(protocol) && protocol.Contains(("OGC:WMTS")))
            //    return tmp + "request=GetCapabilities&service=wmts";

            //if (startQueryString != -1)
            //    return url;

            //if (tmp.EndsWith("?"))
            //    tmp = tmp.Remove(tmp.Length - 1);

            //return tmp;
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
            return !string.IsNullOrWhiteSpace(simpleMetadataDistribution?.URL) && !string.IsNullOrWhiteSpace(simpleMetadataDistribution.Protocol) && (simpleMetadataDistribution.Protocol.Contains("WWW:DOWNLOAD") || simpleMetadataDistribution.Protocol.Contains("GEONORGE:FILEDOWNLOAD") || simpleMetadataDistribution.Protocol.Contains("OPENDAP:OPENDAP")) && (hierarchyLevel == "dataset" || hierarchyLevel == "series");
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

        internal static string GetTypeTranslated(string Type)
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

        /// <summary>  
        /// Get Access Token From JSON Key Async  
        /// </summary>  
        /// <param name="jsonKeyFilePath">Path to your JSON Key file</param>  
        /// <param name="scopes">Scopes required in access token</param>  
        /// <returns>Access token as string Task</returns>  
        public static string GetAccessTokenFromJSONKeyAsync(string jsonKeyFilePath, params string[] scopes)
        {
            using (var stream = new FileStream(jsonKeyFilePath, FileMode.Open, FileAccess.Read))
            {
                var credentials = GoogleCredential
                    .FromStream(stream) // Loads key file  
                    .CreateScoped(scopes) // Gathers scopes requested  
                    .UnderlyingCredential // Gets the credentials  
                    .GetAccessTokenForRequestAsync(); // Gets the Access Token  

                return credentials.Result;
            }
        }

        /// <summary>  
        /// Get Access Token From JSON Key  
        /// </summary>  
        /// <param name="jsonKeyFilePath">Path to your JSON Key file</param>  
        /// <param name="scopes">Scopes required in access token</param>  
        /// <returns>Access token as string</returns>  
        public static string GetAccessTokenFromJSONKey(string jsonKeyFilePath, params string[] scopes)
        {
            return GetAccessTokenFromJSONKeyAsync(jsonKeyFilePath, scopes);
        }

        public static float[] CreateVectorEmbeddings(string text)
        {
            try { 
            var token = GetAccessTokenFromJSONKey(
            WebConfigurationManager.AppSettings["AI:Key"],
            "https://www.googleapis.com/auth/cloud-platform");

            string projectId = WebConfigurationManager.AppSettings["AI:ProjectId"];
            string locationId = WebConfigurationManager.AppSettings["AI:LocationId"]; //https://cloud.google.com/vertex-ai/docs/general/locations#europe
            string publisher = "google";
            string model = WebConfigurationManager.AppSettings["AI:Model"];

            var client = new PredictionServiceClientBuilder
            {
                Endpoint = $"{locationId}-aiplatform.googleapis.com",
                Credential = GoogleCredential.FromAccessToken(token)
            }.Build();

            // Configure the parent resource.
            var endpoint = EndpointName.FromProjectLocationPublisherModel(projectId, locationId, publisher, model);

            // Initialize request argument(s).
            var instances = new List<Value>
            {
                Value.ForStruct( new Google.Protobuf.WellKnownTypes.Struct()
                {
                    Fields =
                    {
                        ["content"] = Value.ForString(text),
                    }
                })
            };

            // Make the request.
            var response = client.Predict(endpoint, instances, null);

            // Parse and return the embedding vector count.
            var values = response.Predictions.First().StructValue.Fields["embeddings"].StructValue.Fields["values"].ListValue.Values;

            return values.Select(n => (float)n.NumberValue).ToArray();

            }
            catch (Exception e)
            {
                Log.Error("Error creating vector embeddings", e);
                return null;
            }
        }

    }
}