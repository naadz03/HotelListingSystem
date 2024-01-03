using HotelListingSystem.Models;
using HotelListingSystem.Models.CuponsOrDiscount;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelListingSystem.Helpers
{
    public class CuponHelper : ICuponHelper
    {
        private ApplicationDbContext context;
        public CuponHelper(ApplicationDbContext dbContext)
        {
            context = dbContext;
        }

        public void CreateCupon(Cupon cupon)
        {
            cupon.IsActive = true;
            cupon.IsDeleted = false;
            context.Cupons.Add(cupon);
            context.SaveChanges();
        }

        public Cupon GetCuponDetils(Int32 Id)
        {
            return context.Cupons.Find(Id);
        }

        public void ModifyCupon(Cupon cupon)
        {
            context.Entry(cupon).State = System.Data.Entity.EntityState.Modified;
            context.SaveChanges();  
        }

        public void DeleteCupon(Int32 Id)
        {
            Cupon cupon = context.Cupons.Find(Id);
            cupon.IsDeleted = true;
            cupon.IsActive = false;
            context.Entry(cupon).State = System.Data.Entity.EntityState.Modified;
            context.SaveChanges();
        }

        public void ReviveCupon(Int32 Id)
        {
            Cupon cupon = context.Cupons.Find(Id);
            cupon.IsDeleted = false;
            cupon.IsActive = true;
            context.Entry(cupon).State = System.Data.Entity.EntityState.Modified;
            context.SaveChanges();
        }

        public IEnumerable<Cupon> GetCupons()
        {
            return context.Cupons;
        }

        public void AddCuponUsage(Int32 cuponId, Int32 reservationId)
        {
            context.CuponUsages.Add(new CuponUsage { CuponId = cuponId, ReservationId = reservationId });
            context.SaveChanges();
        }

        public Boolean ValidateCuponUsage(Int32 cuponId, Int32 reservationId)
        {
            return context.CuponUsages.FirstOrDefault(a => a.CuponId == cuponId && a.ReservationId == reservationId) == null;
        }

        public Cupon GetCuponByCode(String cupon)
        {
            return context.Cupons.FirstOrDefault(a => a.Code.Equals(cupon, StringComparison.OrdinalIgnoreCase));
        }
    }
}