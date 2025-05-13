using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecreationBookingApp.Models
{
	public class RoomFeature
	{
        [Key]
        [Column("feature_id")]
        public string RoomFeatureId { get; set; }

        [ForeignKey(nameof(Room))]
        [Column("room_id")]
        public string RoomId { get; set; }
        public Room Room { get; set; }

        [Column("feature_name")]
        public string? FeatureName { get; set; }
    }
}

