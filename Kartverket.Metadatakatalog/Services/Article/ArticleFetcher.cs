using Kartverket.Metadatakatalog.Models.Article;
using log4net;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace Kartverket.Metadatakatalog.Service.Article
{
    public class ArticleFetcher : IArticleFetcher
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Geonorge.Utilities.Organization.IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        string endPointUri;

        public ArticleFetcher(Geonorge.Utilities.Organization.IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            endPointUri = _configuration["GeonorgeUrl"];
        }

        public async Task<List<ArticleDocument>> FetchArticleDocumentsAsync(string culture)
        {
            string url = endPointUri + "app-api/geonorgeapi/getarticles/" + culture;
            Log.Info("Fetching articles uri: " + url);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Accept", "application/json");
            HttpResponseMessage response = await _httpClientFactory.GetHttpClient().SendAsync(request).ConfigureAwait(false);
            Log.Debug($"Status from [getarticles={url}] was {response.StatusCode}");

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<ArticleDocument>>().ConfigureAwait(false);
        }

        public async Task<ArticleDocument> FetchArticleDocumentAsync(string articleId, string culture)
        {
            string url = endPointUri + "app-api/geonorgeapi/getarticle/" + articleId + "/" + culture;
            Log.Info("Fetching articles uri: " + url);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Accept", "application/json");
            HttpResponseMessage response = await _httpClientFactory.GetHttpClient().SendAsync(request).ConfigureAwait(false);
            Log.Debug($"Status from [getarticle={url}] was {response.StatusCode}");

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<ArticleDocument>().ConfigureAwait(false);
        }

    }
}