using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HotelListingSystem.Models
{
    public class Department
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public Nullable< int > HotelId { get; set; }

        [NotMapped]
        public Hotel Hotel { get; set; }
    }

    public class empDepartment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string DepartmentKey { get; set; }
        public string EmplyeeKey { get; set; }
        public int HotelId { get; set; }
        public bool IsActive { get; set; }

        [NotMapped]
        public Department Department { get; set; }

        [NotMapped]
        public Hotel Hotel { get; set; }

        [NotMapped]
        public HotelUsers User { get; set; }
    }


}