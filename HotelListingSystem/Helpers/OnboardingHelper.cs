using HotelListingSystem.Models;
using HotelListingSystem.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelListingSystem.Helpers
{
    public class OnboardingHelper
    {
        public static string GenerateUsername(string firstName, string lastName)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            string username = $"{firstName.ToLower()}.{lastName.ToLower()}";

            // Add a random number at the end to ensure uniqueness
            Random random = new Random();
            int randomNumber = random.Next(1, 99);
            username += randomNumber;
            _ = (db.HotelUsers.FirstOrDefault(a => a.UserName.Equals(username)) != null) ? GenerateUsername(firstName, lastName) : String.Empty;
            return username;
        }

        private static readonly string LowercaseChars = "abcdefghijklmnopqrstuvwxyz";
        private static readonly string UppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static readonly string NumericChars = "0123456789";
        private static readonly string SpecialChars = "!@#$%^&*()-_=+[]{}|;:,.<>?";

        public static string GenerateStrongPassword(int length = 12)
        {
            string allChars = LowercaseChars + UppercaseChars + NumericChars + SpecialChars;

            Random random = new Random();
            string password = new string(Enumerable.Repeat(allChars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            return password;
        }

        public static void AddUserToEmpDepartment(ApplicationDbContext context, OnboardingViewModel collectoion, SystemUser user)
        {
            try
            {
                empDepartment emp = new empDepartment();
                emp.Id = Guid.NewGuid();
                emp.HotelId = collectoion.HotelId;
                emp.DepartmentKey = collectoion.DepartmentId;
                emp.IsActive = false;
                emp.EmplyeeKey = user.Id.ToString();
                context.empDepartments.Add(emp);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                new Email().SendException(ex);
            }
        }
    }
}