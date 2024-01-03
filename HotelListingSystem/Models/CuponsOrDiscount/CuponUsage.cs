using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HotelListingSystem.Models.CuponsOrDiscount
{
    public class CuponUsage
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 Id { get; set; }
        public Int32 CuponId { get; set; }
        public Nullable<Int32> ReservationId { get; set; }
    }
}