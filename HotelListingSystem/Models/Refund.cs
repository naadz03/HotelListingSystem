using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HotelListingSystem.Models
{
    public class Refund
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public int ReservationId { get; set; }
        [NotMapped]
        public Reservation Reservation { get; set; }
        public int DaysB4ChkIn { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
        public decimal Balance { get; set; }
        public decimal TotalBalance { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime DateOfPurchase { get; set; }
        public string BookingType { get; set; }
        public string TermsOfRefund { get; set; }
    }

    public class RefundViewModel
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public List<Refund> Refunds { get; set; }
        public List<Reservation> Reservations { get; set; }
    }
}