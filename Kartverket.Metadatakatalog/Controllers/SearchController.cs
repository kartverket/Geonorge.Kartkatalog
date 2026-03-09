using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Models.Translations;
using Kartverket.Metadatakatalog.Models.ViewModels;
using Kartverket.Metadatakatalog.Service.Application;
using Kartverket.Metadatakatalog.Service.Article;
using Kartverket.Metadatakatalog.Service.Search;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;

namespace Kartverket.Metadatakatalog.Controllers
{
    public class SearchController : Controller
    {
        private readonly ILogger<SearchController> _logger;

        private readonly ISearchServiceAll _searchService;
        private readonly IArticleService _articleService;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public SearchController(ISearchServiceAll searchService, IArticleService articleSevice, IConfiguration configuration, IWebHostEnvironment environment, ILogger<SearchController> logger)
        {
            _searchService = searchService;
            _articleService = articleSevice;
            _configuration = configuration;
            _environment = environment;
            _logger = logger;
        }


        /// <summary>
        /// Main search page. Contains all datasets
        /// </summary>
        /// <param name="parameters">Facets</param>
        /// <returns>/search</returns>
        public IActionResult Index(SearchParameters parameters)
        {
            return RedirectToActionPermanent("Index", "Download");
        }
        public IActionResult SignIn()
        {
            try
            {
                _logger.LogInformation("SignIn method called - starting authentication challenge");
                _logger.LogInformation($"Current user authenticated: {User.Identity.IsAuthenticated}");

                var redirectUrl = "/nedlasting";
                _logger.LogInformation($"Challenging with redirect URL: {redirectUrl}");

                // Also log cookie information
                _logger.LogInformation($"Current cookies: {string.Join(", ", Request.Cookies.Keys)}");

                return Challenge(new AuthenticationProperties { RedirectUri = redirectUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during authentication challenge");
                throw;
            }
        }

        [HttpGet("test-auth")]
        public IActionResult TestAuth()
        {
            _logger.LogInformation($"User authenticated: {User.Identity.IsAuthenticated}");
            _logger.LogInformation($"User name: {User.Identity.Name}");
            _logger.LogInformation($"Claims count: {User.Claims.Count()}");
            _logger.LogInformation($"Cookies: {string.Join(", ", Request.Cookies.Keys)}");

            return Ok(new
            {
                IsAuthenticated = User.Identity.IsAuthenticated,
                Name = User.Identity.Name,
                Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
            });
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

            var redirectUri = "/nedlasting";
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

