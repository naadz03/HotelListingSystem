using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelListingSystem.Models
{
    public class CustomerQuery : BaseModel
    {
        [Column(Order = 6)]
        public int? CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public HotelUsers Customer { get; set; }

        [Column(Order = 7)]
        public int? ReceptionistId { get; set; }
        [ForeignKey("ReceptionistId")]
        public HotelUsers Receptionist { get; set; }

        [Column(Order = 8)]
        public int? AdministratorId { get; set; }
        [ForeignKey("AdministratorId")]
        public HotelUsers Administrator { get; set; }

        [Column(Order = 9)]
        public int? ReservationId { get; set; }
        [ForeignKey("ReservationId")]
        public Reservation Reservation { get; set; }

        [Column(Order = 10)]
        [Display(Name = "Is Active")]
        public bool IsEscalated { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Is Active")]
        public bool IsClosed { get; set; }

        [Column(Order = 12)]
        [Display(Name = "File Name")]
        public string QueryDescription { get; set; }

        [Column(Order = 13)]
        [Display(Name = "File Name")]
        public string EscalationDesciption { get; set; }

        [Column(Order = 14)]
        [Display(Name = "File Name")]
        public string FinalDescription { get; set; }

        public string  Status { get; set; }
    }

    public class File : BaseModel
    {
        [Column(Order = 6)]
        [Display(Name = "File Name")]
        [StringLength(1024)]
        public string FileName { get; set; }

        [Column(Order = 7)]
        [Display(Name = "Content Type")]
        [StringLength(256)]
        public string ContentType { get; set; }

        [Column(Order = 8)]
        [Display(Name = "Content")]
        public byte[] Content { get; set; }

        [Column(Order = 9)]
        [Display(Name = "File Size (Bytes)")]
        public Int64 FileSize { get; set; }

        [NotMapped]
        public List<Document> Documents { get; set; }
    }

    public class Document : BaseModel
    {
        [Column(Order = 6)]
        public int FileId { get; set; }
        [ForeignKey("FileId")]
        public File File { get; set; }

        [Column(Order = 7)]
        public int? CustomerQueryId { get; set; }
        [ForeignKey("CustomerQueryId")]
        public CustomerQuery CustomerQuery { get; set; }
        [Column(Order = 8)]
        public string DocumentTypeKey { get; set; }

        [Column(Order = 9)]
        public int? ReservationId { get; set; }
        [ForeignKey("ReservationId")]
        public Reservation Reservation { get; set; }

        [Column(Order = 10)]
        public int? HotelId { get; set; }
        [ForeignKey("HotelId")]
        public Hotel Hotel { get; set; }

        [Column(Order = 11)]
        public int? RoomId { get; set; }
        [ForeignKey("RoomId")]
        public Room Room { get; set; }

        [Column(Order = 12)]
        public int? HotelEventId { get; set; }
        [ForeignKey("HotelEventId")]
        public HotelEvent HotelEvent { get; set; }

        public int? UserId { get; set; }
    }

    public class BaseModel
    {
        [Column(Order = 1)]
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Id")]
        public int Id { get; set; }

        [Column(Order = 2)]
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        [Column(Order = 3)]
        [Display(Name = "Is Active")]
        public bool IsDeleted { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Created On")]
        [Column(Order = 4)]
        public DateTime? CreatedDateTime { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Modified On")]
        [Column(Order = 5)]
        public DateTime? ModifiedDateTime { get; set; }
    }
}