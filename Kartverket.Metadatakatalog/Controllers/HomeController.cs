using System;
using System.Net;
using System.Web.Mvc;
using Kartverket.Metadatakatalog.Service;
using Kartverket.Metadatakatalog.Helpers;
using System.Web;

namespace Kartverket.Metadatakatalog.Controllers
{
    [HandleError]
    public class HomeController : Controller
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ActionResult Index()
        {
            return RedirectToActionPermanent("Index", "Search");
        }

        [Route("setculture/{culture}")]
        public ActionResult SetCulture(string culture, string ReturnUrl)
        {
            // Validate input
            culture = CultureHelper.GetImplementedCulture(culture);
            // Save culture in a cookie
            HttpCookie cookie = Request.Cookies["_culture"];
            if (cookie != null) { 
                cookie.Value = culture;   // update cookie value
                cookie.Expires = DateTime.Now.AddYears(1);
                if(!Request.IsLocal)
                    cookie.Domain = ".geonorge.no";
            }
            else
            {
                cookie = new HttpCookie("_culture");
                cookie.Value = culture;
                cookie.Expires = DateTime.Now.AddYears(1);
                if (!Request.IsLocal)
                    cookie.Domain = ".geonorge.no";
            }
            Response.Cookies.Add(cookie);

            if (!string.IsNullOrEmpty(ReturnUrl))
                return Redirect(ReturnUrl);
            else
                return RedirectToAction("Index");
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            
            Log.Error("Error", filterContext.Exception);
        }

    }
}