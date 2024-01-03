using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HotelListingSystem.Models
{
    public class SystemVisitor
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public String SessionId { get; set; }
        public Boolean Authenticated { get; set; }
        public String LCID { get; set; }
        public String Mode { get; set; }
        public DateTime DateTime { get; set; }
    }
}