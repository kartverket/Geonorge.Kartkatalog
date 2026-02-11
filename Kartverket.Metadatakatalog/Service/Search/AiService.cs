using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Configuration;
using System.Web.Helpers;

namespace Kartverket.Metadatakatalog.Service.Search
{
    public interface IAiService { 
       float[] GetPredictions(string text);
    }
    public class AiService : IAiService
    {
        private readonly Geonorge.Utilities.Organization.IHttpClientFactory _httpClientFactory;
        public static readonly bool UseVectorSearch = System.Convert.ToBoolean(WebConfigurationManager.AppSettings["AI:UseVectorSearch"]);
        static log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public AiService() { }
        public AiService(Geonorge.Utilities.Organization.IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>  
        /// Get Access Token From JSON Key Async  
        /// </summary>  
        /// <param name="jsonKeyFilePath">Path to your JSON Key file</param>  
        /// <param name="scopes">Scopes required in access token</param>  
        /// <returns>Access token as string Task</returns>  
        public string GetAccessTokenFromJSONKeyAsync(string jsonKeyFilePath, params string[] scopes)
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
        public string GetAccessTokenFromJSONKey(string jsonKeyFilePath, params string[] scopes)
        {
            return GetAccessTokenFromJSONKeyAsync(jsonKeyFilePath, scopes);
        }

        public float[] GetPredictions(string text)
        {
            if (SimpleMetadataUtil.UseVectorSearch)
            {
                object infoForDebug = "Search for: " + text;
                try
                {
                    var inputRequest = new
                    {
                        instances = new[]
{
                            new
                            {
                                content  = text
                            }
                        }
                    };

                    string projectId = WebConfigurationManager.AppSettings["AI:ProjectId"];
                    string locationId = WebConfigurationManager.AppSettings["AI:LocationId"];
                    string model = WebConfigurationManager.AppSettings["AI:Model"];

                    var endpoint = $"https://{locationId}-aiplatform.googleapis.com/v1/projects/{projectId}/locations/{locationId}/publishers/google/models/{model}:predict";

                    var token = GetAccessTokenFromJSONKey(
                    WebConfigurationManager.AppSettings["AI:Key"],
                    "https://www.googleapis.com/auth/cloud-platform");

                    System.Net.ServicePointManager.Expect100Continue = false; // otherwise google could return http 417 ExpectationFailed (with body ...your computer or network may be sending automated queries)
                    var client = _httpClientFactory.GetHttpClient();

                    var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                    request.Content = new StringContent(JsonConvert.SerializeObject(inputRequest), System.Text.Encoding.UTF8, "application/json");
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    var response = client.SendAsync(request);
                    var result = response.Result.Content.ReadAsStringAsync().Result;
                    infoForDebug = result;

                    var jsonResponse = JsonConvert.DeserializeObject<dynamic>(result);
                    var values = jsonResponse.predictions[0].embeddings.values;
                    float[] floatValues = ((IEnumerable<dynamic>)values).Select(v => (float)v).ToArray();

                    return floatValues;

                }
                catch (Exception e)
                {
                    Log.Error("Error creating vector embeddings returned: " + infoForDebug, e);
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
    }
}