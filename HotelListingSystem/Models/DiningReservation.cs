using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HotelListingSystem.Models
{
    public class DiningReservation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public int TableNumber { get; set; }
        public bool? IsConfirmed { get; set; }
        public int NoOfPeople { get; set; }
        public string Description { get; set; }

        public DateTime? Date { get; set; }
        public int? DiningId { get; set; }
        [ForeignKey("DiningId")]
        public Dining Dining { get; set; }

        public int? HotelUserId { get; set; }
        [ForeignKey("HotelUserId")]
        public HotelUsers HotelUsers { get; set; }

        public DateTime? CreatedOn { get; set; }

    }
}