using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HotelListingSystem.Models
{
    public class Tour360Request
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int HotelId { get; set; }
        [ForeignKey("HotelId")]
        public Hotel Hotel { get; set; }

        public bool Actioned { get; set; }
        public bool Approved { get; set; }

        public string comment { get; set; }
        public DateTime DateRequested { get; set; }

        public DateTime? PaymentDate { get; set; }
        public bool Paid { get; set; }
        public string Payment { get; set; }
        public bool TourUploaded { get; set; }

    }
}