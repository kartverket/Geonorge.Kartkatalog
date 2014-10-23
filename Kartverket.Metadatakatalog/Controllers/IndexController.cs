using System.Web.Mvc;
using Kartverket.Metadatakatalog.Service;

namespace Kartverket.Metadatakatalog.Controllers
{
    public class IndexController : Controller
    {
        private readonly MetadataIndexer _indexer;

        public IndexController(MetadataIndexer indexer)
        {
            _indexer = indexer;
        }

        public ActionResult Index()
        {
            _indexer.RunIndexing();
            
            return View();
        }
    }
}