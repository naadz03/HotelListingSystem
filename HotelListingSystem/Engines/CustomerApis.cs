using HotelListingSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Security.Cryptography;
using HotelListingSystem.Controllers;
using Microsoft.AspNet.Identity.Owin;
using System.Data.Entity;

namespace HotelListingSystem.Engines
{
   
    public static class CustomerApis
    {
        private static ApplicationDbContext core = new ApplicationDbContext();
        public static HotelUsers GetUserLoginDetails(string Username, string Password)
        {
            HotelUsers User = new HotelUsers();
            var AspNetUser = GetUserInfo(new ApplicationDbContext(), Username.Trim());
            if(AspNetUser != null)
            {
                var HashedLoginPassword = SecurityEncryption.EncryptPassword(Password);
                int PasswordIsValid = string.Compare(core.HotelUsers.Find(AspNetUser.HotelUserId).MobileAppPassword, HashedLoginPassword);
                if (PasswordIsValid > 0)
                    return core.HotelUsers.Find(AspNetUser.HotelUserId);
            }
            return User;
        }

        public static  (List<Room>, List<Hotel>) ReturnAvailablleHotelsRooms()
        {
            return (GetAllAvailableRooms(core), GetAllActiveHotels(core));
        }

        public static List<Room> GetAllAvailableRooms(ApplicationDbContext core) 
            => core.Rooms.Where(a => a.HotelId != null).Include(a => a.Hotel).ToList();
        public static List<Hotel> GetAllActiveHotels(ApplicationDbContext core)
            => core.Hotels.Where(a => (bool)a.VerificationApproved && (bool)a.IsVerified).Include(a => a.HotelUser).Include(a => a.Receptionist).ToList();

        public static SystemUser GetUserInfo(ApplicationDbContext core, string Username)
            => core.Users.FirstOrDefault(x => x.UserName == Username);
    }

    public class UserLoginViewModel
    {
        public HotelUsers User { get; set; }
             
    }

}