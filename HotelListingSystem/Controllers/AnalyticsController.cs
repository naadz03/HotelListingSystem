using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HotelListingSystem.Controllers
{
    public class AnalyticsController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetVisitors()
        {
            
            return Json(true, JsonRequestBehavior.AllowGet);
        }
    }
}