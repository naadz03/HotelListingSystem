using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using HotelListingSystem.Helper;
using HotelListingSystem.Helpers;
using HotelListingSystem.Models;
using HotelListingSystem.ViewModel;

namespace HotelListingSystem.Controllers
{
    public class HotelsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private readonly RecentlyViewedHotelHelper viewedHotelHelper;
        public HotelsController()
        {
            viewedHotelHelper = new RecentlyViewedHotelHelper(db);
        }
        // GET: Hotels
        public ActionResult Index()
        {
            var hotels = db.Hotels.Where(a => a.IsVerified == null && (a.VerificationApproved == null || (bool)a.VerificationApproved != false)).Include(h => h.HotelUser);
            return View(hotels.ToList());
        }
        public ActionResult About(int Id)
        {
            var hotels = db.Hotels.Find(Id);
            if (Request.IsAuthenticated)
            {
                viewedHotelHelper.RecordView(hotels, AppHelper.CurrentHotelUser().Id);
                ViewBag.FindRecentlyViewdHotels = viewedHotelHelper.FindRecentlyViewdHotels(AppHelper.CurrentHotelUser().Id);
            }
            return View(hotels);
        }
        public ActionResult About2(int Id)
        {
            var hotels = db.Hotels.Find(Id);
            if (Request.IsAuthenticated)
            {
                viewedHotelHelper.RecordView(hotels, AppHelper.CurrentHotelUser().Id);
                ViewBag.FindRecentlyViewdHotels = viewedHotelHelper.FindRecentlyViewdHotels(AppHelper.CurrentHotelUser().Id);
            }
            return View(hotels);
        }

        public ActionResult HotelLayout(int Id)
        {
            List<SelectListItem> layouts = new List<SelectListItem>();
            layouts.Add(new SelectListItem { Text = "Layout 1", Value = "_layout_1" });
            layouts.Add(new SelectListItem { Text = "Layout 2", Value = "_layout_2" });
            layouts.Add(new SelectListItem { Text = "Layout 3", Value = "_layout_3" });
            layouts.Add(new SelectListItem { Value = "_default_", Text = "Default" });
            ViewBag.Layouts = layouts;
            return View(db.Hotels.Find(1));
        }

        // GET: Hotels
        public ActionResult MyHotels()
        {
            var user = AppHelper.CurrentHotelUser()?.Id;
            var hotels = db.Hotels.Include(h => h.HotelUser).Where(x => x.HotelUser.UserName == User.Identity.Name).ToList();
            if (User.IsInRole("Receptionist"))
            {
                hotels = db.Hotels.Include(h => h.HotelUser).Where(x => x.ReceptionistId == user).ToList();
            }
            return View(hotels);
        }

        public ActionResult HotelPayment()
        {
            var user = AppHelper.CurrentHotelUser()?.Id;
            var hotels = db.Hotels.Include(h => h.HotelUser).Where(x => x.HotelUser.UserName == User.Identity.Name).ToList();
            if (User.IsInRole("Receptionist"))
            {
                //hotels = db.Hotels.Include(h => h.HotelUser).Where(x => x.ReceptionistId == user).ToList();
            }
            if (User.IsInRole("Administrator"))
            {
                hotels = db.Hotels.Include(h => h.HotelUser).ToList();
            }
            return View(hotels);
        }


        public ActionResult ActivateTour360(Int32 Id)
        {
            Hotel hotel = db.Hotels.Find(Id);
            hotel.Tour360 = !hotel.Tour360;   
            db.Entry(hotel).State = EntityState.Modified;
            db.SaveChanges();   
            return Json(true, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        [Authorize]
        public ActionResult HotelVerification(int Id)
        {
            return View(db.Hotels.Find(Id));
        }

        [HttpPost]
        [Authorize]
        public ActionResult HotelVerification(int Id, string ClerkActionKey, string reuploadReason)
        {
            var hotel = db.Hotels.Find(Id);
            var user = db.HotelUsers.FirstOrDefault(x => x.Id == hotel.HotelUserId);
            var body = string.Empty;
            switch (ClerkActionKey)
            {
                case "true":
                    hotel.VerificationApproved = true;
                    hotel.IsVerified = true;
                    body = string.Format($"Hotel {hotel.Name}. <br/>You are required to re-upload the documents for verification");
                    new Email().SendEmail(user?.EmailAddress, "Hotel Verification Failed", user?.FullName, body);
                    db.Entry(hotel).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(new { message = string.Format($"Hotel {hotel.Name} has been approved successfully") });
                default:
                    hotel.VerificationApproved = false;
                    hotel.IsVerified = null;
                    body = string.Format($"Hotel {hotel.Name}. <br/>Your hotel has been aproved, and ready to be discovered by the world.");
                    if (!string.IsNullOrEmpty(reuploadReason)) body = string.Format($"{body} <br/>Due to: {reuploadReason}");
                    new Email().SendEmail(user?.EmailAddress, "Hotel Verification Approved", user?.FullName, body);
                    db.Entry(hotel).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(new { message = string.Format($"Hotel {hotel.Name} submited for re-upload") });
            }
        }




        [HttpPost]
        public JsonResult VerifyHotel(int id)
        {
            // Perform the necessary logic to update the hotel row
            var hotel = db.Hotels.FirstOrDefault(x => x.Id == id);
            hotel.IsVerified = true;
            db.Entry(hotel).State = EntityState.Modified;
            int savechanges = db.SaveChanges();

            var user = db.HotelUsers.FirstOrDefault(x => x.EmailAddress == User.Identity.Name);
            var body = string.Format($"Your hotel {hotel.Name} has been verified.");
            new Email().SendEmail(user?.EmailAddress, "Hotel Deactivation", user?.FullName, body);
            //new Email().SendEmail(User.Identity.Name, "Hotel Validation", user, "verified");

            // Return a JSON response to the AJAX request
            return Json(new { success = savechanges > 0, message = "Hotel updated successfully" });
        }


        [HttpPost]
        public JsonResult UnverifyHotel(int id)
        {
            // Perform the necessary logic to update the hotel row
            var hotel = db.Hotels.FirstOrDefault(x => x.Id == id);
            hotel.IsVerified = false;
            db.Entry(hotel).State = EntityState.Modified;
            int savechanges = db.SaveChanges();

            //var user = db.HotelUsers.FirstOrDefault(x => x.EmailAddress == User.Identity.Name)?.FullName;
            //new Email().SendEmail(User.Identity.Name, "Hotel Validation", user, "unverified");

            var user = db.HotelUsers.FirstOrDefault(x => x.EmailAddress == User.Identity.Name);
            var body = string.Format($"Your hotel {hotel.Name} has been Deactivated.");
            new Email().SendEmail(user?.EmailAddress, "Hotel Deactivation", user?.FullName, body);
            // Return a JSON response to the AJAX request
            return Json(new { success = savechanges > 0, message = "Hotel updated successfully" });
        }

        [HttpPost]
        public JsonResult UpdateHotel(int id, string paymentPaid, DateTime dueDate, DateTime paymentDoneDate)
        {
            // Perform the necessary logic to update the hotel row
            var hotel = db.Hotels.FirstOrDefault(x => x.Id == id);
            hotel.PaymentPaid = paymentPaid?.ToLower() == "true";
            hotel.DueDate = dueDate;
            hotel.PaymentDoneDate = paymentDoneDate;
            db.Entry(hotel).State = EntityState.Modified;
            int savechanges = db.SaveChanges();

            // Return a JSON response to the AJAX request
            return Json(new { success = savechanges > 0, message = "Hotel updated successfully" });
        }

        public ActionResult UpdateHotelLayout(int Id, string LayoutKey)
        {
            using(var db = new ApplicationDbContext())
            {
                var hotel = db.Hotels.Find(Id);
                hotel.LayoutKeyValue = LayoutKey;
                db.Entry(hotel).State = EntityState.Modified;
                db.SaveChanges();
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult BlacklistHotel(int id)
        {
            // Perform the necessary logic to update the hotel row
            var hotel = db.Hotels.FirstOrDefault(x => x.Id == id);
            hotel.Blacklisted = true;
            db.Entry(hotel).State = EntityState.Modified;
            int savechanges = db.SaveChanges();

            var user = db.HotelUsers.FirstOrDefault(x => x.EmailAddress == User.Identity.Name)?.FullName;
            new Email().SendEmail(User.Identity.Name, "Hotel Validation", user, "blacklisted, due to unsettled debt");

            // Return a JSON response to the AJAX request
            return Json(new { success = savechanges > 0, message = "Hotel updated successfully" });
        }

        //GET: Hotels
        public ActionResult FindHotel()
        {
            // Assuming the Search method returns a list of hotels based on the search criteria
            var hotels = Search(string.Empty, string.Empty, null, null);

            // Create a dictionary to store the average rating for each hotel ID
            var averageRatings = new Dictionary<int, double>();

            // Calculate and set the average rating for each hotel
            foreach (var hotel in hotels)
            {
                RatingHelper.UpdateAverageRating(db, hotel.HotelId);

                // Get the updated average rating from the database and store it in the dictionary
                var updatedHotel = db.Hotels.Find(hotel.HotelId);
                averageRatings[hotel.HotelId] = updatedHotel.AverageRating;
            }

            ViewBag.City = new SelectList(db.Hotels.Select(h => h.City).Distinct().ToList());
            ViewBag.Suburb = new SelectList(db.Hotels.Select(h => h.Suburb).Distinct().ToList());

            // Pass the list of hotels and the dictionary of average ratings to the view
            ViewBag.Hotels = hotels;
            ViewBag.AverageRatings = averageRatings;

            return View(hotels);
        }


        [HttpPost]
        public ActionResult FindHotel(string suburb, string city, DateTime? checkin, DateTime? checkout)
        {
            ViewBag.City = new SelectList(db.Hotels.Select(h => h.City).Distinct().ToList());
            ViewBag.Suburb = new SelectList(db.Hotels.Select(h => h.Suburb).Distinct().ToList());
            ViewBag.HotelId = new SelectList(db.Hotels, "Id", "Name");
            ViewBag.HotelUserId = new SelectList(db.HotelUsers, "Id", "FirstName");
            ViewBag.RoomId = new SelectList(db.Rooms, "Id", "Name");

            //var hotels = db.Hotels.Include(h => h.HotelUser);
            return View(Search(suburb, city, checkin, checkout));
        }
        // GET: /Hotels/Search
        public List<HotelReservationVM> Search(string suburb, string city, DateTime? checkin, DateTime? checkout)
        {
            // Store the search criteria in ViewBag to pass to the view for display

            ViewBag.Suburb = suburb;
            ViewBag.City = city;
            ViewBag.CheckInDate = checkin;
            ViewBag.CheckOutDate = checkout;

            if (!String.IsNullOrEmpty(city) && !String.IsNullOrEmpty(suburb) && checkin!=null && checkout != null)
            {
                using(var core = new ApplicationDbContext())
                {
                    //var User = AppHelper.CurrentHotelUser()?.Id;
                    //var reserv = core.Reservations.FirstOrDefault(a => a.CheckInDate >= checkin && a.CheckOutDate <= checkout && a.HotelUserId == User);
                    //if(reserv == null)
                    //{
                    //    var hotelsr = core.Hotels.Where(a => a.City == city || a.Suburb == suburb).ToList();
                    //    foreach (var hotel in hotelsr)
                    //    {
                    //        Reservation reservation = new Reservation
                    //        {
                    //            HotelId = hotel.Id,
                    //            HotelName = hotel.Name,
                    //            CheckInDate = (DateTime)checkin,
                    //            CheckOutDate = (DateTime)checkout
                    //        };
                    //        core.Reservations.Add(reservation);
                    //        core.SaveChanges();
                    //    }
                       
                    //}
                }
            }



            //FIX
            // Query the database to get hotels based on the search criteria
            var result = (from rooms in db.Rooms
                          join hotel in db.Hotels on rooms.HotelId equals hotel.Id
                          join reservation in db.Reservations on hotel.Id equals reservation.HotelId into hotelReservationGroup
                          from reservation in hotelReservationGroup.DefaultIfEmpty()
                          where hotel.Blacklisted != true && hotel.IsVerified == true && hotel.IsBlackListed != true
                          select new HotelReservationVM
                          {
                              HotelId = hotel.Id,
                              HotelName = hotel.Name,
                              Suburb = hotel.Suburb,
                              MaxOccupancy = hotel.MaxOccupancy,
                              RoomId = rooms.Id,
                              RoomName = rooms.Name,
                              City = hotel.City,
                              HotelUserId = hotel.HotelUserId == null ? null : hotel.HotelUserId,
                              Rating = hotel.Rating,
                              CheckInDate = reservation.CheckInDate,
                              CheckOutDate = reservation.CheckOutDate,
                              HotelImageName = hotel.HotelImageName,
                              HotelImageContent = hotel.HotelImageContent,
                              PricePerRoom = rooms.PricePerRoom,
                          }).Distinct().ToList();


            var hotels = result.Where(h =>
                (string.IsNullOrEmpty(suburb) || h.Suburb.Contains(suburb)) &&
                (string.IsNullOrEmpty(city) || h.City.Contains(city)) &&
                (!checkin.HasValue || h.CheckInDate <= checkin.Value) &&
                (!checkout.HasValue || h.CheckOutDate >= checkout.Value)).Distinct().ToList();

            if ((hotels.Count() != result.Count()) && hotels.Count() > 0)
            {
                hotels.FirstOrDefault().IsSearchResults = true;
            }
            hotels = hotels.GroupBy(h => h.HotelId)
              .Select(g => g.First())
              .ToList();
            ViewBag.City = new SelectList(db.Hotels.Select(h => h.City).Distinct().ToList());
            ViewBag.Suburb = new SelectList(db.Hotels.Select(h => h.Suburb).Distinct().ToList());
            ViewBag.HotelId = new SelectList(db.Hotels, "Id", "Name");
            ViewBag.HotelUserId = new SelectList(db.HotelUsers, "Id", "FirstName"); 
            ViewBag.RoomId = new SelectList(db.Rooms, "Id", "Name");

            return hotels; // Return the list of hotels to the view for display
        }
        public List<HotelReservationVM> Search2(string suburb, string city, DateTime? checkin, DateTime? checkout)
        {
            // Store the search criteria in ViewBag to pass to the view for display
            ViewBag.Suburb = suburb;
            ViewBag.City = city;
            ViewBag.CheckInDate = checkin;
            ViewBag.CheckOutDate = checkout;
            //FIX
            // Query the database to get hotels based on the search criteria
            var result = (from rooms in db.Rooms
                          join hotel in db.Hotels on rooms.HotelId equals hotel.Id
                         join reservation in db.Reservations on hotel.Id equals reservation.HotelId into hotelReservationGroup
                         from reservation in hotelReservationGroup.DefaultIfEmpty()
                         where reservation != null && hotel.Blacklisted != true /*&& hotel.IsVerified == true*/
                         select new HotelReservationVM
                         {
                             HotelId = hotel.Id,
                             HotelName = hotel.Name,
                             Suburb = hotel.Suburb,
                             MaxOccupancy = hotel.MaxOccupancy,
                             RoomId = rooms.Id,
                             RoomName = rooms.Name,
                             City = hotel.City,
                             HotelUserId = hotel.HotelUserId == null ? null : hotel.HotelUserId,
                             Rating = hotel.Rating,
                             CheckInDate = reservation.CheckInDate,
                             CheckOutDate = reservation.CheckOutDate,
                             HotelImageName = hotel.HotelImageName,
                             HotelImageContent = hotel.HotelImageContent,
                             PricePerRoom = rooms.PricePerRoom,
                         }).ToList();


            var hotels = result.Where(h =>
                (string.IsNullOrEmpty(suburb) || h.Suburb.Contains(suburb)) &&
                (string.IsNullOrEmpty(city) || h.City.Contains(city)) &&
                (!checkin.HasValue || h.CheckInDate <= checkin.Value) &&
                (!checkout.HasValue || h.CheckOutDate >= checkout.Value)).ToList();

            if ((hotels.Count() != result.Count()) && hotels.Count() > 0)
            {
                hotels.FirstOrDefault().IsSearchResults = true;
            }

            ViewBag.City = new SelectList(db.Hotels.Select(h => h.City).Distinct().ToList());
            ViewBag.Suburb = new SelectList(db.Hotels.Select(h => h.Suburb).Distinct().ToList());
            ViewBag.HotelId = new SelectList(db.Hotels, "Id", "Name");
            ViewBag.HotelUserId = new SelectList(db.HotelUsers, "Id", "FirstName");
            ViewBag.RoomId = new SelectList(db.Rooms, "Id", "Name");

            return hotels; // Return the list of hotels to the view for display
        }


        public ActionResult DisplayImage(int hotelId, int imageType = 1)
        {
            var hotel = db.Hotels.FirstOrDefault(h => h.Id == hotelId);
            if (imageType == 1 && hotel != null && hotel.HotelImageContent != null)
            {
                return File(hotel.HotelImageContent, hotel.HotelImageContentType);
            }
            else if (imageType == 2 && hotel != null && hotel.CertificateOfOccupancyDocContent != null)
            {
                return File(hotel.CertificateOfOccupancyDocContent, hotel.CertificateOfOccupancyDoContentType);
            }
            else if (imageType == 3 && hotel != null && hotel.COADocContent != null)
            {
                return File(hotel.COADocContent, hotel.COADocContentType);
            }
            else
            {
                // Return a default image or an error image
                // depending on your requirements
                return File("~/Content/default_image.jpg", "image/jpeg");
            }
        }


        // GET: Hotels/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                var currentUser = User.Identity.Name;
                var findHotel = db.Hotels.Include(x => x.HotelUser).FirstOrDefault(x => x.HotelUser.UserName == currentUser);
                if (findHotel == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                id = findHotel.Id;
            }
            Hotel hotel = db.Hotels.Find(id);
            if (hotel == null)
            {
                return HttpNotFound();
            }
            return View(hotel);
        }

        // GET: Hotels/Create
        public ActionResult Create()
        {
            ViewBag.HotelUserId = new SelectList(db.HotelUsers, "Id", "FirstName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Hotel hotel, List<HttpPostedFileBase> documents)
        {
            var errors = ModelState.Where(x => x.Value.Errors.Count > 0).Select(x => new { x.Key, x.Value.Errors }).ToArray();
            if (ModelState.IsValid)
            {
                hotel.HotelUserId = AppHelper.CurrentHotelUser()?.Id;
                hotel.CreatedOn = DateTime.Now;
                db.Hotels.Add(hotel);
                db.SaveChanges();
                if (documents != null)
                {
                    int current = 0;
                    foreach(var doc in documents)
                    {
                        var file = doc;
                        var fileContent = file.InputStream;
                        byte[] data;
                        data = new byte[fileContent.Length];
                        file.InputStream.Read(data, 0, file.ContentLength);
                        switch (current)
                        {
                            case 0:
                                #region old
                                //hotel.HotelImageName = file.FileName;
                                //hotel.HotelImageContentType = file.ContentType;
                                //hotel.HotelImageContent = data;
                                //hotel.HotelImageFileSize = (Int64)file.ContentLength;
                                //hotel.CertificateOfOccupancyDocName = file.FileName;
                                //hotel.CertificateOfOccupancyDoContentType = file.ContentType;
                                //hotel.CertificateOfOccupancyDocContent = data;
                                //hotel.CertificateOfOccupancyDoFileSize = (Int64)file.ContentLength;
                                //hotel.COADocName = file.FileName;
                                //hotel.COADocContentType = file.ContentType;
                                //hotel.COADocContent = data;
                                //hotel.COADocFileSize = (Int64)file.ContentLength;
                                #endregion
                                SaveHotelDocument(file.FileName, file.ContentType, data, (Int64)file.ContentLength, hotel.Id, "c_hotel_image_c");
                                break;
                            case 1:
                                SaveHotelDocument(file.FileName, file.ContentType, data, (Int64)file.ContentLength, hotel.Id, "c_certificate_of_occupancy_c");
                                break;
                            default:
                                SaveHotelDocument(file.FileName, file.ContentType, data, (Int64)file.ContentLength, hotel.Id, "c_coa_document_c");
                                break;

                        }
                        current++;
                    }
                }
                
                ViewBag.AddRooms = "true";
                return RedirectToAction("Create", "Rooms", new { hotelId = hotel.Id });
            }

            ViewBag.HotelUserId = new SelectList(db.HotelUsers, "Id", "FirstName", hotel.HotelUserId);
            return View(hotel);
        }


        public static void SaveHotelDocument(string fileName, string fileType, byte[] fileBytes, Int64 fileSize, int hotelId, string doc_key_type)
        {
            using(ApplicationDbContext context = new ApplicationDbContext())
            {
                Document document = context.Documents
                    .FirstOrDefault(c => c.HotelId == hotelId && c.DocumentTypeKey == doc_key_type);
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
                    document.HotelId = hotelId;
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

        public ActionResult DownloadHotelFile(int Id, string doc_key_type)
        {
            try
            {
                using (ApplicationDbContext context = new ApplicationDbContext())
                {
                    Document document = context.Documents.FirstOrDefault(a => a.HotelId == Id && a.DocumentTypeKey == doc_key_type);
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

        // GET: Hotels/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Hotel hotel = db.Hotels.Find(id);
            if (hotel == null)
            {
                return HttpNotFound();
            }
            ViewBag.HotelUserId = new SelectList(db.HotelUsers, "Id", "FirstName", hotel.HotelUserId);
            return View(hotel);
        }

        // POST: Hotels/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Hotel hotel, List<HttpPostedFileBase> documents)
        {
            if (ModelState.IsValid)
            {
                hotel.VerificationApproved = null;
                hotel.IsVerified = null;
                db.Entry(hotel).State = EntityState.Modified;
                db.SaveChanges();
                if (documents != null)
                {
                    int current = 0;
                    foreach (var doc in documents)
                    {
                        var file = doc;
                        var fileContent = file.InputStream;
                        byte[] data;
                        data = new byte[fileContent.Length];
                        file.InputStream.Read(data, 0, file.ContentLength);
                        switch (current)
                        {
                            case 0:
                                #region old
                                //hotel.HotelImageName = file.FileName;
                                //hotel.HotelImageContentType = file.ContentType;
                                //hotel.HotelImageContent = data;
                                //hotel.HotelImageFileSize = (Int64)file.ContentLength;
                                //hotel.CertificateOfOccupancyDocName = file.FileName;
                                //hotel.CertificateOfOccupancyDoContentType = file.ContentType;
                                //hotel.CertificateOfOccupancyDocContent = data;
                                //hotel.CertificateOfOccupancyDoFileSize = (Int64)file.ContentLength;
                                //hotel.COADocName = file.FileName;
                                //hotel.COADocContentType = file.ContentType;
                                //hotel.COADocContent = data;
                                //hotel.COADocFileSize = (Int64)file.ContentLength;
                                #endregion
                                SaveHotelDocument(file.FileName, file.ContentType, data, (Int64)file.ContentLength, hotel.Id, "c_hotel_image_c");
                                break;
                            case 1:
                                SaveHotelDocument(file.FileName, file.ContentType, data, (Int64)file.ContentLength, hotel.Id, "c_certificate_of_occupancy_c");
                                break;
                            default:
                                SaveHotelDocument(file.FileName, file.ContentType, data, (Int64)file.ContentLength, hotel.Id, "c_coa_document_c");
                                break;
                        }
                        current++;
                    }
                }
                return RedirectToAction("MyHotels");
            }
            ViewBag.HotelUserId = new SelectList(db.HotelUsers, "Id", "FirstName", hotel.HotelUserId);
            return View(hotel);
        }

        // GET: Hotels/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Hotel hotel = db.Hotels.Find(id);
            if (hotel == null)
            {
                return HttpNotFound();
            }
            return View(hotel);
        }

        // POST: Hotels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Hotel hotel = db.Hotels.Find(id);
            db.Hotels.Remove(hotel);
            db.SaveChanges();
            return RedirectToAction("MyHotels");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
