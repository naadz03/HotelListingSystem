using HotelListingSystem.Models;
using HotelListingSystem.ViewModel;
using System;
using System.Linq;
using System.Data.Entity;


namespace HotelListingSystem.Engines.PointSystem
{
    public class PointHelper
    {
        private readonly ApplicationDbContext _context;
        public PointHelper(ApplicationDbContext context)
        {
            _context = context;
        }

        Boolean UserPointRegistered(Int32 UserId)
        {
            var user = _context.UserPoints.FirstOrDefault(c => c.SystemUserId == UserId);
            return user  != null;
        }

        public void AddOrDeductPoints(Int32 UserId, Int32 currentpoints)
        {
            UserPoints userpoints = _context.UserPoints.FirstOrDefault(b => b.SystemUserId == UserId);
            userpoints.AvailablePoints += currentpoints;
            if (currentpoints > 0)
            {
                userpoints.PointsAccumulated += currentpoints;
                var user = _context.HotelUsers.Find(UserId);
                new Email()
                    .SendEmail(user.EmailAddress, "Hotel Point System", String.Format("{0} {1}", user.FirstName, user.LastName),
                    $"Travelix point system, you have earned {currentpoints} points," +
                    $" more points when you spent.\nYou currently have {userpoints.AvailablePoints} points.", false);
            }
            else
            {
                var user = _context.HotelUsers.Find(UserId);
                new Email()
                   .SendEmail(user.EmailAddress, "Hotel Point System", String.Format("{0} {1}", user.FirstName, user.LastName),
                   $"Travelix point system, you have used {currentpoints*-1} points," +
                   $" more points when you spent.\nYou currently have {userpoints.AvailablePoints} points.", false);
            }
            _context.Entry(userpoints).State = EntityState.Modified;
            _context.SaveChanges();
        }
        void PointSystemRegistration(Int32 UserId, Int32 Points)
        {
            _context.UserPoints.Add(new UserPoints
            {
                SystemUserId = UserId,
                AvailablePoints = Points,
                PointsAccumulated = Points,
                IsActive = false
            });
            _context.SaveChanges();

            var user = _context.HotelUsers.Find(UserId);
            new Email()
                .SendEmail(user.EmailAddress, "Hotel Point System", String.Format("{0} {1}", user.FirstName, user.LastName),
                "Your profile has been selected for the point system in Travelix, get discounts when you make your bookings," +
                $" more points when you spent, click here to activate your point system.\nYou currently have {Points} points." +
                " Click here for points usage and terms and conditions.", false);
        }
        public void AddUserPoints(Int32 UserId, Decimal PurchaseAmount)
        {
            Int32 accumulated = ((Int32)PurchaseAmount / 100) * 10;

            if (!UserPointRegistered(UserId))
                PointSystemRegistration(UserId, accumulated);
            AddOrDeductPoints(UserId, accumulated);
        }
    }
}