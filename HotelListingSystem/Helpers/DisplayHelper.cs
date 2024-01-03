using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using HotelListingSystem.Models;
using Tavis.UriTemplates;

namespace HotelListingSystem.Helpers
{
    public static class DisplayHelper
    {
        public static Product GetProductInfo(Guid id)
        {
            Product product = null;
            using (ApplicationDbContext _context = new ApplicationDbContext())
            {
                product = _context.Products.Find(id);
            }
            return product;
        } 

        public static bool IsRefundLegible(int Id, bool result = false)
        {
            Reservation rr;
            using (ApplicationDbContext _context = new ApplicationDbContext())
            {
                rr = _context.Reservations.Find(Id);
                String termsnconditions = String.Empty;
                DateTime currentDate = DateTime.Now;
                DateTime checkInDate = rr.CheckInDate;
                TimeSpan difference = checkInDate - currentDate;
                int daysInBetween = difference.Days;
                if (daysInBetween >= 2 || daysInBetween >= 20)
                    result = true;
            }
            return result;
        }

        public static Document GetHotelImage(Int32 Id, String Key)
        {
            using (ApplicationDbContext _context = new ApplicationDbContext())
            {
                return _context.Documents.Include(c => c.File).OrderByDescending(a => a.Id).Where(a => a.HotelId == Id && a.DocumentTypeKey == Key).FirstOrDefault();
            }
        }
    }
}