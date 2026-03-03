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
        private readonly RegisterFetcher _registerFetcher;

        public DownloadController(RegisterFetcher registerFetcher)
        {
            _registerFetcher = registerFetcher;
        }

        // GET: Download
        public IActionResult Index()
        {
            ViewBag.DownloadUseGroups = _registerFetcher.GetDownloadUseGroups();
            ViewBag.DownloadPurposes = _registerFetcher.GetDownloadPurposes();
            return View();
        }
    }
}
