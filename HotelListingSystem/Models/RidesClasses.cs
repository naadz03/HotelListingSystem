using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HotelListingSystem.Models
{
    public class RidesClasses
    {
    }

    public class Vehicle
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]   
        public Guid Id { get; set; }

        [Display(Name = "Plate Number")]
        public string RegNumber { get; set; }
        public string Maker { get; set; }
        public string Color { get; set; }

        public int AddedByUserId { get; set; }
        [NotMapped]
        public HotelUsers AddedByUser { get; set; }

        [Display(Name ="For Hotel")]
        public int ForHotelId { get; set; }
        [NotMapped]
        public Hotel ForHotel { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class RideRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public int ReservationId { get; set; }
        [NotMapped]
        public Reservation Reservation { get; set; }

        public Guid VehicleId { get; set; }
        [NotMapped]
        public Vehicle Vehicle { get; set; }

        public DateTime PickUpDate { get; set; }

        public string Location { get; set; }

        public string Lat { get; set; }
        public string Long { get; set; }

        public bool IsApproved { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public bool Ended { get; set; }
        public bool InProgress { get; set; }
    }
}