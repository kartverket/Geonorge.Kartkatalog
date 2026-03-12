using System.Collections.Generic;
using System.Linq;
using Kartverket.Metadatakatalog.Models;
using SolrNet;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net.Http;

namespace Kartverket.Metadatakatalog.Service
{
    public class SolrIndexerServices : IndexerService
    {
        private readonly ILogger<SolrIndexerServices> _logger;
        private readonly IConfiguration _configuration;
        private readonly ISolrOperationsFactory _solrFactory;
        private ISolrOperations<ServiceIndexDoc> _solr;
        private string _currentCore;

        public SolrIndexerServices(ISolrOperations<ServiceIndexDoc> solrOperations, ILogger<SolrIndexerServices> logger, IConfiguration configuration, ISolrOperationsFactory solrFactory)
        {
            _solr = solrOperations;
            _logger = logger;
            _configuration = configuration;
            _solrFactory = solrFactory;
        }

        public void Index(IEnumerable<ServiceIndexDoc> docs)
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

        public void Index(ServiceIndexDoc doc)
        {
            _logger.LogInformation("Indexing single document uuid={Uuid} title={Title} to core: {CurrentCore}", doc.Uuid, doc.Title, _currentCore ?? "default");
            _solr.Add(doc);
            _solr.Commit();
        }


        public void RemoveIndexDocument(string uuid)
        {
            try
            {
                _logger.LogInformation("=== INDEXER SERVICES REMOVE OPERATION START ===");
                _logger.LogInformation("Attempting to remove document uuid={Uuid} from IndexerService core: {CurrentCore}", uuid, _currentCore ?? "default");
                
                // Log the expected target and actual connection info
                var solrServerUrl = _configuration["SolrServerUrl"] ?? "http://localhost:8983";
                var expectedUrl = $"{solrServerUrl}/solr/{_currentCore}";
                _logger.LogInformation("IndexerService Expected target URL: {ExpectedUrl}", expectedUrl);
                
                // DIRECT APPROACH: Send HTTP request directly to the correct core
                if (!string.IsNullOrEmpty(_currentCore) && RemoveDocumentDirectly(uuid, _currentCore))
                {
                    _logger.LogInformation("IndexerService: Successfully removed document uuid={Uuid} using direct HTTP request to core: {CurrentCore}", uuid, _currentCore);
                }
                else
                {
                    // Fallback to regular SolrNet operations
                    _logger.LogInformation("IndexerService: Using fallback SolrNet operations for uuid={Uuid}", uuid);
                    
                    _logger.LogInformation("IndexerService: Calling _solr.Delete({Uuid}) - This should target: {ExpectedUrl}", uuid, expectedUrl);
                    _solr.Delete(uuid);
                    
                    _logger.LogInformation("IndexerService: Calling _solr.Commit() - This should target: {ExpectedUrl}", expectedUrl);
                    _solr.Commit();
                }
                
                _logger.LogInformation("=== INDEXER SERVICES REMOVE OPERATION END ===");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "IndexerService: Error removing UUID: {Uuid} from core: {CurrentCore}", uuid, _currentCore ?? "default");
                _logger.LogError("=== INDEXER SERVICES REMOVE OPERATION FAILED ===");
                throw; // Re-throw to ensure calling code knows about the failure
            }
        }

        private bool RemoveDocumentDirectly(string uuid, string coreId)
        {
            try
            {
                var solrServerUrl = _configuration["SolrServerUrl"] ?? "http://localhost:8983";
                var deleteUrl = $"{solrServerUrl}/solr/{coreId}/update?commit=true";
                
                _logger.LogInformation("IndexerService: Sending direct DELETE request to: {DeleteUrl} for uuid: {Uuid}", deleteUrl, uuid);
                
                var deleteXml = $"<delete><id>{uuid}</id></delete>";
                
                using var httpClient = new HttpClient();
                var content = new StringContent(deleteXml, System.Text.Encoding.UTF8, "application/xml");
                
                var response = httpClient.PostAsync(deleteUrl, content).Result;
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content.ReadAsStringAsync().Result;
                    _logger.LogInformation("IndexerService: Direct delete successful. Response: {Response}", responseContent);
                    return true;
                }
                else
                {
                    _logger.LogWarning("IndexerService: Direct delete failed with status: {StatusCode}", response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IndexerService: Error in direct delete for uuid: {Uuid} to core: {CoreId}", uuid, coreId);
                return false;
            }
        }

        public void SetSolrIndexer(string coreId)
        {
            try
            {
                _logger.LogInformation("=== INDEXER SERVICES CORE SWITCHING START ===");
                _logger.LogInformation("IndexerService: Setting Solr indexer for core: {CoreId}", coreId);
                _currentCore = coreId;
                
                // ACTUALLY SWITCH THE SOLR OPERATIONS INSTANCE USING FACTORY
                try
                {
                    if (_solrFactory != null && !string.IsNullOrEmpty(coreId))
                    {
                        var newSolrOperations = _solrFactory.GetOperations<ServiceIndexDoc>(coreId);
                        if (newSolrOperations != null)
                        {
                            _solr = newSolrOperations;
                            _logger.LogInformation("🔄 IndexerService: Successfully switched _solr instance to core: {CoreId} using Factory", coreId);
                        }
                        else
                        {
                            _logger.LogWarning("🚨 IndexerService: Factory returned null for core {CoreId}, keeping existing instance", coreId);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("🚨 IndexerService: Factory not available or invalid coreId, keeping existing instance");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "🚨 IndexerService: Error switching SolrOperations using Factory for core {CoreId}, keeping existing instance", coreId);
                }
                
                var solrServerUrl = _configuration["SolrServerUrl"] ?? "http://localhost:8983";
                var coreUrl = $"{solrServerUrl}/solr/{coreId}";
                
                _logger.LogInformation("IndexerService: Target Solr URL for core {CoreId}: {CoreUrl}", coreId, coreUrl);
                _logger.LogInformation("=== INDEXER SERVICES CORE SWITCHING END ===");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IndexerService: Failed to set Solr indexer for core: {CoreId}. Using default operations.", coreId);
            }
        }

        public void SetSolrIndexer(ISolrOperations<ServiceIndexDoc> solrOperations)
        {
            _solr = solrOperations;
        }
    }
}