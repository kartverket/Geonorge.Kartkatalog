using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Kartverket.Metadatakatalog.Service.Search
{
    public interface IAiService {
       float[] GetPredictions(string text);
    }
    public class AiService : IAiService
    {
        // GoogleCredential's UnderlyingCredential has a built-in OAuth token cache that
        // refreshes ~5 min before expiry. Reuse one instance per key file path so the
        // ~hour-long token is actually shared across requests instead of regenerated.
        private static readonly ConcurrentDictionary<string, Lazy<ITokenAccess>> TokenAccessByKeyPath = new();

        private static readonly string[] CloudPlatformScope = { "https://www.googleapis.com/auth/cloud-platform" };

        private readonly Geonorge.Utilities.Organization.IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AiService> _logger;

        public bool UseVectorSearch => Convert.ToBoolean(_configuration["AI:UseVectorSearch"]);

        public AiService(Geonorge.Utilities.Organization.IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<AiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public float[] GetPredictions(string text)
        {
            if (!UseVectorSearch)
                return null;

            string projectId = _configuration["AI:ProjectId"];
            string locationId = _configuration["AI:LocationId"];
            string model = _configuration["AI:Model"];

            object infoForDebug = "Search for: " + text;
            try
            {
                var inputRequest = new
                {
                    instances = new[]
                    {
                        new { content = text }
                    }
                };

                var endpoint = $"https://{locationId}-aiplatform.googleapis.com/v1/projects/{projectId}/locations/{locationId}/publishers/google/models/{model}:predict";

                var token = GetAccessToken(_configuration["AI:Key"]);

                var client = _httpClientFactory.GetHttpClient();

                using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                request.Content = new StringContent(JsonConvert.SerializeObject(inputRequest), System.Text.Encoding.UTF8, "application/json");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                using var response = client.SendAsync(request).GetAwaiter().GetResult();
                var result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                infoForDebug = result;

                var jsonResponse = JsonConvert.DeserializeObject<dynamic>(result);
                var values = jsonResponse.predictions[0].embeddings.values;
                return ((IEnumerable<dynamic>)values).Select(v => (float)v).ToArray();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating vector embeddings returned: {DebugInfo}", infoForDebug);
                return null;
            }
        }

        private static string GetAccessToken(string jsonKeyFilePath)
        {
            var tokenAccess = TokenAccessByKeyPath.GetOrAdd(
                jsonKeyFilePath,
                path => new Lazy<ITokenAccess>(() =>
                {
                    using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                    return GoogleCredential
                        .FromStream(stream)
                        .CreateScoped(CloudPlatformScope)
                        .UnderlyingCredential;
                })).Value;

            return tokenAccess.GetAccessTokenForRequestAsync().GetAwaiter().GetResult();
        }
    }
}
