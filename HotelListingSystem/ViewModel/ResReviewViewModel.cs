using HotelListingSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelListingSystem.ViewModel
{
    public class ResReviewViewModel
    {
        public IEnumerable<Reservation> Reservations { get; set; }
        public IEnumerable<ReviewRating> UserRatings { get; set; }
    }
}