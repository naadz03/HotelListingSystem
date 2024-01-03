using HotelListingSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Web.Mvc;

namespace HotelListingSystem.Controllers
{
    public class RidesController : Controller
    {
        private ApplicationDbContext _context = new ApplicationDbContext();
        public HotelUsers CurrentUser { get; set; }
        public List<int> hIds { get; set; }

        public void Initialise()
        {
            if (Request.IsAuthenticated)
            {
                CurrentUser = AppHelper.CurrentHotelUser();

                if(User.IsInRole("Business Owner") || User.IsInRole("Receptionist"))
                    hIds = _context.Hotels.
                        Where(a => (a.HotelUserId == CurrentUser.Id || a.ReceptionistId == CurrentUser.Id)).
                        Select(a => a.Id).ToList();
            
            }
        }
        [Authorize]
        public ActionResult Index()
        {
            Initialise();
            var hotels = _context.Hotels.Where(a => (a.HotelUserId == CurrentUser.Id || a.ReceptionistId == CurrentUser.Id)).Select(a => a.Id);
            var rrq = _context.Reservations.Where(a => hotels.Contains((int)a.HotelId)).Select(a=>a.Id).ToList();
            return View(_context.RideRequests.Where(c => rrq.Contains(c.ReservationId)).ToList());
        }

        public ActionResult Vehicles()
        {
            Initialise();
            var rrq = _context.Vehicles.Where(a => hIds.Contains(a.ForHotelId)).ToList();
            foreach(var v in rrq)
            {
                rrq.FirstOrDefault(a => a.Id == v.Id).AddedByUser = _context.HotelUsers.FirstOrDefault(a => a.Id == v.AddedByUserId);
                rrq.FirstOrDefault(a => a.Id == v.Id).ForHotel = _context.Hotels.FirstOrDefault(a => a.Id == v.ForHotelId);
            }
            return View(rrq);
        }

        [Authorize]
        public ActionResult AddVehicle()
        {
            Initialise();
            ViewBag.Hotels = new SelectList(_context.Hotels.Where(a => a.HotelUserId == CurrentUser.Id || a.ReceptionistId == CurrentUser.Id), "Id", "Name");
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult AddVehicle(Vehicle collectoin)
        {
            Initialise();
            collectoin.AddedByUserId = CurrentUser.Id;  
            _context.Vehicles.Add(collectoin);  
            _context.SaveChanges();
            return RedirectToAction("","Rides");
        }

        [Authorize]
        public ActionResult EditVehicle(Guid Id)
        {
            Initialise();
            ViewBag.Hotels = new SelectList(_context.Hotels.Where(a => a.HotelUserId == CurrentUser.Id || a.ReceptionistId == CurrentUser.Id), "Id", "Name");
            return View(_context.Vehicles.Find(Id)); 
        }

        [Authorize]
        [HttpPost]
        public ActionResult EditVehicle(Vehicle collectoin)
        {
            var findItem = _context.Vehicles.Find(collectoin.Id);
            findItem.RegNumber = collectoin.RegNumber;
            findItem.ForHotelId = collectoin.ForHotelId;
            findItem.Color = collectoin.Color;
            collectoin = findItem;
            _context.Entry(collectoin).State = System.Data.Entity.EntityState.Modified;
            _context.SaveChanges();
            return RedirectToAction("Vehicles", "Rides");
        }

        #region helpers
        public ActionResult ActivateVehicle(Guid Id) 
        {
            Initialise();
            var findItem = _context.Vehicles.Find(Id);
            findItem.IsDeleted = false;
            findItem.IsActive = true;
            _context.Entry(findItem).State = System.Data.Entity.EntityState.Modified;
            _context.SaveChanges();
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeactivateVehicle(Guid Id) 
        {
            Initialise();
            var findItem = _context.Vehicles.Find(Id);
            findItem.IsDeleted = true;
            findItem.IsActive = false;
            _context.Entry(findItem).State = System.Data.Entity.EntityState.Modified;
            _context.SaveChanges();
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        #endregion

    }
}
