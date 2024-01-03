using HotelListingSystem.Models;
using HotelListingSystem.ViewModel;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HotelListingSystem.Helpers;
using System.Configuration;

namespace HotelListingSystem.Controllers
{
    public class EmployeesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public HotelUsers CurrentUser { get; set; }
        public List<int> hIds { get; set; }

        public void Initialise()
        {
            if (Request.IsAuthenticated)
            {
                CurrentUser = AppHelper.CurrentHotelUser();

                if (User.IsInRole("Business Owner") || User.IsInRole("Receptionist"))
                    hIds = db.Hotels.
                        Where(a => (a.HotelUserId == CurrentUser.Id || a.ReceptionistId == CurrentUser.Id)).
                        Select(a => a.Id).ToList();

            }
        }
        public ActionResult Index()
        {
            var ReceptOrOwner = AppHelper.CurrentHotelUser().Id;
            var users = new IdentityManager().GetUsersInRole("Employee").Select(a => a.HotelUser).Where(a => a.HotelId != null).ToList();
            var hotels = db.Hotels.Where(a => (a.HotelUserId == ReceptOrOwner || a.ReceptionistId == ReceptOrOwner)).Select(a => a.Id).ToList();
            users = users.Where(a => hotels.Contains((int)a.HotelId)).ToList();
            return View(users);
        }
        public ActionResult Details(int id)
        {
            return View();
        }
        [Authorize]
        public ActionResult Onboard()
        {
            var ReceptOrOwner = AppHelper.CurrentHotelUser().Id;
            var OwnerHotels = db.Hotels.Where(a => (a.HotelUserId == ReceptOrOwner || a.ReceptionistId == ReceptOrOwner)).ToList();
            ViewBag.OwnerHotels = new SelectList(OwnerHotels, "Id", "Name");
            ViewBag.departments = new SelectList(db.Departments.ToList(), "Id", "Name");
            return View();
        }

        [HttpPost]
        public ActionResult Onboard(OnboardingViewModel collection)
        {
            try
            {
                // Retrieve the cookie by its name
                HttpCookie myCookie = Request.Cookies["ImageCookie"];
                Document document = new Document();
                if (Session["ImageCookie"] != null)
                {
                    string imageBase64 = Session["ImageCookie"].ToString();

                    File ofile = new File();
                    ofile.ContentType = "image/png";
                    ofile.Content = Convert.FromBase64String(imageBase64.Split(',').ToList()[1]);
                    ofile.FileName = String.Format("{0}_{1}", Guid.NewGuid(), collection.doc_img_key);
                    ofile.FileSize = ofile.Content.Length;
                    ofile.IsActive = true;
                    ofile.IsDeleted = false;
                    ofile.CreatedDateTime = DateTime.Now;
                    ofile.ModifiedDateTime = DateTime.Now;
                    db.Files.Add(ofile);
                    db.SaveChanges();

                    
                    document.IsActive = true;
                    document.IsDeleted = false;
                    document.CreatedDateTime = DateTime.Now;
                    document.ModifiedDateTime = DateTime.Now;
                    document.FileId = ofile.Id;
                    document.DocumentTypeKey = collection.doc_img_key;
                    db.Documents.Add(document);
                    db.SaveChanges();
                }

                String password = OnboardingHelper.GenerateStrongPassword();
                String username = OnboardingHelper.GenerateUsername(collection.FirstName, collection.LastName);
                String email = String.Format("{0}@{1}", username, ConfigurationManager.AppSettings["e-domain"].Replace("http", "").Replace("//", "").Replace("https", "").Replace("www.", "").Replace(":", ""));
                IdentityManager identityManager = new IdentityManager();
                UserStore<SystemUser> store = new UserStore<SystemUser>(db);
                UserManager<SystemUser> UserManager = new UserManager<SystemUser>(store);
                UserManager.UserValidator = new UserValidator<SystemUser>(UserManager) { AllowOnlyAlphanumericUserNames = false };

                SystemUser user = new SystemUser { UserName = username, Email = email };
                user.HotelUser = new HotelUsers
                {
                    FirstName = collection.FirstName,
                    LastName = collection.LastName,
                    IdentificationNumber = collection.IdentificationNumber,
                    UserName = username,
                    CompanyName = String.Empty,
                    EmailAddress = email,
                    MobileNumber = collection.Mobile,
                    CreatedOn = DateTime.Now,
                    HotelUserType = "Employee",
                    HotelId = collection.HotelId,
                    FaceVerified = false
                };

                password = "P@ssw0rd0#"; //remove this when deploying to azure
                var result =  UserManager.Create(user, password);
                if (result.Succeeded)
                {
                    document.UserId = user.HotelUserId;
                    db.Entry(document).State = EntityState.Modified;
                    db.SaveChanges();
                    OnboardingHelper.AddUserToEmpDepartment(db, collection, user);

                    var roleResult = identityManager.AddUserToRole(user.Id, "Employee");

                    //email notification => email
                    String body = String.Empty;
                    body += String.Format("Herewith Hotel Listing profile configuration, on login you're required to update this information.<br /><br />");
                    body += String.Format("<strong>Username: </strong>{0}<br />", username);
                    body += String.Format("<strong>Password: </strong>{0}<br />", password);
                    body += String.Format("<strong>Email: </strong>{0}<br />", email);
                    new Email().SendEmail(collection.emailPrivate, "Hotel User Configuration", String.Format("{0} {1}", collection.FirstName, collection.LastName), body, "e_user_config");

                }
                    // TODO: Add insert logic here

                    return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult CreateImageCookie(string ImageData)
        {
            // Create a new cookie
            HttpCookie myCookie = new HttpCookie("ImageCookie");
            myCookie.Value = ImageData;
            Session["ImageCookie"] = ImageData;
            myCookie.Expires = DateTime.Now.AddHours(1);
            Response.Cookies.Add(myCookie);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Edit(int id)
        {
            return View();
        }

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        public ActionResult Delete(int id)
        {
            return View();
        }

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
