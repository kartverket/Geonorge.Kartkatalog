using System;
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

        public ActionResult Index(string uuid, string organization = null, string title = null)
        {
            MetadataViewModel model = null;
            try
            {
                model = _metadataService.GetMetadataByUuid(uuid);
            }
            catch (InvalidOperationException exception)
            {
                // todo log exception    
            }
            if (model == null)
                return new HttpNotFoundResult();
            
            SeoUrl url = model.CreateSeoUrl();
            if (!url.Matches(organization, title) && !string.IsNullOrWhiteSpace(organization))
                return RedirectToActionPermanent("Index", new { organization = url.Organization, title = url.Title, uuid = uuid });

            return View(model);
        }

    }
}