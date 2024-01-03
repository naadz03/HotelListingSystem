using HotelListingSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Web.Mvc;
using HotelListingSystem.ViewModel;

namespace HotelListingSystem.Controllers
{
    [Authorize]
    public class RefundsController : Controller
    {
        private ApplicationDbContext _context;
        public RefundsController()
        {
            _context = new ApplicationDbContext();  
        }
        public HotelUsers CurrentUser { get; set; }
        public List<int> hIds { get; set; }
        public void Initialise()
        {
            if (Request.IsAuthenticated)
            {
                CurrentUser = AppHelper.CurrentHotelUser();

                if (User.IsInRole("Business Owner") || User.IsInRole("Receptionist"))
                    hIds = _context.Hotels.
                        Where(a => (a.HotelUserId == CurrentUser.Id || a.ReceptionistId == CurrentUser.Id)).
                        Select(a => a.Id).ToList();

            }
        }

        [Authorize]
        public ActionResult Refunds()
        {
            Initialise();
            RefundViewModel refundViewModel = new RefundViewModel();
            List<Refund> refunds;
            List<Reservation> reservations;
            reservations = _context.Reservations.Include(r => r.Hotel).Include(r => r.HotelUser).Include(r => r.Room).Where(a => a.HotelUserId == CurrentUser.Id && !a.CheckInConfirmed && a.PaymentApproved ).ToList();
            refunds = _context.Refunds.ToList();

            foreach(var rr in refunds)
                refunds.FirstOrDefault(a => a.Id == rr.Id).Reservation = _context.Reservations.FirstOrDefault(c => c.Id == rr.ReservationId);

            foreach(var rr in reservations)
            {
                String termsnconditions = String.Empty;
                DateTime currentDate = DateTime.Now;
                DateTime checkInDate = rr.CheckInDate;
                TimeSpan difference = checkInDate - currentDate;
                int daysInBetween = difference.Days;
                if (daysInBetween >= 2 && daysInBetween < 20)
                    termsnconditions = "You are eligible for 50% refund.";
                if (daysInBetween >= 20)
                    termsnconditions = "You are eligible for 100% refund.";
                reservations.FirstOrDefault(a => a.Id == rr.Id).RefundTermsnConditions = termsnconditions;
            }
            //reservations = new List<Reservation>();
            refundViewModel.Refunds = refunds;
            refundViewModel.Reservations = reservations;
            return View(refundViewModel);
        }

        public ActionResult RefundConfirmation(int Id, string type, string confirmed)
        {
            try
            {
                Initialise();
                Reservation reservation = _context.Reservations.Find(Id);

                decimal returningBalance = reservation.TotalCost * (reservation.CheckInDate - reservation.CheckOutDate).Days;
                String termsofrefund = "100% refund to customer.";
                if ((reservation.CheckInDate - DateTime.Now).Days >= 2 && (reservation.CheckInDate - DateTime.Now).Days <20)
                {
                    returningBalance = returningBalance / 2;
                    termsofrefund = "50% refund to customer.";
                }
                    
                Refund collection = new Refund
                {
                    ReservationId = reservation.Id, 
                    RequestDate = DateTime.Now,
                    DateOfPurchase = DateTime.Now,
                    DaysB4ChkIn = (reservation.CheckInDate - DateTime.Now).Days,
                    IsActive = true,
                    Balance = returningBalance,
                    BookingType =type,
                    Status = confirmed=="_confirmed_"? "Refund Confirmed: Approved": "Refund attempt cancelled",
                    TermsOfRefund = termsofrefund,
                    TotalBalance = returningBalance,    
                };
                _context.Refunds.Add(collection);
                _context.SaveChanges();

                if (confirmed == "_confirmed_")
                {
                    reservation.Cancelled = true;
                    reservation.Updated = true;
                    reservation.CancelledById = CurrentUser.Id;
                    _context.Entry(reservation).State = EntityState.Modified;
                    _context.SaveChanges();
                }
                new Email().SendEmail(CurrentUser.EmailAddress, "Refund Request", CurrentUser.FullName, null, "e_refund_mail", _context.Hotels.Find(reservation.HotelId).Name, null);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch 
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            
        }
        public ActionResult RefundCancellation(int Id, string type)
        {
            try
            {


                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch 
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            
        }
    }
}
