using System;
using System.Web.Mvc;
using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Models.ViewModels;
using Kartverket.Metadatakatalog.Service;
using Kartverket.Metadatakatalog.Service.Application;
using System.Linq;
using Kartverket.Metadatakatalog.Service.Search;

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
        
        /// <summary>
        /// Metadata page. 
        /// </summary>
        /// <param name="uuid"></param>
        /// <param name="organization"></param>
        /// <param name="title"></param>
        /// <param name="orderby"></param>
        /// <returns>/metadata/{organization}/{title}/{uuid}</returns>
        public ActionResult Index(string uuid, string organization = null, string title = null, string orderby = "title")
        {
            MetadataViewModel model = null;
            //try
            //{
                model = _metadataService.GetMetadataViewModelByUuid(uuid);
                model = Sort(model, orderby);
            //}
            //catch (InvalidOperationException exception)
            //{
            //    Log.Error("Metadata with uuid: " + uuid + " not found in Geonetwork.", exception);
            //    //throw new Exception("Metadata with uuid: " + uuid + " not found in Geonetwork.");
            //}
            if (model == null)
            {
                Log.Error("Metadata with uuid: " + uuid + " not found.");
                return new HttpNotFoundResult("Metadata with uuid: " + uuid + " not found in Geonetwork.");
            }

            SeoUrl url = model.CreateSeoUrl();
            if (!url.Matches(organization, title) && !string.IsNullOrWhiteSpace(organization))
                return RedirectToActionPermanent("Index", new { organization = url.Organization, title = url.Title, uuid = uuid });

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


        /// <summary>
        /// Shows metadata by selected organization
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns>/etatvis-oversikt/{organization}</returns>
        public ActionResult Organization(SearchByOrganizationParameters parameters)
        {
            parameters.AddDefaultFacetsIfMissing();
            FixOrganizationParameters(parameters);
            SearchResultForOrganization searchResult = _searchService.SearchByOrganization(parameters);
            var model = new SearchByOrganizationViewModel(parameters, searchResult);
            return View(model);
        }


        private static void FixOrganizationParameters(SearchByOrganizationParameters parameters)
        {
            if (parameters.OrganizationSeoName == "organisasjon")
            {
                parameters.OrganizationSeoName = null;
            }
        }

        

        protected override void OnException(ExceptionContext filterContext)
        {
            Log.Error("Error", filterContext.Exception);
        }
    }
}