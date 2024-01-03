using HotelListingSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using Microsoft.Ajax.Utilities;

namespace HotelListingSystem.Helpers
{
    public class RecentlyViewedHotelHelper
    {
        private readonly ApplicationDbContext context;
        public RecentlyViewedHotelHelper(ApplicationDbContext dbContext)
        {
            context = dbContext;    
        }

       public void RecordView(Hotel hotel, Int32 ViewerId)
        {
            context.RecentlyViewedHotels.Add(new RecentlyViewedHotel
            {
                HotelId = hotel.Id,
                ViewerId = ViewerId,
                Taken = DateTime.Now
            });
            context.SaveChanges();
        }

        public IEnumerable<RecentlyViewedHotel> FindRecentlyViewdHotels(Int32 ViewerId)
        {
            return context.RecentlyViewedHotels.Include(d => d.Hotel).OrderByDescending(c => c.Taken).Where(a => a.ViewerId == ViewerId).DistinctBy(d => d.HotelId).Take(5).ToList();
        }

        
    }
}