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
    public class MealTypesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: MealTypes
        public ActionResult Index()
        {
            return View(db.MealTypes.ToList());
        }

        // GET: MealTypes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MealTypes mealTypes = db.MealTypes.Find(id);
            if (mealTypes == null)
            {
                return HttpNotFound();
            }
            return View(mealTypes);
        }

        // GET: MealTypes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: MealTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name")] MealTypes mealTypes)
        {
            if (ModelState.IsValid)
            {
                db.MealTypes.Add(mealTypes);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(mealTypes);
        }

        // GET: MealTypes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MealTypes mealTypes = db.MealTypes.Find(id);
            if (mealTypes == null)
            {
                return HttpNotFound();
            }
            return View(mealTypes);
        }

        // POST: MealTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name")] MealTypes mealTypes)
        {
            if (ModelState.IsValid)
            {
                db.Entry(mealTypes).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(mealTypes);
        }

        // GET: MealTypes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MealTypes mealTypes = db.MealTypes.Find(id);
            if (mealTypes == null)
            {
                return HttpNotFound();
            }
            return View(mealTypes);
        }

        // POST: MealTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            MealTypes mealTypes = db.MealTypes.Find(id);
            db.MealTypes.Remove(mealTypes);
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
