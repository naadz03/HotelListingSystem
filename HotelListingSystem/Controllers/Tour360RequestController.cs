using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using HotelListingSystem.Models;
using Microsoft.Graph.Models;

namespace HotelListingSystem.Controllers
{
    public class Tour360RequestController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Tour360Request
        public ActionResult Index()
        {
            if (User.IsInRole("Administrator"))
            {
                return View(db.Tour360Requests.Include(c=>c.Hotel).ToList());
            }
            var user = AppHelper.CurrentHotelUser().Id;
            var hotelsId = db.Hotels.Where(a => a.HotelUserId == user).Select(a => a.Id).ToList();
            return View(db.Tour360Requests.Include(c => c.Hotel).Where(c => hotelsId.Contains(c.HotelId)).ToList());
            
        }

        // GET: Tour360Request/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tour360Request tour360Request = db.Tour360Requests.Find(id);
            if (tour360Request == null)
            {
                return HttpNotFound();
            }
            return View(tour360Request);
        }

        // GET: Tour360Request/Create
        public ActionResult Create()
        {
            var user = AppHelper.CurrentHotelUser().Id;
            ViewBag.MyHotels = new SelectList(db.Hotels.Where(a => a.HotelUserId == user).ToList(), "Id", "Name");
            return View();
        }

        // POST: Tour360Request/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create( Tour360Request tour360Request)
        {
            if (ModelState.IsValid)
            {
                tour360Request.DateRequested = DateTime.Now;    
                db.Tour360Requests.Add(tour360Request);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(tour360Request);
        }

        // GET: Tour360Request/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tour360Request tour360Request = db.Tour360Requests.Find(id);
            ViewBag.MyHotels = new SelectList(db.Hotels.Where(a => a.Id == tour360Request.HotelId).ToList(), "Id", "Name");
            if (tour360Request == null)
            {
                return HttpNotFound();
            }
            return View(tour360Request);
        }

        // POST: Tour360Request/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit( Tour360Request tour360Request)
        {
            var tour = db.Tour360Requests.Find(tour360Request.Id);
            if (ModelState.IsValid)
            {
                tour.Actioned = true;
                tour.Approved = tour360Request.Approved;
                db.Entry(tour).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tour360Request);
        }

        // GET: Tour360Request/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tour360Request tour360Request = db.Tour360Requests.Find(id);
            if (tour360Request == null)
            {
                return HttpNotFound();
            }
            return View(tour360Request);
        }

        // POST: Tour360Request/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Tour360Request tour360Request = db.Tour360Requests.Find(id);
            db.Tour360Requests.Remove(tour360Request);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Payment(Int32 Id, object Payment)
        {
            Tour360Request tour = db.Tour360Requests.Find(Id);
            tour.Payment = Payment.ToString();
            tour.Paid = true;
            tour.PaymentDate = DateTime.Now;
            db.SaveChanges();
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        public ActionResult TourAdd(Int32 Id, string TourId)
        {
            Tour360Request tour = db.Tour360Requests.Include(x => x.Hotel).FirstOrDefault(c => c.Id == Id);
            tour.TourUploaded = true;
            tour.Hotel.Tour360Id = TourId;
            db.Entry(tour).State = EntityState.Modified;
            db.Entry(tour.Hotel).State = EntityState.Modified;
            db.SaveChanges();
            return Json(true, JsonRequestBehavior.AllowGet);
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
