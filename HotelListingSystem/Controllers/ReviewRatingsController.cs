using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using HotelListingSystem.Models;
using Microsoft.AspNet.Identity;
using Org.BouncyCastle.Bcpg;

namespace HotelListingSystem.Controllers
{
    public class ReviewRatingsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: ReviewRatings
        public ActionResult Index()
        {
            var reviewRatings = db.ReviewRatings.Include(r => r.Hotel);
            return View(reviewRatings.ToList());
        }




        // GET: ReviewRatings/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ReviewRating reviewRating = db.ReviewRatings.Find(id);
            if (reviewRating == null)
            {
                return HttpNotFound();
            }
            return View(reviewRating);
        }






        // GET: ReviewRatings/Create
        public ActionResult Create(int hotelId)
        {
            ViewBag.HotelId = hotelId;
            return View();
        }

        // POST: ReviewRatings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,ReviewerId,HotelId,Rating,Review,CreatedAt,IsActive,IsDeleted,CreatedDateTime,ModifiedDateTime")] ReviewRating reviewRating)
        {
            if (ModelState.IsValid)
            {
                reviewRating.ReviewerId = User.Identity.GetUserId();
                reviewRating.CreatedAt = DateTime.Now;
                db.ReviewRatings.Add(reviewRating);
                db.SaveChanges();

                // Recalculate and update the average rating for the hotel
                UpdateAverageRating(reviewRating.HotelId);

                return RedirectToAction("ThankYou");
            }
            ViewBag.AverageRatingForHotelId = reviewRating.Hotel.AverageRating;
            ViewBag.HotelId = reviewRating.HotelId;

            // return View(reviewRating);
            return RedirectToAction("ThankYou", "ReviewRating");
        }






        // GET: ReviewRatings/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ReviewRating reviewRating = db.ReviewRatings.Find(id);
            if (reviewRating == null)
            {
                return HttpNotFound();
            }
            ViewBag.HotelId = new SelectList(db.Hotels, "Id", "Name", reviewRating.HotelId);
            return View("ThankYou");
        }

        // POST: ReviewRatings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: ReviewRatings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ReviewerId,HotelId,Rating,Review,CreatedAt,IsActive,IsDeleted,CreatedDateTime,ModifiedDateTime")] ReviewRating reviewRating)
        {
            if (ModelState.IsValid)
            {
                db.Entry(reviewRating).State = EntityState.Modified;
                db.SaveChanges();

                // Recalculate and update the average rating for the hotel
                UpdateAverageRating(reviewRating.HotelId);

                return RedirectToAction("ThankYou");
            }
            ViewBag.HotelId = new SelectList(db.Hotels, "Id", "Name", reviewRating.HotelId);
            return View("ThankYou");
        }



        // Method to update the average rating for a hotel
        private void UpdateAverageRating(int hotelId)
        {
            var hotel = db.Hotels.Find(hotelId);
            if (hotel != null)
            {
                var ratings = db.ReviewRatings.Where(r => r.HotelId == hotelId).Select(r => r.Rating).ToList();
                double averageRating = ratings.Any() ? ratings.Average() : 0.0;
                hotel.AverageRating = averageRating;
                db.SaveChanges();
            }
        }



        public ActionResult StoreHotelReviews(int hotelId)
        {
            var reviews = db.ReviewRatings
                           .Where(r => r.HotelId == hotelId)
                           .ToList();

            var reviewerIds = reviews.Select(r => r.ReviewerId).ToList();
            var usernamesDictionary = db.Users
                                       .Where(u => reviewerIds.Contains(u.Id))
                                       .ToDictionary(u => u.Id, u => u.Email);

            ViewBag.UsernamesDictionary = usernamesDictionary;
            ViewBag.Reviews = reviews;
            ViewBag.HotelId = hotelId;

            return View();
        }



        public PartialViewResult GetReviewsByRating(int hotelId, int reviewValue)
        {
            var reviews = db.ReviewRatings
                            .Where(r => r.HotelId == hotelId && r.Rating == reviewValue)
                            .ToList();

            var reviewerIds = reviews.Select(r => r.ReviewerId).ToList();
            var usernamesDictionary = db.Users
                                       .Where(u => reviewerIds.Contains(u.Id))
                                       .ToDictionary(u => u.Id, u => u.Email);

            ViewBag.UsernamesDictionary = usernamesDictionary;
            ViewBag.Reviews = reviews;

            return PartialView("_ReviewCards");
        }



        //[HttpPost]
        //public ActionResult GetReviewsByRating(int hotelId, int reviewValue)
        //{
        //    var reviews = db.ReviewRatings
        //                   .Where(r => r.HotelId == hotelId && r.Rating == reviewValue)
        //                   .ToList();

        //    return PartialView("_ReviewPartialView", reviews); // Assuming "_ReviewPartialView" is your partial view for displaying reviews.
        //}










        public ActionResult ThankYou()
        {
            return View();
        }




        public ActionResult Image()
        {
            return View();
        }














        // GET: ReviewRatings/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ReviewRating reviewRating = db.ReviewRatings.Find(id);
            if (reviewRating == null)
            {
                return HttpNotFound();
            }
            return View(reviewRating);
        }

        // POST: ReviewRatings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ReviewRating reviewRating = db.ReviewRatings.Find(id);
            db.ReviewRatings.Remove(reviewRating);
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
