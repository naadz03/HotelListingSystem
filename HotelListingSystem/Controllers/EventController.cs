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


namespace HotelListingSystem.Controllers
{
    [Authorize]
    public class EventController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        

        public ActionResult Index()
        {
            var user = AppHelper.CurrentHotelUser().Id;
            var events = db.HotelEvents.Include(d => d.Hotel).Include(d => d.CreateByUser).Where(a => a.CreateByUserId == user && !a.IsDeleted).ToList();
            ViewBag.Hotels = new SelectList(db.Hotels.Where(a => a.ReceptionistId == user), "Id", "Name");
            return View(events);
        }
        public ActionResult myevents()
        {
            var user = AppHelper.CurrentHotelUser().Id;
            var events = db.EventReservations
                .Include(d => d.Reservation.Hotel)
                .Include(d => d.HotelEvent)
                .Include(d => d.Customer)
                .Where(a => a.CustomerId == user).ToList();
            ViewBag.Hotels = new SelectList(db.Hotels, "Id", "Name");
            return View(events);
        }
        
        public ActionResult Events(int id, int reservationId)
        {
            HttpCookie cookie = new HttpCookie("reservationId");
            cookie.Value = $"{reservationId}";
            cookie.Expires = DateTime.Now.AddDays(7);
            Response.Cookies.Add(cookie);

            var hotel = db.Hotels.Find(id);
            ViewBag.HotelName = hotel.Name;
            var events = db.HotelEvents.Include(d => d.Hotel).Include(d => d.CreateByUser).Where(a => a.HotelId == id).ToList();
            return View(events);
        }



        public ActionResult Create(int Id)
        {
            return View(new HotelEvent { HotelId = Id });
        }
        public ActionResult book(int id)
        {
            return View(new EventReservation { HotelEvent = db.HotelEvents.Find(id) });
        }

        public ActionResult bookings(int id)
        {
            return View(new EventReservation { HotelEvent = db.HotelEvents.Find(id) });
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(HotelEvent model, int hotelId, HttpPostedFileBase file)
        {
            using(ApplicationDbContext core = new ApplicationDbContext())
            {
                model.HotelId = hotelId;
                model.CreatedDateTime = DateTime.Now;
                model.CreateByUserId = AppHelper.CurrentHotelUser().Id;
                core.HotelEvents.Add(model);
                core.SaveChanges();

                var fileContent = file.InputStream;
                byte[] data;
                data = new byte[fileContent.Length];
                file.InputStream.Read(data, 0, file.ContentLength);
                SaveHotelDocument(file.FileName, file.ContentType, data, (Int64)file.ContentLength, model.Id, "e_event_display_image");

                return RedirectToAction("Index");
            }
        }

        public ActionResult Update(int Id)
        {
            return View(db.HotelEvents.Find(Id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(HotelEvent model, HttpPostedFileBase file)
        {
            using(ApplicationDbContext core = new ApplicationDbContext())
            {
                model.ModifiedDateTime = DateTime.Now;
                core.Entry(model).State = EntityState.Modified;
                core.SaveChanges();

                if (file != null)
                {
                    var fileContent = file.InputStream;
                    byte[] data;
                    data = new byte[fileContent.Length];
                    file.InputStream.Read(data, 0, file.ContentLength);
                    SaveHotelDocument(file.FileName, file.ContentType, data, (Int64)file.ContentLength, model.Id, "e_event_display_image");
                }
                
                return View(model);
            }
        }



        public static void SaveHotelDocument(string fileName, string fileType, byte[] fileBytes, Int64 fileSize, int eventId, string doc_key_type)
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                Document document = context.Documents
                    .FirstOrDefault(c => c.HotelEventId == eventId && c.DocumentTypeKey == doc_key_type);
                if (document == null)
                {
                    document = new Document();
                    File ofile = new File();
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
                    document.DocumentTypeKey = doc_key_type;
                    document.HotelEventId = eventId;
                    context.Documents.Add(document);
                    context.SaveChanges();
                }
                else
                {
                    File ofile = context.Files.Find(document.FileId);
                    ofile.ContentType = fileType;
                    ofile.Content = fileBytes;
                    ofile.FileName = fileName;
                    ofile.FileSize = fileSize;
                    ofile.ModifiedDateTime = DateTime.Now;
                    document.ModifiedDateTime = DateTime.Now;
                    context.Entry(ofile).State = EntityState.Modified;
                    context.Entry(document).State = EntityState.Modified;
                    context.SaveChanges();
                }
            }
        }

        public ActionResult DeleteHotelEvent(int id)
        {
            try
            {
                using (ApplicationDbContext core = new ApplicationDbContext())
                {
                    var Event = core.HotelEvents.Find(id);
                    Event.IsActive = false;
                    Event.IsDeleted = true;
                    Event.HasEnded = true;
                    core.Entry(Event).State = EntityState.Modified;
                    core.SaveChanges();
                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult ActivateHotelEvent(int id)
        {
            try
            {
                using (ApplicationDbContext core = new ApplicationDbContext())
                {
                    var Event = core.HotelEvents.Find(id);
                    Event.IsActive = Event.IsActive ? false : true;
                    Event.IsDeleted = false;
                    Event.HasEnded = false;
                    core.Entry(Event).State = EntityState.Modified;
                    core.SaveChanges();
                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DownloadRoomImageFile(int Id, string doc_key_type)
        {
            try
            {
                using (ApplicationDbContext context = new ApplicationDbContext())
                {
                    Document document = context.Documents.FirstOrDefault(a => a.HotelEventId == Id && a.DocumentTypeKey == doc_key_type);
                    File file = context.Files.Find(document.FileId);
                    if (document.DocumentTypeKey == "a_customer_liveness_image")
                    {
                        Response.ContentType = "image/png";
                        return File(file.Content, file.FileName);
                    }
                    return File(file.Content, file.ContentType, file.FileName);
                }
            }
            catch
            {

            }
            return null;
        }

        public ActionResult CreditDebitPaymentYoco(string yocco_ref, int eventId, decimal amount, int NoOfTickets)
        {
            try
            {
                HttpCookie retrievedCookie = Request.Cookies["reservationId"];
                int reservationId = 0;
                if (retrievedCookie != null)
                    reservationId = int.Parse(retrievedCookie.Value);
                amount = amount / 100;
                using (ApplicationDbContext context = new ApplicationDbContext())
                {
                    var reservation = context.Reservations.Find(reservationId);
                    Payment payment = new Payment();
                    payment.YoccoReferrence = yocco_ref;
                    payment.CreatedDateTime = DateTime.Now;
                    payment.ReservationId = reservation.Id;
                    payment.HotelUserId = reservation.HotelUserId;
                    payment.HotelEventId = eventId;
                    payment.Amount = amount;
                    payment.RefNo = GetpaymentReferrence("YC", context);
                    payment.IsActive = true;
                    payment.IsPaid = false;
                    payment.PaymentMethod = "YOCCO Debit/Credit";
                    payment.InvoiceNumber = InvReferenceGenerator();
                    payment.IsPaid = true;
                    payment.ModifiedDateTime = DateTime.Now;
                    context.Payments.Add(payment);
                    context.SaveChanges();

                    EventReservation _event = new EventReservation();
                    _event.ReservationId = reservationId;
                    _event.TicketNumber = GetpaymentReferrence("TCK: ", context);
                    _event.TotalTickesCost = amount;
                    _event.CustomerId = (int)reservation.HotelUserId;
                    _event.NoOfTickets = NoOfTickets;
                    _event.NoOfTickets = NoOfTickets;
                    _event.HotelEventId = eventId;
                    context.EventReservations.Add(_event);
                    context.SaveChanges();
                    var user = context.HotelUsers.Find((int)reservation.HotelUserId);
                    new Email().SendEmail(user.EmailAddress, "Entertainment Payment", $"{user.FullName}", $"Thank for having fun with us. <br/><br/> Here is your ticket no: {_event.TicketNumber}");

                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {

            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }
        public static string GetpaymentReferrence(string Prefix, ApplicationDbContext context)
        {
            try
            {
                Random random = new Random();
                int randNumber = random.Next(10000, 99999);
                string refnos = string.Format("{0}{1}", Prefix, randNumber);
                _ = (context.Payments.Select(x => x.RefNo).Contains(refnos)) ? GetpaymentReferrence(Prefix, context) : refnos;
                return refnos;
            }
            catch
            {
                GetpaymentReferrence(Prefix, context);
            }
            return null;
        }


        public static string InvReferenceGenerator()
        {
            try
            {
                ApplicationDbContext context = new ApplicationDbContext();
                Random random = new Random();
                int randNumber = random.Next(10000, 99999);
                string RefPrefix = "INV";

                string refnos = string.Format("{0}{1}", RefPrefix, randNumber);
                _ = (context.Payments.Select(x => x.RefNo).Contains(refnos)) ? InvReferenceGenerator() : refnos;
                return refnos;
            }
            catch (Exception ex)
            {
                return InvReferenceGenerator();
            }
        }



    }
}