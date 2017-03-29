using System.Web.Mvc;
using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Models.ViewModels;
using Kartverket.Metadatakatalog.Service.Application;

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
            var searchResult = _searchService.Search(parameters);

            var model = new SearchViewModel(parameters, searchResult);

            return View(model);
        }


        //[System.Web.Http.HttpGet]
        //[Route("search/organization")]
        public ActionResult Organization(SearchByOrganizationParameters parameters)
        {
            parameters.AddDefaultFacetsIfMissing();
            var searchResult = _searchService.SearchByOrganization(parameters);      
            var model = new SearchByOrganizationViewModel(parameters, searchResult);
            return View(model);
        }

        //[System.Web.Http.HttpGet]
        //[Route("search/area")]
        public ActionResult Area(SearchParameters parameters)
        {
            parameters.AddDefaultFacetsIfMissing();
            parameters.AddComplexFacetsIfMissing();
            var searchResult = _searchService.Search(parameters);
            var model = new SearchByAreaViewModel(parameters, searchResult);
            return View(model);
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            Log.Error("Error", filterContext.Exception);
        }
    }
}