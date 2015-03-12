using System;
using System.Net;
using System.Web.Mvc;
using Kartverket.Metadatakatalog.Service;

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

        protected override void OnException(ExceptionContext filterContext)
        {
            
            Log.Error("Error", filterContext.Exception);
        }

    }
}