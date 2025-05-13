using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecreationBookingApp.Models
{
	public class Promocode
	{
        [Key]
        [Column("promo_id")]
        public string PromocodeId { get; set; }

        [Column("code")]
        public string Code { get; set; }

        [Column("discount_percent")]
        public decimal DiscountPercent { get; set; }

        [Column("valid_until")]
        public DateTime? ValidUntil { get; set; }

        [Column("max_uses")]
        public int MaxUses { get; set; }
    }
}

