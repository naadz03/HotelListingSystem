namespace HotelListingSystem.Migrations
{
    using HotelListingSystem.Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<HotelListingSystem.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            ContextKey = "HotelListingSystem.Models.ApplicationDbContext";
        }

        protected override void Seed(HotelListingSystem.Models.ApplicationDbContext context)
        {
            var idManager = new IdentityManager(context);

            if (!idManager.RoleExists("Administrator"))
                idManager.CreateRole("Administrator");

            if (!idManager.RoleExists("Receptionist"))
                idManager.CreateRole("Receptionist");

            if (!idManager.RoleExists("Business Owner"))
                idManager.CreateRole("Business Owner");

            if (!idManager.RoleExists("Customer"))
                idManager.CreateRole("Customer");

            var admin = new SystemUser()
            {
                UserName = "Admin@hotelgroup.com",
                Email = "Admin@hotelgroup.com",
                EmailConfirmed = true,
                HotelUser = new HotelUsers()
                {
                    FirstName = "Admin",
                    LastName = "Admin",
                    UserName = "Admin@hotelgroup.com",
                    EmailAddress = "Admin@hotelgroup.com"
                }
            };

            if (!idManager.UserExists(admin.UserName))
            {
                idManager.CreateUser(admin, "password");
                idManager.AddUserToRole(admin.Id, "Administrator");
            }

            var clerk = new SystemUser()
            {
                UserName = "Receptionist@hotelgroup.com",
                Email = "Receptionist@hotelgroup.com",
                EmailConfirmed = true,
                HotelUser = new HotelUsers()
                {
                    FirstName = "Receptionist",
                    LastName = "Receptionist",
                    UserName = "Receptionist@hotelgroup.com",
                    EmailAddress = "Receptionist@hotelgroup.com"
                }
            };

            if (!idManager.UserExists(clerk.UserName))
            {
                idManager.CreateUser(clerk, "password");
                idManager.AddUserToRole(clerk.Id, "Receptionist");
            }

            var bs = new SystemUser()
            {
                UserName = "BusinessOwner@hotelgroup.com",
                Email = "BusinessOwner@hotelgroup.com",
                EmailConfirmed = true,
                HotelUser = new HotelUsers()
                {
                    FirstName = "Business",
                    LastName = "Owner",
                    UserName = "BusinessOwner@hotelgroup.com",
                    EmailAddress = "BusinessOwner@hotelgroup.com"
                }
            };

            if (!idManager.UserExists(bs.UserName))
            {
                idManager.CreateUser(bs, "password");
                idManager.AddUserToRole(bs.Id, "Business Owner");
            }




            var user01 = new SystemUser()
            {
                UserName = "JohnDoe@hotelgroup.com",
                Email = "JohnDoe@hotelgroup.com",
                EmailConfirmed = true,
                HotelUser = new HotelUsers()
                {
                    FirstName = "John",
                    LastName = "Doe",
                    UserName = "JohnDoe@hotelgroup.com",
                    EmailAddress = "JohnDoe@hotelgroup.com"
                }
            };

            if (!idManager.UserExists(user01.UserName))
            {
                idManager.CreateUser(user01, "password");
                idManager.AddUserToRole(user01.Id, "Customer");
            }
        }
    }
}
