using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecreationBookingApp.Models;

[Table("users")]
public class User
{
    [Key]
    [Column("user_id")]
    public string UserId { get; set; }

    [Required]
    [Column("email")]
    public string Email { get; set; }

    [Required]
    [Column("password_hash")]
    public string PasswordHash { get; set; }

    [Column("full_name")]
    public string? FullName { get; set; }

    [Column("phone")]
    public string? Phone { get; set; }

    [Required]
    [Column("role")]
    public string Role { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("verified")]
    public bool Verified { get; set; }
}