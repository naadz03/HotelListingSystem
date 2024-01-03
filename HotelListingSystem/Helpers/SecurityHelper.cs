using HotelListingSystem.Models;
using HotelListingSystem.Controllers;
using HotelListingSystem.ViewModel;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace HotelListingSystem.Helpers
{
    public static class SecurityHelper
    {
        public static async Task<(GenericCustomerResponse, string)> AuthenticateUser(LoginViewModel model, ApplicationDbContext db, UserManager<SystemUser> UserManager)
        {
            GenericCustomerResponse genericCustomerResponse= new GenericCustomerResponse();
            string message = string.Empty;
            try
            {
                using (var context = db)
                {
                    if (model.Email != null && model.Password != null)
                    {
                        dynamic user = await UserManager.FindAsync(model.Email.Trim(), model.Password.Trim());

                        if (user != null)
                        {
                            var token = user.Id;
                            user = db.HotelUsers.Find(user.HotelUserId);
                            user.token = token;

                            genericCustomerResponse.returnData = user;
                            genericCustomerResponse.StatusCode = "200";
                        }
                        else
                        {
                            message = "Invalid username/Email or password Combination";
                            genericCustomerResponse.returnData = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                message = "Internal server error";
                genericCustomerResponse.returnData = null;
            }
            return (genericCustomerResponse, message);
        }

    }
}