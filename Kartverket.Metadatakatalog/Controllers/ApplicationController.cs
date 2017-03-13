using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Models.ViewModels;
using Kartverket.Metadatakatalog.Service.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Kartverket.Metadatakatalog.Controllers
{
    public class ApplicationController : Controller
    {
        // GET: Application
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IApplicationService _applicationService;
        private readonly ISearchService _searchService;

        public ApplicationController(IApplicationService applicationService, ISearchService searchService)
        {
            _applicationService = applicationService;
            _searchService = searchService;
        }


        // GET: ServiceDirectory
        public ActionResult Index(SearchParameters parameters)
        {
            parameters.AddComplexFacetsIfMissing();
            SearchResult searchResult = _applicationService.Applications(parameters);

            SearchViewModel model = new SearchViewModel(parameters, searchResult);

            return View(model);
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            Log.Error("Error", filterContext.Exception);
        }
    }
}