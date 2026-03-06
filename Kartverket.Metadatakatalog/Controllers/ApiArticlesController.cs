using Kartverket.Metadatakatalog.ActionFilters;
using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Service;
using Kartverket.Metadatakatalog.Service.Article;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace Kartverket.Metadatakatalog.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [ApiAuthorize]
    public class ApiArticlesController : ControllerBase
    {
        private readonly ArticleIndexer _indexer;
        private readonly IErrorService _errorService;
        private readonly ILogger<ApiArticlesController> _logger;

        public ApiArticlesController(ArticleIndexer indexer, IErrorService errorService, ILogger<ApiArticlesController> logger)
        {
            _indexer = indexer;
            _errorService = errorService;
            _logger = logger;
        }

        /// <summary>
        /// Article updated
        /// </summary>
        [Route("api/articleupdated")]
        [HttpPost]
        public IActionResult ArticleUpdated(ArticleStatus article)
        {
            HttpStatusCode statusCode;

            string Id = article.Id;
            string status = article.Status;

            try
            {
                _logger.LogInformation("Received notification of updated article: Id={Id}, status: {Status}", Id, status);

                if (!string.IsNullOrWhiteSpace(Id))
                {
                    _logger.LogInformation("Running single indexing of article with Id={Id}", Id);

                    _indexer.RunIndexingOn(Id);

                    statusCode = HttpStatusCode.OK;
                }
                else
                {
                    _logger.LogWarning("Not indexing article - id was empty");
                    statusCode = HttpStatusCode.BadRequest;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception while indexing single article");
                _errorService.AddError(Id, e);
                statusCode = HttpStatusCode.BadRequest;
            }
            return StatusCode((int)statusCode);
        }

        /// <summary>
        /// Run article indexing
        /// </summary>
        [Route("api/index-articles")]
        [HttpGet]
        public IActionResult Index()
        {
            HttpStatusCode statusCode;

            try
            {
                _logger.LogInformation("Run indexing of entire article catalogue");
                DateTime start = DateTime.Now;

                _indexer.RunIndexing();

                DateTime stop = DateTime.Now;
                double seconds = stop.Subtract(start).TotalSeconds;
                _logger.LogInformation("Indexing finished after {Seconds} seconds", seconds);

                statusCode = HttpStatusCode.OK;

            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception while indexing articles");
                statusCode = HttpStatusCode.BadRequest;
            }
            return StatusCode((int)statusCode);
        }

        /// <summary>
        /// Run article re-indexing
        /// </summary>
        [Route("api/reindex-articles")]
        [HttpGet]
        public IActionResult ReIndex()
        {
            HttpStatusCode statusCode;

            try
            {
                _logger.LogInformation("Run re-indexing of entire article catalogue");
                DateTime start = DateTime.Now;

                _indexer.RunReIndexing();

                DateTime stop = DateTime.Now;
                double seconds = stop.Subtract(start).TotalSeconds;
                _logger.LogInformation("Indexing finished after {Seconds} seconds", seconds);

                statusCode = HttpStatusCode.OK;

            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception while re-indexing articles");
                statusCode = HttpStatusCode.BadRequest;
            }
            return StatusCode((int)statusCode);
        }
    }

    public class ArticleStatus
    {
        public string Id { get; set; }
        public string Status { get; set; }
    }
}
