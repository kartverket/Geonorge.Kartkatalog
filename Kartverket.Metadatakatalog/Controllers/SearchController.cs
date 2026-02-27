using System;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Models.Translations;
using Kartverket.Metadatakatalog.Models.ViewModels;
using Kartverket.Metadatakatalog.Service.Application;
using Kartverket.Metadatakatalog.Service.Article;
using Kartverket.Metadatakatalog.Service.Search;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Kartverket.Metadatakatalog.Controllers
{
    public class SearchController : Controller
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ISearchServiceAll _searchService;
        private readonly IArticleService _articleService;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public SearchController(ISearchServiceAll searchService, IArticleService articleSevice, IConfiguration configuration, IWebHostEnvironment environment)
        {
            _searchService = searchService;
            _articleService = articleSevice;
            _configuration = configuration;
            _environment = environment;
        }


        /// <summary>
        /// Main search page. Contains all datasets
        /// </summary>
        /// <param name="parameters">Facets</param>
        /// <returns>/search</returns>
        public IActionResult Index(SearchParameters parameters)
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
        public IActionResult Area(SearchByAreaParameters parameters)
        {
            parameters.AddDefaultFacetsIfMissing();
            parameters.CreateFacetOfArea();

            var searchResult = _searchService.Search(parameters);
            var model = new SearchByAreaViewModel(parameters, searchResult);

            return View(model);
        }

        public IActionResult SignIn()
        {
            var redirectUrl = "/nedlasting";
            return Challenge(new AuthenticationProperties { RedirectUri = redirectUrl }, OpenIdConnectDefaults.AuthenticationScheme);
        }

        public new IActionResult SignOut()
        {
            // Save redirect to basket in a cookie
            var cookieOptions = new CookieOptions
            {
                Path = "/",
                SameSite = SameSiteMode.Lax
            };

            if (!_environment.IsDevelopment())
                cookieOptions.Domain = ".geonorge.no";

            // Set redirect download cookie
            Response.Cookies.Append("_redirectDownload", "false", cookieOptions);

            // Set logged in cookie
            Response.Cookies.Append("_loggedIn", "false", cookieOptions);

            var redirectUri = _configuration["GeoID:PostLogoutRedirectUri"];
            return base.SignOut(new AuthenticationProperties { RedirectUri = redirectUri },
                OpenIdConnectDefaults.AuthenticationScheme,
                CookieAuthenticationDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// This is the action responding to /signout-callback-oidc route after logout at the identity provider
        /// </summary>
        /// <returns></returns>
        public IActionResult SignOutCallback()
        {
            return RedirectToAction(nameof(SearchController.Index), "Search");
        }
    }
}
