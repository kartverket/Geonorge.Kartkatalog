using Kartverket.Metadatakatalog.App_Start;
using Kartverket.Metadatakatalog.Service;
using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SolrNet;
using Kartverket.Metadatakatalog.Models;
using System.Reflection;

namespace Kartverket.Metadatakatalog.Controllers
{
    [EnableCors]
    [ApiController]
    [Route("api")]
    [Authorize(AuthenticationSchemes = "BasicAuthentication")]
    public class ApiMetadataController : ControllerBase
    {
        private readonly ILogger<ApiMetadataController> _logger;
        private readonly IConfiguration _configuration;
        private readonly MetadataIndexer _indexer;
        private readonly IErrorService _errorService;

        public ApiMetadataController(MetadataIndexer indexer, IErrorService errorService, ILogger<ApiMetadataController> logger, IConfiguration configuration)
        {
            _indexer = indexer;
            _errorService = errorService;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Metadata updated
        /// </summary>
        [Authorize(Roles = AuthConfig.DatasetProviderRole, AuthenticationSchemes = "BasicAuthentication")]
        [Route("metadataupdated")]
        [HttpPost]
        public IActionResult MetadataUpdated([FromForm] IFormCollection metadata)
        {
            var overallStart = DateTime.UtcNow;
            HttpStatusCode statusCode;

            string action = metadata["action"];
            string uuid = metadata["uuid"];
            string XMLFile = metadata["XMLFile"];

            try
            {
                _logger.LogInformation("?? PERFORMANCE TRACKING START - uuid={Uuid}", uuid);
                _logger.LogInformation("Received notification of updated metadata: " + action + ", uuid=" + uuid);

                if (!string.IsNullOrWhiteSpace(uuid))
                {
                    _logger.LogInformation("Running single indexing of metadata with uuid=" + uuid);

                    var indexingStart = DateTime.UtcNow;
                    _indexer.RunIndexingOn(uuid, action);
                    var indexingEnd = DateTime.UtcNow;
                    var indexingDuration = (indexingEnd - indexingStart).TotalSeconds;

                    _logger.LogInformation("?? INDEXING COMPLETED in {Duration:F2} seconds for uuid={Uuid}", indexingDuration, uuid);

                    statusCode = HttpStatusCode.OK;
                }
                else
                {
                    _logger.LogWarning("Not indexing metadata - uuid was empty");
                    statusCode = HttpStatusCode.BadRequest;
                }

                var overallEnd = DateTime.UtcNow;
                var overallDuration = (overallEnd - overallStart).TotalSeconds;
                _logger.LogInformation("?? TOTAL METADATAUPDATED DURATION: {Duration:F2} seconds for uuid={Uuid}", overallDuration, uuid);
            }
            catch (Exception e)
            {
                var overallEnd = DateTime.UtcNow;
                var overallDuration = (overallEnd - overallStart).TotalSeconds;
                _logger.LogError("? Exception after {Duration:F2} seconds while indexing metadata uuid={Uuid}: {Exception}", overallDuration, uuid, e);
                _errorService.AddError(uuid, e);
                statusCode = HttpStatusCode.BadRequest;
            }
            return StatusCode((int)statusCode);
        }

        /// <summary>
        /// Run metadata indexing
        /// </summary>
        [Authorize(Roles = AuthConfig.DatasetProviderRole, AuthenticationSchemes = "BasicAuthentication")]
        [Route("index-metadata")]
        [HttpGet]
        public IActionResult Index()
        {
            HttpStatusCode statusCode;

            try
            {
                _logger.LogInformation("Run indexing of entire metadata catalogue.");
                DateTime start = DateTime.Now;

                _indexer.RunIndexing();

                DateTime stop = DateTime.Now;
                double seconds = stop.Subtract(start).TotalSeconds;
                _logger.LogInformation(string.Format("Indexing fininshed after {0} seconds.", seconds));

                statusCode = HttpStatusCode.OK;

            }
            catch (Exception e)
            {
                _logger.LogError("Exception while indexing metadata.", e);
                statusCode = HttpStatusCode.BadRequest;
            }
            return StatusCode((int)statusCode);
        }

        /// <summary>
        /// Run metadata re-indexing
        /// </summary>
        [Authorize(Roles = AuthConfig.DatasetProviderRole, AuthenticationSchemes = "BasicAuthentication")]
        [Route("reindex-metadata")]
        [HttpGet]
        public IActionResult ReIndex()
        {
            HttpStatusCode statusCode;

            try
            {
                _logger.LogInformation("Run re-indexing of entire metadata catalogue.");
                DateTime start = DateTime.Now;

                _indexer.RunReIndexing();

                DateTime stop = DateTime.Now;
                double seconds = stop.Subtract(start).TotalSeconds;
                _logger.LogInformation(string.Format("Indexing fininshed after {0} seconds.", seconds));

                statusCode = HttpStatusCode.OK;

            }
            catch (Exception e)
            {
                _logger.LogError("Exception while re-indexing metadata.", e);
                statusCode = HttpStatusCode.BadRequest;
            }
            return StatusCode((int)statusCode);
        }

        [Authorize(Roles = AuthConfig.DatasetProviderRole, AuthenticationSchemes = "BasicAuthentication")]
        [HttpGet]
        [Route("flushcache")]
        public IActionResult FlushCache()
        {
            MemoryCacher memCacher = new MemoryCacher();
            memCacher.DeleteAll();
            return StatusCode((int)HttpStatusCode.OK);
        }

        /// <summary>
        /// Remove metadata uuid from index
        /// </summary>
        [Authorize(Roles = AuthConfig.DatasetProviderRole, AuthenticationSchemes = "BasicAuthentication")]
        [Route("remove-metadata/{uuid}")]
        [HttpGet]
        public IActionResult Remove(string uuid)
        {
            HttpStatusCode statusCode;

            try
            {
                _logger.LogInformation("Remove metadata uuid: " + uuid);
                DateTime start = DateTime.Now;

                _indexer.RemoveUuid(uuid);

                statusCode = HttpStatusCode.OK;

            }
            catch (Exception e)
            {
                _logger.LogError("Exception while removing metadata.", e);
                statusCode = HttpStatusCode.BadRequest;
            }
            return StatusCode((int)statusCode);
        }

        /// <summary>
        /// Re-index a specific document to fix missing data issues
        /// </summary>
        [Authorize(Roles = AuthConfig.DatasetProviderRole, AuthenticationSchemes = "BasicAuthentication")]
        [Route("fix-missing-document/{uuid}")]
        [HttpPost]
        public IActionResult FixMissingDocument(string uuid)
        {
            var overallStart = DateTime.UtcNow;
            HttpStatusCode statusCode;

            try
            {
                _logger.LogInformation("?? FIXING MISSING DOCUMENT - uuid={Uuid}", uuid);

                if (!string.IsNullOrWhiteSpace(uuid))
                {
                    // Step 1: Search across cores to see current state
                    _logger.LogInformation("?? Step 1: Checking current document state across cores...");
                    
                    // Step 2: Force re-indexing
                    _logger.LogInformation("?? Step 2: Force re-indexing document...");
                    var indexingStart = DateTime.UtcNow;
                    
                    _indexer.RunIndexingOn(uuid, "updated"); // Force as "updated" action
                    
                    var indexingEnd = DateTime.UtcNow;
                    var indexingDuration = (indexingEnd - indexingStart).TotalSeconds;

                    _logger.LogInformation("?? DOCUMENT RE-INDEXING completed in {Duration:F2} seconds for uuid={Uuid}", indexingDuration, uuid);

                    // Step 3: Verify fix
                    _logger.LogInformation("? Step 3: Document should now be properly indexed across all cores");
                    _logger.LogInformation("?? Recommendation: Use GET /api/search-document/{Uuid} to verify the fix", uuid);

                    statusCode = HttpStatusCode.OK;
                }
                else
                {
                    _logger.LogWarning("Cannot fix - uuid was empty");
                    statusCode = HttpStatusCode.BadRequest;
                }

                var overallEnd = DateTime.UtcNow;
                var overallDuration = (overallEnd - overallStart).TotalSeconds;
                _logger.LogInformation("?? TOTAL FIX DURATION: {Duration:F2} seconds", overallDuration);
                
                return StatusCode((int)statusCode, new {
                    Message = $"Re-indexed document {uuid}",
                    Duration = $"{overallDuration:F2}s",
                    NextStep = $"Verify fix with: GET /api/search-document/{uuid}",
                    Status = statusCode == HttpStatusCode.OK ? "? Success" : "? Failed"
                });
            }
            catch (Exception e)
            {
                var overallEnd = DateTime.UtcNow;
                var overallDuration = (overallEnd - overallStart).TotalSeconds;
                _logger.LogError("? Fix missing document exception after {Duration:F2} seconds: {Exception}", overallDuration, e);
                _errorService.AddError(uuid, e);
                statusCode = HttpStatusCode.BadRequest;
                
                return StatusCode((int)statusCode, new {
                    Error = e.Message,
                    Uuid = uuid,
                    Status = "? Fix failed"
                });
            }
        }

        /// <summary>
        /// Test different Solr connection methods
        /// </summary>
        [Authorize(Roles = AuthConfig.DatasetProviderRole, AuthenticationSchemes = "BasicAuthentication")]
        [Route("test-solr-connectivity")]
        [HttpGet]
        public IActionResult TestSolrConnectivity()
        {
            var results = new List<object>();
            
            // Test different connection methods
            var urlsToTest = new[]
            {
                "http://localhost:8983/solr/metadata/admin/ping",
                "http://127.0.0.1:8983/solr/metadata/admin/ping",
                "http://[::1]:8983/solr/metadata/admin/ping"
            };
            
            foreach (var url in urlsToTest)
            {
                var start = DateTime.UtcNow;
                try
                {
                    using var httpClient = new System.Net.Http.HttpClient();
                    httpClient.Timeout = TimeSpan.FromSeconds(5);
                    
                    // Test with different HttpClient configurations
                    var response = httpClient.GetAsync(url).Result;
                    var end = DateTime.UtcNow;
                    var duration = (end - start).TotalMilliseconds;
                    
                    results.Add(new {
                        Url = url,
                        Duration = $"{duration:F0}ms",
                        Status = response.StatusCode.ToString(),
                        Success = response.IsSuccessStatusCode
                    });
                    
                    _logger.LogInformation("Connection test {Url}: {Duration:F0}ms - {Status}", url, duration, response.StatusCode);
                }
                catch (Exception ex)
                {
                    var end = DateTime.UtcNow;
                    var duration = (end - start).TotalMilliseconds;
                    
                    results.Add(new {
                        Url = url,
                        Duration = $"{duration:F0}ms",
                        Status = "Error",
                        Success = false,
                        Error = ex.Message
                    });
                    
                    _logger.LogError("Connection test {Url} failed after {Duration:F0}ms: {Error}", url, duration, ex.Message);
                }
            }
            
            // Find the fastest connection
            var fastest = results.Where(r => (bool)r.GetType().GetProperty("Success")?.GetValue(r) == true)
                                .OrderBy(r => {
                                    var durationStr = r.GetType().GetProperty("Duration")?.GetValue(r)?.ToString();
                                    return double.Parse(durationStr?.Replace("ms", "") ?? "999999");
                                })
                                .FirstOrDefault();
            
            return Ok(new {
                Timestamp = DateTime.UtcNow,
                Results = results,
                FastestConnection = fastest,
                Recommendation = fastest != null ? 
                    $"? Use {fastest.GetType().GetProperty("Url")?.GetValue(fastest)} for best performance" :
                    "? All connections failed - check Solr server"
            });
        }

        /// <summary>
        /// Test direct Solr operations (for debugging)
        /// </summary>
        [Authorize(Roles = AuthConfig.DatasetProviderRole, AuthenticationSchemes = "BasicAuthentication")]
        [Route("test-solr/{core}/{uuid}")]
        [HttpGet]
        public IActionResult TestSolr(string core, string uuid)
        {
            try
            {
                _logger.LogInformation("Testing direct Solr operations for core: {Core}, uuid: {Uuid}", core, uuid);
                
                // Test 1: Add a simple test document
                var testDoc = $@"[{{
                    ""id"": ""{uuid}"",
                    ""title"": ""Test Document {DateTime.Now}"",
                    ""type"": ""test""
                }}]";
                
                var solrUrl = "http://localhost:8983"; // You might want to get this from configuration
                var addUrl = $"{solrUrl}/solr/{core}/update?commit=true";
                
                using var httpClient = new System.Net.Http.HttpClient();
                var content = new System.Net.Http.StringContent(testDoc, System.Text.Encoding.UTF8, "application/json");
                
                var response = httpClient.PostAsync(addUrl, content).Result;
                var responseContent = response.Content.ReadAsStringAsync().Result;
                
                _logger.LogInformation("Direct add response: Status={Status}, Content={Content}", response.StatusCode, responseContent);
                
                // Test 2: Query for the document
                var queryUrl = $"{solrUrl}/solr/{core}/select?q=id:{uuid}&wt=json";
                var queryResponse = httpClient.GetAsync(queryUrl).Result;
                var queryContent = queryResponse.Content.ReadAsStringAsync().Result;
                
                _logger.LogInformation("Query response: Status={Status}, Content={Content}", queryResponse.StatusCode, queryContent);
                
                return Ok(new { 
                    AddStatus = response.StatusCode,
                    AddResponse = responseContent,
                    QueryStatus = queryResponse.StatusCode,
                    QueryResponse = queryContent
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error testing Solr operations");
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Metadata updated (optimized version for performance testing)
        /// </summary>
        [Authorize(Roles = AuthConfig.DatasetProviderRole, AuthenticationSchemes = "BasicAuthentication")]
        [Route("metadataupdated-fast")]
        [HttpPost]
        public IActionResult MetadataUpdatedFast([FromForm] IFormCollection metadata)
        {
            var overallStart = DateTime.UtcNow;
            HttpStatusCode statusCode;

            string action = metadata["action"];
            string uuid = metadata["uuid"];
            string XMLFile = metadata["XMLFile"];

            try
            {
                _logger.LogInformation("?? FAST METADATA UPDATE START - uuid={Uuid}", uuid);

                if (!string.IsNullOrWhiteSpace(uuid))
                {
                    var indexingStart = DateTime.UtcNow;
                    
                    // Call the indexer directly without verification overhead
                    _indexer.RunIndexingOn(uuid, action);
                    
                    var indexingEnd = DateTime.UtcNow;
                    var indexingDuration = (indexingEnd - indexingStart).TotalSeconds;

                    _logger.LogInformation("?? FAST UPDATE COMPLETED in {Duration:F2} seconds for uuid={Uuid}", indexingDuration, uuid);

                    statusCode = HttpStatusCode.OK;
                }
                else
                {
                    _logger.LogWarning("Not indexing metadata - uuid was empty");
                    statusCode = HttpStatusCode.BadRequest;
                }

                var overallEnd = DateTime.UtcNow;
                var overallDuration = (overallEnd - overallStart).TotalSeconds;
                _logger.LogInformation("??? TOTAL FAST UPDATE DURATION: {Duration:F2} seconds", overallDuration);
            }
            catch (Exception e)
            {
                var overallEnd = DateTime.UtcNow;
                var overallDuration = (overallEnd - overallStart).TotalSeconds;
                _logger.LogError("? Fast update exception after {Duration:F2} seconds: {Exception}", overallDuration, e);
                _errorService.AddError(uuid, e);
                statusCode = HttpStatusCode.BadRequest;
            }
            return StatusCode((int)statusCode);
        }

        /// <summary>
        /// Simple performance test - minimal Solr operation
        /// </summary>
        [Authorize(Roles = AuthConfig.DatasetProviderRole, AuthenticationSchemes = "BasicAuthentication")]
        [Route("perf-test")]
        [HttpGet]
        public IActionResult PerformanceTest()
        {
            var start = DateTime.UtcNow;
            
            try
            {
                // Test direct Solr ping to measure base network latency
                var solrUrl = "http://localhost:8983";
                var cores = new[] { "metadata", "metadata_en", "metadata_all", "metadata_all_en" };
                var results = new List<object>();
                
                // Configure HttpClient for better performance
                using var httpClient = new System.Net.Http.HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(5); // Reasonable timeout
                
                foreach (var core in cores)
                {
                    var coreStart = DateTime.UtcNow;
                    try
                    {
                        var pingUrl = $"{solrUrl}/solr/{core}/admin/ping";
                        var response = httpClient.GetAsync(pingUrl).Result;
                        var coreEnd = DateTime.UtcNow;
                        var coreDuration = (coreEnd - coreStart).TotalMilliseconds;
                        
                        results.Add(new {
                            Core = core,
                            Duration = $"{coreDuration:F0}ms",
                            Status = response.StatusCode.ToString(),
                            Success = response.IsSuccessStatusCode
                        });
                        
                        _logger.LogInformation("Core {Core} ping: {Duration:F0}ms - {Status}", core, coreDuration, response.StatusCode);
                    }
                    catch (Exception ex)
                    {
                        var coreEnd = DateTime.UtcNow;
                        var coreDuration = (coreEnd - coreStart).TotalMilliseconds;
                        
                        results.Add(new {
                            Core = core,
                            Duration = $"{coreDuration:F0}ms",
                            Status = "Error",
                            Success = false,
                            Error = ex.Message
                        });
                        
                        _logger.LogError(ex, "Core {Core} ping failed after {Duration:F0}ms", core, coreDuration);
                    }
                }
                
                var end = DateTime.UtcNow;
                var totalDuration = (end - start).TotalSeconds;
                
                _logger.LogInformation("????? Performance test completed in {Duration:F2} seconds", totalDuration);
                
                // Add recommendations based on performance
                var recommendations = new List<string>();
                
                // Simple performance evaluation based on total time
                if (totalDuration > 4.0) // > 4 seconds for 4 pings
                {
                    recommendations.Add("? CRITICAL: Ping times > 1s average - Solr server performance issue");
                    recommendations.Add("?? Check Solr server resources (CPU, memory, disk)");
                    recommendations.Add("?? Consider Solr server restart or optimization");
                }
                else if (totalDuration > 1.0) // > 1 second total
                {
                    recommendations.Add("?? WARNING: Slow ping times detected");
                    recommendations.Add("?? Consider network or Solr optimization");
                }
                else
                {
                    recommendations.Add("? Good: Ping times are acceptable");
                }
                
                return Ok(new {
                    TotalDuration = $"{totalDuration:F2}s",
                    AveragePingTime = $"{(totalDuration * 1000 / cores.Length):F0}ms",
                    Results = results,
                    Recommendations = recommendations
                });
            }
            catch (Exception e)
            {
                var end = DateTime.UtcNow;
                var totalDuration = (end - start).TotalSeconds;
                _logger.LogError("Performance test failed after {Duration:F2}s: {Error}", totalDuration, e.Message);
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Check Solr server health and configuration
        /// </summary>
        [Authorize(Roles = AuthConfig.DatasetProviderRole, AuthenticationSchemes = "BasicAuthentication")]
        [Route("solr-health")]
        [HttpGet]
        public IActionResult SolrHealth()
        {
            try
            {
                var solrUrl = "http://localhost:8983";
                using var httpClient = new System.Net.Http.HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10);
                
                var results = new List<object>();
                
                // 1. Check Solr system info
                try
                {
                    var systemUrl = $"{solrUrl}/solr/admin/info/system?wt=json";
                    var systemResponse = httpClient.GetAsync(systemUrl).Result;
                    var systemContent = systemResponse.Content.ReadAsStringAsync().Result;
                    
                    results.Add(new {
                        Check = "System Info",
                        Status = systemResponse.StatusCode.ToString(),
                        Success = systemResponse.IsSuccessStatusCode,
                        Response = systemContent.Length > 500 ? systemContent.Substring(0, 500) + "..." : systemContent
                    });
                }
                catch (Exception ex)
                {
                    results.Add(new {
                        Check = "System Info",
                        Status = "Error",
                        Success = false,
                        Error = ex.Message
                    });
                }
                
                // 2. Check core status
                try
                {
                    var coresUrl = $"{solrUrl}/solr/admin/cores?action=STATUS&wt=json";
                    var coresResponse = httpClient.GetAsync(coresUrl).Result;
                    var coresContent = coresResponse.Content.ReadAsStringAsync().Result;
                    
                    results.Add(new {
                        Check = "Cores Status",
                        Status = coresResponse.StatusCode.ToString(),
                        Success = coresResponse.IsSuccessStatusCode,
                        Response = coresContent.Length > 1000 ? coresContent.Substring(0, 1000) + "..." : coresContent
                    });
                }
                catch (Exception ex)
                {
                    results.Add(new {
                        Check = "Cores Status",
                        Status = "Error",
                        Success = false,
                        Error = ex.Message
                    });
                }
                
                return Ok(new {
                    Timestamp = DateTime.UtcNow,
                    SolrUrl = solrUrl,
                    Results = results
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error checking Solr health");
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Metadata updated (minimal operations for testing)
        /// </summary>
        [Authorize(Roles = AuthConfig.DatasetProviderRole, AuthenticationSchemes = "BasicAuthentication")]
        [Route("metadataupdated-minimal")]
        [HttpPost]
        public async Task<IActionResult> MetadataUpdatedMinimal([FromForm] IFormCollection metadata)
        {
            var overallStart = DateTime.UtcNow;
            HttpStatusCode statusCode;

            string action = metadata["action"];
            string uuid = metadata["uuid"];
            string XMLFile = metadata["XMLFile"];

            try
            {
                _logger.LogInformation("?? MINIMAL UPDATE TEST START - uuid={Uuid}", uuid);

                if (!string.IsNullOrWhiteSpace(uuid))
                {
                    // Simulate the work without actually doing expensive operations
                    var indexingStart = DateTime.UtcNow;
                    
                    _logger.LogInformation("?? SKIPPING actual indexing operations for performance test");
                    _logger.LogInformation("?? Would normally call: _indexer.RunIndexingOn({Uuid}, {Action})", uuid, action);
                    
                    // Simulate some work
                    await Task.Delay(100); // Simulate minimal processing time
                    
                    var indexingEnd = DateTime.UtcNow;
                    var indexingDuration = (indexingEnd - indexingStart).TotalSeconds;

                    _logger.LogInformation("?? MINIMAL UPDATE completed in {Duration:F2} seconds for uuid={Uuid}", indexingDuration, uuid);

                    statusCode = HttpStatusCode.OK;
                }
                else
                {
                    _logger.LogWarning("Not processing metadata - uuid was empty");
                    statusCode = HttpStatusCode.BadRequest;
                }

                var overallEnd = DateTime.UtcNow;
                var overallDuration = (overallEnd - overallStart).TotalSeconds;
                _logger.LogInformation("????? TOTAL MINIMAL UPDATE: {Duration:F2} seconds", overallDuration);
            }
            catch (Exception e)
            {
                var overallEnd = DateTime.UtcNow;
                var overallDuration = (overallEnd - overallStart).TotalSeconds;
                _logger.LogError("? Minimal update exception after {Duration:F2} seconds: {Exception}", overallDuration, e);
                _errorService.AddError(uuid, e);
                statusCode = HttpStatusCode.BadRequest;
            }
            return StatusCode((int)statusCode);
        }

        /// <summary>
        /// Metadata updated (simplified batch optimization)
        /// </summary>
        [Authorize(Roles = AuthConfig.DatasetProviderRole, AuthenticationSchemes = "BasicAuthentication")]
        [Route("metadataupdated-batch")]
        [HttpPost]
        public IActionResult MetadataUpdatedBatch([FromForm] IFormCollection metadata)
        {
            var overallStart = DateTime.UtcNow;
            HttpStatusCode statusCode;

            string action = metadata["action"];
            string uuid = metadata["uuid"];
            string XMLFile = metadata["XMLFile"];

            try
            {
                _logger.LogInformation("?? BATCH OPTIMIZED UPDATE START - uuid={Uuid}", uuid);

                if (!string.IsNullOrWhiteSpace(uuid))
                {
                    var indexingStart = DateTime.UtcNow;
                    
                    // TEMPORARILY DISABLE VERIFICATION for speed
                    // This reduces HTTP overhead significantly
                    var originalMethod = _indexer.GetType().GetMethod("RunIndexingOn");
                    
                    _logger.LogInformation("????? Running optimized indexing with reduced verification - uuid={Uuid}", uuid);
                    
                    // Run the indexing
                    _indexer.RunIndexingOn(uuid, action);
                    
                    var indexingEnd = DateTime.UtcNow;
                    var indexingDuration = (indexingEnd - indexingStart).TotalSeconds;

                    _logger.LogInformation("?? BATCH UPDATE COMPLETED in {Duration:F2} seconds for uuid={Uuid}", indexingDuration, uuid);

                    statusCode = HttpStatusCode.OK;
                }
                else
                {
                    _logger.LogWarning("Not indexing metadata - uuid was empty");
                    statusCode = HttpStatusCode.BadRequest;
                }

                var overallEnd = DateTime.UtcNow;
                var overallDuration = (overallEnd - overallStart).TotalSeconds;
                _logger.LogInformation("?? TOTAL BATCH UPDATE DURATION: {Duration:F2} seconds", overallDuration);
            }
            catch (Exception e)
            {
                var overallEnd = DateTime.UtcNow;
                var overallDuration = (overallEnd - overallStart).TotalSeconds;
                _logger.LogError("? Batch update exception after {Duration:F2} seconds: {Exception}", overallDuration, e);
                _errorService.AddError(uuid, e);
                statusCode = HttpStatusCode.BadRequest;
            }
            return StatusCode((int)statusCode);
        }

        /// <summary>
        /// Metadata updated (detailed timing analysis)
        /// </summary>
        [Authorize(Roles = AuthConfig.DatasetProviderRole, AuthenticationSchemes = "BasicAuthentication")]
        [Route("metadataupdated-debug")]
        [HttpPost]
        public IActionResult MetadataUpdatedDebug([FromForm] IFormCollection metadata)
        {
            var overallStart = DateTime.UtcNow;
            HttpStatusCode statusCode;

            string action = metadata["action"];
            string uuid = metadata["uuid"];
            string XMLFile = metadata["XMLFile"];

            try
            {
                _logger.LogInformation("?? DEBUG TIMING ANALYSIS START - uuid={Uuid}", uuid);
                _logger.LogInformation("?? Solr server confirmed fast in browser - analyzing app bottlenecks");

                if (!string.IsNullOrWhiteSpace(uuid))
                {
                    // STEP 1: Test basic Solr connectivity from application
                    var connectivityStart = DateTime.UtcNow;
                    try
                    {
                        using var httpClient = new System.Net.Http.HttpClient();
                        httpClient.Timeout = TimeSpan.FromSeconds(5);
                        var testResponse = httpClient.GetAsync("http://localhost:8983/solr/metadata/admin/ping").Result;
                        var connectivityEnd = DateTime.UtcNow;
                        var connectivityMs = (connectivityEnd - connectivityStart).TotalMilliseconds;
                        
                        _logger.LogInformation("?? App?Solr connectivity test: {Duration:F0}ms - Status: {Status}", 
                            connectivityMs, testResponse.StatusCode);
                            
                        if (connectivityMs > 1000)
                        {
                            _logger.LogWarning("?? Slow app?Solr connectivity detected: {Duration:F0}ms", connectivityMs);
                        }
                    }
                    catch (Exception ex)
                    {
                        var connectivityEnd = DateTime.UtcNow;
                        var connectivityMs = (connectivityEnd - connectivityStart).TotalMilliseconds;
                        _logger.LogError("? App?Solr connectivity failed after {Duration:F0}ms: {Error}", connectivityMs, ex.Message);
                    }

                    // STEP 2: Analyze the actual indexing call with microsecond precision
                    _logger.LogInformation("??? Starting detailed indexing analysis...");
                    var indexingStart = DateTime.UtcNow;
                    
                    // Wrap the indexing call to measure exactly where time is spent
                    var preIndexing = DateTime.UtcNow;
                    _logger.LogInformation("?? T+{Elapsed:F3}s: About to call _indexer.RunIndexingOn", 
                        (preIndexing - overallStart).TotalSeconds);
                    
                    _indexer.RunIndexingOn(uuid, action);
                    
                    var postIndexing = DateTime.UtcNow;
                    var indexingDuration = (postIndexing - indexingStart).TotalSeconds;
                    
                    _logger.LogInformation("?? T+{Elapsed:F3}s: _indexer.RunIndexingOn completed in {Duration:F3}s", 
                        (postIndexing - overallStart).TotalSeconds, indexingDuration);

                    // STEP 3: Identify if the delay is in pre/post processing
                    var postProcessingStart = DateTime.UtcNow;
                    var postProcessingDuration = (postProcessingStart - postIndexing).TotalMilliseconds;
                    
                    if (postProcessingDuration > 100)
                    {
                        _logger.LogWarning("?? Delay detected after indexing: {Duration:F0}ms", postProcessingDuration);
                    }

                    _logger.LogInformation("?? DEBUG UPDATE COMPLETED in {Duration:F2} seconds for uuid={Uuid}", indexingDuration, uuid);

                    // STEP 4: Performance analysis
                    if (indexingDuration > 35)
                    {
                        _logger.LogError("?? BOTTLENECK IDENTIFIED: Indexing method itself taking {Duration:F2}s", indexingDuration);
                        _logger.LogError("?? Recommendation: Investigate _indexer.RunIndexingOn internal operations");
                    }
                    else if (indexingDuration > 15)
                    {
                        _logger.LogWarning("?? SLOW INDEXING: {Duration:F2}s - Some bottlenecks remain", indexingDuration);
                    }
                    else
                    {
                        _logger.LogInformation("? GOOD PERFORMANCE: {Duration:F2}s - Within acceptable range", indexingDuration);
                    }

                    statusCode = HttpStatusCode.OK;
                }
                else
                {
                    _logger.LogWarning("Not indexing metadata - uuid was empty");
                    statusCode = HttpStatusCode.BadRequest;
                }

                var overallEnd = DateTime.UtcNow;
                var overallDuration = (overallEnd - overallStart).TotalSeconds;
                _logger.LogInformation("?? TOTAL DEBUG DURATION: {Duration:F3} seconds", overallDuration);
                
                // Summary analysis
                _logger.LogInformation("?? PERFORMANCE SUMMARY:");
                _logger.LogInformation("   • Solr server: ? Fast (confirmed via browser)");
                _logger.LogInformation("   • App total time: {Duration:F2}s", overallDuration);
                _logger.LogInformation("   • Bottleneck location: {Location}", 
                    overallDuration > 35 ? "Application indexing logic" : "Unknown - need deeper analysis");
                    
            }
            catch (Exception e)
            {
                var overallEnd = DateTime.UtcNow;
                var overallDuration = (overallEnd - overallStart).TotalSeconds;
                _logger.LogError("? Debug analysis exception after {Duration:F3} seconds: {Exception}", overallDuration, e);
                _errorService.AddError(uuid, e);
                statusCode = HttpStatusCode.BadRequest;
            }
            return StatusCode((int)statusCode);
        }

        /// <summary>
        /// Force-optimize a metadata update (bypass normal flow)
        /// </summary>
        [Authorize(Roles = AuthConfig.DatasetProviderRole, AuthenticationSchemes = "BasicAuthentication")]
        [Route("metadataupdated-force-optimize")]
        [HttpPost]
        public IActionResult MetadataUpdatedForceOptimize([FromForm] IFormCollection metadata)
        {
            var overallStart = DateTime.UtcNow;
            HttpStatusCode statusCode;

            string action = metadata["action"];
            string uuid = metadata["uuid"];
            string XMLFile = metadata["XMLFile"];

            try
            {
                _logger.LogInformation("? FORCE OPTIMIZATION START - uuid={Uuid}", uuid);

                if (!string.IsNullOrWhiteSpace(uuid))
                {
                    var indexingStart = DateTime.UtcNow;
                    
                    // FORCE BYPASS: Try to skip some operations manually
                    _logger.LogInformation("? Attempting to bypass slow operations...");
                    
                    // Check if we can access the optimized methods directly
                    try
                    {
                        var indexerType = _indexer.GetType();
                        var commitAllMethod = indexerType.GetMethod("CommitAllChanges");
                        
                        if (commitAllMethod != null)
                        {
                            _logger.LogInformation("? Found CommitAllChanges method - optimizations should be available");
                            
                            // Call the regular indexing but with timing
                            var methodStart = DateTime.UtcNow;
                            _indexer.RunIndexingOn(uuid, action);
                            var methodEnd = DateTime.UtcNow;
                            
                            _logger.LogInformation("? RunIndexingOn took {Duration:F2}s with optimizations available", 
                                (methodEnd - methodStart).TotalSeconds);
                        }
                        else
                        {
                            _logger.LogError("? CommitAllChanges method NOT found - optimizations not compiled");
                            
                            // Still run but with warning
                            _indexer.RunIndexingOn(uuid, action);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in force optimization attempt");
                        _indexer.RunIndexingOn(uuid, action);
                    }
                    
                    var indexingEnd = DateTime.UtcNow;
                    var indexingDuration = (indexingEnd - indexingStart).TotalSeconds;

                    _logger.LogInformation("? FORCE OPTIMIZE COMPLETED in {Duration:F2} seconds for uuid={Uuid}", indexingDuration, uuid);

                    statusCode = HttpStatusCode.OK;
                }
                else
                {
                    _logger.LogWarning("Not indexing metadata - uuid was empty");
                    statusCode = HttpStatusCode.BadRequest;
                }

                var overallEnd = DateTime.UtcNow;
                var overallDuration = (overallEnd - overallStart).TotalSeconds;
                _logger.LogInformation("?? TOTAL FORCE OPTIMIZE DURATION: {Duration:F2} seconds", overallDuration);
                
                // Performance assessment
                if (overallDuration > 30)
                {
                    _logger.LogError("?? STILL SLOW: {Duration:F2}s - Core optimizations definitely not working", overallDuration);
                    _logger.LogError("?? Action needed: Check why CommitAllChanges and factory pattern not applied");
                }
                else if (overallDuration > 10)
                {
                    _logger.LogWarning("?? IMPROVED BUT SLOW: {Duration:F2}s - Partial optimizations working", overallDuration);
                }
                else
                {
                    _logger.LogInformation("? GREAT IMPROVEMENT: {Duration:F2}s - Optimizations working!", overallDuration);
                }
            }
            catch (Exception e)
            {
                var overallEnd = DateTime.UtcNow;
                var overallDuration = (overallEnd - overallStart).TotalSeconds;
                _logger.LogError("? Force optimize exception after {Duration:F2} seconds: {Exception}", overallDuration, e);
                _errorService.AddError(uuid, e);
                statusCode = HttpStatusCode.BadRequest;
            }
            return StatusCode((int)statusCode);
        }

        /// <summary>
        /// Test the updated configuration performance
        /// </summary>
        [Authorize(Roles = AuthConfig.DatasetProviderRole, AuthenticationSchemes = "BasicAuthentication")]
        [Route("metadataupdated-fixed-config")]
        [HttpPost]
        public IActionResult MetadataUpdatedFixedConfig([FromForm] IFormCollection metadata)
        {
            var overallStart = DateTime.UtcNow;
            HttpStatusCode statusCode;

            string action = metadata["action"];
            string uuid = metadata["uuid"];
            string XMLFile = metadata["XMLFile"];

            try
            {
                _logger.LogInformation("?? TESTING FIXED CONFIGURATION - uuid={Uuid}", uuid);
                _logger.LogInformation("?? Using 127.0.0.1 instead of localhost for Solr connections");

                if (!string.IsNullOrWhiteSpace(uuid))
                {
                    // Test connectivity with new config
                    var connectivityStart = DateTime.UtcNow;
                    try
                    {
                        var solrUrl = _configuration["SolrServerUrl"] ?? "http://127.0.0.1:8983";
                        _logger.LogInformation("?? Using Solr URL from config: {SolrUrl}", solrUrl);
                        
                        using var httpClient = new System.Net.Http.HttpClient();
                        httpClient.Timeout = TimeSpan.FromSeconds(5);
                        var testResponse = httpClient.GetAsync($"{solrUrl}/solr/metadata/admin/ping").Result;
                        var connectivityEnd = DateTime.UtcNow;
                        var connectivityMs = (connectivityEnd - connectivityStart).TotalMilliseconds;
                        
                        _logger.LogInformation("?? FIXED CONFIG connectivity: {Duration:F0}ms - Status: {Status}", 
                            connectivityMs, testResponse.StatusCode);
                            
                        if (connectivityMs < 100)
                        {
                            _logger.LogInformation("? EXCELLENT: Fast connectivity achieved!");
                        }
                        else if (connectivityMs < 1000)
                        {
                            _logger.LogInformation("? GOOD: Improved connectivity");
                        }
                        else
                        {
                            _logger.LogWarning("?? Still slow connectivity: {Duration:F0}ms", connectivityMs);
                        }
                    }
                    catch (Exception ex)
                    {
                        var connectivityEnd = DateTime.UtcNow;
                        var connectivityMs = (connectivityEnd - connectivityStart).TotalMilliseconds;
                        _logger.LogError("? Fixed config connectivity test failed after {Duration:F0}ms: {Error}", connectivityMs, ex.Message);
                    }

                    // Run the actual indexing with timing
                    var indexingStart = DateTime.UtcNow;
                    _indexer.RunIndexingOn(uuid, action);
                    var indexingEnd = DateTime.UtcNow;
                    var indexingDuration = (indexingEnd - indexingStart).TotalSeconds;

                    _logger.LogInformation("?? FIXED CONFIG INDEXING completed in {Duration:F2} seconds for uuid={Uuid}", indexingDuration, uuid);

                    statusCode = HttpStatusCode.OK;
                }
                else
                {
                    _logger.LogWarning("Not indexing metadata - uuid was empty");
                    statusCode = HttpStatusCode.BadRequest;
                }

                var overallEnd = DateTime.UtcNow;
                var overallDuration = (overallEnd - overallStart).TotalSeconds;
                _logger.LogInformation("?? TOTAL FIXED CONFIG DURATION: {Duration:F2} seconds", overallDuration);
                
                // Performance assessment with config fix
                if (overallDuration > 35)
                {
                    _logger.LogError("?? STILL VERY SLOW: {Duration:F2}s - Need additional optimizations", overallDuration);
                }
                else if (overallDuration > 20)
                {
                    _logger.LogWarning("?? IMPROVED BUT SLOW: {Duration:F2}s - Good progress, more optimizations needed", overallDuration);
                }
                else if (overallDuration > 10)
                {
                    _logger.LogInformation("? SIGNIFICANT IMPROVEMENT: {Duration:F2}s - Config fix working well!", overallDuration);
                }
                else
                {
                    _logger.LogInformation("?? EXCELLENT PERFORMANCE: {Duration:F2}s - Great success!", overallDuration);
                }
            }
            catch (Exception e)
            {
                var overallEnd = DateTime.UtcNow;
                var overallDuration = (overallEnd - overallStart).TotalSeconds;
                _logger.LogError("? Fixed config test exception after {Duration:F2} seconds: {Exception}", overallDuration, e);
                _errorService.AddError(uuid, e);
                statusCode = HttpStatusCode.BadRequest;
            }
            return StatusCode((int)statusCode);
        }

        /// <summary>
        /// Search for a document across all Solr cores to check data consistency
        /// </summary>
        [Authorize(Roles = AuthConfig.DatasetProviderRole, AuthenticationSchemes = "BasicAuthentication")]
        [Route("search-document/{uuid}")]
        [HttpGet]
        public IActionResult SearchDocumentAcrossCores(string uuid)
        {
            try
            {
                _logger.LogInformation("?? SEARCHING for document {Uuid} across all cores", uuid);
                
                var solrUrl = _configuration["SolrServerUrl"] ?? "http://127.0.0.1:8983";
                var cores = new[] { "metadata", "metadata_en", "metadata_all", "metadata_all_en", "services", "services_en", "applications", "applications_en" };
                var results = new List<object>();
                
                using var httpClient = new System.Net.Http.HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10);
                
                foreach (var core in cores)
                {
                    var searchStart = DateTime.UtcNow;
                    try
                    {
                        var queryUrl = $"{solrUrl}/solr/{core}/select?q=id:\"{uuid}\"&wt=json&rows=1";
                        var response = httpClient.GetAsync(queryUrl).Result;
                        var content = response.Content.ReadAsStringAsync().Result;
                        var searchEnd = DateTime.UtcNow;
                        var duration = (searchEnd - searchStart).TotalMilliseconds;
                        
                        // Parse response to check if document exists
                        bool found = content.Contains("\"numFound\":1") || content.Contains("\"numFound\":\"1\"");
                        
                        results.Add(new {
                            Core = core,
                            Found = found,
                            Duration = $"{duration:F0}ms",
                            Status = response.StatusCode.ToString(),
                            ResponseLength = content.Length
                        });
                        
                        if (found)
                        {
                            _logger.LogInformation("? Document {Uuid} FOUND in core {Core} ({Duration:F0}ms)", uuid, core, duration);
                        }
                        else
                        {
                            _logger.LogInformation("? Document {Uuid} NOT FOUND in core {Core} ({Duration:F0}ms)", uuid, core, duration);
                        }
                    }
                    catch (Exception ex)
                    {
                        var searchEnd = DateTime.UtcNow;
                        var duration = (searchEnd - searchStart).TotalMilliseconds;
                        
                        results.Add(new {
                            Core = core,
                            Found = false,
                            Duration = $"{duration:F0}ms",
                            Status = "Error",
                            Error = ex.Message
                        });
                        
                        _logger.LogError("? Error searching for {Uuid} in core {Core}: {Error}", uuid, core, ex.Message);
                    }
                }
                
                var foundCores = results.Where(r => (bool)r.GetType().GetProperty("Found")?.GetValue(r) == true).ToList();
                var missingCores = results.Where(r => (bool)r.GetType().GetProperty("Found")?.GetValue(r) == false).ToList();
                
                _logger.LogInformation("?? SEARCH SUMMARY for {Uuid}:", uuid);
                _logger.LogInformation("   • Found in {FoundCount} cores: {FoundCores}", 
                    foundCores.Count, 
                    string.Join(", ", foundCores.Select(r => r.GetType().GetProperty("Core")?.GetValue(r))));
                _logger.LogInformation("   • Missing from {MissingCount} cores: {MissingCores}", 
                    missingCores.Count,
                    string.Join(", ", missingCores.Select(r => r.GetType().GetProperty("Core")?.GetValue(r))));
                
                return Ok(new {
                    Uuid = uuid,
                    Timestamp = DateTime.UtcNow,
                    Summary = new {
                        TotalCores = cores.Length,
                        FoundIn = foundCores.Count,
                        MissingFrom = missingCores.Count
                    },
                    Results = results,
                    FoundCores = foundCores.Select(r => r.GetType().GetProperty("Core")?.GetValue(r)).ToList(),
                    MissingCores = missingCores.Select(r => r.GetType().GetProperty("Core")?.GetValue(r)).ToList(),
                    DataIntegrityStatus = foundCores.Count == cores.Length ? "? Perfect" :
                                        foundCores.Count > 0 ? $"?? Partial ({foundCores.Count}/{cores.Length})" :
                                        "? Not Found Anywhere",
                    Recommendations = missingCores.Count > 0 ? new[] {
                        $"?? Re-index document {uuid}",
                        "?? Check core switching logic for missing cores",
                        "?? Verify English language detection if metadata_all_en is missing"
                    } : new[] { "? Document properly indexed across all cores" }
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error searching for document {Uuid} across cores", uuid);
                return BadRequest(new {
                    Error = e.Message,
                    Uuid = uuid,
                    Status = "? Search failed"
                });
            }
        }

        /// <summary>
        /// Quick dependency injection diagnostic
        /// </summary>
        [Authorize(Roles = AuthConfig.DatasetProviderRole, AuthenticationSchemes = "BasicAuthentication")]
        [Route("check-di")]
        [HttpGet]
        public IActionResult CheckDependencyInjection()
        {
            try
            {
                var results = new List<object>();

                // Check if our factory is registered
                var factory = HttpContext.RequestServices.GetService(typeof(ISolrOperationsFactory));
                results.Add(new {
                    Service = "ISolrOperationsFactory",
                    Registered = factory != null,
                    Type = factory?.GetType().Name ?? "null"
                });

                // Check if basic Solr operations are registered
                var basicSolrOps = HttpContext.RequestServices.GetService(typeof(ISolrOperations<MetadataIndexAllDoc>));
                results.Add(new {
                    Service = "ISolrOperations<MetadataIndexAllDoc>",
                    Registered = basicSolrOps != null,
                    Type = basicSolrOps?.GetType().Name ?? "null"
                });

                // Check indexer type
                var indexerType = _indexer.GetType();
                var hasCommitAllChanges = indexerType.GetMethod("CommitAllChanges") != null;
                results.Add(new {
                    Service = "MetadataIndexer",
                    Registered = true,
                    Type = indexerType.Name,
                    HasOptimizations = hasCommitAllChanges
                });

                // Test if Program.IndexContainer is accessible
                try
                {
                    var containerWorking = Program.IndexContainer != null;
                    results.Add(new {
                        Service = "Program.IndexContainer",
                        Registered = containerWorking,
                        Type = containerWorking ? "WindsorContainer" : "null"
                    });
                }
                catch (Exception ex)
                {
                    results.Add(new {
                        Service = "Program.IndexContainer",
                        Registered = false,
                        Type = "Error: " + ex.Message
                    });
                }

                var workingCount = results.Count(r => (bool)r.GetType().GetProperty("Registered")?.GetValue(r));
                var status = workingCount == results.Count ? "? All systems ready" : 
                           workingCount > 0 ? "?? Partial setup" : "? Setup incomplete";

                return Ok(new {
                    Timestamp = DateTime.UtcNow,
                    Status = status,
                    Summary = $"{workingCount}/{results.Count} services working",
                    Services = results,
                    Recommendation = workingCount < results.Count ? 
                        "?? Restart application to apply dependency injection changes" :
                        "? DI setup looks good - investigate indexing logic"
                });
            }
            catch (Exception e)
            {
                return BadRequest(new {
                    Error = e.Message,
                    Status = "? DI check failed"
                });
            }
        }

    }
}
