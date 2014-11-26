using System;
using System.Net;
using System.Web.Mvc;
using Kartverket.Metadatakatalog.Service;

namespace Kartverket.Metadatakatalog.Controllers
{
    public class IndexController : Controller
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly MetadataIndexer _indexer;

        public IndexController(MetadataIndexer indexer)
        {
            _indexer = indexer;
        }

        //[Authorize]
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

        [HttpPost]
        [ValidateInput(false)] // allow posting of XML to go through
        public ActionResult MetadataUpdated(string action, string uuid, string XMLFile)
        {
            HttpStatusCode statusCode;
            try
            {
                Log.Info("Received notification of updated metadata: " + Request.HttpMethod + ", " + action + ", " + uuid);

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
                statusCode = HttpStatusCode.BadRequest;
            }
            return new HttpStatusCodeResult(statusCode);
        }
    }
}