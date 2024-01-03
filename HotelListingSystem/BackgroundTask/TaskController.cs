using HotelListingSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace HotelListingSystem.BackgroundTask
{
    public class TaskController : Controller
    {
        private readonly IBackgroundJobHelper backgroundJobHelper;
        public TaskController()
        {
            backgroundJobHelper = new BackgroundJobHelper(ApplicationDbContext.Create());
        }
        public async Task<ActionResult> Execute()
        {
            backgroundJobHelper.NotificationsOrReminders();
            throw new NotImplementedException();
        }
    }
}       