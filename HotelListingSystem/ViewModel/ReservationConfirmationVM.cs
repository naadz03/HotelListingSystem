using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelListingSystem.ViewModel
{
    public class ReservationConfirmationVM
    {
        public int HotelId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NoOfRooms { get; set; }
        public int RoomId { get; set; }

        public string Bank { get; set; }
        public string AccountNumber { get; set; }
        public decimal Amount { get; set; }
    }
}