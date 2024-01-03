using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HotelListingSystem.Models
{
    public class Reservation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateTime CheckInDate { get; set; }

        public DateTime CheckOutDate { get; set; }

        public int? HotelId { get; set; }
        [ForeignKey("HotelId")]
        public Hotel Hotel { get; set; }

        [NotMapped]
        public string HotelName { get; set; }

        public int? RoomId { get; set; }
        [ForeignKey("RoomId")]
        public Room Room { get; set; }

        public int? HotelUserId { get; set; }
        [ForeignKey("HotelUserId")]
        public HotelUsers HotelUser { get; set; }

        public int NoOfRooms { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }

        public decimal TotalCost { get; set; }
        public decimal TotalFees { get; set; }
        public bool? Booked { get; set; }

        [NotMapped]
        public List<Dining> Dinings { get; set; }
            
        public int ? AddOnsId { get; set; }
        public decimal? AddOnsCost { get; set; }

        public bool Cancelled { get; set; }
        public bool Updated { get; set; }
        public bool PaymentApproved { get; set; }

        public int? CancelledById { get; set; }
        [ForeignKey("CancelledById")]
        public HotelUsers CancelledBy { get; set; }
        public int? UpdatedById { get; set; }
        [ForeignKey("UpdatedById")]
        public HotelUsers UpdatedBy { get; set; }
        public int? CheckInRoomId { get; set; }
        [ForeignKey("CheckInRoomId")]
        public CheckInRoom CheckInRoom { get; set; }

        public string RoomNumber { get; set; }
        public bool RoomAllocated { get; set; }
        public bool CheckInConfirmed { get; set; }
        public bool CheckOutConfirmed { get; set; }

        public bool ChkInReminder { get; set; }
        public bool ChkOutReminder { get; set; }

       

        [Display(Name = "User")]
        public string FullName
        {
            get { return string.Format("{0}, {1}", Hotel.Name, CheckInDate); }
        }

        [NotMapped]
        public List<Dining> breakfastmeals { get; set; }
        [NotMapped]
        public List<Dining> lunchmeals { get; set; }
        [NotMapped]
        public Payment payment { get; set; }
        [NotMapped]
        public Document Document { get; set; }
        [NotMapped]
        public AddOnsR Addons { get; set; }

        [NotMapped]
        public string RefundTermsnConditions { get; set; }
        public bool isVehicleoffered { get; set; }
    }
}


