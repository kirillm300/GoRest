using Microsoft.Data.Sqlite;
using Microsoft.Maui.Controls;
using RecreationBookingApp.Data;
using RecreationBookingApp.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;

namespace RecreationBookingApp.Views;

[QueryProperty(nameof(PlaceId), "placeId")]
public partial class PlaceDetailPage : ContentPage
{
    private readonly AppDbContext _dbContext;
    private string _placeId;
    private Place _place;
    private string _errorMessage;
    private DateTime _startDate;
    private DateTime _endDate;
    private TimeSpan _startTime;
    private TimeSpan _endTime;
    private Room _selectedRoom;
    private decimal _totalPrice;
    private int _peopleCount;
    private int _maxCapacity;

    public string PlaceId
    {
        get => _placeId;
        set
        {
            _placeId = value;
            OnPropertyChanged();
            LoadPlaceDetailsAsync();
        }
    }

    public Place Place
    {
        get => _place;
        set
        {
            _place = value;
            OnPropertyChanged();
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<Room> Rooms { get; } = new ObservableCollection<Room>();
    public ObservableCollection<string> Images { get; } = new ObservableCollection<string>();
    public ObservableCollection<string> RoomFeatures { get; } = new ObservableCollection<string>();
    public decimal TotalPrice
    {
        get => _totalPrice;
        set
        {
            _totalPrice = value;
            OnPropertyChanged();
        }
    }

    public Room SelectedRoom
    {
        get => _selectedRoom;
        set
        {
            _selectedRoom = value;
            OnPropertyChanged();
            UpdateTotalPrice(); // Обновляем цену при смене комнаты
            MaxCapacity = _selectedRoom?.Capacity ?? 0; // Обновляем максимальную вместимость
        }
    }

    public DateTime StartDate
    {
        get => _startDate;
        set
        {
            _startDate = value;
            OnPropertyChanged();
            UpdateTotalPrice(); // Обновляем цену при смене даты
        }
    }

    public DateTime EndDate
    {
        get => _endDate;
        set
        {
            _endDate = value;
            OnPropertyChanged();
            UpdateTotalPrice(); // Обновляем цену при смене даты
        }
    }

    public TimeSpan StartTime
    {
        get => _startTime;
        set
        {
            _startTime = value;
            OnPropertyChanged();
            UpdateTotalPrice(); // Обновляем цену при смене времени
        }
    }

    public TimeSpan EndTime
    {
        get => _endTime;
        set
        {
            _endTime = value;
            OnPropertyChanged();
            UpdateTotalPrice(); // Обновляем цену при смене времени
        }
    }

    public int PeopleCount
    {
        get => _peopleCount;
        set
        {
            _peopleCount = value;
            OnPropertyChanged();
            UpdateTotalPrice(); // Обновляем цену при изменении количества людей (если нужно)
        }
    }

    public int MaxCapacity
    {
        get => _maxCapacity;
        set
        {
            _maxCapacity = value;
            OnPropertyChanged();
        }
    }

    public IRelayCommand BookPlaceCommand { get; }

    public PlaceDetailPage(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext), "AppDbContext cannot be null.");
        BookPlaceCommand = new RelayCommand(async () => await BookPlaceAsync());
        InitializeComponent();
        StartDate = DateTime.Today;
        EndDate = DateTime.Today.AddDays(1);
        StartTime = new TimeSpan(14, 0, 0); // 14:00 по умолчанию
        EndTime = new TimeSpan(0, 0, 0);    // 00:00 по умолчанию
        PeopleCount = 1; // Значение по умолчанию
        BindingContext = this;
        Debug.WriteLine("PlaceDetailPage: Constructed with AppDbContext.");
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Debug.WriteLine("PlaceDetailPage: OnAppearing called.");
    }

    private async void LoadPlaceDetailsAsync()
    {
        if (string.IsNullOrEmpty(PlaceId))
        {
            Debug.WriteLine("PlaceDetailPage: PlaceId is null or empty.");
            return;
        }

        try
        {
            Debug.WriteLine($"PlaceDetailPage: Loading details for placeId={PlaceId}");

            using (var connection = _dbContext.Database.GetDbConnection() as SqliteConnection)
            {
                await connection.OpenAsync();
                Debug.WriteLine("PlaceDetailPage: Database connection opened.");

                // Загрузка данных места
                var placeCommand = connection.CreateCommand();
                placeCommand.CommandText = @"
                    SELECT place_id, owner_id, name, description, address, latitude, longitude, contact_phone, category_id, status, created_at 
                    FROM places 
                    WHERE place_id = $placeId";
                placeCommand.Parameters.AddWithValue("$placeId", PlaceId);

                using (var reader = await placeCommand.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        Place = new Place
                        {
                            PlaceId = reader.GetString(0),
                            OwnerId = reader.GetString(1),
                            Name = reader.GetString(2),
                            Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                            Address = reader.IsDBNull(4) ? null : reader.GetString(4),
                            Latitude = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5),
                            Longitude = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6),
                            ContactPhone = reader.IsDBNull(7) ? null : reader.GetString(7),
                            CategoryId = reader.GetString(8),
                            Status = reader.GetString(9),
                            CreatedAt = reader.GetDateTime(10)
                        };
                        Debug.WriteLine($"PlaceDetailPage: Loaded place: {Place.Name}");
                    }
                }

                // Загрузка комнат
                var roomCommand = connection.CreateCommand();
                roomCommand.CommandText = @"
                    SELECT room_id, place_id, name, capacity, description, base_price, created_at
                    FROM rooms 
                    WHERE place_id = $placeId";
                roomCommand.Parameters.AddWithValue("$placeId", PlaceId);

                using (var reader = await roomCommand.ExecuteReaderAsync())
                {
                    Rooms.Clear();
                    while (await reader.ReadAsync())
                    {
                        var room = new Room
                        {
                            RoomId = reader.GetString(0),
                            PlaceId = reader.GetString(1),
                            Name = reader.GetString(2),
                            Capacity = reader.GetInt32(3),
                            Description = reader.IsDBNull(4) ? null : reader.GetString(4),
                            BasePrice = reader.GetDecimal(5),
                            CreatedAt = reader.GetDateTime(6)
                        };
                        Rooms.Add(room);
                    }
                    Debug.WriteLine($"PlaceDetailPage: Loaded {Rooms.Count} rooms for placeId={PlaceId}");
                }

                // Загрузка изображений
                var imageCommand = connection.CreateCommand();
                imageCommand.CommandText = "SELECT url FROM images WHERE place_id = $placeId";
                imageCommand.Parameters.AddWithValue("$placeId", PlaceId);

                using (var reader = await imageCommand.ExecuteReaderAsync())
                {
                    Images.Clear();
                    while (await reader.ReadAsync())
                    {
                        Images.Add(reader.GetString(0));
                    }
                    Debug.WriteLine($"PlaceDetailPage: Loaded {Images.Count} images for placeId={PlaceId}");
                }

                // Загрузка особенностей комнат
                var featureCommand = connection.CreateCommand();
                featureCommand.CommandText = @"
                    SELECT rf.feature_name
                    FROM room_features rf
                    WHERE rf.room_id IN (
                        SELECT r.room_id 
                        FROM rooms r 
                        WHERE r.place_id = $placeId
                    )";
                featureCommand.Parameters.AddWithValue("$placeId", PlaceId);

                using (var reader = await featureCommand.ExecuteReaderAsync())
                {
                    RoomFeatures.Clear();
                    while (await reader.ReadAsync())
                    {
                        var featureName = reader.GetString(0);
                        if (!string.IsNullOrEmpty(featureName))
                        {
                            RoomFeatures.Add(featureName);
                        }
                    }
                    Debug.WriteLine($"PlaceDetailPage: Loaded {RoomFeatures.Count} room features for placeId={PlaceId}");
                }

                // Загрузка правил ценообразования
                var pricingCommand = connection.CreateCommand();
                pricingCommand.CommandText = @"
                    SELECT pr.rule_id, pr.room_id, pr.type, pr.price, pr.start_date, pr.end_date
                    FROM pricing_rules pr
                    WHERE pr.room_id IN (
                        SELECT r.room_id 
                        FROM rooms r 
                        WHERE r.place_id = $placeId
                    )";
                pricingCommand.Parameters.AddWithValue("$placeId", PlaceId);

                using (var reader = await pricingCommand.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var pricingRule = new PricingRule
                        {
                            PricingRuleId = reader.GetString(0),
                            RoomId = reader.GetString(1),
                            Type = reader.GetString(2),
                            Price = reader.GetDecimal(3),
                            StartDate = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                            EndDate = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5)
                        };
                        var room = Rooms.FirstOrDefault(r => r.RoomId == pricingRule.RoomId);
                        if (room != null)
                        {
                            if (room.PricingRules == null) room.PricingRules = new List<PricingRule>();
                            room.PricingRules.Add(pricingRule);
                        }
                    }
                    Debug.WriteLine($"PlaceDetailPage: Loaded pricing rules for placeId={PlaceId}");
                }
            }
            UpdateTotalPrice();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка загрузки данных: {ex.Message}";
            Debug.WriteLine($"PlaceDetailPage: Error loading place details: {ex.Message}");
        }
    }

    private bool IsWeekend(DateTime date)
    {
        return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
    }

    private void UpdateTotalPrice()
    {
        if (SelectedRoom == null || StartDate > EndDate)
        {
            TotalPrice = 0;
            return;
        }

        TotalPrice = CalculateTotalPrice();
    }

    private decimal CalculateTotalPrice()
    {
        if (SelectedRoom == null || SelectedRoom.PricingRules == null) return 0;

        var days = (EndDate - StartDate).Days + 1;
        if (days <= 0) return 0;

        decimal total = 0;
        var currentDate = StartDate;
        var startDateTime = StartDate.Date + StartTime;
        var endDateTime = EndDate.Date + EndTime;
        var totalDuration = (endDateTime - startDateTime).TotalHours;

        for (int i = 0; i < days; i++)
        {
            var price = GetPriceForDate(currentDate);
            decimal hoursInDay;

            if (days == 1)
            {
                hoursInDay = (decimal)totalDuration;
            }
            else if (i == 0) // Первый день
            {
                var endOfDay = StartDate.Date.AddDays(1);
                hoursInDay = (decimal)(endOfDay - startDateTime).TotalHours;
            }
            else if (i == days - 1) // Последний день
            {
                var startOfDay = EndDate.Date;
                hoursInDay = (decimal)(endDateTime - startOfDay).TotalHours;
            }
            else // Полный день
            {
                hoursInDay = 24;
            }

            total += price * hoursInDay / 24;
            currentDate = currentDate.AddDays(1);
        }

        // Учитываем количество людей (простейшая логика: цена увеличивается пропорционально)
        if (PeopleCount > 0)
        {
            total *= PeopleCount;
        }

        return Math.Round(total, 2);
    }

    private decimal GetPriceForDate(DateTime date)
    {
        if (SelectedRoom?.PricingRules == null) return 0;

        bool isWeekend = IsWeekend(date);
        var applicableRules = SelectedRoom.PricingRules
            .Where(r => (r.StartDate == null || date >= r.StartDate) && (r.EndDate == null || date <= r.EndDate))
            .ToList();

        PricingRule rule = null;
        if (isWeekend && applicableRules.Any(r => r.Type == "holiday"))
        {
            rule = applicableRules.First(r => r.Type == "holiday");
        }
        else if (applicableRules.Any(r => r.Type == "base"))
        {
            rule = applicableRules.First(r => r.Type == "base");
        }

        return rule?.Price ?? SelectedRoom.BasePrice;
    }

    private async Task BookPlaceAsync()
    {
        if (string.IsNullOrWhiteSpace(Preferences.Get("UserId", null)))
        {
            ErrorMessage = "Пожалуйста, войдите в аккаунт для бронирования.";
            return;
        }

        if (SelectedRoom == null)
        {
            ErrorMessage = "Пожалуйста, выберите комнату.";
            return;
        }

        if (PeopleCount <= 0)
        {
            ErrorMessage = "Количество человек должно быть больше 0.";
            return;
        }

        if (PeopleCount > MaxCapacity)
        {
            ErrorMessage = $"Количество человек ({PeopleCount}) превышает максимальную вместимость ({MaxCapacity}).";
            return;
        }

        var startDateTime = StartDate.Date + StartTime;
        var endDateTime = EndDate.Date + EndTime;
        if (startDateTime >= endDateTime)
        {
            ErrorMessage = "Дата и время окончания должны быть позже даты и времени начала.";
            return;
        }

        try
        {
            ErrorMessage = string.Empty;
            var userId = Preferences.Get("UserId", null);
            var createdAt = DateTime.UtcNow;

            using (var connection = _dbContext.Database.GetDbConnection() as SqliteConnection)
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Проверяем существование user_id
                        var userCheckCommand = connection.CreateCommand();
                        userCheckCommand.CommandText = "SELECT COUNT(*) FROM users WHERE user_id = $userId";
                        userCheckCommand.Parameters.AddWithValue("$userId", userId);
                        var userExists = Convert.ToInt32(await userCheckCommand.ExecuteScalarAsync()) > 0;
                        if (!userExists)
                        {
                            throw new Exception($"User with user_id={userId} does not exist in the users table.");
                        }
                        Debug.WriteLine($"PlaceDetailPage: UserId={userId} exists in users table.");

                        // Проверяем существование place_id
                        var placeCheckCommand = connection.CreateCommand();
                        placeCheckCommand.CommandText = "SELECT COUNT(*) FROM places WHERE place_id = $placeId";
                        placeCheckCommand.Parameters.AddWithValue("$placeId", PlaceId);
                        var placeExists = Convert.ToInt32(await placeCheckCommand.ExecuteScalarAsync()) > 0;
                        if (!placeExists)
                        {
                            throw new Exception($"Place with place_id={PlaceId} does not exist in the places table.");
                        }
                        Debug.WriteLine($"PlaceDetailPage: PlaceId={PlaceId} exists in places table.");

                        // Проверяем существование room_id
                        var roomCheckCommand = connection.CreateCommand();
                        roomCheckCommand.CommandText = "SELECT COUNT(*) FROM rooms WHERE room_id = $roomId";
                        roomCheckCommand.Parameters.AddWithValue("$roomId", SelectedRoom.RoomId);
                        var roomExists = Convert.ToInt32(await roomCheckCommand.ExecuteScalarAsync()) > 0;
                        if (!roomExists)
                        {
                            throw new Exception($"Room with room_id={SelectedRoom.RoomId} does not exist in the rooms table.");
                        }
                        Debug.WriteLine($"PlaceDetailPage: RoomId={SelectedRoom.RoomId} exists in rooms table.");

                        // Проверяем доступность для каждого дня в диапазоне
                        var currentDate = StartDate;
                        while (currentDate <= EndDate)
                        {
                            var (scheduleId, isAvailable) = await CheckAvailabilityAsync(currentDate, connection);
                            if (!isAvailable)
                            {
                                ErrorMessage = $"Время на {currentDate:dd/MM/yyyy} уже занято. Выберите другой временной слот.";
                                return;
                            }

                            string newScheduleId = null;
                            if (scheduleId == null) // Если подходящего слота нет, создаём новый
                            {
                                newScheduleId = Guid.NewGuid().ToString();
                                var scheduleCommand = connection.CreateCommand();
                                scheduleCommand.Transaction = transaction;
                                scheduleCommand.CommandText = @"
                                    INSERT INTO schedules (schedule_id, room_id, date, start_time, end_time, is_available)
                                    VALUES ($scheduleId, $roomId, $date, $startTime, $endTime, $isAvailable)";
                                scheduleCommand.Parameters.AddWithValue("$scheduleId", newScheduleId);
                                scheduleCommand.Parameters.AddWithValue("$roomId", SelectedRoom.RoomId);
                                scheduleCommand.Parameters.AddWithValue("$date", currentDate);
                                scheduleCommand.Parameters.AddWithValue("$startTime", StartTime.ToString());
                                scheduleCommand.Parameters.AddWithValue("$endTime", EndTime.ToString());
                                scheduleCommand.Parameters.AddWithValue("$isAvailable", false); // Помечаем как занятое
                                await scheduleCommand.ExecuteNonQueryAsync();
                                Debug.WriteLine($"PlaceDetailPage: Created schedule with scheduleId={newScheduleId} for date {currentDate:dd/MM/yyyy}");
                            }
                            else
                            {
                                // Обновляем существующий слот, помечая его как занятый
                                var updateScheduleCommand = connection.CreateCommand();
                                updateScheduleCommand.Transaction = transaction;
                                updateScheduleCommand.CommandText = @"
                                    UPDATE schedules 
                                    SET is_available = 0 
                                    WHERE schedule_id = $scheduleId";
                                updateScheduleCommand.Parameters.AddWithValue("$scheduleId", scheduleId);
                                await updateScheduleCommand.ExecuteNonQueryAsync();
                                Debug.WriteLine($"PlaceDetailPage: Updated schedule with scheduleId={scheduleId} to unavailable for date {currentDate:dd/MM/yyyy}");
                                newScheduleId = scheduleId;
                            }

                            // Проверяем, что schedule_id существует
                            var scheduleCheckCommand = connection.CreateCommand();
                            scheduleCheckCommand.Transaction = transaction;
                            scheduleCheckCommand.CommandText = "SELECT COUNT(*) FROM schedules WHERE schedule_id = $scheduleId";
                            scheduleCheckCommand.Parameters.AddWithValue("$scheduleId", newScheduleId);
                            var scheduleExists = Convert.ToInt32(await scheduleCheckCommand.ExecuteScalarAsync()) > 0;
                            if (!scheduleExists)
                            {
                                throw new Exception($"Schedule with schedule_id={newScheduleId} does not exist in the schedules table.");
                            }
                            Debug.WriteLine($"PlaceDetailPage: ScheduleId={newScheduleId} exists in schedules table.");

                            // Создание записи в bookings для текущего дня
                            var booking = new Booking
                            {
                                BookingId = Guid.NewGuid().ToString(),
                                UserId = userId,
                                PlaceId = PlaceId,
                                ScheduleId = newScheduleId,
                                PromocodeId = null,
                                Status = "pending",
                                TotalPrice = CalculateTotalPrice(),
                                PeopleCount = PeopleCount, // Записываем количество человек
                                PaymentStatus = "unpaid",
                                CreatedAt = createdAt
                            };

                            var bookingCommand = connection.CreateCommand();
                            bookingCommand.Transaction = transaction;
                            bookingCommand.CommandText = @"
                                INSERT INTO bookings (booking_id, user_id, place_id, schedule_id, promo_id, status, total_price, people_count, payment_status, created_at)
                                VALUES ($bookingId, $userId, $placeId, $scheduleId, $promoId, $status, $totalPrice, $peopleCount, $paymentStatus, $createdAt)";
                            bookingCommand.Parameters.AddWithValue("$bookingId", booking.BookingId);
                            bookingCommand.Parameters.AddWithValue("$userId", booking.UserId);
                            bookingCommand.Parameters.AddWithValue("$placeId", booking.PlaceId);
                            bookingCommand.Parameters.AddWithValue("$scheduleId", booking.ScheduleId);
                            bookingCommand.Parameters.AddWithValue("$promoId", (object)booking.PromocodeId ?? DBNull.Value);
                            bookingCommand.Parameters.AddWithValue("$status", booking.Status);
                            bookingCommand.Parameters.AddWithValue("$totalPrice", booking.TotalPrice);
                            bookingCommand.Parameters.AddWithValue("$peopleCount", booking.PeopleCount);
                            bookingCommand.Parameters.AddWithValue("$paymentStatus", booking.PaymentStatus);
                            bookingCommand.Parameters.AddWithValue("$createdAt", booking.CreatedAt);

                            await bookingCommand.ExecuteNonQueryAsync();
                            Debug.WriteLine($"PlaceDetailPage: Created booking with bookingId={booking.BookingId} for date {currentDate:dd/MM/yyyy}");

                            currentDate = currentDate.AddDays(1);
                        }

                        transaction.Commit();
                        await DisplayAlert("Успех", "Бронирование успешно создано!", "OK");
                        await Shell.Current.Navigation.PopAsync();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ErrorMessage = $"Ошибка при бронировании: {ex.Message}";
                        Debug.WriteLine($"PlaceDetailPage: Error in BookPlaceAsync: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка при бронировании: {ex.Message}";
            Debug.WriteLine($"PlaceDetailPage: Error in BookPlaceAsync: {ex.Message}");
        }
    }

    private async Task<(string scheduleId, bool isAvailable)> CheckAvailabilityAsync(DateTime date, SqliteConnection connection)
    {
        try
        {
            // Проверяем, есть ли подходящий слот
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT s.schedule_id, s.is_available
                FROM schedules s
                WHERE s.room_id = $roomId
                AND s.date = $date
                AND s.start_time = $startTime
                AND s.end_time = $endTime";
            command.Parameters.AddWithValue("$roomId", SelectedRoom.RoomId);
            command.Parameters.AddWithValue("$date", date);
            command.Parameters.AddWithValue("$startTime", StartTime.ToString());
            command.Parameters.AddWithValue("$endTime", EndTime.ToString());

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    var scheduleId = reader.GetString(0);
                    var isAvailable = reader.GetBoolean(1);
                    Debug.WriteLine($"PlaceDetailPage: Found schedule with scheduleId={scheduleId}, isAvailable={isAvailable} for date {date:dd/MM/yyyy}");
                    return (scheduleId, isAvailable);
                }
            }

            // Если слота нет, проверяем, пересекается ли выбранное время с существующими бронированиями
            var overlapCommand = connection.CreateCommand();
            overlapCommand.CommandText = @"
                SELECT COUNT(*) 
                FROM schedules s
                JOIN bookings b ON s.schedule_id = b.schedule_id
                WHERE s.room_id = $roomId
                AND s.date = $date
                AND b.status IN ('pending', 'confirmed')
                AND (
                    (s.start_time < $endTime AND s.end_time > $startTime)
                )";
            overlapCommand.Parameters.AddWithValue("$roomId", SelectedRoom.RoomId);
            overlapCommand.Parameters.AddWithValue("$date", date);
            overlapCommand.Parameters.AddWithValue("$startTime", StartTime.ToString());
            overlapCommand.Parameters.AddWithValue("$endTime", EndTime.ToString());

            var result = await overlapCommand.ExecuteScalarAsync();
            int overlappingBookings = Convert.ToInt32(result);
            Debug.WriteLine($"PlaceDetailPage: Found {overlappingBookings} overlapping bookings for roomId={SelectedRoom.RoomId} on date {date:dd/MM/yyyy}");
            return (null, overlappingBookings == 0);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"PlaceDetailPage: Error checking availability: {ex.Message}");
            return (null, false);
        }
    }
}