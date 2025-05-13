using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecreationBookingApp.Models
{
	public class Schedule
	{
        [Key]
        [Column("schedule_id")]
        public string ScheduleId { get; set; }

        [ForeignKey(nameof(Room))]
        [Column("room_id")]
        public string RoomId { get; set; }
        public Room Room { get; set; }

        [ForeignKey(nameof(PricingRule))]
        [Column("pricing_rule_id")]
        public string? PricingRuleId { get; set; }
        public PricingRule? PricingRule { get; set; }

        [Column("date")]
        public DateTime Date { get; set; }

        [Column("start_time")]
        public TimeSpan StartTime { get; set; }

        [Column("end_time")]
        public TimeSpan EndTime { get; set; }

        [Column("is_available")]
        public bool IsAvailable { get; set; }
    }
}

