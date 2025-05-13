using Microsoft.Data.Sqlite;
using RecreationBookingApp.Data;
using RecreationBookingApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RecreationBookingApp.Repositories;

public class BookingRepository : IRepository<Booking>
{
    private readonly string _connectionString;

    public BookingRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IEnumerable<Booking>> GetAllAsync()
    {
        var bookings = new List<Booking>();
        using (var connection = new SqliteConnection(_connectionString))
        {
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT booking_id, user_id, place_id, schedule_id, promo_id, status, total_price, people_count, payment_status, created_at FROM bookings";

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var booking = new Booking
                    {
                        BookingId = reader.GetString(0),
                        UserId = reader.GetString(1),
                        PlaceId = reader.GetString(2),
                        ScheduleId = reader.IsDBNull(3) ? null : reader.GetString(3),
                        PromocodeId = reader.IsDBNull(4) ? null : reader.GetString(4),
                        Status = reader.GetString(5),
                        TotalPrice = reader.GetDecimal(6),
                        PeopleCount = reader.GetInt32(7),
                        PaymentStatus = reader.GetString(8),
                        CreatedAt = reader.GetDateTime(9)
                    };
                    bookings.Add(booking);
                }
            }
        }

        return bookings;
    }

    public async Task<Booking> GetAsync(System.Linq.Expressions.Expression<System.Func<Booking, bool>> predicate)
    {
        var allBookings = await GetAllAsync();
        return allBookings.AsQueryable().FirstOrDefault(predicate);
    }

    public async Task AddAsync(Booking entity)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO bookings (booking_id, user_id, place_id, schedule_id, promo_id, status, total_price, people_count, payment_status, created_at)
                VALUES ($booking_id, $user_id, $place_id, $schedule_id, $promo_id, $status, $total_price, $people_count, $payment_status, $created_at)";

            command.Parameters.AddWithValue("$booking_id", entity.BookingId);
            command.Parameters.AddWithValue("$user_id", entity.UserId);
            command.Parameters.AddWithValue("$place_id", entity.PlaceId);
            command.Parameters.AddWithValue("$schedule_id", (object)entity.ScheduleId ?? DBNull.Value);
            command.Parameters.AddWithValue("$promo_id", (object)entity.PromocodeId ?? DBNull.Value);
            command.Parameters.AddWithValue("$status", entity.Status);
            command.Parameters.AddWithValue("$total_price", entity.TotalPrice);
            command.Parameters.AddWithValue("$people_count", entity.PeopleCount);
            command.Parameters.AddWithValue("$payment_status", entity.PaymentStatus);
            command.Parameters.AddWithValue("$created_at", entity.CreatedAt);

            await command.ExecuteNonQueryAsync();
        }
    }

    public async Task UpdateAsync(Booking entity)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE bookings
                SET user_id = $user_id, place_id = $place_id, schedule_id = $schedule_id, promo_id = $promo_id,
                    status = $status, total_price = $total_price, people_count = $people_count,
                    payment_status = $payment_status, created_at = $created_at
                WHERE booking_id = $booking_id";

            command.Parameters.AddWithValue("$booking_id", entity.BookingId);
            command.Parameters.AddWithValue("$user_id", entity.UserId);
            command.Parameters.AddWithValue("$place_id", entity.PlaceId);
            command.Parameters.AddWithValue("$schedule_id", (object)entity.ScheduleId ?? DBNull.Value);
            command.Parameters.AddWithValue("$promo_id", (object)entity.PromocodeId ?? DBNull.Value);
            command.Parameters.AddWithValue("$status", entity.Status);
            command.Parameters.AddWithValue("$total_price", entity.TotalPrice);
            command.Parameters.AddWithValue("$people_count", entity.PeopleCount);
            command.Parameters.AddWithValue("$payment_status", entity.PaymentStatus);
            command.Parameters.AddWithValue("$created_at", entity.CreatedAt);

            await command.ExecuteNonQueryAsync();
        }
    }

    public async Task DeleteAsync(Booking entity)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM bookings WHERE booking_id = $booking_id";
            command.Parameters.AddWithValue("$booking_id", entity.BookingId);

            await command.ExecuteNonQueryAsync();
        }
    }
}