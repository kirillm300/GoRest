using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecreationBookingApp.Models
{
	public class Notification
	{
        [Key]
        [Column("notification_id")]
        public string NotificationId { get; set; }

        [ForeignKey(nameof(User))]
        [Column("user_id")]
        public string UserId { get; set; }
        public User User { get; set; }

        [Column("type")]
        public string Type { get; set; }

        [Column("message")]
        public string Message { get; set; }

        [Column("is_read")]
        public bool IsRead { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}

