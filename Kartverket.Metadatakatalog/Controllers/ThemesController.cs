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
using System.Security.Claims;
using Geonorge.AuthLib.Common;

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
        [Authorize]
        public ActionResult Index()
        {
            var model = _themeService.GetThemes();

            return View(model);
        }

        // GET: Themes
        public ActionResult Details(int? id)
        {
            var theme = _themeService.GetTheme(id);
            if (theme == null) return HttpNotFound();

            ViewBag.IsAdmin = ClaimsPrincipal.Current.IsInRole(GeonorgeRoles.MetadataAdmin);

            return View(theme);
        }

        // GET: Themes/Create
        [Authorize]
        public ActionResult Create()
        {
            Theme theme = new Theme();
            theme.AddMissingTranslations();
            ViewBag.ParentId = new SelectList(_themeService.GetThemes(), "Id", "Name");
            return View(theme);
        }

        // POST: Themes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Theme theme, HttpPostedFileBase uploadFile)
        {
            if (!ClaimsPrincipal.Current.IsInRole(GeonorgeRoles.MetadataAdmin))
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (uploadFile != null)
            {
                if (!(uploadFile.ContentType == "image/jpeg" || uploadFile.ContentType == "image/gif"
                    || uploadFile.ContentType == "image/png" || uploadFile.ContentType == "image/svg+xml"))
                {
                    ModelState.AddModelError(string.Empty, "Thumnail må være bilde format");
                }
                else { 
                theme.ThumbnailUrl = SaveFile(theme, uploadFile);
                }
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
        [Authorize]
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
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Theme theme, string[] operatesOn, HttpPostedFileBase uploadFile)
        {

            if (!ClaimsPrincipal.Current.IsInRole(GeonorgeRoles.MetadataAdmin))
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (uploadFile != null)
            {
                if (!(uploadFile.ContentType == "image/jpeg" || uploadFile.ContentType == "image/gif"
                    || uploadFile.ContentType == "image/png" || uploadFile.ContentType == "image/svg+xml"))
                {
                    ModelState.AddModelError(string.Empty, "Thumnail må være bilde format");
                }
                else { 
                theme.ThumbnailUrl = SaveFile(theme, uploadFile);
                }
            }

            if (ModelState.IsValid)
            {
                _themeService.UpdateTheme(theme, operatesOn);
                return RedirectToAction("Details", "Themes", new { Id = theme.Id, ThemeSeoName = theme.Name });
            }
            ViewBag.ParentId = new SelectList(_themeService.GetThemes(), "Id", "Name", theme.ParentId);
            return RedirectToAction("Details", "Themes", new { Id = theme.Id, ThemeSeoName = theme.Name });
        }

        // GET: Themes/Delete/5
        [Authorize]
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
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if(!ClaimsPrincipal.Current.IsInRole(GeonorgeRoles.MetadataAdmin))
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            _themeService.RemoveTheme(id);

            return RedirectToAction("Index");
        }

    }
}
