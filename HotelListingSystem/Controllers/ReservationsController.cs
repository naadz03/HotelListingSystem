using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using HotelListingSystem.Models;
using HotelListingSystem.ViewModel;
using Microsoft.AspNet.Identity;

namespace HotelListingSystem.Controllers
{
    public class ReservationsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Reservations
        public ActionResult IndexAll()
        {
            var reservations = db.Reservations.Include(r => r.Hotel).Include(r => r.HotelUser).Include(r => r.Room);
            return View(reservations.ToList());
        }       
        
        // GET: Reservations
        public ActionResult Index()
        {
            var user = AppHelper.CurrentHotelUser()?.Id;
            if (user == null) 
                user = db.HotelUsers.FirstOrDefault(a => a.UserName == User.Identity.Name).Id;
            var reservations = db.Reservations.Include(r => r.Hotel).Include(r => r.HotelUser).Include(r => r.Room).Where(x=>x.HotelUserId == user).ToList();
            return View(reservations);
        }
        public ActionResult MyReservations()
        {
            var user = AppHelper.CurrentHotelUser();
            var reservations = db.Reservations.Include(r => r.Hotel).Include(r => r.HotelUser).Include(r => r.Room).Where(x=>x.HotelUserId == user.Id).ToList();
            return View(reservations);
        }


        // GET: Reservations/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reservation reservation = db.Reservations
                .Include(c => c.Hotel)
                .Include(c => c.Room)
                .Include(c => c.HotelUser)
                .Include(c => c.CheckInRoom)
                .FirstOrDefault(a => a.Id == id);
            reservation.breakfastmeals = db.Dinings.Include(d => d.MealTypes).Where(a => a.MealTypes.Name.Contains("Breakfast")).ToList();
            reservation.lunchmeals = db.Dinings.Where(a => a.MealTypes.Name.Contains("lunch")).ToList();
            if (reservation.AddOnsId != null)
                reservation.Addons = db.AddOnsRs.Find(reservation.AddOnsId);
            ViewBag.ThisHotelRooms = new SelectList(db.Rooms.Where(a => a.HotelId == reservation.HotelId).ToList(), "Id", "Name");
            if (reservation == null)
            {
                return HttpNotFound();
            }
            ViewBag.HotelId = new SelectList(db.Hotels, "Id", "Name");
            ViewBag.HotelUserId = new SelectList(db.HotelUsers, "Id", "FirstName");
            ViewBag.ThisHotelRooms = new SelectList(db.Rooms, "Id", "Name");
            return View(reservation);
        }

        public ActionResult CreateReservation()
        {
            ViewBag.HotelId = new SelectList(db.Hotels, "Id", "Name");
            ViewBag.HotelUserId = new SelectList(db.HotelUsers, "Id", "FirstName");
            ViewBag.RoomId = new SelectList(db.Rooms, "Id", "Name");
            var reservations = db.Rooms.Include(x => x.Hotel).ToList();
            return View(reservations);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateReservation([Bind(Include = "Id,CheckInDate,CheckOutDate,HotelId,RoomId,HotelUserId")] Reservation reservation)
        {
            reservation.HotelUserId = AppHelper.CurrentHotelUser()?.Id;
            if (ModelState.IsValid)
            {
                db.Reservations.Add(reservation);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.HotelId = new SelectList(db.Hotels, "Id", "Name", reservation.HotelId);
            ViewBag.HotelUserId = new SelectList(db.HotelUsers, "Id", "FirstName", reservation.HotelUserId);
            ViewBag.RoomId = new SelectList(db.Rooms, "Id", "Name", reservation.RoomId);
            var reservations = db.Rooms.Include(x => x.Hotel).ToList();
            return View(reservations);
        }


        public ActionResult Create(int id)
        {
            Reservation reservation = new Reservation
            {
                HotelId = id,
                HotelName = db.Hotels.AsNoTracking().FirstOrDefault(x => x.Id == id)?.Name,
                CheckInDate = DateTime.Now,
                CheckOutDate = DateTime.Now.AddDays(5),
                HotelUser = AppHelper.CurrentHotelUser(),
                Hotel = db.Hotels.Find(id),
                NoOfRooms = 1,
                AddOnsCost = 0,
                CheckInRoom = new CheckInRoom { RooomNumber = "n/a" },
                breakfastmeals = db.Dinings.Where(a => a.MealTypes.Name.Contains("Breakfast")).ToList(),
                lunchmeals = db.Dinings.Where(a => a.MealTypes.Name.Contains("lunch")).ToList()
            };
            ViewBag.RoomImageId = db.Rooms.FirstOrDefault(a => a.HotelId == (int)id)?.Id;
            ViewBag.HotelId = new SelectList(db.Hotels, "Id", "Name");
            ViewBag.HotelUserId = new SelectList(db.HotelUsers, "Id", "FirstName");
            ViewBag.RoomId = new SelectList(db.Rooms.Where(a => a.HotelId == id), "Id", "Name");
            return View(reservation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Reservation reservation, string [] selectedMeals, int HotelId)
        {
            if (ModelState.IsValid)
            {
                using(ApplicationDbContext core = new ApplicationDbContext())
                {
                    var room = core.Rooms.Find(reservation.RoomId);


                    Reservation save = new Reservation();
                    save.HotelId = HotelId;
                    save.CheckInDate = reservation.CheckInDate;
                    save.CheckOutDate = reservation.CheckOutDate;
                    save.RoomId = room.Id;
                    save.HotelUserId = AppHelper.CurrentHotelUser()?.Id;
                    save.CreatedOn = DateTime.Now;
                    save.AddOnsCost = reservation.AddOnsCost;
                    save.NoOfRooms = reservation.NoOfRooms;
                    save.TotalFees = (room.PricePerRoom * reservation.NoOfRooms);
                    save.TotalCost = (room.PricePerRoom * reservation.NoOfRooms) + (int)reservation.AddOnsCost;
                    core.Reservations.Add(save);
                    core.SaveChanges();

                    var adds = "";
                    foreach (var meal in selectedMeals)
                        adds = (!String.IsNullOrEmpty(adds)) ? $"{adds},{meal}" : meal;
                    if (!string.IsNullOrEmpty(adds))
                    {
                        var add = new AddOnsR { ReservationId = save.Id, AddOns = adds, HotelUserId = (int)save.HotelUserId };
                        core.AddOnsRs.Add(add);
                        core.SaveChanges();

                        save.AddOnsId = add.Id;
                        core.Entry(save).State = EntityState.Modified;
                        core.SaveChanges();
                    }
                    
                }
                return RedirectToAction("Index");
            }

            ViewBag.HotelId = new SelectList(db.Hotels, "Id", "Name", reservation.HotelId);
            ViewBag.HotelUserId = new SelectList(db.HotelUsers, "Id", "FirstName", reservation.HotelUserId);
            ViewBag.RoomId = new SelectList(db.Rooms, "Id", "Name", reservation.RoomId);
            var reservations = db.Rooms.Include(x => x.Hotel).ToList();
            return View(reservations);
        }

        // GET: Reservations/Create
        public ActionResult AddOns(int id)
        {
            Reservation reservation = new Reservation
            {
                HotelId = id,
                HotelName = db.Hotels.AsNoTracking().FirstOrDefault(x=>x.Id == id)?.Name,
                CheckInDate = DateTime.Now,
                CheckOutDate = DateTime.Now.AddDays(5),
                Dinings = db.Dinings.ToList()
            };
            ViewBag.HotelId = new SelectList(db.Hotels, "Id", "Name");
            ViewBag.HotelUserId = new SelectList(db.HotelUsers, "Id", "FirstName");
            ViewBag.RoomId = new SelectList(db.Rooms, "Id", "Name");
            return View(reservation);
        }

        // POST: Reservations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddOns(int Id,string [] DiningSelectList, Reservation reservation)
        {


            var User = AppHelper.CurrentHotelUser()?.Id;
            using (ApplicationDbContext core = new ApplicationDbContext())
            {
                var AddOns = "";
                var addValue = 0.0;
                reservation = core.Reservations.Find(Id);
                foreach (var selected in DiningSelectList)
                {
                    AddOns = (String.IsNullOrEmpty(AddOns)) ? selected : String.Format($"{AddOns},{selected}");
                    var dining = core.Dinings.Find(Convert.ToInt16(selected));
                    addValue += dining.CostPerPerson;
                }
                var addons = new AddOnsR
                {
                    HotelUserId = (int)User,
                    ReservationId = Id,
                    AddOns = AddOns
                };
                if (reservation.AddOnsId != null)
                {
                    addons = core.AddOnsRs.Find(reservation.AddOnsId);
                    addons.AddOns = AddOns;
                    core.Entry(addons).State = EntityState.Modified;
                    core.SaveChanges();
                }
                else{
                    core.AddOnsRs.Add(addons);
                    core.SaveChanges();
                }

                reservation.TotalCost -= (decimal)reservation.AddOnsCost;
                reservation.AddOnsCost = decimal.Parse(addValue.ToString());
                reservation.TotalCost += decimal.Parse(addValue.ToString());
                reservation.AddOnsId = addons.Id;
                core.Entry(reservation).State = EntityState.Modified;
                core.SaveChanges();
            }
            
            return RedirectToAction("Index");
        }

        // GET: Reservations/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reservation reservation = db.Reservations.Find(id);
            if (reservation == null)
            {
                return HttpNotFound();
            }
            ViewBag.HotelId = new SelectList(db.Hotels, "Id", "Name", reservation.HotelId);
            ViewBag.HotelUserId = new SelectList(db.HotelUsers, "Id", "FirstName", reservation.HotelUserId);
            ViewBag.RoomId = new SelectList(db.Rooms, "Id", "Name", reservation.RoomId);
            return View(reservation);
        }

        // POST: Reservations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,CheckInDate,CheckOutDate,HotelId,RoomId,HotelUserId")] Reservation reservation)
        {
            if (ModelState.IsValid)
            {
                db.Entry(reservation).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.HotelId = new SelectList(db.Hotels, "Id", "Name", reservation.HotelId);
            ViewBag.HotelUserId = new SelectList(db.HotelUsers, "Id", "FirstName", reservation.HotelUserId);
            ViewBag.RoomId = new SelectList(db.Rooms, "Id", "Name", reservation.RoomId);
            return View(reservation);
        }

        // GET: Reservations/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reservation reservation = db.Reservations.Find(id);
            if (reservation == null)
            {
                return HttpNotFound();
            }
            return View(reservation);
        }

        // POST: Reservations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Reservation reservation = db.Reservations.Find(id);
            db.Reservations.Remove(reservation);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        //[HttpPost]
        //public ActionResult BookReservationConfimation(int hotelId, DateTime checkInDate, DateTime checkOutDate, int noOfRooms, int roomId)
        //{
        //    ReservationConfirmationVM reservationConfirmationVM = new ReservationConfirmationVM
        //    {
        //        HotelId = hotelId,
        //        CheckInDate = checkInDate,
        //        CheckOutDate = checkOutDate,
        //        NoOfRooms = noOfRooms,
        //        RoomId = roomId
        //    };
        //    return View(reservationConfirmationVM);
        //}

        [HttpPost]
        public ActionResult BookReservation(ReservationConfirmationVM reservationVM /*int hotelId, DateTime checkInDate, DateTime checkOutDate, int noOfRooms, int roomId*/)
        {

            int reservationCount = db.Reservations.Where(x => x.HotelId == reservationVM.HotelId).Count();
            var hotelInfo = db.Hotels.FirstOrDefault(x=>x.Id == reservationVM.HotelId);
            var roomInfo = db.Rooms.FirstOrDefault(x=>x.Id == reservationVM.RoomId);
            var cost = roomInfo.PricePerRoom * reservationVM.NoOfRooms;
            string message = "";

            // Check if room quantity is available
            if (hotelInfo.MaxOccupancy >= reservationCount)
            {
                var user = AppHelper.CurrentHotelUser();
                // Return success status with empty message
                Reservation reservation = new Reservation();
                reservation.HotelId = reservationVM.HotelId;
                reservation.HotelUserId = user?.Id;
                reservation.RoomId = reservationVM.RoomId;
                reservation.NoOfRooms = (int)reservationVM.NoOfRooms;
                reservation.CreatedOn = DateTime.Now;
                reservation.CheckOutDate = reservationVM.CheckOutDate;
                reservation.CheckInDate = reservationVM.CheckInDate;
                reservation.TotalCost = cost;
                reservation.TotalFees = Math.Round((cost * 0.02m), 2);
                db.Reservations.Add(reservation);
                db.SaveChanges();

                //user = new HotelUsers { EmailAddress = "farhaanhotd1@gmail.com" };
                new Email().SendEmail(user.EmailAddress, "Hotel Reservation Success", user.FirstName + " "+user.LastName, "Your booking for " + hotelInfo.Name +", " + reservationVM.NoOfRooms + " at the cost of R"+ cost , false);

                return Json(new { success = true, message = "" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                // Return failure status with error message
                message = "The requested number of rooms is not available for the selected dates.";
                return Json(new { success = false, message = message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetTotalCost(int roomId, int noOfRooms)
        {
            var roomInfo = db.Rooms.Include(x=>x.Hotel).FirstOrDefault(x => x.Id == roomId);
            var cost = roomInfo.PricePerRoom * noOfRooms;
            
            return Json(new { success = true, message = cost }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult SubmitLiveImage(int reservationId, string LiveBase64)
        {
            bool ImageSubmitedResults = false;
            try
            {
                using (ApplicationDbContext context = new ApplicationDbContext())
                {
                    var resrvation = context.Reservations.Find(reservationId);
                    var document = context.Documents.FirstOrDefault(a => a.DocumentTypeKey == "a_customer_liveness_image" && a.ReservationId == reservationId);
                    if (document != null)
                    {
                        var ofile = context.Files.Find(document.FileId);
                        ofile.Content = Convert.FromBase64String(LiveBase64.Split(',').ToList()[1]);
                        ofile.FileSize = ofile.Content.Length;
                        ofile.ModifiedDateTime = DateTime.Now;
                        context.Entry(ofile).State = EntityState.Modified;
                        context.SaveChanges();
                    }
                    else
                    {
                        var customer = context.HotelUsers.Find(resrvation.HotelUserId);
                        document = new Document();
                        File ofile = new File();
                        ofile.ContentType = "image/png";
                        ofile.Content = Convert.FromBase64String(LiveBase64.Split(',').ToList()[1]);
                        ofile.FileName = $"c_{customer.FullName.Replace(' ', '_').ToLower()}_{resrvation.Id}";
                        ofile.FileSize = ofile.Content.Length;
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
                        document.DocumentTypeKey = "a_customer_liveness_image";
                        document.ReservationId = resrvation.Id;
                        context.Documents.Add(document);
                        context.SaveChanges();
                    }
                }
                ImageSubmitedResults = true;
            }
            catch
            {

            }
            return Json(ImageSubmitedResults, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ResReview()
        {
            var user = AppHelper.CurrentHotelUser();
            var currentUser = User.Identity.GetUserId();

            var reservations = db.Reservations
                .Include(r => r.Hotel)
                .Include(r => r.HotelUser)
                .Include(r => r.Room)
                .Where(r => r.HotelUserId == user.Id)
                .ToList();

            var ratedHotelIds = db.ReviewRatings
                .Where(r => r.ReviewerId == currentUser)
                .Select(r => r.HotelId)
                .ToList();

            var userRatings = db.ReviewRatings
                .Where(r => r.ReviewerId == currentUser && ratedHotelIds.Contains(r.HotelId))
                .ToList();

            var viewModel = new ResReviewViewModel
            {
                Reservations = reservations,
                UserRatings = userRatings
            };

            return View(viewModel);
        }

    }
}
