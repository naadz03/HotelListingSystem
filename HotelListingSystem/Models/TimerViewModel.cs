using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace HotelListingSystem.Models
{
    [NotMapped]
    public class TimerViewModel
    {
        /// Gets or sets the HTML content property.  
        /// </summary>  
        [Display(Name = "Time")]
        public string Timer { get; set; }

        /// <summary>  
        /// Gets or sets message property.  
        /// </summary>  
        [Display(Name = "Message")]
        public string Message { get; set; }

        /// <summary>  
        /// Gets or sets a value indicating whether it is valid or not property.  
        /// </summary>  
        [Display(Name = "Is Valid")]
        public bool IsValid { get; set; }
    }


    public class BusinessPDFStatement
    {
        public string HotelName { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string PaymentBy { get; set; }
        public decimal Amountpaid { get; set; }
        public decimal TravixComission { get; set; }
        public int PaymentId { get; set; }
    }

    public class ServicePaymentPDFStatement
    {
        public string HotelName { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentBy { get; set; }
        public decimal Amountpaid { get; set; }
        public string paymentType { get; set; }
        public string paymentRef { get; set; }
    }

}