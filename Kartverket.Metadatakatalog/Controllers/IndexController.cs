﻿using System;
using System.Net;
using System.Web.Mvc;
using Kartverket.Metadatakatalog.Service;

namespace Kartverket.Metadatakatalog.Controllers
{
    [HandleError]
    public class IndexController : Controller
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IErrorService _errorService;

        private readonly MetadataIndexer _indexer;

        public IndexController(MetadataIndexer indexer, IErrorService errorService)
        {
            _indexer = indexer;
            _errorService = errorService;
        }


        [Authorize]
        public ActionResult Index()
        {
            Log.Info("Run indexing of entire metadata catalogue.");
            DateTime start = DateTime.Now;
            
            _indexer.RunIndexing();

            DateTime stop = DateTime.Now;
            double seconds = stop.Subtract(start).TotalSeconds;
            Log.Info(string.Format("Indexing fininshed after {0} seconds.", seconds));

            return View();
        }

        [Route("IndexSingle/{uuid}")]
        public ActionResult IndexSingle(string uuid)
        {
            Log.Info("Run indexing of single metadata.");
            DateTime start = DateTime.Now;

            if (!string.IsNullOrWhiteSpace(uuid))
            {
                Log.Info("Running single indexing of metadata with uuid=" + uuid);
                _indexer.RunIndexingOn(uuid);
                
            }

            DateTime stop = DateTime.Now;
            double seconds = stop.Subtract(start).TotalSeconds;
            Log.Info(string.Format("Indexing fininshed after {0} seconds.", seconds));

            return View();
        }

        [Authorize]
        public ActionResult ReIndex()
        {
            string role = GetSecurityClaim("role");
            if (role == "nd.metadata_admin")
            {
                Log.Info("Run reindexing of entire metadata catalogue.");
                DateTime start = DateTime.Now;

                _indexer.RunReIndexing();

                DateTime stop = DateTime.Now;
                double seconds = stop.Subtract(start).TotalSeconds;
                Log.Info(string.Format("Indexing fininshed after {0} seconds.", seconds));

           
                return View();
            }
            else
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
        }

        [HttpPost]
        [ValidateInput(false)] // allow posting of XML to go through
        public ActionResult MetadataUpdated(string action, string uuid, string XMLFile)
        {
            HttpStatusCode statusCode;
            try
            {
                Log.Info("Received notification of updated metadata: " + Request.HttpMethod + ", " + action + ", " + uuid + ", AUTH_USER: " + Request.ServerVariables["AUTH_USER"] + ", httpHeader: " + Request.ServerVariables["ALL_RAW"]);

                if (!string.IsNullOrWhiteSpace(uuid))
                {
                    Log.Info("Running single indexing of metadata with uuid=" + uuid);

                    _indexer.RunIndexingOn(uuid);

                    statusCode = HttpStatusCode.Accepted;
                }
                else
                {
                    Log.Warn("Not indexing metadata - uuid was empty");
                    statusCode = HttpStatusCode.BadRequest;
                }
            }
            catch (Exception e)
            {
                Log.Error("Exception while indexing single metadata.", e);
                _errorService.AddError(uuid, e);
                statusCode = HttpStatusCode.BadRequest;
            }
            return new HttpStatusCodeResult(statusCode);
        }
        protected override void OnException(ExceptionContext filterContext)
        {
            Log.Error("Error", filterContext.Exception);
        }

        private string GetSecurityClaim(string type)
        {
            string result = null;
            foreach (var claim in System.Security.Claims.ClaimsPrincipal.Current.Claims)
            {
                if (claim.Type == type && !string.IsNullOrWhiteSpace(claim.Value))
                {
                    result = claim.Value;
                    break;
                }
            }

            // bad hack, must fix BAAT
            if (!string.IsNullOrWhiteSpace(result) && type.Equals("organization") && result.Equals("Statens kartverk"))
            {
                result = "Kartverket";
            }

            return result;
        }
    }
}