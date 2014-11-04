using System.Web.Mvc;

namespace Kartverket.Metadatakatalog.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return RedirectToActionPermanent("Index", "Search");
        }

    }
}