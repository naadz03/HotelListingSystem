using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using HotelListingSystem.Models.CuponsOrDiscount;
using HotelListingSystem.Models.Driver;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using static HotelListingSystem.Controllers.ReservationsController;

namespace HotelListingSystem.Models
{
    public class ApplicationDbContext : IdentityDbContext<SystemUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
        public DbSet<Dining> Dinings { get; set; }
        public DbSet<DiningTableTypes> DiningTableTypes { get; set; }
        public DbSet<MealTypes> MealTypes { get; set; }
        public DbSet<DiningReservation> DiningReservations { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<HotelUsers> HotelUsers { get; set; }
        public DbSet<Business> Businesses { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<AddOnsR> AddOnsRs { get; set; }
        public DbSet<CheckInRoom> CheckInRooms { get; set; }
        public DbSet<CustomerQuery> CustomerQueries { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<HotelEvent>  HotelEvents{ get; set; }
        public DbSet<EventReservation> EventReservations { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<empDepartment> empDepartments { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<RideRequest> RideRequests { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<WaitingList> WaitingLists { get; set; }
        public DbSet<ProductRequest> ProductRequests { get; set; }
        public DbSet<CheckoutHistory> CheckoutHistories { get; set; }
        public DbSet<Refund> Refunds { get; set; }  
        public DbSet<Trip> Trips{ get; set; }  
        public DbSet<Driverprofile> Driverprofile { get; set; }  
        public DbSet<RecentlyViewedHotel> RecentlyViewedHotels { get; set; }  
        public DbSet<Cupon> Cupons { get; set; } 
        public DbSet<CuponUsage> CuponUsages { get; set; }
        public DbSet<ReviewRating> ReviewRatings { get; set; }
        public DbSet<UserPoints> UserPoints { get; set; }
        public DbSet<Tour360Request> Tour360Requests { get; set; }
        public DbSet<SystemVisitor> SystemVisitors { get; set; }
    }
}