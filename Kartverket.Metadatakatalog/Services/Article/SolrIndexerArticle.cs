using Kartverket.Metadatakatalog;
using Kartverket.Metadatakatalog.Models.SearchIndex;
using Kartverket.Metadatakatalog.Service;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using SolrNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;

namespace Kartverket.Metadatakatalog.Service.Article
{
    public class SolrIndexerArticle : IndexerArticle
    {
        private readonly ILogger<SolrIndexerArticle> _logger;
        private readonly IConfiguration _configuration;
        private string _currentCore;

        public SolrIndexerArticle(ILogger<SolrIndexerArticle> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public void Index(IEnumerable<ArticleIndexDoc> docs)
        {
            var docList = docs as IList<ArticleIndexDoc> ?? docs.ToList();
            _logger.LogInformation("Indexing {DocumentCount} docs to core: {CurrentCore}", docList.Count, _currentCore ?? "default");
            if (docList.Count == 0) return;
            PostUpdateXml(BuildAddXml(docList), commit: true, opName: "AddRange");
        }

        public void Index(ArticleIndexDoc doc)
        {
            _logger.LogInformation("Indexing single document Id={Id} Heading={Heading} to core: {CurrentCore}", doc.Id, doc.Heading, _currentCore ?? "default");
            PostUpdateXml(BuildAddXml(new[] { doc }), commit: true, opName: "Add");
        }

        public void DeleteIndex()
        {
            _logger.LogInformation("Deletes entire index for reindexing on core: {CurrentCore}", _currentCore ?? "default");
            PostUpdateXml("<delete><query>*:*</query></delete>", commit: true, opName: "DeleteIndex");
        }

        public void RemoveIndexDocument(string uuid)
        {
            try
            {
                _logger.LogInformation("Removes document uuid={Uuid} from index on core: {CurrentCore}", uuid, _currentCore ?? "default");
                var body = $"<delete><id>{System.Security.SecurityElement.Escape(uuid)}</id></delete>";
                PostUpdateXml(body, commit: true, opName: "RemoveDocument");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error removing UUID: {Uuid} from core: {CurrentCore}", uuid, _currentCore ?? "default");
            }
        }

        public void SetSolrIndexer(string coreId)
        {
            _logger.LogInformation("Setting Solr indexer for core: {CoreId}", coreId);
            _currentCore = coreId;
        }

        private string BuildAddXml(IEnumerable<ArticleIndexDoc> docs)
        {
            var serializer = Program.IndexContainer.Resolve<ISolrDocumentSerializer<ArticleIndexDoc>>();
            var add = new XElement("add");
            foreach (var doc in docs)
            {
                add.Add(serializer.Serialize(doc, null));
            }
            return add.ToString(SaveOptions.DisableFormatting);
        }

        private void PostUpdateXml(string body, bool commit, string opName)
        {
            var coreId = _currentCore ?? SolrCores.Articles;
            var solrServerUrl = (_configuration["SolrServerUrl"] ?? "http://localhost:8983").TrimEnd('/');
            var url = $"{solrServerUrl}/solr/{coreId}/update" + (commit ? "?commit=true" : string.Empty);

            using var httpClient = new HttpClient();
            var content = new StringContent(body, Encoding.UTF8, "application/xml");
            var response = httpClient.PostAsync(url, content).GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode)
            {
                var respBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                _logger.LogError("{Op} failed {Status} for {Url}: {Body}", opName, response.StatusCode, url, respBody);
                throw new HttpRequestException($"Solr {opName} failed: {(int)response.StatusCode} {response.StatusCode}");
            }
        }
    }
}
