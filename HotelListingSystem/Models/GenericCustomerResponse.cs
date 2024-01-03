using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelListingSystem.Models
{
    public class GenericCustomerResponse
    {
        public string StatusCode { get; set; }
        public string Message { get; set; }
        public dynamic returnData { get; set; }
    }
}