using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecreationBookingApp.Models
{
	public class Category
	{
        [Key]
        [Column("category_id")]
        public string CategoryId { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("icon_url")]
        public string IconUrl { get; set; }
    }
}

