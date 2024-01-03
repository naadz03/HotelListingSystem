using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HotelListingSystem.Models
{
    public class Trip
    {
        [Key]
        public int Id { get; set; }
        public string Useremail { get; set; }
        public int hotelbbkId { get; set; }
        [ForeignKey("hotelbbkId")]
        public Hotel hotelbbk { get; set; }
        public string UserLatitude { get; set; }
        public string UserLongitude { get; set; }
        public string Driveremail { get; set; }
        public double DriverLatitude { get; set; }
        public double DriverLongitude { get; set; }
        public bool isUserpicked { get; set; }
        public bool isTripprosecced { get; set; }
        public bool isTripaccepted { get; set; }
        public bool isTripcomplete { get; set; }
        public double Distance { get; set; }
        public double Tripfee { get; set; }
        public string Status { get; set; }
        public int Statusnum { get; set; }
        public int ReservationId { get; set; }
        public Reservation Reservation { get; set; }
        public IEnumerable<SelectListItem> Drivers { get; set; }
    }
}