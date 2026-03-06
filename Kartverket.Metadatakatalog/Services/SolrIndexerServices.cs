using System.Collections.Generic;
using System.Linq;
using Kartverket.Metadatakatalog.Models;
using SolrNet;
using System;
using Microsoft.Extensions.Logging;

namespace Kartverket.Metadatakatalog.Service
{
    public class SolrIndexerServices : IndexerService
    {
        private readonly ILogger<SolrIndexerServices> _logger;
        private ISolrOperations<ServiceIndexDoc> _solr;

        public SolrIndexerServices(ISolrOperations<ServiceIndexDoc> solrOperations, ILogger<SolrIndexerServices> logger)
        {
            _solr = solrOperations;
            _logger = logger;
        }

        public void Index(IEnumerable<ServiceIndexDoc> docs)
        {
            _logger.LogInformation("Indexing {DocCount} docs", docs.Count());
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

        public void Index(ServiceIndexDoc doc)
        {
            _logger.LogInformation("Indexing single document uuid={Uuid} title={Title}", doc.Uuid, doc.Title);
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

        public void SetSolrIndexer(ISolrOperations<ServiceIndexDoc> solrOperations)
        {
            _solr = solrOperations;
        }
    }
}