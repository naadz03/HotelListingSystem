using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelListingSystem.ViewModel
{
    public class OnboardingViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Mobile { get; set; }
        public string IdentificationNumber { get; set; }
        public string Gender { get; set; }
        public byte[] Image { get; set; }
        public string ImageBase64 { get; set; }
        public string doc_img_key => "d_user_image_reg";
        public int HotelId { get; set; }
        public string emailPrivate { get; set; }
        public string Email { get; set; }
        public string DepartmentId { get; set; }


    }
}