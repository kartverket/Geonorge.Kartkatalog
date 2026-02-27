using Kartverket.Metadatakatalog.Models.ViewModels;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace Kartverket.Metadatakatalog.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult NotFound(string url)
        {
            var originalUri = url ?? 
                             (Request.Query.ContainsKey("aspxerrorpath") ? Request.Query["aspxerrorpath"].ToString() : "") ?? 
                             Request.Path;

            var controllerName = (string)RouteData.Values["controller"];
            var actionName = (string)RouteData.Values["action"];
            var model = new NotFoundModel(new Exception("Failed to find page"), controllerName, actionName)
            {
                RequestedUrl = originalUri,
                ReferrerUrl = Request.Headers.ContainsKey("Referer") ? Request.Headers["Referer"].ToString() : ""
            };

            Response.StatusCode = 404;
            return View("NotFound", model);
        }
    }
}
