using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelListingSystem.Models
{
    public class HotelEvent : BaseModel
    {
        [Column(Order = 6)]
        public string Name { get; set; }

        [Column(Order = 7)]
        public string Description { get; set; }

        [Column(Order = 8)]
        public string SpecialFeatures { get; set; }

        [Column(Order = 9)]
        public decimal Price { get; set; }

        [Column(Order = 10)]
        public int Duration { get; set; }

        [Column(Order = 11)]
        public DateTime StartDate { get; set; }

        [Column(Order = 12)]
        public bool DailyOccurance { get; set; }

        [Column(Order = 13)]
        public int HotelId { get; set; }
        [ForeignKey("HotelId")]
        public Hotel Hotel { get; set; }

        [Column(Order = 14)]
        public int CreateByUserId { get; set; }
        [ForeignKey("CreateByUserId")]
        public HotelUsers CreateByUser { get; set; }

        [Column(Order = 15)]
        public int NoOfTickets { get; set; }

        [Column(Order = 16)]
        public bool HasEnded { get; set; }
    }

    public class EventReservation : BaseModel
    {
        [Column(Order = 6)]
        public int ReservationId { get; set; }
        [ForeignKey("ReservationId")]
        public Reservation Reservation { get; set; }

        [Column(Order = 7)]
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public HotelUsers Customer { get; set; }

        [Column(Order = 8)]
        public int NoOfTickets { get; set; }

        [Column(Order = 9)]
        public decimal TotalTickesCost { get; set; }

        [Column(Order = 10)]
        public int? HotelEventId { get; set; }
        [ForeignKey("HotelEventId")]
        public HotelEvent HotelEvent { get; set; }

        public string TicketNumber { get; set; }
        public bool IsUsed { get; set; }
    }


    public class Review : BaseModel
    {
        [Column(Order = 6)]
        public int? EventId { get; set; }
        [ForeignKey("EventId")]
        public HotelEvent Event { get; set; }

        [Column(Order = 7)]
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public HotelUsers Customer { get; set; }

        [Column(Order = 8)]
        public string Description { get; set; }

        [Column(Order = 9)]
        public int Rating { get; set; }
    }
}