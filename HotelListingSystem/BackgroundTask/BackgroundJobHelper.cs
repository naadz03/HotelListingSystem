using HotelListingSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.Entity;
using System.Collections.ObjectModel;
using HotelListingSystem.ViewModel;

namespace HotelListingSystem.BackgroundTask
{
    public class BackgroundJobHelper : IBackgroundJobHelper
    {
        public void NotificationsOrReminders()
        {
            currentDate = DateTime.Now.AddDays(3);
            _reservs = context.Reservations.Include(a => a.HotelUser).Include(c => c.Hotel).Where(c => c.CheckInDate <= currentDate).ToList();
            foreach (Reservation reserv in _reservs)
            {
                //if (!reserv.ChkInReminder)
                //{
                //    _email.SendEmail(reserv.HotelUser.EmailAddress, "Travelix: Check-In Reminder", null, null, "e_checkin_reminder", reserv.Hotel.Name);
                //    reserv.ChkInReminder = true;
                //    context.Entry(reserv).State = EntityState.Modified;
                //    context.SaveChanges();
                //}
            }
                

            currentDate = DateTime.Now.AddDays(1);
            _reservs = context.Reservations.Include(a => a.HotelUser).Include(c => c.Hotel).Where(c => c.CheckOutDate <= currentDate).ToList();
            foreach (Reservation reserv in _reservs)
            {
                //if (!reserv.ChkOutReminder)
                //{
                //    _email.SendEmail(reserv.HotelUser.EmailAddress, "Travelix: Check-Out Reminder", null, null, "e_checkout_reminder", reserv.Hotel.Name);
                //    reserv.ChkOutReminder = true;
                //    context.Entry(reserv).State = EntityState.Modified;
                //    context.SaveChanges();
                //}
            }
              
        }

        public void SendEmailNotification(String Email, String Name, String Subject, String Body, String Type)
        {
            //_email.SendEmail(Email, String.Format("Travelix: {0}", Subject), Name, Body, "e_user_config");
        }

        private readonly Email _email;
        private readonly ApplicationDbContext context;
        private IEnumerable<Reservation> _reservs;
        private DateTime currentDate;
        public BackgroundJobHelper(ApplicationDbContext _context)
        {
            context = _context;
            _email = new Email();
        }
    }
}