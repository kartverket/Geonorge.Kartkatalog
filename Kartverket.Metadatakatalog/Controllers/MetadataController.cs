using System;
using System.Web.Mvc;
using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Models.ViewModels;
using Kartverket.Metadatakatalog.Service;
using Kartverket.Metadatakatalog.Service.Search;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Metadatakatalog.Controllers
{
    [HandleError]
    public class MetadataController : Controller
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IMetadataService _metadataService;
        private readonly ISearchService _searchService;

        public MetadataController(IMetadataService metadataService, ISearchService searchService)
        {
            _metadataService = metadataService;
            _searchService = searchService;
        }

        public ActionResult Index(string uuid, string organization = null, string title = null, string orderby = "title")
        {
            MetadataViewModel model = null;
            try
            {
                model = _metadataService.GetMetadataByUuid(uuid);
                model = Sort(model, orderby);
            }
            catch (InvalidOperationException exception)
            {
                Log.Error("Metadata with uuid: " + uuid + " not found in Geonetwork.", exception);
                //throw new Exception("Metadata with uuid: " + uuid + " not found in Geonetwork.");
            }
            if (model == null)
            {
                Log.Error("Metadata with uuid: " + uuid + " not found.");
                throw new HttpException(404, "Metadata with uuid: " + uuid + " not found in Geonetwork.");
            }

            if (model != null)
            { 
                SeoUrl url = model.CreateSeoUrl();
                if (!url.Matches(organization, title) && !string.IsNullOrWhiteSpace(organization))
                   return RedirectToActionPermanent("Index", new { organization = url.Organization, title = url.Title, uuid = uuid });
            }

            return View(model);
            

        }

        private MetadataViewModel Sort(MetadataViewModel model, string orderby)
        {
            
            if (model != null && model.Related != null)
            {
                if (orderby == "title")
                    model.Related = model.Related.OrderBy(o => o.Title).ToList();
                else if(orderby == "title_desc")
                    model.Related = model.Related.OrderByDescending(o => o.Title).ToList();
                else if (orderby == "organization")
                    model.Related = model.Related.OrderBy(o => o.ContactOwner.Organization).ToList();
                else if (orderby == "organization_desc")
                    model.Related = model.Related.OrderByDescending(o => o.ContactOwner.Organization).ToList();
            }

            return model;
        }

        public ActionResult Organization(SearchByOrganizationParameters parameters)
        {
            parameters.AddDefaultFacetsIfMissing();
            //parameters.Limit = 30;
            //parameters.orderby = OrderBy.title;
            SearchResultForOrganization searchResult = _searchService.SearchByOrganization(parameters);
            var model = new SearchByOrganizationViewModel(parameters, searchResult);
            return View(model);
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            Log.Error("Error", filterContext.Exception);
        }
    }
}