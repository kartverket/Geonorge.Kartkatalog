using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Models.SearchIndex;
using Microsoft.Extensions.Logging;
using SolrNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Metadatakatalog.Service.Article
{
    public class SolrIndexerArticle : IndexerArticle
    {
        private readonly ILogger<SolrIndexerArticle> _logger;
        private ISolrOperations<ArticleIndexDoc> _solr;

        public SolrIndexerArticle(ISolrOperations<ArticleIndexDoc> solrOperations, ILogger<SolrIndexerArticle> logger)
        {
            _solr = solrOperations;
            _logger = logger;
        }

        public void Index(IEnumerable<ArticleIndexDoc> docs)
        {
            _logger.LogInformation("Indexing {DocumentCount} docs", docs.Count());
            _solr.AddRange(docs);
            _solr.Commit();
        }

        public void DeleteIndex()
        {
            _logger.LogInformation("Deletes entire index for reindexing");
            SolrQuery sq = new SolrQuery("*:*");
            _solr.Delete(sq);
            _solr.Commit();
        }

        public void Index(ArticleIndexDoc doc)
        {
            _logger.LogInformation("Indexing single document Id={Id} Heading={Heading}", doc.Id, doc.Heading);
            _solr.Add(doc);
            _solr.Commit();
        }


        public void RemoveIndexDocument(string uuid)
        {
            try
            {
                _logger.LogInformation("Removes document uuid={Uuid} from index", uuid);
                _solr.Delete(uuid);
                _solr.Commit();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error removing UUID: {Uuid}", uuid);
            }
        }

        public void SetSolrIndexer(string coreId)
        {
            // For backwards compatibility - this should be called from DI container setup
            // In practice, the ISolrOperations should be injected via constructor
        }

        public void SetSolrIndexer(ISolrOperations<ArticleIndexDoc> solrOperations)
        {
            _solr = solrOperations;
        }

    }
}