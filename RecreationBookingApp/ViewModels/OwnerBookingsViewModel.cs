using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls;
using RecreationBookingApp.Data;
using RecreationBookingApp.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace RecreationBookingApp.ViewModels;

public partial class OwnerBookingsViewModel : ObservableObject
{
    private readonly AppDbContext _dbContext;
    private string _ownerId;

    [ObservableProperty]
    private ObservableCollection<Booking> bookings;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string errorMessage;

    public ObservableCollection<string> StatusOptions { get; } = new ObservableCollection<string> { "pending", "confirmed", "canceled", "completed" };

    public OwnerBookingsViewModel(AppDbContext dbContext)
    {
        _dbContext = dbContext;
        Bookings = new ObservableCollection<Booking>();
        _ownerId = Preferences.Get("UserId", null);
        if (string.IsNullOrEmpty(_ownerId))
        {
            ErrorMessage = "Пользователь не авторизован.";
            return;
        }
        LoadBookingsAsync().GetAwaiter().GetResult();
    }

    [RelayCommand]
    private async Task LoadBookingsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            using (var connection = _dbContext.Database.GetDbConnection() as Microsoft.Data.Sqlite.SqliteConnection)
            {
                await connection.OpenAsync();

                // Проверяем роль пользователя
                var roleCommand = connection.CreateCommand();
                roleCommand.CommandText = "SELECT role FROM users WHERE user_id = $userId";
                roleCommand.Parameters.AddWithValue("$userId", _ownerId);
                var role = await roleCommand.ExecuteScalarAsync() as string;
                if (role != "owner")
                {
                    ErrorMessage = "Доступ запрещён: только владельцы могут просматривать бронирования.";
                    return;
                }

                // Получаем все места, принадлежащие владельцу
                var placeCommand = connection.CreateCommand();
                placeCommand.CommandText = "SELECT place_id FROM places WHERE owner_id = $ownerId";
                placeCommand.Parameters.AddWithValue("$ownerId", _ownerId);
                var placeIds = new System.Collections.Generic.List<string>();
                using (var reader = await placeCommand.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        placeIds.Add(reader.GetString(0));
                    }
                }
                Debug.WriteLine($"OwnerBookingsViewModel: Found {placeIds.Count} places for ownerId={_ownerId}");

                if (!placeIds.Any())
                {
                    ErrorMessage = "У вас нет зарегистрированных мест.";
                    return;
                }

                // Получаем бронирования для всех мест владельца
                Bookings.Clear();
                var bookingCommand = connection.CreateCommand();
                bookingCommand.CommandText = @"
                    SELECT b.booking_id, b.user_id, b.schedule_id, b.promo_id, b.status, b.total_price, 
                           b.people_count, b.payment_status, b.created_at, b.place_id, 
                           u.full_name AS user_name, p.name AS place_name
                    FROM bookings b
                    JOIN users u ON b.user_id = u.user_id
                    JOIN places p ON b.place_id = p.place_id
                    WHERE b.place_id IN (" + string.Join(",", placeIds.Select((_, i) => $"@placeId{i}")) + ")";
                for (int i = 0; i < placeIds.Count; i++)
                {
                    bookingCommand.Parameters.AddWithValue($"@placeId{i}", placeIds[i]);
                }

                using (var reader = await bookingCommand.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var booking = new Booking
                        {
                            BookingId = reader.GetString(0),
                            UserId = reader.GetString(1),
                            ScheduleId = reader.GetString(2),
                            PromocodeId = reader.IsDBNull(3) ? null : reader.GetString(3),
                            Status = reader.GetString(4),
                            TotalPrice = reader.GetDecimal(5),
                            PeopleCount = reader.GetInt32(6),
                            PaymentStatus = reader.GetString(7),
                            CreatedAt = reader.GetDateTime(8),
                            PlaceId = reader.GetString(9),
                            UserName = reader.GetString(10), // Дополнительное поле для имени пользователя
                            PlaceName = reader.GetString(11) // Дополнительное поле для названия места
                        };
                        Bookings.Add(booking);
                    }
                }
                Debug.WriteLine($"OwnerBookingsViewModel: Loaded {Bookings.Count} bookings for ownerId={_ownerId}");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка загрузки бронирований: {ex.Message}";
            Debug.WriteLine($"OwnerBookingsViewModel: Error loading bookings: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task UpdateBookingStatus(Booking booking)
    {
        if (booking == null || string.IsNullOrEmpty(booking.Status))
            return;

        try
        {
            using (var connection = _dbContext.Database.GetDbConnection() as Microsoft.Data.Sqlite.SqliteConnection)
            {
                await connection.OpenAsync();
                var command = connection.CreateCommand();
                command.CommandText = "UPDATE bookings SET status = $status WHERE booking_id = $bookingId";
                command.Parameters.AddWithValue("$status", booking.Status);
                command.Parameters.AddWithValue("$bookingId", booking.BookingId);
                await command.ExecuteNonQueryAsync();

                Debug.WriteLine($"OwnerBookingsViewModel: Updated status for bookingId={booking.BookingId} to {booking.Status}");
                await Application.Current.MainPage.DisplayAlert("Успех", "Статус бронирования обновлён.", "OK");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка обновления статуса: {ex.Message}";
            Debug.WriteLine($"OwnerBookingsViewModel: Error updating booking status: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Ошибка", ErrorMessage, "OK");
        }
    }
}