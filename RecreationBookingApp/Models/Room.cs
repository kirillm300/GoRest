using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecreationBookingApp.Models;

[Table("rooms")]
public class Room
{
    [Key]
    [Column("room_id")]
    public string RoomId { get; set; }

    [ForeignKey(nameof(Place))]
    [Column("place_id")]
    public string PlaceId { get; set; }
    public Place Place { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("capacity")]
    public int Capacity { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("base_price")]
    public decimal BasePrice { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    public List<RoomFeature> Features { get; set; } = new List<RoomFeature>(); // Коллекция особенностей
}