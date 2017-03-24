﻿using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Models.ViewModels;
using Kartverket.Metadatakatalog.Service.Application;
using Kartverket.Metadatakatalog.Service.ServiceDirectory;
using System.Web.Mvc;

namespace Kartverket.Metadatakatalog.Controllers
{
    public class ServiceDirectoryController : Controller
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IServiceDirectoryService _ServiceDirectoryService;
        private readonly ISearchService _searchService;

        public ServiceDirectoryController(IServiceDirectoryService ServiceDirectoryService, ISearchService searchService)
        {
            _ServiceDirectoryService = ServiceDirectoryService;
            _searchService = searchService;
        }


        // GET: ServiceDirectory
        public ActionResult Index(SearchParameters parameters)
        {
            parameters.AddComplexFacetsIfMissing();
            SearchResult searchResult = _ServiceDirectoryService.Services(parameters);

            SearchViewModel model = new SearchViewModel(parameters, searchResult);

            return View(model);
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            Log.Error("Error", filterContext.Exception);
        }
    }
}