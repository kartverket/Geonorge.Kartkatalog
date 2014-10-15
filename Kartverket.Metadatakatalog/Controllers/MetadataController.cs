using System.Web.Mvc;
using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Service;

namespace Kartverket.Metadatakatalog.Controllers
{
    public class MetadataController : Controller
    {
        private readonly IMetadataService _metadataService;

        public MetadataController(IMetadataService metadataService)
        {
            _metadataService = metadataService;
        }

        public ActionResult Index(string uuid)
        {
            MetadataViewModel model = _metadataService.FindMetadata(uuid);

            if (model == null)
                return new HttpNotFoundResult();

            return View(model);
        }
    }
}