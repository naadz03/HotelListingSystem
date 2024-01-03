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
    public class DiningReservationsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: DiningReservations
        public ActionResult IndexAll()
        {
            var diningReservations = db.DiningReservations.Include(d => d.Dining).Include(d => d.HotelUsers);
            return View(diningReservations.ToList());
        }

        public ActionResult Index()
        {
            var user = AppHelper.CurrentHotelUser()?.Id;

            var diningReservations = db.DiningReservations.Include(d => d.Dining).Include(d => d.HotelUsers).Where(x=>x.HotelUserId == user).ToList();
            return View(diningReservations);
        }

        // GET: DiningReservations/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DiningReservation diningReservation = db.DiningReservations.Find(id);
            if (diningReservation == null)
            {
                return HttpNotFound();
            }
            return View(diningReservation);
        }

        // GET: DiningReservations/Create
        public ActionResult Create()
        {
            ViewBag.DiningId = new SelectList(db.Dinings, "Id", "Name");
            ViewBag.HotelUserId = new SelectList(db.HotelUsers, "Id", "FirstName");
            return View();
        }

        // POST: DiningReservations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,IsConfirmed,NoOfPeople,Description,Date,DiningId,HotelUserId")] DiningReservation diningReservation)
        {
            int totalTables = db.Dinings.Where(x=>x.Id == diningReservation.DiningId).FirstOrDefault().NoOfTables ?? 0;
            var reservedTables = db.DiningReservations.Select(d => d.TableNumber).ToList();


            List<int> availableTables = Enumerable.Range(1, totalTables).Except(reservedTables).ToList();


            diningReservation.HotelUserId = AppHelper.CurrentHotelUser()?.Id;
            diningReservation.TableNumber = availableTables.FirstOrDefault();
            diningReservation.CreatedOn = DateTime.Now;
            if (ModelState.IsValid)
            {
                db.DiningReservations.Add(diningReservation);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.DiningId = new SelectList(db.Dinings, "Id", "Name", diningReservation.DiningId);
            ViewBag.HotelUserId = new SelectList(db.HotelUsers, "Id", "FirstName", diningReservation.HotelUserId);
            return View(diningReservation);
        }

        // GET: DiningReservations/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DiningReservation diningReservation = db.DiningReservations.Find(id);
            if (diningReservation == null)
            {
                return HttpNotFound();
            }
            ViewBag.DiningId = new SelectList(db.Dinings, "Id", "Name", diningReservation.DiningId);
            ViewBag.HotelUserId = new SelectList(db.HotelUsers, "Id", "FirstName", diningReservation.HotelUserId);
            return View(diningReservation);
        }

        // POST: DiningReservations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,IsConfirmed,NoOfPeople,Description,Date,DiningId,HotelUserId")] DiningReservation diningReservation)
        {
            if (ModelState.IsValid)
            {
                db.Entry(diningReservation).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.DiningId = new SelectList(db.Dinings, "Id", "Name", diningReservation.DiningId);
            ViewBag.HotelUserId = new SelectList(db.HotelUsers, "Id", "FirstName", diningReservation.HotelUserId);
            return View(diningReservation);
        }

        // GET: DiningReservations/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DiningReservation diningReservation = db.DiningReservations.Find(id);
            if (diningReservation == null)
            {
                return HttpNotFound();
            }
            return View(diningReservation);
        }

        // POST: DiningReservations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            DiningReservation diningReservation = db.DiningReservations.Find(id);
            db.DiningReservations.Remove(diningReservation);
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
