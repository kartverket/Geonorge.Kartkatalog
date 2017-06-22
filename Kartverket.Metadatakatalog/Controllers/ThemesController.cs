using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Service;
using System.IO;

namespace Kartverket.Metadatakatalog.Controllers
{
    public class ThemesController : Controller
    {
        private IThemeService _themeService;

        public ThemesController(IThemeService themeService)
        {
            _themeService = themeService;
        }

        // GET: Themes
        public ActionResult Index()
        {
            var model = _themeService.GetThemes();

            return View(model);
        }

        // GET: Themes/Create
        public ActionResult Create()
        {
            ViewBag.ParentId = new SelectList(_themeService.GetThemes(), "Id", "Name");
            return View();
        }

        // POST: Themes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Description,ThumbnailUrl,ParentId")] Theme theme, HttpPostedFileBase uploadFile)
        {
            if (uploadFile != null)
            {
                if (!(uploadFile.ContentType == "image/jpeg" || uploadFile.ContentType == "image/gif"
                    || uploadFile.ContentType == "image/png" || uploadFile.ContentType == "image/svg+xml"))
                {
                    ModelState.AddModelError(string.Empty, "Thumnail må være bilde format");
                }

                theme.ThumbnailUrl = SaveFile(theme, uploadFile);
            }

            if (ModelState.IsValid)
            {
                _themeService.AddTheme(theme);
                return RedirectToAction("Index");
            }

            ViewBag.ParentId = new SelectList(_themeService.GetThemes(), "Id", "Name", theme.ParentId);
            return View(theme);
        }

        private string SaveFile(Theme theme, HttpPostedFileBase uploadFile)
        {
            string targetFolder = System.Web.HttpContext.Current.Server.MapPath("~/temafiler");
            string targetPath = Path.Combine(targetFolder, uploadFile.FileName);
            uploadFile.SaveAs(targetPath);

            return CurrentDomain() + "/temafiler/" + uploadFile.FileName;

        }

        string CurrentDomain()
        {
            return Request.Url.Scheme + System.Uri.SchemeDelimiter
                 + Request.Url.Host +
                 (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
        }

        // GET: Themes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Theme theme = _themeService.GetTheme(id);
            if (theme == null)
            {
                return HttpNotFound();
            }
            ViewBag.ParentId = new SelectList(_themeService.GetThemes(), "Id", "Name", theme.ParentId);
            return View(theme);
        }

        // POST: Themes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Description,ThumbnailUrl,ParentId")] Theme theme, string[] operatesOn, HttpPostedFileBase uploadFile)
        {
            if (uploadFile != null)
            {
                if (!(uploadFile.ContentType == "image/jpeg" || uploadFile.ContentType == "image/gif"
                    || uploadFile.ContentType == "image/png" || uploadFile.ContentType == "image/svg+xml"))
                {
                    ModelState.AddModelError(string.Empty, "Thumnail må være bilde format");
                }

                theme.ThumbnailUrl = SaveFile(theme, uploadFile);
            }

            if (ModelState.IsValid)
            {
                _themeService.UpdateTheme(theme, operatesOn);
                return RedirectToAction("Index");
            }
            ViewBag.ParentId = new SelectList(_themeService.GetThemes(), "Id", "Name", theme.ParentId);
            return View(theme);
        }

        // GET: Themes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Theme theme = _themeService.GetTheme(id);
            if (theme == null)
            {
                return HttpNotFound();
            }
            return View(theme);
        }

        // POST: Themes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            _themeService.RemoveTheme(id);

            return RedirectToAction("Index");
        }
    }
}
