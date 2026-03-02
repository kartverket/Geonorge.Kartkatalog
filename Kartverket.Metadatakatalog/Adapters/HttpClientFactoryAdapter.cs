using System.Net.Http;

namespace Kartverket.Metadatakatalog.Adapters
{
    /// <summary>
    /// Adapter to bridge the custom IHttpClientFactory interface with the standard .NET HttpClientFactory
    /// </summary>
    public class HttpClientFactoryAdapter : Kartverket.Geonorge.Utilities.Organization.IHttpClientFactory
    {
        private readonly System.Net.Http.IHttpClientFactory _httpClientFactory;

        public HttpClientFactoryAdapter(System.Net.Http.IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Creates an HttpClient using the standard .NET HttpClientFactory
        /// </summary>
        /// <param name="name">The logical name of the client to create</param>
        /// <returns>A new HttpClient instance</returns>
        public HttpClient CreateClient(string name = null)
        {
            return _httpClientFactory.CreateClient(name ?? string.Empty);
        }

        /// <summary>
        /// Creates an HttpClient for the given type using the standard .NET HttpClientFactory
        /// </summary>
        /// <typeparam name="T">The type to create a client for</typeparam>
        /// <returns>A new HttpClient instance</returns>
        public HttpClient CreateClient<T>()
        {
            return _httpClientFactory.CreateClient(typeof(T).Name);
        }

        /// <summary>
        /// Implementation of GetHttpClient method required by the interface
        /// </summary>
        /// <returns>A new HttpClient instance</returns>
        public HttpClient GetHttpClient()
        {
            return _httpClientFactory.CreateClient();
        }
    }
}