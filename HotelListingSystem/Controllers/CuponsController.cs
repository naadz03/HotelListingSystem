using HotelListingSystem.Helpers;
using HotelListingSystem.Models;
using HotelListingSystem.Models.CuponsOrDiscount;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Web.Mvc;
using HotelListingSystem.Engines.PointSystem;
using HotelListingSystem.ViewModel;
using PayPal.Api;
using static Azure.Core.HttpHeader;
using System.Threading.Tasks;


namespace HotelListingSystem.Controllers
{
    public class CuponsController : Controller
    {
        private ApplicationDbContext context;
        private readonly ICuponHelper cuponHelper;
        public CuponsController()
        {
            context = new ApplicationDbContext();
            cuponHelper = new CuponHelper(context);
        }

        public ActionResult Index()
        {
            return View(cuponHelper.GetCupons());
        }

        public ActionResult Details(Int32 id)
        {
            return View(cuponHelper.GetCuponDetils(id));
        }

        public ActionResult Create()
        {
            var user = AppHelper.CurrentHotelUser()?.Id;
            ViewBag.Hotels = new SelectList(context.Hotels.Where(x => x.HotelUserId == user), "Id", "Name");
            return View();
        }

        [HttpPost]
        public ActionResult Create(Cupon cupon)
        {
            try
            {
                cuponHelper.CreateCupon(cupon);
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Edit(Int32 id)
        {
            return View(cuponHelper.GetCuponDetils(id));
        }

        [HttpPost]
        public ActionResult Edit(Int32 id, Cupon cupon)
        {
            try
            {
                cuponHelper.ModifyCupon(cupon);
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Delete(int id)
        {
            return View(cuponHelper.GetCuponDetils(id));
        }

        [HttpPost]
        public ActionResult Delete(int id, Cupon cupon)
        {
            try
            {
                cuponHelper.DeleteCupon(id);
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult ValidateCupon(String Code, Int32 id)
        {
            Cupon cupon = cuponHelper.GetCuponByCode(Code);
            if (cupon == null) return Json(new { status = false, message = "invalid code" }, JsonRequestBehavior.AllowGet);
            else if (!cuponHelper.ValidateCuponUsage(cupon.Id, id)) return Json(new { status = false, message = "the cupon has used for this reservation" }, JsonRequestBehavior.AllowGet);
            else cuponHelper.AddCuponUsage(cupon.Id, id);
            return Json(new { status = true, message = "activated", cupon = cupon }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ValidatePoints(Int32 points)
        {
            var user = AppHelper.CurrentHotelUser().Id;
            new PointHelper(context).AddOrDeductPoints(user, (-points));
            return Json(true,JsonRequestBehavior.AllowGet);
        }

        public async Task< ActionResult> SendCuponSmsEmail(Int32 Id)
        {
            var context = new ApplicationDbContext();
            var cupon = cuponHelper.GetCuponDetils(Id);
            var users = new IdentityManager().GetUsersInRole("Customer").Select(a => a.HotelUserId).ToList();
            var aUsers = context.HotelUsers.Where(a => users.Contains((Int32)a.Id)).ToList();
            var str = cupon.IsPercentage ? "percent" : "ZAR";
            foreach (var t_user in aUsers)
            {
                new Email()
                    .SendEmail(
                    t_user.EmailAddress,
                    "Travelix: Promotion",
                    $"{t_user.FullName}",
                    $"You have been gifted with {cupon.Amount} {str} and here is your promotional code {cupon.Code}. ");

                string subject = "Get a Discount with Travelix. Use our Coupon to get discount on your purchase -  " + cupon.Code;
                SMSHelper _helper = new SMSHelper(context);
                await _helper.SMSSend(subject, t_user.MobileNumber);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

    }
}
