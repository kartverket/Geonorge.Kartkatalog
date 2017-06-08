using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Kartverket.Metadatakatalog.Models;

namespace Kartverket.Metadatakatalog.Controllers
{
    public class ThemesController : Controller
    {
        private MetadataContext db = new MetadataContext();

        // GET: Themes
        public ActionResult Index()
        {
            var model = new List<Theme>();

                model = db.Themes.ToList();
            return View(model);
        }

        // GET: Themes/Create
        public ActionResult Create()
        {
            ViewBag.ParentId = new SelectList(db.Themes, "Id", "Name");
            return View();
        }

        // POST: Themes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Description,ThumbnailUrl,ParentId")] Theme theme)
        {
            if (ModelState.IsValid)
            {
                db.Themes.Add(theme);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ParentId = new SelectList(db.Themes, "Id", "Name", theme.ParentId);
            return View(theme);
        }

        // GET: Themes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Theme theme = db.Themes.Find(id);
            if (theme == null)
            {
                return HttpNotFound();
            }
            ViewBag.ParentId = new SelectList(db.Themes, "Id", "Name", theme.ParentId);
            return View(theme);
        }

        // POST: Themes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Description,ThumbnailUrl,ParentId")] Theme theme)
        {
            if (ModelState.IsValid)
            {
                db.Entry(theme).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ParentId = new SelectList(db.Themes, "Id", "Name", theme.ParentId);
            return View(theme);
        }

        // GET: Themes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Theme theme = db.Themes.Find(id);
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
            Theme theme = db.Themes.Find(id);
            db.Themes.Remove(theme);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
