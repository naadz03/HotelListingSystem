using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HotelListingSystem.Models
{
    public class Payment
    {
        [Column(Order = 1)]
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Id")]
        public int Id { get; set; }

        [Column(Order = 2)]
        public int? HotelUserId { get; set; }
        [ForeignKey("HotelUserId")]
        public HotelUsers HotelUser { get; set; }


        [Column(Order = 3)]
        public int? ReservationId { get; set; }
        [ForeignKey("ReservationId")]
        public Reservation Reservation { get; set; }

        [Column(Order = 4)]
        public string RefNo { get; set; }

        public bool IsPaid { get; set; }

        [Column(Order = 5)]
        public decimal Amount { get; set; }


        [Column(Order = 100)]
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Created On")]
        [Column(Order = 104)]
        public DateTime? CreatedDateTime { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Modified On")]
        [Column(Order = 106)]
        public DateTime? ModifiedDateTime { get; set; }

        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; }

        [Display(Name = "Invoice Number")]
        public string InvoiceNumber { get; set; }
        public string YoccoReferrence { get; set; }

        [Column(Order = 107)]
        public int? HotelId { get; set; }
        [ForeignKey("HotelId")]
        public Hotel Hotel { get; set; }

        public bool Servicepayment { get; set; }
        public string PaymentType { get; set; }

        [Column(Order = 110)]
        public int? HotelEventId { get; set; }
        [ForeignKey("HotelEventId")]
        public HotelEvent HotelEvent { get; set; }

        public string PaymentStatus
        {
            get { return this.IsPaid ? "Approved" : "Pending"; }
        }

    }
}