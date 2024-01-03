using HotelListingSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelListingSystem.ViewModel
{
    public class ReservationListVM
    {
        public string HotelName { get; set; }
        public string RoomName { get; set; }
        public int RoomQuantity { get; set; }
        public DateTime? Checkin { get; set; }
        public DateTime? Checkout { get; set; }
        public decimal? Cost { get; set; }
    }
    public class ReceiptDetailsViewModel
    {
        public Payment Payment { get; set; }
        public HotelUsers ReservationBy { get; set; }
        public Hotel BookedHotel { get; set; }
        public HotelUsers Receptionist { get; set; }
        public Reservation Reservation { get; set; }
        public AddOnsR Add_Ons { get; set; }
        public Room Room { get; set; }
    }
}