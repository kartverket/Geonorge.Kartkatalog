using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Kartverket.Metadatakatalog.Service;
using Kartverket.Metadatakatalog.Models;

namespace Kartverket.Metadatakatalog.Controllers
{
    public class DownloadController : Controller
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // GET: Download
        public IActionResult Index()
        {
            RegisterFetcher registerFetcher = new RegisterFetcher();
            ViewBag.DownloadUseGroups = registerFetcher.GetDownloadUseGroups();
            ViewBag.DownloadPurposes = registerFetcher.GetDownloadPurposes();
            return View();
        }
    }
}
