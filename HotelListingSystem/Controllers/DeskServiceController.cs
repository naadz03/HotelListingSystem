using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using HotelListingSystem.Models;
using HotelListingSystem.ViewModel;
using static HotelListingSystem.Controllers.ReservationsController;

namespace HotelListingSystem.Controllers
{
    [Authorize]
    public class DeskServiceController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: DeskService
        public ActionResult Index()
        {
            var user = AppHelper.CurrentHotelUser().Id;
            var reservations = db.Reservations
                .Include(c => c.Hotel)
                .Include(c => c.Room)
                .Include(c => c.HotelUser)
                .Where(a => a.Hotel.ReceptionistId == user).ToList();
            foreach (var r in reservations)
                r.payment = db.Payments.FirstOrDefault(b => (int)b.ReservationId == r.Id);
            return View(reservations);
        }


        public ActionResult CurrentActiveReservations()
        {
            var user = AppHelper.CurrentHotelUser().Id;
            var reservations = db.Reservations
                .Include(c => c.Hotel)
                .Include(c => c.Room)
                .Include(c => c.HotelUser)
                .Where(a => a.Hotel.ReceptionistId == user && a.CheckInConfirmed && !a.CheckOutConfirmed).ToList();
            foreach (var r in reservations)
                r.payment = db.Payments.FirstOrDefault(b => (int)b.ReservationId == r.Id);
            return View(reservations);
        }

        public ActionResult CheckoutReservation(Int32 id)
        {
            try
            {
                Reservation reservation = db.Reservations.Find(id);
                reservation.CheckOutConfirmed = true;
                db.Entry(reservation).State = EntityState.Modified;
                db.SaveChanges();
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult SearchReservations(string IdentityNumber)
        {
            var user = AppHelper.CurrentHotelUser().Id;
            var reservations = db.Reservations
                .Include(c => c.Hotel)
                .Include(c => c.Room)
                .Include(c => c.HotelUser)
                .Where(a => a.Hotel.ReceptionistId == user && a.HotelUser.IdentificationNumber == IdentityNumber).ToList();
            foreach (var r in reservations)
                r.payment = db.Payments.FirstOrDefault(b => (int)b.ReservationId == r.Id);
            return View("Index", reservations);
        }
        public ActionResult SearchReservations2(string IdentityNumber)
        {
            var user = AppHelper.CurrentHotelUser().Id;
            var reservations = db.Reservations
                .Include(c => c.Hotel)
                .Include(c => c.Room)
                .Include(c => c.HotelUser)
                .Where(a => a.Hotel.ReceptionistId == user && a.HotelUser.IdentificationNumber == IdentityNumber).ToList();
            foreach (var r in reservations)
                r.payment = db.Payments.FirstOrDefault(b => (int)b.ReservationId == r.Id);
            return View("CurrentActiveReservations", reservations);
        }

        public ActionResult Update(int Id)
        {
            var results = db.Reservations
                .Include(c => c.Hotel)
                .Include(c => c.Room)
                .Include(c => c.HotelUser)
                .Include(c => c.CheckInRoom)
                .FirstOrDefault(a => a.Id == Id);
            results.breakfastmeals = db.Dinings.Include(d => d.MealTypes).Where(a => a.MealTypes.Name.Contains("Breakfast")).ToList();
            results.lunchmeals = db.Dinings.Where(a => a.MealTypes.Name.Contains("lunch")).ToList();
            if (results.AddOnsId != null)
                results.Addons = db.AddOnsRs.Find(results.AddOnsId);
            ViewBag.ThisHotelRooms = new SelectList(db.Rooms.Where(a => a.HotelId == results.HotelId).ToList(), "Id", "Name");
            return View(results);
        }

        public ActionResult DeskCheckIns(int Id)
        {
            var results = db.Reservations
                .Include(c => c.Hotel)
                .Include(c => c.Room)
                .Include(c => c.HotelUser)
                .Include(c => c.CheckInRoom)
                .FirstOrDefault(a => a.Id == Id);
            results.breakfastmeals = db.Dinings.Include(d => d.MealTypes).Where(a => a.MealTypes.Name.Contains("Breakfast")).ToList();
            results.lunchmeals = db.Dinings.Where(a => a.MealTypes.Name.Contains("lunch")).ToList();
            if (results.AddOnsId != null)
                results.Addons = db.AddOnsRs.Find(results.AddOnsId);
            results.Document = db.Documents.FirstOrDefault(a => ((int)a.ReservationId == results.Id) && (a.DocumentTypeKey == "a_customer_liveness_image"));
            ViewBag.ThisHotelRooms = new SelectList(db.Rooms.Where(a => a.HotelId == results.HotelId).ToList(), "Id", "Name");
            return View(results);
        }



       public ActionResult AllocateRoomsToReservation(int id)
        {
            bool IsRoomAllocatedResults = false;
            try
            {
                using(ApplicationDbContext context = new ApplicationDbContext())
                {
                    var reservation = context.Reservations.Find(id);
                    var hotel = context.Hotels.Find(reservation.HotelId);
                    var customer = context.HotelUsers.Find(reservation.HotelUserId);
                    GenerateHotelRoomNumbers(context, hotel);

                    var rooms = context.CheckInRooms.Where(c => !c.IsTaken).ToList().Take(reservation.NoOfRooms);
                    string roomNumber = "";
                    foreach(var room in rooms)
                    {
                        reservation.CheckInRoomId = room.Id; 
                        room.IsTaken = true;
                        context.Entry(room).State = EntityState.Modified;
                        roomNumber = string.IsNullOrEmpty(roomNumber) ? room.RooomNumber : $"{roomNumber},{room.RooomNumber}";
                    }
                    reservation.RoomNumber = roomNumber;
                    reservation.RoomAllocated = true;
                    context.Entry(reservation).State = EntityState.Modified;
                    context.SaveChanges();
                }
                IsRoomAllocatedResults = true;
            }
            catch
            {
                return Json(IsRoomAllocatedResults, JsonRequestBehavior.AllowGet);
            }
            return Json(IsRoomAllocatedResults, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ConfirmHotelCheckIn(int id)
        {
            bool IsRoomAllocatedResults = false;
            try
            {
                using (ApplicationDbContext context = new ApplicationDbContext())
                {
                    var reservation = context.Reservations.Include(c=>c.Hotel).Include(c=>c.HotelUser).Where(a=>a.Id==id).FirstOrDefault();
                    reservation.CheckInConfirmed = true;
                    reservation.ModifiedOn = DateTime.Now;
                    context.Entry(reservation).State = EntityState.Modified;
                    context.SaveChanges();

                    new Email().SendEmail($"{reservation.HotelUser.EmailAddress}", $"Travix System: reservation Confirmation", $"{reservation.HotelUser.FullName}",
                        $"You have been chcked into {reservation.Hotel.Name} for the reservation on {reservation.CheckInDate}<br/>");
                }
                IsRoomAllocatedResults = true;
            }
            catch
            {
                return Json(IsRoomAllocatedResults, JsonRequestBehavior.AllowGet);
            }
            return Json(IsRoomAllocatedResults, JsonRequestBehavior.AllowGet);
        }

        public static void GenerateHotelRoomNumbers(ApplicationDbContext context, Hotel hotel )
        {
            var roomnumbers = context.CheckInRooms.Where(a => a.HotelId == hotel.Id).ToList();
            if (roomnumbers.Count == 0)
            {
                for (int roomNumber = 1; roomNumber <= hotel.MaxOccupancy; roomNumber++)
                {
                    string room = GetRoomNumber(roomNumber, (int)hotel.MaxOccupancy, 25);
                    context.CheckInRooms.Add(new CheckInRoom { RooomNumber = room, HotelId = hotel.Id, IsTaken = false });
                    context.SaveChanges();
                }
            }

        }
        public static string GetRoomNumber(int roomNumber, int totalRooms, int roomsPerFloor)
        {
            if (roomNumber < 1 || roomNumber > totalRooms)
            {
                throw new ArgumentException("Invalid room number.");
            }

            int floorNumber = (roomNumber - 1) / roomsPerFloor + 1;
            char floorLetter = (char)('A' + floorNumber - 1);
            int roomInFloor = (roomNumber - 1) % roomsPerFloor + 1;

            return $"{floorLetter}{roomInFloor:D2}";
        }

        public ActionResult UpdateReservationByRecept(int id, DateTime CheckIn, DateTime ChexkOut, int roomId, string[] selectedAddons, decimal amount, int rooms)
        {
            try
            {
                using (ApplicationDbContext core = new ApplicationDbContext())
                {
                    var room = core.Rooms.Find(roomId);
                    Reservation save = core.Reservations.Find(id);
                    save.CheckInDate = CheckIn;
                    save.CheckOutDate = ChexkOut;
                    save.RoomId = room.Id;
                    save.ModifiedOn = DateTime.Now;
                    save.AddOnsCost = amount;
                    save.NoOfRooms = rooms;
                    save.TotalFees = (room.PricePerRoom * rooms);
                    save.TotalCost = (room.PricePerRoom * rooms) + amount;
                    core.Entry(save).State = EntityState.Modified;
                    core.SaveChanges();

                    var adds = "";
                    foreach (var meal in selectedAddons)
                        adds = (!String.IsNullOrEmpty(adds)) ? $"{adds},{meal}" : meal;
                    if (!string.IsNullOrEmpty(adds))
                    {
                        if (save.AddOnsId != null)
                        {
                            var currentaads = core.AddOnsRs.Find(save.AddOnsId);
                            currentaads.AddOns = adds;
                            core.Entry(currentaads).State = EntityState.Modified;
                            core.SaveChanges();
                        }
                        else
                        {
                            var add = new AddOnsR { ReservationId = save.Id, AddOns = adds, HotelUserId = (int)save.HotelUserId };
                            core.AddOnsRs.Add(add);
                            core.SaveChanges();

                            save.AddOnsId = add.Id;
                            core.Entry(save).State = EntityState.Modified;
                            core.SaveChanges();
                        }
                    }
                    var user = core.HotelUsers.Find(save.HotelUserId);
                    var hotel = core.Hotels.Find(save.HotelId);
                    new Email().SendEmail($"{user.EmailAddress}", $"Travix System: reservation update", $"{user.FullName}",
                                $"Your reservation with {hotel.Name} hotel on {save.CheckInDate} has been updated by {AppHelper.CurrentHotelUser().FullName}. Please contact the hotels reception for more info.<br/>");

                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult CacelBookedReservation(int id)
        {
            var find = db.Reservations.Include(d => d.Hotel).Include(d => d.HotelUser).Where(a => a.Id == id).FirstOrDefault();
            if (find == null)
                return Json(false, JsonRequestBehavior.AllowGet);
            find.Cancelled = true;
            find.CancelledById = AppHelper.CurrentHotelUser().Id;
            db.Entry(find).State = EntityState.Modified;
            db.SaveChanges();

            new Email().SendEmail($"{find.HotelUser.EmailAddress}", $"Travix System: reservation cancellation", $"{find.HotelUser.FullName}",
                        $"Your reservation with {find.Hotel.Name} hotel on {find.CheckInDate} has been cancelled. Please contact the receptionist for more information.<br/>");

            return Json(true, JsonRequestBehavior.AllowGet);
        }





    }

    public class ResevationViewModel
    {
        public Reservation Reservation { get; set; }
        public List<AddOnsR>  AddOnsR { get; set; }
        public DiningReservation DiningReservation { get; set; }

    }
}