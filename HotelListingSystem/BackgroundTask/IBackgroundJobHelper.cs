using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelListingSystem.BackgroundTask
{
    public interface IBackgroundJobHelper
    {
        void NotificationsOrReminders();
        void SendEmailNotification(String Email, String Name, String Subject, String Body, String Type);
    }
}