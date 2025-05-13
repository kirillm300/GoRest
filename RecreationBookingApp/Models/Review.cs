using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecreationBookingApp.Models
{
	public class Review
	{
        [Key]
        [Column("review_id")]
        public string ReviewId { get; set; }

        [ForeignKey(nameof(User))]
        [Column("user_id")]
        public string UserId { get; set; }
        public User User { get; set; }

        [ForeignKey(nameof(Place))]
        [Column("place_id")]
        public string PlaceId { get; set; }
        public Place Place { get; set; }

        [Column("rating")]
        public int Rating { get; set; }

        [Column("comment")]
        public string? Comment { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}

