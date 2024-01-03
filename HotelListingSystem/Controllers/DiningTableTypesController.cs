using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using HotelListingSystem.Models;

namespace HotelListingSystem.Controllers
{
    public class DiningTableTypesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: DiningTableTypes
        public ActionResult Index()
        {
            return View(db.DiningTableTypes.ToList());
        }

        // GET: DiningTableTypes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DiningTableTypes diningTableTypes = db.DiningTableTypes.Find(id);
            if (diningTableTypes == null)
            {
                return HttpNotFound();
            }
            return View(diningTableTypes);
        }

        // GET: DiningTableTypes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: DiningTableTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name")] DiningTableTypes diningTableTypes)
        {
            if (ModelState.IsValid)
            {
                db.DiningTableTypes.Add(diningTableTypes);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(diningTableTypes);
        }

        // GET: DiningTableTypes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DiningTableTypes diningTableTypes = db.DiningTableTypes.Find(id);
            if (diningTableTypes == null)
            {
                return HttpNotFound();
            }
            return View(diningTableTypes);
        }

        // POST: DiningTableTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name")] DiningTableTypes diningTableTypes)
        {
            if (ModelState.IsValid)
            {
                db.Entry(diningTableTypes).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(diningTableTypes);
        }

        // GET: DiningTableTypes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DiningTableTypes diningTableTypes = db.DiningTableTypes.Find(id);
            if (diningTableTypes == null)
            {
                return HttpNotFound();
            }
            return View(diningTableTypes);
        }

        // POST: DiningTableTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            DiningTableTypes diningTableTypes = db.DiningTableTypes.Find(id);
            db.DiningTableTypes.Remove(diningTableTypes);
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
