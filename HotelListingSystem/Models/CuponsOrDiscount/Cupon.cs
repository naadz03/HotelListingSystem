using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HotelListingSystem.Models.CuponsOrDiscount
{
    public class Cupon
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]  
        public Int32 Id { get; set; }

        [Display(Name ="Cupon Code")]
        public String Code { get; set; }

        [Display(Name = "Percentage Cupon?")]
        public Boolean IsPercentage { get; set; }

        [Display(Name = "Amount R/%")]
        public Int32 Amount { get; set; }

        [Display(Name = "Active Flag")]
        public Boolean IsActive { get; set; }

        [Display(Name = "Deleted Flag")]
        public Boolean IsDeleted { get; set; }

        public int HotelId { get; set; }
        [ForeignKey("HotelId")]
        public Hotel Hotel { get; set; }
    }
}