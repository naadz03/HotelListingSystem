using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using HotelListingSystem.Models;
using System.Text;
using Microsoft.AspNet.Identity.EntityFramework;
using HotelListingSystem.ViewModel;
using System.Net.Http;
using System.Data.Entity;
using HotelListingSystem.Engines;
using System.Web.UI.WebControls;

namespace HotelListingSystem.Controllers
{
  
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private SystemUserManager _userManager;
        private ApplicationDbContext db = new ApplicationDbContext();

        public AccountController()
        {

        }
        public AccountController(SystemUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public SystemUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<SystemUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl = null)
        {
            return View();
        }

        //public static async  Task<dynamic> PasswordSignInAsync(ApplicationDbContext core, UserManager<SystemUser> userManager, UserStore<SystemUser> store,  string email,string password)
        //{
        //    try
        //    {
        //        return  await SignInManager.PasswordSignInAsync(email, password, false, shouldLockout: false);
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}



        #region mobile app region
        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> jsonlogin(LoginViewModel model)
        {
            try
            {
                using (ApplicationDbContext _context = new ApplicationDbContext())
                {
                    SystemUser user = _context.Users.FirstOrDefault(a => a.Email == model.Email || a.UserName == model.Email);
                    SignInStatus result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, false, shouldLockout: false);
                    if (result == SignInStatus.Success)
                    {
                        HotelUsers _user = _context.HotelUsers.FirstOrDefault(a => a.Id == user.HotelUserId);
                        return new JsonResult
                        {
                            Data = _user,
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet
                        };
                    }
                    else
                    {
                        return new JsonResult
                        {
                            Data = "Invalid username or password.",
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet
                        };
                    }
                }
            }
            catch (Exception)
            {
                return new JsonResult
                {
                    Data = "Error 404",
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            
        }

        [HttpGet]
        [AllowAnonymous]
        public JsonResult jsonusers()
        {
            try
            {
                using (ApplicationDbContext _context = new ApplicationDbContext())
                {
                    return new JsonResult
                    {
                        Data = _context.Users.ToList(),
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
            }
            catch (Exception)
            {
                return new JsonResult
                {
                    Data = "Error 404",
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
        }
        #endregion









        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Find the user by username instead of email
            var store = new UserStore<SystemUser>(db);
            var UserManager = new UserManager<SystemUser>(store);
            UserManager.UserValidator = new UserValidator<SystemUser>(UserManager) { AllowOnlyAlphanumericUserNames = false };
            var user = await UserManager.FindByNameAsync(model.Email);

            //if (user != null)
            //{
            // Use PasswordSignInAsync with the found user
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            //var result = await SignInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    if (string.IsNullOrEmpty(user.HotelUser.MobileAppPassword))
                    {
                        using (ApplicationDbContext core = new ApplicationDbContext())
                        {
                            HotelUsers register = core.HotelUsers.Find(user.HotelUserId);
                            register.MobileAppPassword = SecurityEncryption.EncryptPassword(model.Password);
                            core.Entry(register).State = EntityState.Modified;
                            core.SaveChanges();
                        }
                    }
                    if (UserManager.IsInRole(user.Id, "Receptionist"))
                    {
                        return RedirectToAction("", "Dashboard");
                    }
                    else if (UserManager.IsInRole(user.Id, "Business Owner"))
                    {
                        return RedirectToAction("", "Dashboard");
                    }
                    else if (UserManager.IsInRole(user.Id, "Customer"))
                    {
                        return RedirectToAction("FindHotel", "Hotels");
                    }
                    else if (UserManager.IsInRole(user.Id, "Administrator"))
                    {
                        return RedirectToAction("", "Dashboard");
                    }
                    else if (UserManager.IsInRole(user.Id, "Employee"))
                    {
                        empDepartment emp = db.empDepartments.FirstOrDefault(a => a.EmplyeeKey == user.Id.ToString());

                        if (!emp.IsActive)
                        {
                            if (UserManager.IsInRole(user.Id, "Receptionist"))
                            {
                                return RedirectToAction("", "Dashboard");
                            }
                            else if (UserManager.IsInRole(user.Id, "Business Owner"))
                            {
                                return RedirectToAction("", "Dashboard");
                            }
                            return RedirectToAction("Details", "HotelUsers", new { id = user.HotelUserId });
                        }
                           
                        else
                            return RedirectToAction("Index", "Home");
                    }
                        return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
            //}
            //ModelState.AddModelError("", "Invalid login attempt.");
            //return View(model);
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }


        // GET: HotelUsers/Create
        public ActionResult AddReceptionist()
        {
            var user = AppHelper.CurrentHotelUser().Id;
            var hotels = db.Hotels.Where(a => a.HotelUserId == user).ToList();
            ViewBag.HotelsList = new SelectList(hotels, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddReceptionist(RegisterViewModel model)
        {
            var identityManager = new IdentityManager();
            var user = new SystemUser { UserName = model.Email, Email = model.Email, EmailConfirmed = true };
            user.HotelUser = new HotelUsers
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                IdentificationNumber = model.IdentificationNumber,
                UserName = model.Email,
                CompanyName = model?.CompanyName,
                EmailAddress = model.Email,
                MobileNumber = model?.MobileNumber,
                CreatedOn = DateTime.Now,
                HotelUserType = model.HotelUserType
            };

            var password = GenerateProfilePassword();
            if (model.HotelId != 0)
            {
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    var roleResult = identityManager.AddUserToRole(user.Id, "Receptionist");
                    using(ApplicationDbContext core = new ApplicationDbContext())
                    {
                        var hotel = core.Hotels.Find(model.HotelId);
                        hotel.ReceptionistId = user.HotelUserId;
                        core.Entry(hotel).State = EntityState.Modified;
                        core.SaveChanges();
                    }
                    string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    new Email().SendEmail(model.Email, "Receptionist Confirmation", model.FirstName + " " + model.LastName, "Please user the below information to confirm your account <br/> Username: " + model.Email + "<br/>Password" + password, false);
                    return RedirectToAction("Index", "Home", new { id = User.Identity.GetUserId(), message = "Receptionist added successfully" });
                }
                AddErrors(result);
            }
            var user2 = AppHelper.CurrentHotelUser().Id;
            var hotels = db.Hotels.Where(a => a.HotelUserId == user2).ToList();
            ViewBag.HotelsList = new SelectList(hotels, "Id", "Name");
            return View(model);
        }

        public ActionResult AddUserToReceptionist()
        {
            var role = db.Roles.FirstOrDefault(x=>x.Name == "Customer").Id;
            ViewBag.HotelId = new SelectList(db.Hotels, "Id", "Name");
            var users = (from user in db.Users
                         where user.Roles.Any(x=>x.RoleId == role)
                         select new UserVM
                         {
                             UserId = user.Id,
                             UserName = user.UserName,
                             FullName = user.HotelUser.FirstName +" "+user.HotelUser.LastName, 
                             Email = user.Email,
                         }).ToList();
            return View(users);
        }

        [HttpPost]
        public async Task<JsonResult> AddUserToReceptionist(string userId, int hotelId)
        {

            var user = await UserManager.FindByNameAsync(userId);
            if (user == null)
            {
                // User not found
                return Json(new { message = "User not found" });
            }

            var currentRole = (await UserManager.GetRolesAsync(user.Id)).FirstOrDefault();
            if (currentRole != null)
            {
                // Remove user from current role
                var result = await UserManager.RemoveFromRoleAsync(user.Id, currentRole);
                if (!result.Succeeded)
                {
                    // Failed to remove user from current role
                    return Json(new { message = "Operation failed" });
                }
            }

            // Add user to Receptionist role
            var receptionistRole = "Receptionist";
            var addResult = await UserManager.AddToRoleAsync(user.Id, receptionistRole);
            if (!addResult.Succeeded)
            {
                // Failed to add user to Receptionist role
                return Json(new { message = "Operation failed" });
            }

            var hotel = db.Hotels.FirstOrDefault(x => x.Id == hotelId);
            hotel.ReceptionistId = user.HotelUserId;
            db.Entry(hotel).State = EntityState.Modified;
            int count = db.SaveChanges();


            // User successfully added to Receptionist role
            return Json(new { message = (count > 0 ? "Success" : "Failed" )});
            
        }



        public static string GenerateProfilePassword()
        {
            Random randomLength = new Random();
            int passwordLength = randomLength.Next(8, 10);

            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()_+-=";
            StringBuilder password = new StringBuilder();

            Random random = new Random();
            while (0 < passwordLength--)
            {
                password.Append(validChars[random.Next(validChars.Length)]);
            }

            return password.ToString();
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var identityManager = new IdentityManager();
                var user = new SystemUser { UserName = model.Email, Email = model.Email };
                user.HotelUser = new HotelUsers
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    IdentificationNumber = model.IdentificationNumber,
                    UserName = model.Email,
                    CompanyName = model?.CompanyName,
                    EmailAddress = model.Email,
                    MobileNumber = model?.MobileNumber,
                    CreatedOn = DateTime.Now,
                    HotelUserType = model.HotelUserType
                };

                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    var roleResult = model.HotelUserType == "Business" ? identityManager.AddUserToRole(user.Id, "Business Owner") : identityManager.AddUserToRole(user.Id, "Customer");

                    await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);

                    using (ApplicationDbContext core = new ApplicationDbContext())
                    {
                        HotelUsers register = core.HotelUsers.Find(user.HotelUserId);
                        register.MobileAppPassword = SecurityEncryption.EncryptPassword(model.Password);
                        core.Entry(register).State = EntityState.Modified;
                        core.SaveChanges();
                    }
                        
                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");
                    if (model.HotelUserType == "Business")
                    {
                        return RedirectToAction("Create", "Hotels"/*, new {id = user.HotelUserId}*/);
                    }
                    return RedirectToAction("Index", "Home"/*, new {id = user.HotelUserId}*/);
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new SystemUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }

        #endregion
    }
}