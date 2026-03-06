using Kartverket.Metadatakatalog.App_Start;
using Kartverket.Metadatakatalog.Service;
using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Kartverket.Metadatakatalog.Controllers
{
    [EnableCors]
    [ApiController]
    [Route("api")]
    public class ApiMetadataController : ControllerBase
    {
        private readonly ILogger<ApiMetadataController> _logger;

        private readonly MetadataIndexer _indexer;
        private readonly IErrorService _errorService;

        public ApiMetadataController(MetadataIndexer indexer, IErrorService errorService, ILogger<ApiMetadataController> logger)
        {
            _indexer = indexer;
            _errorService = errorService;
            _logger = logger;
        }

        /// <summary>
        /// Metadata updated
        /// </summary>
        [Authorize(Roles = AuthConfig.DatasetProviderRole)]
        [Route("metadataupdated")]
        [HttpPost]
        public IActionResult MetadataUpdated([FromForm] IFormCollection metadata)
        {
            HttpStatusCode statusCode;

            string action = metadata["action"];
            string uuid = metadata["uuid"];
            string XMLFile = metadata["XMLFile"];

            try
            {
                _logger.LogInformation("Received notification of updated metadata: " + action + ", uuid=" + uuid);

                if (!string.IsNullOrWhiteSpace(uuid))
                {
                    _logger.LogInformation("Running single indexing of metadata with uuid=" + uuid);

                    _indexer.RunIndexingOn(uuid, action);

                    statusCode = HttpStatusCode.OK;
                }
                else
                {
                    _logger.LogWarning("Not indexing metadata - uuid was empty");
                    statusCode = HttpStatusCode.BadRequest;
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Exception while indexing single metadata.", e);
                _errorService.AddError(uuid, e);
                statusCode = HttpStatusCode.BadRequest;
            }
            return StatusCode((int)statusCode);
        }

        /// <summary>
        /// Run metadata indexing
        /// </summary>
        [Authorize(Roles = AuthConfig.DatasetProviderRole)]
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
        [Authorize(Roles = AuthConfig.DatasetProviderRole)]
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

        [Authorize(Roles = AuthConfig.DatasetProviderRole)]
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
        [Authorize(Roles = AuthConfig.DatasetProviderRole)]
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
