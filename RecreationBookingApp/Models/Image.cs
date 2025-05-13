using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecreationBookingApp.Models
{
    public class Image
    {
        [Key]
        [Column("image_id")]
        public string ImageUrlId { get; set; }

        [ForeignKey(nameof(Place))]
        [Column("place_id")]
        public string? PlaceId { get; set; }
        public Place? Place { get; set; }

        [ForeignKey(nameof(Room))]
        [Column("room_id")]
        public string? RoomId { get; set; }
        public Room? Room { get; set; }

        [Column("url")]
        public string Url { get; set; }

        [Column("is_main")]
        public bool IsMain { get; set; }
    }
}

