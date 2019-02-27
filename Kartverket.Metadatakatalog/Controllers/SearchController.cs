using System.Web.Mvc;
using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Models.ViewModels;
using Kartverket.Metadatakatalog.Service.Application;
using Kartverket.Metadatakatalog.Service.Article;
using Kartverket.Metadatakatalog.Service.Search;

namespace Kartverket.Metadatakatalog.Controllers
{
    [HandleError]
    public class SearchController : Controller
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ISearchServiceAll _searchService;
        private readonly IArticleService _articleService;

        public SearchController(ISearchServiceAll searchService, IArticleService articleSevice)
        {
            _searchService = searchService;
            _articleService = articleSevice;
        }


        /// <summary>
        /// Main search page. Contains all datasets
        /// </summary>
        /// <param name="parameters">Facets</param>
        /// <returns>/search</returns>
        public ActionResult Index(SearchParameters parameters)
        {
            parameters.AddComplexFacetsIfMissing();
            var searchResult = _searchService.Search(parameters);
            Kartverket.Metadatakatalog.Models.Article.SearchParameters articleParameters = new Models.Article.SearchParameters();
            articleParameters.Text = parameters.Text;
            articleParameters.Limit = 200;
            if (string.IsNullOrEmpty(parameters.Text))
                articleParameters.orderby = "StartPublish";
            var articleResult = _articleService.Search(articleParameters);


            var model = new SearchViewModel(parameters, searchResult, articleResult);

            return View(model);
        }

        /// <summary>
        /// Shows datasets by selected municipality or county.
        /// </summary>
        /// <param name="parameters">Uses AreaCode to find selected municipality or county. </param>
        /// <returns>/hva-finnes-i-kommunen-eller-fylket</returns>
        public ActionResult Area(SearchByAreaParameters parameters)
        {
            parameters.AddDefaultFacetsIfMissing();
            parameters.CreateFacetOfArea();

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