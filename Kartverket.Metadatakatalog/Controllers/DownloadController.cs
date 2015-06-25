using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kartverket.Metadatakatalog.Service;

namespace Kartverket.Metadatakatalog.Controllers
{
    public class DownloadController : Controller
    {
        // GET: Download
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Order(FormCollection order)
        {

            foreach (var key in order.AllKeys)
            {
                var value = order[key];
            }

            DownloadService download = new DownloadService();

            //send order model as input
            var model = download.Order();

            return View(model);
        }
    }
}