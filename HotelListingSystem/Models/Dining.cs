using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HotelListingSystem.Models
{
    public class Dining
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }

        public int? MealTypeId { get; set; }
        [ForeignKey("MealTypeId")]
        public MealTypes MealTypes { get; set; }

        public int? DiningTableTypeId { get; set; }
        [ForeignKey("DiningTableTypeId")]
        public DiningTableTypes DiningTableTypes { get; set; }

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public int? NoOfTables { get; set; }

        public double CostPerPerson { get; set; }
    }
}