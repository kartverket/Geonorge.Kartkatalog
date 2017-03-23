using System.Web.Mvc;
using System;
using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Models.ViewModels;
using Kartverket.Metadatakatalog.Service.Application;
using System.Linq;

namespace Kartverket.Metadatakatalog.Controllers
{
    [HandleError]
    public class SearchController : Controller
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        public ActionResult Index(SearchParameters parameters)
        {
            parameters.AddComplexFacetsIfMissing();
            SearchResult searchResult = _searchService.Search(parameters);

            SearchViewModel model = new SearchViewModel(parameters, searchResult);

            return View(model);
        }


        //[System.Web.Http.HttpGet]
        //[Route("search/organization")]
        public ActionResult Organization(SearchByOrganizationParameters parameters)
        {
            parameters.AddDefaultFacetsIfMissing();
            SearchResultForOrganization searchResult = _searchService.SearchByOrganization(parameters);
            var organizations = searchResult.Organizations();

            ViewBag.OrganizationList = new SelectList(organizations, "key","value", parameters.OrganizationSeoName);

            var model = new SearchByOrganizationViewModel(parameters, searchResult);
            return View(model);
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            Log.Error("Error", filterContext.Exception);
        }

    }
}