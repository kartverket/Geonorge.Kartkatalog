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
    [ApiExplorerSettings(IgnoreApi = true)]
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

    }
}
