using PayPal.Api;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HotelListingSystem.Models
{
    public class ReviewRating
    {

        [Key, Column(Order = 0)]
        public string ReviewerId { get; set; }

        [Key, Column(Order = 1)]
        public int HotelId { get; set; }

        [ForeignKey("HotelId")]
        [Display(Name = "HotelId")]
        public Hotel Hotel { get; set; }

        public double Rating { get; set; }
        public string Review { get; set; }
        public DateTime CreatedAt { get; set; }

        //public int? HotelUserId { get; set; }
        //[ForeignKey("HotelUserId")]
        //public HotelUsers HotelUser { get; set; }

        // Navigation properties
        //public virtual ApplicationUser User { get; set; } // If using ApplicationUser class for User management
        //public virtual Hotel Hotel { get; set; }

    }
}