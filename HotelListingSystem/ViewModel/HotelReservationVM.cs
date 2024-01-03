using HotelListingSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelListingSystem.ViewModel
{
    public class HotelReservationVM
    {
        public int HotelId { get; set; }
        public string HotelName { get; set; }
        public int? RoomId { get; set; }
        public string RoomName { get; set; }
        public string Suburb { get; set; }
        public string City { get; set; }
        public int? HotelUserId { get; set; }
        public int? Rating { get; set; }
        public int? MaxOccupancy { get; set; }
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public string HotelImageName { get; set; }
        public byte[] HotelImageContent { get; set; }
        public bool? IsSearchResults { get; set; }
        public decimal PricePerRoom { get; set; }

        public HotelRatingVM HotelRatingVM { get; set; }

        public Hotel Hotel { get; set; }

    }
}