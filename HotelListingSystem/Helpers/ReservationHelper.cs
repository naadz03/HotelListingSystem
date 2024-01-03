using HotelListingSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelListingSystem.Helpers
{
    public class ReservationHelper
    {
        public static Int32 GetMinumumUserPoints()
        {
            using(ApplicationDbContext context = new ApplicationDbContext())
            {
                var user = AppHelper.CurrentHotelUser()?.Id;
                var getUserPoints = context.UserPoints.FirstOrDefault(a => a.SystemUserId == user);
                return (getUserPoints == null) ? (Int32)0 : (Int32)getUserPoints.AvailablePoints;
            }
        }
    }
}