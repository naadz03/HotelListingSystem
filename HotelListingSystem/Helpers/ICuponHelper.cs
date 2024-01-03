using HotelListingSystem.Models.CuponsOrDiscount;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelListingSystem.Helpers
{
    public interface ICuponHelper
    {
        void CreateCupon(Cupon cupon);
        Cupon GetCuponDetils(Int32 Id);
        void ModifyCupon(Cupon cupon);
        void DeleteCupon(Int32 Id);
        void ReviveCupon(Int32 Id);
        IEnumerable<Cupon> GetCupons();
        void AddCuponUsage(Int32 cuponId, Int32 reservationId);
        Boolean ValidateCuponUsage(Int32 cuponId, Int32 reservationId);
        Cupon GetCuponByCode(String cupon);
    }
}