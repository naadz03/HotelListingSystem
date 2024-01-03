using HotelListingSystem.Controllers;
using HotelListingSystem.Models;
using HotelListingSystem.Helpers;
using HotelListingSystem.ViewModel;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Helpers;

namespace HotelListingSystem.ApiControllers
{
    public class UserApiController : ApiController
    {

        [HttpPost]
        [Route("api/users/login")]
        public async Task<IHttpActionResult> Login([FromBody] LoginViewModel loginViewModel)
        {
            GenericCustomerResponse genericCustomerResponse = new GenericCustomerResponse();
            string message = string.Empty;
            try
            {
                using (ApplicationDbContext context = new ApplicationDbContext())
                {
                    var store = new UserStore<SystemUser>(context);
                    var UserManager = new UserManager<SystemUser>(store);
                    UserManager.UserValidator = new UserValidator<SystemUser>(UserManager) { AllowOnlyAlphanumericUserNames = false };

                    loginViewModel.Email = loginViewModel.Email.Trim();
                    loginViewModel.Password = loginViewModel.Password.Trim();

                    (GenericCustomerResponse, string) loginVM = await Helpers.SecurityHelper.AuthenticateUser(loginViewModel, context, UserManager);
                    genericCustomerResponse = loginVM.Item1;
                    message = loginVM.Item2;
                    genericCustomerResponse.Message = message;
                }
            }
            catch (Exception)
            {

                // throw;
            }

            if (genericCustomerResponse.returnData != null && genericCustomerResponse.StatusCode == "200")
                return Ok(genericCustomerResponse);
            else
                return BadRequest(message);
        }
    }
}
