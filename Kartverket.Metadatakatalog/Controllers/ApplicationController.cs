using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Models.ViewModels;
using Kartverket.Metadatakatalog.Service.Application;
using System.Web.Mvc;

namespace Kartverket.Metadatakatalog.Controllers
{
    public class ApplicationController : Controller
    {
        // GET: Application
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IApplicationService _applicationService;

        public ApplicationController(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }


        // GET: ServiceDirectory
        public ActionResult Index(SearchParameters parameters)
        {
            parameters.AddComplexFacetsIfMissing();
            SearchResult searchResult = _applicationService.Applications(parameters);

            SearchViewModel model = new SearchViewModel(parameters, searchResult);
            model.EnabledFacets = model.FacetApplications();

            return View(model);
        }

        public ActionResult IndexNy(SearchParameters parameters)
        {
            parameters.AddComplexFacetsIfMissing();
            SearchResult searchResult = _applicationService.Applications(parameters);

            SearchViewModel model = new SearchViewModel(parameters, searchResult);
            model.EnabledFacets = model.FacetApplications();

            return View(model);
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            Log.Error("Error", filterContext.Exception);
        }
    }
}