using System.Web.Mvc;
using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Models.ViewModels;
using Kartverket.Metadatakatalog.Service.Search;

namespace Kartverket.Metadatakatalog.Controllers
{
    [HandleError]
    public class SearchController : Controller
    {
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

    }
}