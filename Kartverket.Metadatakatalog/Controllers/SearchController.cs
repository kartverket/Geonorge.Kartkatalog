using System;
using System.Net;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Models.Translations;
using Kartverket.Metadatakatalog.Models.ViewModels;
using Kartverket.Metadatakatalog.Service.Application;
using Kartverket.Metadatakatalog.Service.Article;
using Kartverket.Metadatakatalog.Service.Search;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;

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

        public void SignIn()
        {
            // Save redirect to basket in a cookie
            HttpCookie cookie = Request.Cookies["_redirectDownload"];

            if (cookie != null)
            {
                if (cookie.Domain != ".geonorge.no")
                {
                    HttpCookie oldCookie = new HttpCookie("_redirectDownload");
                    oldCookie.Domain = cookie.Domain;
                    oldCookie.Expires = DateTime.Now.AddDays(-1d);
                    Response.Cookies.Add(oldCookie);
                }
            }

            if (cookie != null)
            {
                cookie.Value = "true";   // update cookie value
                cookie.Expires = DateTime.Now.AddYears(1);
                //cookie.SameSite = SameSiteMode.Lax;
                if (!Request.IsLocal)
                    cookie.Domain = ".geonorge.no";
            }
            else
            {
                cookie = new HttpCookie("_redirectDownload");
                cookie.Value = "true";
                cookie.Expires = DateTime.Now.AddYears(1);
                //cookie.SameSite = SameSiteMode.Lax;

                if (!Request.IsLocal)
                    cookie.Domain = ".geonorge.no";
            }
            Response.Cookies.Add(cookie);


            var redirectUrl = Url.Action(nameof(SearchController.Index), "Search");
            HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = redirectUrl },
                OpenIdConnectAuthenticationDefaults.AuthenticationType);
        }

        public void SignOut()
        {
            // Save redirect to basket in a cookie
            HttpCookie cookie = Request.Cookies["_redirectDownload"];

            if (cookie != null)
            {
                if (cookie.Domain != ".geonorge.no")
                {
                    HttpCookie oldCookie = new HttpCookie("_redirectDownload");
                    oldCookie.Domain = cookie.Domain;
                    oldCookie.Expires = DateTime.Now.AddDays(-1d);
                    Response.Cookies.Add(oldCookie);
                }
            }

            if (cookie != null)
            {
                cookie.Value = "true";   // update cookie value
                cookie.Expires = DateTime.Now.AddYears(1);
                //cookie.SameSite = SameSiteMode.Lax;
                if (!Request.IsLocal)
                    cookie.Domain = ".geonorge.no";
            }
            else
            {
                cookie = new HttpCookie("_redirectDownload");
                cookie.Value = "true";
                cookie.Expires = DateTime.Now.AddYears(1);
                //cookie.SameSite = SameSiteMode.Lax;

                if (!Request.IsLocal)
                    cookie.Domain = ".geonorge.no";
            }
            Response.Cookies.Add(cookie);


            // Change loggedIn cookie
            cookie = Request.Cookies["_loggedIn"];

            if (cookie != null)
            {
                if (cookie.Domain != ".geonorge.no")
                {
                    HttpCookie oldCookie = new HttpCookie("_loggedIn");
                    oldCookie.Domain = cookie.Domain;
                    oldCookie.Expires = DateTime.Now.AddDays(-1d);
                    Response.Cookies.Add(oldCookie);
                }
            }

            if (cookie != null)
            {
                cookie.Value = "false";   // update cookie value
                cookie.Expires = DateTime.Now.AddYears(1);
                //cookie.SameSite = SameSiteMode.Lax;
                if (!Request.IsLocal)
                    cookie.Domain = ".geonorge.no";
            }
            else
            {
                cookie = new HttpCookie("_loggedIn");
                cookie.Value = "false";
                cookie.Expires = DateTime.Now.AddYears(1);
                //cookie.SameSite = SameSiteMode.Lax;

                if (!Request.IsLocal)
                    cookie.Domain = ".geonorge.no";
            }
            Response.Cookies.Add(cookie);


            var redirectUri = WebConfigurationManager.AppSettings["GeoID:PostLogoutRedirectUri"];
            HttpContext.GetOwinContext().Authentication.SignOut(
                new AuthenticationProperties { RedirectUri = redirectUri },
                OpenIdConnectAuthenticationDefaults.AuthenticationType,
                CookieAuthenticationDefaults.AuthenticationType);
        }

        /// <summary>
        /// This is the action responding to /signout-callback-oidc route after logout at the identity provider
        /// </summary>
        /// <returns></returns>
        public ActionResult SignOutCallback()
        {
            return RedirectToAction(nameof(SearchController.Index), "Search");
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            Log.Error("Error", filterContext.Exception);
        }
    }
}