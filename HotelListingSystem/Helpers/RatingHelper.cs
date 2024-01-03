using HotelListingSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelListingSystem.Helper
{
    public static class RatingHelper
    {
        public static void UpdateAverageRating(ApplicationDbContext db, int hotelId)
        {
            var hotel = db.Hotels.Find(hotelId);
            if (hotel != null)
            {
                var ratings = db.ReviewRatings.Where(r => r.HotelId == hotelId).Select(r => r.Rating).ToList();
                if (ratings.Count > 0)
                {
                    double averageRating = ratings.Average();
                    hotel.AverageRating = averageRating;
                }
                else
                {
                    hotel.AverageRating = 0;
                }
                db.SaveChanges();
            }


        }
        //public static double GetRatingById(ApplicationDbContext db, int id)
        //{
        //    var reviewRating = db.ReviewRatings.FirstOrDefault(a => a.HotelId == id);
        //    return reviewRating != null ? reviewRating.Rating : 0;
        //}

        public static double GetRatingById(ApplicationDbContext db, int hotelId)
        {
            var ratings = db.ReviewRatings.Where(r => r.HotelId == hotelId).Select(r => r.Rating).ToList();
            if (ratings.Count > 0)
            {
                return ratings.Average();
            }
            return 0; // Return 0 if no ratings are found for the hotel.
        }










        public static string GetReviewById(ApplicationDbContext db, int id)
        {
            var reviewRating = db.ReviewRatings.FirstOrDefault(a => a.HotelId == id);
            return reviewRating != null ? reviewRating.Review : "";
        }

    }

}