using System;
using System.IO;
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

        public ActionResult MetadataUpdated()
        {
            HttpStatusCode statusCode;
            try
            {
                using (var stream = new StreamReader(Request.InputStream))
                {
                    string body = stream.ReadToEnd();
                    if (!string.IsNullOrWhiteSpace(body))
                    {
                        body = body.Trim();
                        Log.Info("Running single indexing of metadata with uuid=" + body);
                        _indexer.RunIndexingOn(body);
                        statusCode = HttpStatusCode.Accepted;
                    }
                    else
                    {
                        Log.Warn("Not indexing metadata - body was empty");
                        statusCode = HttpStatusCode.BadRequest;
                    }
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