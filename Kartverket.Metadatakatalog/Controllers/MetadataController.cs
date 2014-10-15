using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Kartverket.Metadatakatalog.Controllers
{
    public class MetadataController : Controller
    {
        public ActionResult Index(string uuid)
        {

            return View();
        }
    }
}