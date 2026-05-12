using System.Collections.Generic;
using System.Linq;
using Kartverket.Metadatakatalog.Models;
using SolrNet;
using SolrNet.Impl;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
using SolrNet.Commands.Parameters;
using System.Net.Http;
using Kartverket.Metadatakatalog;

namespace Kartverket.Metadatakatalog.Service
{
    public class SolrIndexer : Indexer
    {
        private readonly ILogger<SolrIndexer> _logger;
        private readonly IConfiguration _configuration;
        private readonly ISolrOperationsFactory _solrFactory;
        private ISolrOperations<MetadataIndexDoc> _solr;
        private string _currentCore;
        private readonly ISolrOperations<MetadataIndexDoc> _defaultSolr;
        private readonly ConcurrentDictionary<string, ISolrOperations<MetadataIndexDoc>> _coreOperations = new();

        public SolrIndexer(ISolrOperations<MetadataIndexDoc> solrOperations, ILogger<SolrIndexer> logger, IConfiguration configuration, ISolrOperationsFactory solrFactory)
        {
            _solr = solrOperations;
            _defaultSolr = solrOperations; // Keep reference to the default
            _logger = logger;
            _configuration = configuration;
            _solrFactory = solrFactory;
        }

        public void SetSolrIndexer(string coreId)
        {
            try
            {
                _logger.LogInformation("=== CORE SWITCHING START ===");
                _logger.LogInformation("Setting Solr indexer for core: {CoreId}", coreId);
                
                // VERIFY THE CORE ID VALUE
                _logger.LogInformation("🔍 MAIN INDEXER CORE VERIFICATION - Received coreId: '{CoreId}'", coreId ?? "NULL");
                
                _currentCore = coreId;
                
                // ACTUALLY SWITCH THE SOLR OPERATIONS INSTANCE USING FACTORY
                try
                {
                    if (_solrFactory != null && !string.IsNullOrEmpty(coreId))
                    {
                        var newSolrOperations = _solrFactory.GetOperations<MetadataIndexDoc>(coreId);
                        if (newSolrOperations != null)
                        {
                            _solr = newSolrOperations;
                            _logger.LogInformation("🔄 Main Indexer: Successfully switched _solr instance to core: {CoreId} using Factory", coreId);
                        }
                        else
                        {
                            _logger.LogWarning("🚨 Main Indexer: Factory returned null for core {CoreId}, keeping existing instance", coreId);
                            _solr = _defaultSolr;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("🚨 Main Indexer: Factory not available or invalid coreId, using default instance");
                        _solr = _defaultSolr;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "🚨 Main Indexer: Error switching SolrOperations using Factory for core {CoreId}, using default instance", coreId);
                    _solr = _defaultSolr;
                }
                
                var solrServerUrl = _configuration["SolrServerUrl"] ?? "http://localhost:8983";
                var coreUrl = $"{solrServerUrl}/solr/{coreId}";
                
                _logger.LogInformation("Target Solr URL for core {CoreId}: {CoreUrl}", coreId, coreUrl);
                _logger.LogInformation("=== CORE SWITCHING END ===");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set Solr indexer for core: {CoreId}. Using default operations.", coreId);
                _solr = _defaultSolr;
            }
        }

        public void SetSolrIndexer(ISolrOperations<MetadataIndexDoc> solrOperations)
        {
            _solr = solrOperations;
        }

        public void Index(IEnumerable<MetadataIndexDoc> docs)
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

        public void Index(MetadataIndexDoc doc)
        {
            _logger.LogInformation("Indexing single document uuid={Uuid} title={Title} to core: {CurrentCore}", doc.Uuid, doc.Title, _currentCore ?? "default");
            _solr.Add(doc);
            _solr.Commit();
        }


        public void RemoveIndexDocument(string uuid)
        {
            try
            {
                _logger.LogInformation("=== REMOVE OPERATION START ===");
                _logger.LogInformation("Attempting to remove document uuid={Uuid} from core: {CurrentCore}", uuid, _currentCore ?? "default");
                
                // Log the expected target and actual connection info
                var solrServerUrl = _configuration["SolrServerUrl"] ?? "http://localhost:8983";
                var expectedUrl = $"{solrServerUrl}/solr/{_currentCore}";
                _logger.LogInformation("Expected target URL: {ExpectedUrl}", expectedUrl);
                
                // DIRECT APPROACH: Send HTTP request directly to the correct core
                if (!string.IsNullOrEmpty(_currentCore) && RemoveDocumentDirectly(uuid, _currentCore))
                {
                    _logger.LogInformation("Successfully removed document uuid={Uuid} using direct HTTP request to core: {CurrentCore}", uuid, _currentCore);
                }
                else
                {
                    // Fallback to regular SolrNet operations
                    _logger.LogInformation("Using fallback SolrNet operations for uuid={Uuid}", uuid);
                    LogCurrentConnectionUrl();
                    
                    _logger.LogInformation("Calling _solr.Delete({Uuid}) - This should target: {ExpectedUrl}", uuid, expectedUrl);
                    _solr.Delete(uuid);
                    
                    _logger.LogInformation("Calling _solr.Commit() - This should target: {ExpectedUrl}", expectedUrl);
                    _solr.Commit();
                }
                
                _logger.LogInformation("=== REMOVE OPERATION END ===");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error removing UUID: {Uuid} from core: {CurrentCore}", uuid, _currentCore ?? "default");
                _logger.LogError("=== REMOVE OPERATION FAILED ===");
                throw; // Re-throw to ensure calling code knows about the failure
            }
        }

        private bool RemoveDocumentDirectly(string uuid, string coreId)
        {
            try
            {
                var solrServerUrl = _configuration["SolrServerUrl"] ?? "http://localhost:8983";
                var deleteUrl = $"{solrServerUrl}/solr/{coreId}/update?commit=true";
                
                _logger.LogInformation("Sending direct DELETE request to: {DeleteUrl} for uuid: {Uuid}", deleteUrl, uuid);
                
                var deleteXml = $"<delete><id>{uuid}</id></delete>";
                
                using var httpClient = new HttpClient();
                var content = new StringContent(deleteXml, System.Text.Encoding.UTF8, "application/xml");
                
                var response = httpClient.PostAsync(deleteUrl, content).Result;
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content.ReadAsStringAsync().Result;
                    _logger.LogInformation("Direct delete successful. Response: {Response}", responseContent);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Direct delete failed with status: {StatusCode}", response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in direct delete for uuid: {Uuid} to core: {CoreId}", uuid, coreId);
                return false;
            }
        }

        private void LogCurrentConnectionUrl()
        {
            try
            {
                _logger.LogInformation("Current target core: {CurrentCore}", _currentCore ?? "default");
                
                var solrServerUrl = _configuration["SolrServerUrl"] ?? "http://localhost:8983";
                var expectedUrl = $"{solrServerUrl}/solr/{_currentCore}";
                _logger.LogInformation("Expected target URL: {ExpectedUrl}", expectedUrl);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error determining expected target URL for core: {CurrentCore}", _currentCore ?? "default");
            }
        }
    }
}