using System.Collections.Generic;
using System.Linq;
using Kartverket.Metadatakatalog.Models;
using SolrNet;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kartverket.Metadatakatalog.Service
{
    public class SolrIndexerApplication : IndexerApplication
    {
        private readonly ILogger<SolrIndexerApplication> _logger;
        private ISolrOperations<ApplicationIndexDoc> _solr;
        private readonly IServiceProvider _serviceProvider;

        public SolrIndexerApplication(IServiceProvider serviceProvider, ILogger<SolrIndexerApplication> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _solr = GetSolrOperations(SolrCores.Applications);
        }

        private ISolrOperations<ApplicationIndexDoc> GetSolrOperations(string coreId)
        {
            // Use the Windsor container until fully migrated to native DI
            return Program.IndexContainer.Resolve<ISolrOperations<ApplicationIndexDoc>>(coreId);
        }

        public void Index(IEnumerable<ApplicationIndexDoc> docs)
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

        public void Index(ApplicationIndexDoc doc)
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
            _solr = GetSolrOperations(coreId);
        }
    }
}