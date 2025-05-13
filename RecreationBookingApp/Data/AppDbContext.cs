using Microsoft.EntityFrameworkCore;
using RecreationBookingApp.Models;

namespace RecreationBookingApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Place> Places { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<RoomFeature> RoomFeatures { get; set; }
    public DbSet<PricingRule> PricingRules { get; set; }
    public DbSet<Schedule> Schedules { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<RecreationBookingApp.Models.Image> Images { get; set; }
    public DbSet<Promocode> Promocodes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasCheckConstraint("CK_User_Role", "role IN ('client', 'owner', 'admin')");

        modelBuilder.Entity<Place>()
            .HasCheckConstraint("CK_Place_Status", "status IN ('active', 'pending', 'archived')");

        modelBuilder.Entity<PricingRule>()
            .HasCheckConstraint("CK_PricingRule_Type", "type IN ('base', 'holiday', 'weekend', 'discount')");

        modelBuilder.Entity<Booking>()
            .HasCheckConstraint("CK_Booking_Status", "status IN ('pending', 'confirmed', 'canceled', 'completed')");

        modelBuilder.Entity<Booking>()
            .HasCheckConstraint("CK_Booking_PaymentStatus", "payment_status IN ('paid', 'unpaid', 'refunded')");

        modelBuilder.Entity<Place>()
            .HasOne(p => p.Category)
            .WithMany()
            .HasForeignKey(p => p.CategoryId);

        modelBuilder.Entity<Place>()
            .Property(p => p.CategoryId)
            .HasColumnName("category_id");

        modelBuilder.Entity<Place>().Ignore(p => p.Images);
    }
}