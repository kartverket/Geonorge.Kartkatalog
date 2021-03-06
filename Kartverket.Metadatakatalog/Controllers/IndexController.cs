﻿using System;
using System.Net;
using System.Security.Claims;
using System.Web.Mvc;
using Geonorge.AuthLib.Common;
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

        [Authorize]
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
            if (ClaimsPrincipal.Current.IsInRole(GeonorgeRoles.MetadataAdmin))
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


        protected override void OnException(ExceptionContext filterContext)
        {
            Log.Error("Error", filterContext.Exception);
        }
    }
}