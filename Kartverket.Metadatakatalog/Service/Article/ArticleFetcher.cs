using Kartverket.Geonorge.Utilities.Organization;
using Kartverket.Metadatakatalog.Models.Article;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

namespace Kartverket.Metadatakatalog.Service.Article
{
    public class ArticleFetcher : IArticleFetcher
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IHttpClientFactory _httpClientFactory;

        string endPointUri = WebConfigurationManager.AppSettings["GeonorgeUrl"];

        public ArticleFetcher(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
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

            return await response.Content.ReadAsAsync<List<ArticleDocument>>().ConfigureAwait(false);
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

            return await response.Content.ReadAsAsync<ArticleDocument>().ConfigureAwait(false);
        }

    }
}