using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HotelListingSystem.Models
{
    public class Hotel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }

        public int? MaxOccupancy { get; set; }

        [Display(Name = "Street No")]
        public string StreetNumber { get; set; }

        [Display(Name = "Street Name")]
        public string StreetName { get; set; }

        [Display(Name = "Suburb")]
        public string Suburb { get; set; }

        [Display(Name = "City")]
        public string City { get; set; }

        [Display(Name = "Postal Code")]
        public int PhysicalAddressCode { get; set; }
        [Display(Name = "Hotel Image")]
        public string HotelImageName { get; set; }
        public byte[] HotelImageContent { get; set; }

        public string HotelImageContentType { get; set; }
        public Int64? HotelImageFileSize { get; set; }

        public bool? IsVerified { get; set; }

        public int? Rating { get; set; }

        public bool? PaymentPaid { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? PaymentDoneDate { get; set; }
        public bool? Blacklisted { get; set; }
        public int? Age { get; set; }

        public string NameOfBank { get; set; }

        public string AccountNumber { get; set; }
        public string BranchCode { get; set; }


        public string BusinessEmail { get; set; }

        [Display(Name = "Certificate Of Occupancy")]
        public string CertificateOfOccupancyDocName { get; set; }
        public byte[] CertificateOfOccupancyDocContent { get; set; }
        public string CertificateOfOccupancyDoContentType { get; set; }
        public Int64? CertificateOfOccupancyDoFileSize { get; set; }
        public bool? CertificateOfOccupancyDocVerified { get; set; }

        [Display(Name = "COA")]
        public string COADocName { get; set; }
        public byte[] COADocContent { get; set; }
        public string COADocContentType { get; set; }
        public Int64? COADocFileSize { get; set; }
        public bool? COADocVerified { get; set; }

        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public int? HotelUserId { get; set; }
        [ForeignKey("HotelUserId")]
        public HotelUsers HotelUser { get; set; }

        public int? ReceptionistId { get; set; }
        [ForeignKey("ReceptionistId")]
        public HotelUsers Receptionist { get; set; }
        public bool? VerificationApproved { get; set; }
        public string LayoutKeyValue { get; set; }
        public decimal AmountOwed { get; set; }
        public bool IsBlackListed { get; set; }
        public bool IsNotified { get; set; }
        public DateTime? NotificationDate { get; set; }

        public double AverageRating { get; set; }
        public String Tour360Id { get; set; }
        public bool Tour360 { get; set; }

        public virtual ICollection<Reservation> Reservations { get; set; }
        public virtual ICollection<ReviewRating> ReviewRatings { get; set; }



    }
}