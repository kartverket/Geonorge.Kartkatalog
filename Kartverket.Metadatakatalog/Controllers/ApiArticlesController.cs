using Kartverket.Metadatakatalog.ActionFilters;
using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Service;
using Kartverket.Metadatakatalog.Service.Article;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace Kartverket.Metadatakatalog.Controllers
{
    [ApiAuthorize]
    public class ApiArticlesController : ApiController
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ArticleIndexer _indexer;
        private readonly IErrorService _errorService;

        public ApiArticlesController(ArticleIndexer indexer, IErrorService errorService)
        {
            _indexer = indexer;
            _errorService = errorService;
        }

        /// <summary>
        /// Article updated
        /// </summary>
        [System.Web.Http.Route("api/articleupdated")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult ArticleUpdated(FormDataCollection article)
        {
            HttpStatusCode statusCode;

            string Id = article.Get("id");
            string status = article.Get("status");

            try
            {
                Log.Info("Received notification of updated article: Id=" + Id + ", status: " + status);

                if (!string.IsNullOrWhiteSpace(Id))
                {
                    Log.Info("Running single indexing of article with Id=" + Id);

                    _indexer.RunIndexingOn(Id);

                    statusCode = HttpStatusCode.OK;
                }
                else
                {
                    Log.Warn("Not indexing article - id was empty");
                    statusCode = HttpStatusCode.BadRequest;
                }
            }
            catch (Exception e)
            {
                Log.Error("Exception while indexing single article.", e);
                _errorService.AddError(Id, e);
                statusCode = HttpStatusCode.BadRequest;
            }
            return StatusCode(statusCode);
        }

        /// <summary>
        /// Run article indexing
        /// </summary>
        [System.Web.Http.Route("api/index-articles")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult Index()
        {
            HttpStatusCode statusCode;

            try
            {
                Log.Info("Run indexing of entire article catalogue.");
                DateTime start = DateTime.Now;

                _indexer.RunIndexing();

                DateTime stop = DateTime.Now;
                double seconds = stop.Subtract(start).TotalSeconds;
                Log.Info(string.Format("Indexing fininshed after {0} seconds.", seconds));

                statusCode = HttpStatusCode.OK;

            }
            catch (Exception e)
            {
                Log.Error("Exception while indexing articles.", e);
                statusCode = HttpStatusCode.BadRequest;
            }
            return StatusCode(statusCode);
        }

        /// <summary>
        /// Run article re-indexing
        /// </summary>
        [System.Web.Http.Route("api/reindex-articles")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult ReIndex()
        {
            HttpStatusCode statusCode;

            try
            {
                Log.Info("Run re-indexing of entire article catalogue.");
                DateTime start = DateTime.Now;

                _indexer.RunReIndexing();

                DateTime stop = DateTime.Now;
                double seconds = stop.Subtract(start).TotalSeconds;
                Log.Info(string.Format("Indexing fininshed after {0} seconds.", seconds));

                statusCode = HttpStatusCode.OK;

            }
            catch (Exception e)
            {
                Log.Error("Exception while re-indexing articles.", e);
                statusCode = HttpStatusCode.BadRequest;
            }
            return StatusCode(statusCode);
        }
    }
}
