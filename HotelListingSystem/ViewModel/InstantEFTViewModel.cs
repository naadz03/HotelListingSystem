using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelListingSystem.ViewModel
{
    public class InstantEFTViewModel
    {
        public string Bank { get; set; }
        public string AccountNumber { get; set; }
        public decimal Amount { get; set; }
    }
}