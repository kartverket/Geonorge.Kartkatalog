using System.Collections.Generic;
using System.Linq;
using Kartverket.Metadatakatalog.Models;
using SolrNet;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Kartverket.Metadatakatalog;

namespace Kartverket.Metadatakatalog.Service
{
    public class SolrIndexerAll : IndexerAll
    {
        private readonly ILogger<SolrIndexerAll> _logger;
        private readonly IConfiguration _configuration;
        private readonly ISolrOperationsFactory _solrFactory;
        private ISolrOperations<MetadataIndexAllDoc> _solr;
        private string _currentCore;

        public SolrIndexerAll(ISolrOperations<MetadataIndexAllDoc> solrOperations, ILogger<SolrIndexerAll> logger, IConfiguration configuration, ISolrOperationsFactory solrFactory)
        {
            _solr = solrOperations;
            _logger = logger;
            _configuration = configuration;
            _solrFactory = solrFactory;
        }

        public void Index(IEnumerable<MetadataIndexAllDoc> docs)
        {
            _logger.LogInformation("Indexing {DocCount} docs to core: {CurrentCore}", docs.Count(), _currentCore ?? "default");
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

        public void Index(MetadataIndexAllDoc doc)
        {
            try
            {
                _logger.LogInformation("=== INDEXER ALL ADD OPERATION START ===");
                _logger.LogInformation("Indexing single document uuid={Uuid} title={Title} to core: {CurrentCore}", doc.Uuid, doc.Title, _currentCore ?? "default");
                
                var solrServerUrl = _configuration["SolrServerUrl"] ?? "http://localhost:8983";
                var expectedUrl = $"{solrServerUrl}/solr/{_currentCore}";
                _logger.LogInformation("IndexerAll Add: Expected target URL: {ExpectedUrl}", expectedUrl);
                
                // Use SolrNet operations - ADD ONLY (commit separately for better performance)
                _logger.LogInformation("IndexerAll: Using SolrNet operations for comprehensive document indexing - uuid={Uuid}", doc.Uuid);
                _logger.LogInformation("IndexerAll: Calling _solr.Add() - This should target: {ExpectedUrl}", expectedUrl);
                _solr.Add(doc);
                
                // NOTE: Commit is done separately in CommitPendingChanges() for better performance
                _logger.LogInformation("IndexerAll: Document added (commit pending) for uuid={Uuid}", doc.Uuid);
                
                _logger.LogInformation("=== INDEXER ALL ADD OPERATION END ===");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IndexerAll: Error indexing document uuid={Uuid} to core: {CurrentCore}", doc.Uuid, _currentCore ?? "default");
                _logger.LogError("=== INDEXER ALL ADD OPERATION FAILED ===");
                throw;
            }
        }

        public void CommitPendingChanges()
        {
            try
            {
                _logger.LogInformation("=== INDEXER ALL COMMIT START ===");
                _logger.LogInformation("IndexerAll: Committing pending changes to core: {CurrentCore}", _currentCore ?? "default");
                
                var solrServerUrl = _configuration["SolrServerUrl"] ?? "http://localhost:8983";
                var expectedUrl = $"{solrServerUrl}/solr/{_currentCore}";
                
                _logger.LogInformation("IndexerAll: Calling _solr.Commit() - This should target: {ExpectedUrl}", expectedUrl);
                _solr.Commit();
                _logger.LogInformation("IndexerAll: Commit completed for core: {CurrentCore}", _currentCore ?? "default");
                
                _logger.LogInformation("=== INDEXER ALL COMMIT END ===");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IndexerAll: Error committing changes to core: {CurrentCore}", _currentCore ?? "default");
                _logger.LogError("=== INDEXER ALL COMMIT FAILED ===");
                throw;
            }
        }

        public void RemoveIndexDocument(string uuid)
        {
            try
            {
                _logger.LogInformation("=== INDEXER ALL REMOVE OPERATION START ===");
                _logger.LogInformation("Attempting to remove document uuid={Uuid} from IndexerAll core: {CurrentCore}", uuid, _currentCore ?? "default");
                
                // Log the expected target and actual connection info
                var solrServerUrl = _configuration["SolrServerUrl"] ?? "http://localhost:8983";
                var expectedUrl = $"{solrServerUrl}/solr/{_currentCore}";
                _logger.LogInformation("IndexerAll Expected target URL: {ExpectedUrl}", expectedUrl);
                
                // DIRECT APPROACH: Send HTTP request directly to the correct core
                if (!string.IsNullOrEmpty(_currentCore) && RemoveDocumentDirectly(uuid, _currentCore))
                {
                    _logger.LogInformation("IndexerAll: Successfully removed document uuid={Uuid} using direct HTTP request to core: {CurrentCore}", uuid, _currentCore);
                }
                else
                {
                    // Fallback to regular SolrNet operations
                    _logger.LogInformation("IndexerAll: Using fallback SolrNet operations for uuid={Uuid}", uuid);
                    
                    _logger.LogInformation("IndexerAll: Calling _solr.Delete({Uuid}) - This should target: {ExpectedUrl}", uuid, expectedUrl);
                    _solr.Delete(uuid);
                    
                    _logger.LogInformation("IndexerAll: Calling _solr.Commit() - This should target: {ExpectedUrl}", expectedUrl);
                    _solr.Commit();
                }
                
                _logger.LogInformation("=== INDEXER ALL REMOVE OPERATION END ===");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "IndexerAll: Error removing UUID: {Uuid} from core: {CurrentCore}", uuid, _currentCore ?? "default");
                _logger.LogError("=== INDEXER ALL REMOVE OPERATION FAILED ===");
                throw; // Re-throw to ensure calling code knows about the failure
            }
        }

        private bool RemoveDocumentDirectly(string uuid, string coreId)
        {
            try
            {
                var solrServerUrl = _configuration["SolrServerUrl"] ?? "http://localhost:8983";
                var deleteUrl = $"{solrServerUrl}/solr/{coreId}/update?commit=true";
                
                _logger.LogInformation("IndexerAll: Sending direct DELETE request to: {DeleteUrl} for uuid: {Uuid}", deleteUrl, uuid);
                
                var deleteXml = $"<delete><id>{uuid}</id></delete>";
                
                using var httpClient = new HttpClient();
                var content = new StringContent(deleteXml, System.Text.Encoding.UTF8, "application/xml");
                
                var response = httpClient.PostAsync(deleteUrl, content).Result;
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content.ReadAsStringAsync().Result;
                    _logger.LogInformation("IndexerAll: Direct delete successful. Response: {Response}", responseContent);
                    return true;
                }
                else
                {
                    _logger.LogWarning("IndexerAll: Direct delete failed with status: {StatusCode}", response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IndexerAll: Error in direct delete for uuid: {Uuid} to core: {CoreId}", uuid, coreId);
                return false;
            }
        }

        public void SetSolrIndexer(string coreId)
        {
            try
            {
                _logger.LogInformation("=== INDEXER ALL CORE SWITCHING START ===");
                _logger.LogInformation("IndexerAll: Setting Solr indexer for core: {CoreId}", coreId);
                
                // VERIFY THE CORE ID VALUE
                _logger.LogInformation("🔍 CORE VERIFICATION - Received coreId: '{CoreId}'", coreId ?? "NULL");
                _logger.LogInformation("🔍 SolrCores.MetadataAll = '{MetadataAll}'", SolrCores.MetadataAll);
                _logger.LogInformation("🔍 SolrCores.MetadataAllEnglish = '{MetadataAllEnglish}'", SolrCores.MetadataAllEnglish);
                
                _currentCore = coreId;
                
                // ACTUALLY SWITCH THE SOLR OPERATIONS INSTANCE USING FACTORY
                try
                {
                    if (_solrFactory != null && !string.IsNullOrEmpty(coreId))
                    {
                        var newSolrOperations = _solrFactory.GetOperations<MetadataIndexAllDoc>(coreId);
                        if (newSolrOperations != null)
                        {
                            _solr = newSolrOperations;
                            _logger.LogInformation("🔄 Successfully switched _solr instance to core: {CoreId} using Factory", coreId);
                        }
                        else
                        {
                            _logger.LogWarning("🚨 Factory returned null for core {CoreId}, keeping existing instance", coreId);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("🚨 Factory not available or invalid coreId, keeping existing instance");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "🚨 Error switching SolrOperations using Factory for core {CoreId}, keeping existing instance", coreId);
                }
                
                var solrServerUrl = _configuration["SolrServerUrl"] ?? "http://localhost:8983";
                var coreUrl = $"{solrServerUrl}/solr/{coreId}";
                
                _logger.LogInformation("IndexerAll: Target Solr URL for core {CoreId}: {CoreUrl}", coreId, coreUrl);
                
                // VERIFY WHAT'S ACTUALLY STORED
                _logger.LogInformation("🔍 _currentCore is now set to: '{CurrentCore}'", _currentCore ?? "NULL");
                
                _logger.LogInformation("=== INDEXER ALL CORE SWITCHING END ===");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IndexerAll: Failed to set Solr indexer for core: {CoreId}. Using default operations.", coreId);
            }
        }

        public void SetSolrIndexer(ISolrOperations<MetadataIndexAllDoc> solrOperations)
        {
            _solr = solrOperations;
        }
    }
}