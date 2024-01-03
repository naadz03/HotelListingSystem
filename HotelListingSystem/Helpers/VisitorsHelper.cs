using HotelListingSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelListingSystem.Helpers
{
    public class VisitorsHelper
    {
        public static void AddVisit(HttpSessionStateBase state, HttpRequestBase req)
        {
            using(ApplicationDbContext core = new ApplicationDbContext())
            {
                if (core.SystemVisitors.FirstOrDefault(c => c.SessionId == state.SessionID) == null)
                {
                    core.SystemVisitors.Add(new SystemVisitor
                    {
                        SessionId = state.SessionID,
                        DateTime = DateTime.Now,
                        LCID = state.LCID.ToString(),
                        Mode = state.Mode.ToString(),
                        Authenticated = req.IsAuthenticated
                    });
                    core.SaveChanges();
                }
            }
        }
    }
}