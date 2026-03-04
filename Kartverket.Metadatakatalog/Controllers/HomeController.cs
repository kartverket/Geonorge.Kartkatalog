using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Kartverket.Metadatakatalog.Service;
using Kartverket.Metadatakatalog.Helpers;

namespace Kartverket.Metadatakatalog.Controllers
{
    public class HomeController : Controller
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public IActionResult Index()
        {
            return RedirectToActionPermanent("Index", "Download");
        }

        [Route("setculture/{culture}")]
        public IActionResult SetCulture(string culture, string ReturnUrl)
        {
            // Validate input
            culture = CultureHelper.GetImplementedCulture(culture);
            
            // Save culture in a cookie using ASP.NET Core approach
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddYears(1),
                HttpOnly = true,
                Secure = true
            };

            if (!Request.IsLocal())
            {
                cookieOptions.Domain = ".geonorge.no";
            }

            Response.Cookies.Append("_culture", culture, cookieOptions);

            if (!string.IsNullOrEmpty(ReturnUrl))
                return Redirect(ReturnUrl);
            else
                return RedirectToAction("Index");
        }

        public override void OnActionExecuted(Microsoft.AspNetCore.Mvc.Filters.ActionExecutedContext context)
        {
            if (context.Exception != null)
            {
                Log.Error("Error", context.Exception);
            }
            base.OnActionExecuted(context);
        }

    }
}
