using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelListingSystem.Models
{
    public static class AppHelper
    {
        public static IdentityManager IdentityManager { get; set; }

        public static HotelUsers CurrentHotelUser() 
        {
            try
            {
                using (var core = new ApplicationDbContext())
                {
                    var identityUser = HttpContext.Current?.User;

                    if (identityUser == null || !identityUser.Identity.IsAuthenticated) return null;

                    if (identityUser != null && identityUser.Identity.IsAuthenticated)
                    {
                        var systemUser = core.HotelUsers.FirstOrDefault(o => o.EmailAddress == identityUser.Identity.Name || o.UserName == identityUser.Identity.Name || o.EmailAddress.Contains(identityUser.Identity.Name)) ?? null;

                        return systemUser;
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
                //throw ex;
            }
        }

        public static bool? IsVerifiedHotel(string username)
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                var check = context.Hotels.Include("HotelUser").FirstOrDefault(x => x.HotelUser.EmailAddress == username && x.HotelUser.HotelUserType == "Business")?.IsVerified ?? true;
                return check;
            }
        }
    }
}