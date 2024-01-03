using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HotelListingSystem.Controllers
{
    public class DriverController : Controller
    {
        // GET: Driver
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult Trips()
        {
            return RedirectToAction("Index");
        }

        public ActionResult AddDriver()
        {
            return View();
        }
   }
}