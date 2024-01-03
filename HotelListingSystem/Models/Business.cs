using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HotelListingSystem.Models
{
    public class Business
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }
        public int? Age { get; set; }

        public string NameOfBank { get; set; }

        public string AccountNumber { get; set; }
        public string BranchCode { get; set; }

        [Display(Name = "Unit No")]
        public string PhysicalAddress1 { get; set; }

        [Display(Name = "Block/ Complex Name")]
        public string PhysicalAddress2 { get; set; }

   
        [Display(Name = "Street No")]
        public string PhysicalAddress3 { get; set; }

        [Display(Name = "Street Name")]
        public string PhysicalAddress4 { get; set; }

        [Display(Name = "Suburb")]
        public string PhysicalAddress5 { get; set; }

        [Display(Name = "Postal Code")]
        public int PhysicalAddressCode { get; set; }

        public string BusinessEmail { get; set; }

        public string CertificateOfOccupancyDocName { get; set; }
        public byte[] CertificateOfOccupancyDocContent { get; set; }
        public string CertificateOfOccupancyDoContentType { get; set;}
        public int? CertificateOfOccupancyDoFileSize { get; set;}
        public bool? CertificateOfOccupancyDocVerified { get; set;}

        public string COADocName { get; set; }
        public byte[] COADocContent { get; set; }
        public string COADocContentType { get; set; }
        public int? COADocFileSize { get; set; }
        public bool? COADocVerified { get; set; }

        public DateTime? CreatedOn { get; set; }

        public int? HotelUserId { get; set; }
        [ForeignKey("HotelUserId")]
        public HotelUsers HotelUser { get; set; }

       
    }
}