using Microsoft.AspNetCore.Mvc;
using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Models.ViewModels;
using Kartverket.Metadatakatalog.Service.Application;
using Kartverket.Metadatakatalog.Service.Search;

namespace Kartverket.Metadatakatalog.Controllers
{
    public class OpenDataController : Controller
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ISearchService _searchService;

        public OpenDataController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        public IActionResult Index(SearchParameters parameters)
        {
            parameters.SetFacetOpenData();
            parameters.AddComplexFacetsIfMissing();
            var searchResult = _searchService.Search(parameters);

            var model = new SearchViewModel(parameters, searchResult);
            model.EnabledFacets = model.FacetsOpenData();

            return View(model);
        }

        public IActionResult Area(SearchByAreaParameters parameters)
        {
            parameters.AddDefaultFacetsIfMissing();
            parameters.CreateFacetOfArea();
            FixAreaParameters(parameters);

            var searchResult = _searchService.Search(parameters);
            var model = new SearchByAreaViewModel(parameters, searchResult);

            return View(model);
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            Log.Error("Error", filterContext.Exception);
        }

        private static void FixAreaParameters(SearchByAreaParameters parameters)
        {
            if (parameters.AreaCode == "omrade")
            {
                parameters.AreaCode = null;
            }
        }
    }
}
