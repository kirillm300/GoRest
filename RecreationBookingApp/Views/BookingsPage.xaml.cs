using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using RecreationBookingApp.Data;
using RecreationBookingApp.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;

namespace RecreationBookingApp.Views;

public class BookingViewModel : BindableObject
{
    private string _bookingId;
    private string _placeName;
    private string _status;
    private decimal _totalPrice;
    private DateTime _createdAt;
    private bool _isCancelable;

    public string BookingId
    {
        get => _bookingId;
        set
        {
            _bookingId = value;
            OnPropertyChanged();
        }
    }

    public string PlaceName
    {
        get => _placeName;
        set
        {
            _placeName = value;
            OnPropertyChanged();
        }
    }

    public string Status
    {
        get => _status;
        set
        {
            _status = value;
            OnPropertyChanged();
            IsCancelable = _status != "completed"; // Обновляем IsCancelable при изменении статуса
        }
    }

    public decimal TotalPrice
    {
        get => _totalPrice;
        set
        {
            _totalPrice = value;
            OnPropertyChanged();
        }
    }

    public DateTime CreatedAt
    {
        get => _createdAt;
        set
        {
            _createdAt = value;
            OnPropertyChanged();
        }
    }

    public bool IsCancelable
    {
        get => _isCancelable;
        set
        {
            _isCancelable = value;
            OnPropertyChanged();
        }
    }
}

public partial class BookingsPage : ContentPage
{
    private readonly AppDbContext _dbContext;
    private string _errorMessage;

    public ObservableCollection<BookingViewModel> Bookings { get; } = new ObservableCollection<BookingViewModel>();
    public IRelayCommand<BookingViewModel> CancelBookingCommand { get; }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged();
        }
    }

    public BookingsPage()
    {
        // Создаём AppDbContext вручную
        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "gorest_db.db");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;
        _dbContext = new AppDbContext(options);

        CancelBookingCommand = new RelayCommand<BookingViewModel>(async (booking) => await CancelBookingAsync(booking));
        InitializeComponent();
        BindingContext = this;
        _ = LoadBookingsAsync(); // Запуск асинхронной загрузки
    }

    private async Task LoadBookingsAsync()
    {
        var userId = Preferences.Get("UserId", null);
        if (string.IsNullOrWhiteSpace(userId))
        {
            ErrorMessage = "Пожалуйста, войдите в аккаунт, чтобы увидеть брони.";
            return;
        }

        try
        {
            using (var connection = _dbContext.Database.GetDbConnection() as SqliteConnection)
            {
                await connection.OpenAsync();
                Debug.WriteLine("BookingsPage: Database connection opened.");

                var bookingsCommand = connection.CreateCommand();
                bookingsCommand.CommandText = @"
                    SELECT b.booking_id, p.name AS place_name, b.status, b.total_price, b.created_at
                    FROM bookings b
                    JOIN places p ON b.place_id = p.place_id
                    WHERE b.user_id = $userId";
                bookingsCommand.Parameters.AddWithValue("$userId", userId);

                Bookings.Clear();
                using (var reader = await bookingsCommand.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var booking = new BookingViewModel
                        {
                            BookingId = reader.GetString(0),
                            PlaceName = reader.GetString(1),
                            Status = reader.GetString(2),
                            TotalPrice = reader.GetDecimal(3),
                            CreatedAt = reader.GetDateTime(4)
                        };
                        Bookings.Add(booking);
                    }
                }

                Debug.WriteLine($"BookingsPage: Loaded {Bookings.Count} bookings for userId={userId}");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка загрузки броней: {ex.Message}";
            Debug.WriteLine($"BookingsPage: Error loading bookings: {ex.Message}");
        }
    }

    private async Task CancelBookingAsync(BookingViewModel booking)
    {
        if (booking.Status == "completed")
        {
            ErrorMessage = "Нельзя отменить завершенную бронь.";
            return;
        }

        try
        {
            ErrorMessage = string.Empty;
            using (var connection = _dbContext.Database.GetDbConnection() as SqliteConnection)
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var command = connection.CreateCommand();
                        command.Transaction = transaction;
                        command.CommandText = @"
                            UPDATE bookings 
                            SET status = 'canceled'
                            WHERE booking_id = $bookingId";
                        command.Parameters.AddWithValue("$bookingId", booking.BookingId);

                        await command.ExecuteNonQueryAsync();
                        transaction.Commit();

                        // Обновляем статус в модели
                        booking.Status = "canceled";
                        await DisplayAlert("Успех", "Бронь успешно отменена!", "OK");
                        Debug.WriteLine($"BookingsPage: Booking {booking.BookingId} canceled.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ErrorMessage = $"Ошибка при отмене брони: {ex.Message}";
                        Debug.WriteLine($"BookingsPage: Error canceling booking: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка при отмене брони: {ex.Message}";
            Debug.WriteLine($"BookingsPage: Error canceling booking: {ex.Message}");
        }
    }
}