using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HotelListingSystem.Models
{
    public class Inventory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public int Quantity { get; set; }

        [Display(Name = "For Hotel")]
        public int ForHotelId { get; set; }
        [NotMapped]
        public Hotel ForHotel { get; set; }

        [Display(Name = "Added by User")]
        public int AddedByUserId { get; set; }
        [NotMapped]
        public HotelUsers AddedByUser { get; set; }

        [Display(Name = "Department")]
        public Guid DepartmentKey { get; set; }
        [NotMapped]
        public Department Department { get; set; }

        [Display(Name = "Product")]
        public Guid ProductKey { get; set; }
        [NotMapped]
        public Product Product { get; set; }

        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
    }

    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Tags { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class WaitingList
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public Guid ProductId { get; set; }
        public int WaiterId { get; set; }

        [NotMapped]
        public HotelUsers Waiter { get; set; }

        [Display(Name = "For Hotel")]
        public int ForHotelId { get; set; }
        [NotMapped]
        public Hotel ForHotel { get; set; }
    }

    public class ProductRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public bool IsAdded { get; set; }
        public bool IsDeleted { get; set; }
        public int WaiterId { get; set; }

        [NotMapped]
        public HotelUsers Waiter { get; set; }
    }


    public class CheckoutHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        [NotMapped]
        public Product Product { get; set; }
        public int Quantity { get; set; }

        [Display(Name = "For Hotel")]
        public int ForHotelId { get; set; }

        [NotMapped]
        public Hotel ForHotel { get; set; }
        
        public int ByUserId { get; set; }

        [NotMapped]
        public HotelUsers ByUser { get; set; }

        public DateTime ChkDateTime { get; set; }
        public Nullable< DateTime> RtnDateTime { get; set; }

        public int LogQuantity { get; set; }
    }
}