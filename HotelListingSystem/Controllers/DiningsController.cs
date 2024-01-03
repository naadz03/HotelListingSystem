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
    public class DiningsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Dinings
        public ActionResult Index()
        {
            var dinings = db.Dinings.Include(d => d.DiningTableTypes).Include(d => d.MealTypes);
            return View(dinings.ToList());
        }

        // GET: Dinings/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Dining dining = db.Dinings.Find(id);
            if (dining == null)
            {
                return HttpNotFound();
            }
            return View(dining);
        }

        // GET: Dinings/Create
        public ActionResult Create()
        {
            ViewBag.DiningTableTypeId = new SelectList(db.DiningTableTypes, "Id", "Name");
            ViewBag.MealTypeId = new SelectList(db.MealTypes, "Id", "Name");
            return View(); 
        }

        // POST: Dinings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,MealTypeId,DiningTableTypeId,StartTime,EndTime,NoOfTables")] Dining dining, double CostPerPerson)
        {
            var rqstStartTime = Request.Form["StartTime"];
            if (dining.StartTime == null)
            {
                dining.StartTime = DateTime.Parse(rqstStartTime);
            }
            if (ModelState.IsValid)
            {
                dining.CostPerPerson = CostPerPerson;
                db.Dinings.Add(dining);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.MealTypeId = new SelectList(db.DiningTableTypes, "Id", "Name", dining.MealTypeId);
            ViewBag.MealTypeId = new SelectList(db.MealTypes, "Id", "Name", dining.MealTypeId);
            return View(dining);
        }

        // GET: Dinings/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Dining dining = db.Dinings.Find(id);
            if (dining == null)
            {
                return HttpNotFound();
            }
            ViewBag.MealTypeId = new SelectList(db.DiningTableTypes, "Id", "Name", dining.MealTypeId);
            ViewBag.MealTypeId = new SelectList(db.MealTypes, "Id", "Name", dining.MealTypeId);
            return View(dining);
        }

        // POST: Dinings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,MealTypeId,DiningTableTypeId,StartTime,EndTime,NoOfTables")] Dining dining)
        {
            if (ModelState.IsValid)
            {
                db.Entry(dining).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.MealTypeId = new SelectList(db.DiningTableTypes, "Id", "Name", dining.MealTypeId);
            ViewBag.MealTypeId = new SelectList(db.MealTypes, "Id", "Name", dining.MealTypeId);
            return View(dining);
        }

        // GET: Dinings/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Dining dining = db.Dinings.Find(id);
            if (dining == null)
            {
                return HttpNotFound();
            }
            return View(dining);
        }

        // POST: Dinings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Dining dining = db.Dinings.Find(id);
            db.Dinings.Remove(dining);
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
