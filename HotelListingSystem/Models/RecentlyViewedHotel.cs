using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HotelListingSystem.Models
{
    public class RecentlyViewedHotel
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public int HotelId { get; set; }
        [ForeignKey("HotelId")]
        public Hotel Hotel { get; set; }

        public int ViewerId { get; set; }
        [ForeignKey("ViewerId")]
        public HotelUsers Viewer { get; set; }

        public DateTime Taken { get; set; }

    }
}