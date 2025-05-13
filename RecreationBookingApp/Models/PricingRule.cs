using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecreationBookingApp.Models
{
	public class PricingRule
	{
        [Key]
        [Column("rule_id")]
        public string PricingRuleId { get; set; }

        [ForeignKey(nameof(Room))]
        [Column("room_id")]
        public string RoomId { get; set; }
        public Room Room { get; set; }

        [Column("type")]
        public string Type { get; set; }

        [Column("price")]
        public decimal Price { get; set; }

        [Column("start_date")]
        public DateTime? StartDate { get; set; }

        [Column("end_date")]
        public DateTime? EndDate { get; set; }
    }
}

