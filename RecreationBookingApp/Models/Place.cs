using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecreationBookingApp.Models;

[Table("places")]
public class Place
{
    [Key]
    [Column("place_id")]
    public string PlaceId { get; set; }

    [Column("owner_id")]
    public string? OwnerId { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("description")]
    public string Description { get; set; }

    [Column("address")]
    public string Address { get; set; }

    [Column("latitude")]
    public decimal Latitude { get; set; }

    [Column("longitude")]
    public decimal Longitude { get; set; }

    [Column("contact_phone")]
    public string ContactPhone { get; set; }

    [Column("category_id")]
    public string? CategoryId { get; set; }

    [Column("status")]
    public string Status { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("owner_id")]
    public User? Owner { get; set; }

    [ForeignKey("category_id")]
    public Category? Category { get; set; }

    public List<Room> Rooms { get; set; } = new List<Room>(); // Коллекция комнат
    public List<string> Images { get; set; } = new List<string>(); // Коллекция URL изображений
}