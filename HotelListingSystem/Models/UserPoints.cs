using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HotelListingSystem.Models
{
    public class UserPoints
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Display(Name = "System User")]
        public Int32 SystemUserId { get; set; }
        [ForeignKey("SystemUserId")]
        public HotelUsers SystemUser { get; set; }

        [Display(Name = "Available Points")]
        public Int32 AvailablePoints { get; set; }

        [Display(Name = "Points Accumulated")]
        public Int32 PointsAccumulated { get; set; }

        [Display(Name = "Point System Activated")]
        public Boolean IsActive { get; set; }
    }
}