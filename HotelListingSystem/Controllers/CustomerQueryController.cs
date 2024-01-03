using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using HotelListingSystem.Models;
using HotelListingSystem.ViewModel;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Http;

namespace HotelListingSystem.Controllers
{
    [Authorize]
    public class CustomerQueryController : Controller
    {
        ApplicationDbContext context = new ApplicationDbContext();
        // GET: CustomerQuery
        public ActionResult Queries()
        {
            List<CustomerQuery> customerQuery = new List<CustomerQuery>();
            try
            {
                using (ApplicationDbContext context = new ApplicationDbContext())
                {
                    var currentUserId = AppHelper.CurrentHotelUser().Id;
                    customerQuery = context.CustomerQueries.Include(c => c.Administrator).Include(c => c.Reservation.Hotel)
                        .Include(c => c.Receptionist).Include(c => c.Customer).Where(a => a.IsActive).ToList();
                    if (User.IsInRole("Customer"))
                        customerQuery = 
                            customerQuery.Where(d => d.CustomerId == currentUserId).ToList();

                    if (User.IsInRole("Administrator"))
                        customerQuery =
                            customerQuery.Where(d => d.IsEscalated && !d.IsClosed).ToList();

                    if (User.IsInRole("Receptionist"))
                        customerQuery =
                            customerQuery.Where(d => d.ReceptionistId == currentUserId && (!d.IsEscalated || d.IsClosed)).ToList();
                }
                return View(customerQuery);
            }
            catch (Exception)
            {

                return View(customerQuery);
            }
            
        }

        // GET: CustomerQuery/Details/5
        public ActionResult Details(int id)
        {
            
            return View();
        }

        // GET: CustomerQuery/Create
        public ActionResult LogQuery()
        {
            var currentUserId = AppHelper.CurrentHotelUser().Id;
            ViewBag.Reservations = new SelectList(context.Reservations.Include(c => c.Hotel).Where(a => a.HotelUserId == currentUserId).ToList(), "Id", "FullName");
            return View();
        }

        // POST: CustomerQuery/Create
        [HttpPost]
        public ActionResult LogQuery(CustomerQuery customerQuery, HttpPostedFileBase file)
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                try
                {
                    Document document = new Document();
                    HotelUsers currentUser = AppHelper.CurrentHotelUser();

                    customerQuery.CreatedDateTime = DateTime.Now;
                    customerQuery.CustomerId = currentUser.Id;
                    customerQuery.IsClosed = false;
                    customerQuery.IsEscalated = false;
                    customerQuery.IsActive = true;
                    customerQuery.IsDeleted = false;
                    customerQuery.Status = "Customer Query Ongoing";
                    customerQuery.ReceptionistId = context.Reservations.Include(d => d.Hotel)
                        .Where(a => a.Id == customerQuery.ReservationId).FirstOrDefault().Hotel.ReceptionistId;
                    context.CustomerQueries.Add(customerQuery);
                    context.SaveChanges();
                    if (file != null)
                    {
                        document = SaveFileOrDocument(context, document, file);
                        document.DocumentTypeKey = "a_customer_log_query";
                        document.CustomerQueryId = customerQuery.Id;
                        context.Documents.Add(document);
                        context.SaveChanges();
                    }

                    return RedirectToAction("Queries");
                }
                catch
                {
                    return RedirectToAction("Queries");
                }
            }
        }

        public static Document SaveFileOrDocument(ApplicationDbContext context, Document document, HttpPostedFileBase file)
        {
            Models.File ofile = new Models.File();
            if (file != null && file.ContentLength > 0)
            {
                byte[] fileBytes;
                string fileName;
                string fileType;
                int fileSize;


                using (var binaryReader = new BinaryReader(file.InputStream))
                {
                    fileBytes = binaryReader.ReadBytes(file.ContentLength);
                    fileSize = file.ContentLength;
                }
                fileName = file.FileName;
                fileType = file.ContentType;
                
                #region chatOpenAi Url
                //https://chat.openai.com/c/ab2887b7-b2a3-45cb-b4b3-53fc95fd0735
                #endregion

                ofile.ContentType = fileType;
                ofile.Content = fileBytes;
                ofile.FileName = fileName;
                ofile.FileSize = fileSize;
                ofile.IsActive = true;
                ofile.IsDeleted = false;
                ofile.CreatedDateTime = DateTime.Now;
                ofile.ModifiedDateTime = DateTime.Now;
                context.Files.Add(ofile);
                context.SaveChanges();

                document.IsActive = true;
                document.IsDeleted = false;
                document.CreatedDateTime = DateTime.Now;
                document.ModifiedDateTime = DateTime.Now;
                document.FileId = ofile.Id;
            }
            return document;
        }

        public ActionResult DownloadFile(int FileId)
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                Models.File file = context.Files.Find(FileId);
                Document document = context.Documents.FirstOrDefault(a => a.FileId == FileId);
                if (document.DocumentTypeKey == "a_customer_liveness_image")
                {
                    Response.ContentType = "image/png";
                    return File(file.Content, file.FileName);
                }
                return File(file.Content, file.ContentType, file.FileName);
            }
        }


        public ActionResult CustomerQuery(int id)
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                CustomerQuery customerQuery = context.CustomerQueries
                    .Include(c => c.Receptionist).Include(c => c.Reservation.Hotel)
                    .Include(c => c.Customer).Include(c => c.Administrator)
                    .Where(x => x.Id == id).FirstOrDefault();
                var customerQueryDocs = context.Documents.Include(c => c.File)
                    .Where(d => d.CustomerQueryId == customerQuery.Id).ToList();
                var docsCust = customerQueryDocs.FirstOrDefault(c => c.DocumentTypeKey == "a_customer_log_query");
                ViewBag.CustomerDocName = docsCust.File.FileName;
                ViewBag.CustomerFileId = docsCust.File.Id;
                if (User.IsInRole("Administrator"))
                {
                    var docsRecept = customerQueryDocs.FirstOrDefault(c => c.DocumentTypeKey == "a_customer_query_r_review");
                    ViewBag.RecepionistDocsName = docsCust.File.FileName;
                    ViewBag.RecepionistDocsFileId = docsCust.File.Id;
                    return View("AdministratorCustomerQuery", customerQuery);
                }
                return View(customerQuery);
            }
        }

        [HttpPost]
        public ActionResult AdministratorCustomerQuery(CustomerQuery customerQuery, HttpPostedFileBase file, string approvalStatus)
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                try
                {
                    var userManager = context.HotelUsers.FirstOrDefault(c => c.UserName == "Admin@hotelgroup.com");
                    CustomerQuery queryUpdate = context.CustomerQueries.Find(customerQuery.Id);
                    queryUpdate.ModifiedDateTime = DateTime.Now;
                    queryUpdate.Status = getQueryUpdateStatus(approvalStatus);
                    queryUpdate.IsClosed = true;
                    queryUpdate.FinalDescription = customerQuery.FinalDescription;
                    context.Entry(queryUpdate).State = EntityState.Modified;
                    context.SaveChanges();

                    Document document = new Document();
                    if (file != null)
                    {
                        document = SaveFileOrDocument(context, document, file);
                        document.DocumentTypeKey = "a_admin_query_docs";
                        document.CustomerQueryId = queryUpdate.Id;
                        context.Documents.Add(document);
                        context.SaveChanges();
                    }

                    var customer = context.HotelUsers.Find(queryUpdate.CustomerId);
                    var receptionist = context.HotelUsers.Find(queryUpdate.ReceptionistId);

                    new Email().SendEmail($"{customer.EmailAddress}", $"Customer Query: {queryUpdate.CreatedDateTime}", $"{customer.FullName}",
                        $"Customer query has been resolved<br/> Description: {queryUpdate.FinalDescription}");

                    new Email().SendEmail($"{receptionist.EmailAddress}", $"Customer Query: {queryUpdate.CreatedDateTime}", $"{receptionist.FullName}",
                        $"Customer query has been resolved<br/> Description: {queryUpdate.FinalDescription}");

                    return RedirectToAction("Queries");
                }
                catch
                {
                    return RedirectToAction("Queries");
                }
            }
        }
        [HttpPost]
        public ActionResult CustomerQuery(CustomerQuery customerQuery, HttpPostedFileBase file, string approvalStatus)
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                try
                {
                    var userManager = context.HotelUsers.FirstOrDefault(c => c.UserName == "Admin@hotelgroup.com");
                    CustomerQuery queryUpdate = context.CustomerQueries.Find(customerQuery.Id);
                    queryUpdate.ModifiedDateTime = DateTime.Now;
                    queryUpdate.Status = getQueryUpdateStatus(approvalStatus);

                    if (approvalStatus == "c_query_escalated_opened" && userManager != null)
                    {
                        queryUpdate.AdministratorId = userManager.Id;
                        queryUpdate.IsEscalated = true;
                    }
                    queryUpdate.EscalationDesciption = customerQuery.EscalationDesciption;
                    context.Entry(queryUpdate).State = EntityState.Modified;
                    context.SaveChanges();
                    Document document = new Document();
                    if (file != null)
                    {
                        document = SaveFileOrDocument(context, document, file);
                        document.DocumentTypeKey = "a_receptionist_query_review";
                        document.CustomerQueryId = queryUpdate.Id;
                        context.Documents.Add(document);
                        context.SaveChanges();
                    }

                    new Email().SendEmail("Admin@hotelgroup.com", "Customer Query", $"{userManager.FullName}", $"Customer query assigned to you from receptionist<br/> Description: {queryUpdate.QueryDescription}");
                    return RedirectToAction("Queries");
                }
                catch
                {
                    return RedirectToAction("Queries");
                }
            }
        }

        public ActionResult CustomerQueryDetails(int id)
        {
            var context = new ApplicationDbContext();
            CustomerQuery customerQuery = context.CustomerQueries
                    .Include(c => c.Receptionist).Include(c => c.Reservation.Hotel)
                    .Include(c => c.Customer).Include(c => c.Administrator)
                    .Where(x => x.Id == id).FirstOrDefault();
            var customerQueryDocs = context.Documents.Include(c => c.File)
                    .Where(d => d.CustomerQueryId == customerQuery.Id).ToList();
            var docsCust = customerQueryDocs.FirstOrDefault(c => c.DocumentTypeKey == "a_customer_log_query");
            var docsRecept = customerQueryDocs.FirstOrDefault(c => c.DocumentTypeKey == "a_customer_query_r_review");
            var docsAdmin = customerQueryDocs.FirstOrDefault(c => c.DocumentTypeKey == "a_admin_query_docs");
            ViewBag.CustomerDocName = docsCust.File.FileName;
            ViewBag.CustomerFileId = docsCust.File.Id;
            if (docsRecept != null)
            {
                ViewBag.RecepionistDocsName = docsRecept.File.FileName;
                ViewBag.RecepionistDocsFileId = docsRecept.File.Id;
            }
            if (docsAdmin != null)
            {
                ViewBag.AdminDocsName = docsAdmin.File.FileName;
                ViewBag.AdminDocsFileId = docsAdmin.File.Id;
            }


            return View(customerQuery);

        }

        public static string getQueryUpdateStatus(string approvalStatus)
            => (approvalStatus == "c_query_escalated_opened") ? "Customer Query Escalated/Open" : "Customer Query Closed/Solved";




        public ActionResult SendEmailNotification(string email, string userFullName, string messageBody, string messageTitle)
        {
            if (!String.IsNullOrEmpty(email) && !String.IsNullOrEmpty(userFullName) && !String.IsNullOrEmpty(messageBody) && !String.IsNullOrEmpty(messageTitle))
                new Email().SendEmail(email.Trim(), messageTitle, userFullName, messageBody);
            else
                return Json(false, JsonRequestBehavior.AllowGet);
            return Json(true, JsonRequestBehavior.AllowGet);
        }


        public ActionResult BlaclistCustomer(string email, int queryId)
        {
            try
            {
                using (ApplicationDbContext context = new ApplicationDbContext())
                {
                    var customer = context.Users.Where(c => c.UserName == email).FirstOrDefault();
                    customer.LockoutEnabled = true;
                    customer.LockoutEndDateUtc = DateTime.UtcNow.AddYears(10);
                    context.Entry(customer).State = EntityState.Modified;
                    context.SaveChanges();

                    var userManager = context.HotelUsers.FirstOrDefault(c => c.UserName == "Admin@hotelgroup.com");
                    CustomerQuery queryUpdate = context.CustomerQueries.Find(queryId);
                    queryUpdate.ModifiedDateTime = DateTime.Now;
                    queryUpdate.Status = getQueryUpdateStatus("closed");
                    queryUpdate.IsClosed = true;
                    context.Entry(queryUpdate).State = EntityState.Modified;
                    context.SaveChanges();

                    var customer1 = context.HotelUsers.Find(queryUpdate.CustomerId);
                    var receptionist = context.HotelUsers.Find(queryUpdate.ReceptionistId);

                    new Email().SendEmail($"{customer.Email}", $"Travix System: {queryUpdate.CreatedDateTime}", $"{customer1.FullName}",
                        $"You have been permanantly locked out of the system <br/>");

                    new Email().SendEmail($"{receptionist.EmailAddress}", $"Travix System:  {queryUpdate.CreatedDateTime}", $"{receptionist.FullName}",
                        $"The customer has been locked out of the system permananlty<br/>");
                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

        }

    }
}
