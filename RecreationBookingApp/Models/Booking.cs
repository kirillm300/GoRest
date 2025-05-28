using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecreationBookingApp.Models;

[Table("bookings")]
public class Booking
{
    [Key]
    [Column("booking_id")]
    public string BookingId { get; set; }

    [ForeignKey(nameof(User))]
    [Column("user_id")]
    public string UserId { get; set; }
    public User User { get; set; }

    [ForeignKey(nameof(Schedule))]
    [Column("schedule_id")]
    public string? ScheduleId { get; set; }
    public Schedule Schedule { get; set; }

    [ForeignKey(nameof(Place))]
    [Column("place_id")]
    public string PlaceId { get; set; }
    public Place Place { get; set; }

    [ForeignKey(nameof(Promocode))]
    [Column("promo_id")]
    public string? PromocodeId { get; set; }
    public Promocode? Promocode { get; set; }

    [Column("status")]
    [RegularExpression("^(pending|confirmed|canceled|completed)$", ErrorMessage = "Status must be one of: pending, confirmed, canceled, completed")]
    public string Status { get; set; }

    [Column("total_price")]
    public decimal TotalPrice { get; set; }

    [Column("people_count")]
    public int PeopleCount { get; set; }

    [Column("payment_status")]
    [RegularExpression("^(paid|unpaid|refunded)$", ErrorMessage = "PaymentStatus must be one of: paid, unpaid, refunded")]
    public string PaymentStatus { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    // Добавляем для отображения
    public string UserName { get; set; }
    public string PlaceName { get; set; }
}