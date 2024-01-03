using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HotelListingSystem.Models.Driver
{
    public class Driverprofile
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Vehicleplate { get; set; }
        public int Tripscompleted { get; set; }
        public string Currentlatitude { get; set; }
        public string Currentlongitude { get; set; }
        public bool isAvailable { get; set; }

        public int HotelUserId { get; set; }
        [ForeignKey("HotelUserId")]
        public HotelUsers HotelUser { get; set; }

        public Nullable< int > UserHotelId { get; set; }
        [ForeignKey("UserHotelId")]
        public Hotel UserHotel { get; set; }

        
    }
}