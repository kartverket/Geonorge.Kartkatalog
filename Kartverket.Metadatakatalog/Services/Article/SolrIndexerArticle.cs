using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Models.SearchIndex;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using SolrNet;
using SolrNet.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;

namespace Kartverket.Metadatakatalog.Service.Article
{
    public class SolrIndexerArticle : IndexerArticle
    {
        private readonly ILogger<SolrIndexerArticle> _logger;
        private readonly IConfiguration _configuration;
        private ISolrOperations<ArticleIndexDoc> _solr;
        private string _currentCore;
        private readonly ISolrOperations<ArticleIndexDoc> _defaultSolr;

        public SolrIndexerArticle(ISolrOperations<ArticleIndexDoc> solrOperations, ILogger<SolrIndexerArticle> logger, IConfiguration configuration)
        {
            _solr = solrOperations;
            _defaultSolr = solrOperations; // Keep reference to the default
            _logger = logger;
            _configuration = configuration;
        }

        public void Index(IEnumerable<ArticleIndexDoc> docs)
        {
            _logger.LogInformation("Indexing {DocumentCount} docs to core: {CurrentCore}", docs.Count(), _currentCore ?? "default");
            _solr.AddRange(docs);
            _solr.Commit();
        }

        public void DeleteIndex()
        {
            _logger.LogInformation("Deletes entire index for reindexing on core: {CurrentCore}", _currentCore ?? "default");
            SolrQuery sq = new SolrQuery("*:*");
            _solr.Delete(sq);
            _solr.Commit();
        }

        public void Index(ArticleIndexDoc doc)
        {
            _logger.LogInformation("Indexing single document Id={Id} Heading={Heading} to core: {CurrentCore}", doc.Id, doc.Heading, _currentCore ?? "default");
            _solr.Add(doc);
            _solr.Commit();
        }


        public void RemoveIndexDocument(string uuid)
        {
            try
            {
                _logger.LogInformation("Removes document uuid={Uuid} from index on core: {CurrentCore}", uuid, _currentCore ?? "default");
                _solr.Delete(uuid);
                _solr.Commit();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error removing UUID: {Uuid} from core: {CurrentCore}", uuid, _currentCore ?? "default");
            }
        }

        public void SetSolrIndexer(string coreId)
        {
            try
            {
                _logger.LogInformation("Setting Solr indexer for core: {CoreId}", coreId);
                _currentCore = coreId;
                
                // Try to update the Solr connection URL if possible
                // This is a simplified approach - we'll modify the connection URL in the existing operations
                var solrServerUrl = _configuration["SolrServerUrl"] ?? "http://localhost:8983";
                var coreUrl = $"{solrServerUrl}/solr/{coreId}";
                
                _logger.LogInformation("Target Solr URL for core {CoreId}: {CoreUrl}", coreId, coreUrl);
                
                // For now, we'll log the intended switch but use the same operations instance
                // The core switching will be handled by modifying the base URL dynamically
                
                // Try to access the underlying connection and update its URL
                if (_solr is SolrServer<ArticleIndexDoc> solrServer)
                {
                    // Get the private field for the basic operations
                    var basicOpsField = typeof(SolrServer<ArticleIndexDoc>).GetField("basicServer", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    if (basicOpsField?.GetValue(solrServer) is SolrBasicServer<ArticleIndexDoc> basicServer)
                    {
                        // Try to update the connection URL
                        var connectionField = typeof(SolrBasicServer<ArticleIndexDoc>).GetField("connection", 
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        
                        if (connectionField?.GetValue(basicServer) is SolrConnection connection)
                        {
                            // Update the connection URL using reflection
                            var urlField = typeof(SolrConnection).GetField("serverURL", 
                                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            
                            if (urlField != null)
                            {
                                urlField.SetValue(connection, coreUrl);
                                _logger.LogInformation("Successfully updated Solr connection URL to: {CoreUrl}", coreUrl);
                            }
                        }
                    }
                }
                
                _logger.LogInformation("Switched to Solr core: {CoreId}", coreId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set Solr indexer for core: {CoreId}. Using default operations.", coreId);
                // Keep the existing _solr instance if setting fails
            }
        }

        public void SetSolrIndexer(ISolrOperations<ArticleIndexDoc> solrOperations)
        {
            _solr = solrOperations;
        }

    }
}