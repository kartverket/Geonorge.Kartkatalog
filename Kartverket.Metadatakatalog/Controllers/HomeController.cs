using System.Web.Mvc;

namespace Kartverket.Metadatakatalog.Controllers
{
    [HandleError]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return RedirectToActionPermanent("Index", "Search");
        }

    }
}