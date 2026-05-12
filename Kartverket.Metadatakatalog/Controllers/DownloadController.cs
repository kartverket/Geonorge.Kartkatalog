using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kartverket.Metadatakatalog.Controllers
{
    public class DownloadController : Controller
    {
        private readonly ILogger<DownloadController> _logger;
        private readonly RegisterFetcher _registerFetcher;

        public DownloadController(RegisterFetcher registerFetcher, ILogger<DownloadController> logger)
        {
            _registerFetcher = registerFetcher;
            _logger = logger;
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
