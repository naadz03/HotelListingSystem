using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HotelListingSystem.Models
{
    public class Room
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }

        public int? Sleeps { get; set; }

        public int? NoOfBeds { get; set; }
        public int? Baths { get; set; }
        public bool? Balcony { get; set; }

        public int? HotelId { get; set; }
        [ForeignKey("HotelId")]
        public Hotel Hotel { get; set; }

        public string RoomImageName1 { get; set; }
        public byte[] RoomImageContent1 { get; set; }

        public string RoomImageContentType1 { get; set; }
        public Int64? RoomImageFileSize1 { get; set; }

        public string RoomImageName2 { get; set; }
        public byte[] RoomImageContent2 { get; set; }

        public string RoomImageContentType2 { get; set; }
        public Int64? RoomImageFileSize2 { get; set; }
        public DateTime? CreatedOn { get; set; }

        public decimal PricePerRoom { get; set; }

        [NotMapped]
        public Reservation Reservation { get; set; }

    }
}