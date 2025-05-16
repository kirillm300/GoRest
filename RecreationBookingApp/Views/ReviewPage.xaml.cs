using Microsoft.Data.Sqlite;
using Microsoft.Maui.Controls;
using RecreationBookingApp.Data;
using RecreationBookingApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;

namespace RecreationBookingApp.Views;

public class BookedPlaceViewModel : BindableObject
{
    private string _placeId;
    private string _name;
    private string _address;
    private bool _hasReview;
    private int _rating;
    private string _comment;

    public string PlaceId
    {
        get => _placeId;
        set
        {
            _placeId = value;
            OnPropertyChanged();
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
        }
    }

    public string Address
    {
        get => _address;
        set
        {
            _address = value;
            OnPropertyChanged();
        }
    }

    public bool HasReview
    {
        get => _hasReview;
        set
        {
            _hasReview = value;
            OnPropertyChanged();
        }
    }

    public int Rating
    {
        get => _rating;
        set
        {
            _rating = value;
            OnPropertyChanged();
        }
    }

    public string Comment
    {
        get => _comment;
        set
        {
            _comment = value;
            OnPropertyChanged();
        }
    }
}

public partial class ReviewPage : ContentPage
{
    private readonly AppDbContext _dbContext;
    private string _errorMessage;

    public ObservableCollection<BookedPlaceViewModel> BookedPlaces { get; } = new ObservableCollection<BookedPlaceViewModel>();
    public List<int> RatingOptions { get; } = new List<int> { 1, 2, 3, 4, 5 };
    public IRelayCommand<BookedPlaceViewModel> SubmitReviewCommand { get; }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged();
        }
    }

    public ReviewPage(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext), "AppDbContext cannot be null.");
        SubmitReviewCommand = new RelayCommand<BookedPlaceViewModel>(async (place) => await SubmitReviewAsync(place));
        InitializeComponent();
        BindingContext = this;
        LoadBookedPlacesAsync();
    }

    private async void LoadBookedPlacesAsync()
    {
        var userId = Preferences.Get("UserId", null);
        if (string.IsNullOrWhiteSpace(userId))
        {
            ErrorMessage = "Пожалуйста, войдите в аккаунт, чтобы оставить отзыв.";
            return;
        }

        try
        {
            using (var connection = _dbContext.Database.GetDbConnection() as SqliteConnection)
            {
                await connection.OpenAsync();
                Debug.WriteLine("ReviewPage: Database connection opened.");

                // Загружаем подтверждённые брони пользователя
                var bookingsCommand = connection.CreateCommand();
                bookingsCommand.CommandText = @"
                    SELECT DISTINCT p.place_id, p.name, p.address
                    FROM bookings b
                    JOIN places p ON b.place_id = p.place_id
                    WHERE b.user_id = $userId AND b.status = 'confirmed'";
                bookingsCommand.Parameters.AddWithValue("$userId", userId);

                var bookedPlaces = new List<BookedPlaceViewModel>();
                using (var reader = await bookingsCommand.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var place = new BookedPlaceViewModel
                        {
                            PlaceId = reader.GetString(0),
                            Name = reader.GetString(1),
                            Address = reader.GetString(2),
                            Rating = 5 // Значение по умолчанию
                        };
                        bookedPlaces.Add(place);
                    }
                }

                // Проверяем, оставил ли пользователь уже отзыв на каждое место
                foreach (var place in bookedPlaces)
                {
                    var reviewCheckCommand = connection.CreateCommand();
                    reviewCheckCommand.CommandText = @"
                        SELECT COUNT(*) 
                        FROM reviews 
                        WHERE user_id = $userId AND place_id = $placeId";
                    reviewCheckCommand.Parameters.AddWithValue("$userId", userId);
                    reviewCheckCommand.Parameters.AddWithValue("$placeId", place.PlaceId);

                    var reviewCount = Convert.ToInt32(await reviewCheckCommand.ExecuteScalarAsync());
                    place.HasReview = reviewCount > 0;
                }

                // Обновляем коллекцию
                BookedPlaces.Clear();
                foreach (var place in bookedPlaces)
                {
                    BookedPlaces.Add(place);
                }

                Debug.WriteLine($"ReviewPage: Loaded {BookedPlaces.Count} places for userId={userId}");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка загрузки мест: {ex.Message}";
            Debug.WriteLine($"ReviewPage: Error loading booked places: {ex.Message}");
        }
    }

    private async Task SubmitReviewAsync(BookedPlaceViewModel place)
    {
        var userId = Preferences.Get("UserId", null);
        if (string.IsNullOrWhiteSpace(userId))
        {
            ErrorMessage = "Пожалуйста, войдите в аккаунт, чтобы оставить отзыв.";
            return;
        }

        // Валидация рейтинга
        if (place.Rating < 1 || place.Rating > 5)
        {
            ErrorMessage = "Оценка должна быть от 1 до 5.";
            return;
        }

        try
        {
            ErrorMessage = string.Empty;
            var review = new Review
            {
                ReviewId = Guid.NewGuid().ToString(),
                UserId = userId,
                PlaceId = place.PlaceId,
                Rating = place.Rating,
                Comment = string.IsNullOrWhiteSpace(place.Comment) ? null : place.Comment,
                CreatedAt = DateTime.UtcNow
            };

            using (var connection = _dbContext.Database.GetDbConnection() as SqliteConnection)
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Проверяем, не оставил ли пользователь уже отзыв
                        var reviewCheckCommand = connection.CreateCommand();
                        reviewCheckCommand.CommandText = @"
                            SELECT COUNT(*) 
                            FROM reviews 
                            WHERE user_id = $userId AND place_id = $placeId";
                        reviewCheckCommand.Parameters.AddWithValue("$userId", userId);
                        reviewCheckCommand.Parameters.AddWithValue("$placeId", place.PlaceId);

                        var reviewCount = Convert.ToInt32(await reviewCheckCommand.ExecuteScalarAsync());
                        if (reviewCount > 0)
                        {
                            ErrorMessage = "Вы уже оставили отзыв на это место.";
                            return;
                        }

                        // Добавляем отзыв
                        var command = connection.CreateCommand();
                        command.Transaction = transaction;
                        command.CommandText = @"
                            INSERT INTO reviews (review_id, user_id, place_id, rating, comment, created_at)
                            VALUES ($reviewId, $userId, $placeId, $rating, $comment, $createdAt)";
                        command.Parameters.AddWithValue("$reviewId", review.ReviewId);
                        command.Parameters.AddWithValue("$userId", review.UserId);
                        command.Parameters.AddWithValue("$placeId", review.PlaceId);
                        command.Parameters.AddWithValue("$rating", review.Rating);
                        command.Parameters.AddWithValue("$comment", (object)review.Comment ?? DBNull.Value);
                        command.Parameters.AddWithValue("$createdAt", review.CreatedAt);

                        await command.ExecuteNonQueryAsync();
                        transaction.Commit();

                        // Обновляем состояние
                        place.HasReview = true;
                        await DisplayAlert("Успех", "Отзыв успешно отправлен!", "OK");
                        Debug.WriteLine($"ReviewPage: Review submitted for placeId={place.PlaceId}, rating={place.Rating}");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ErrorMessage = $"Ошибка при отправке отзыва: {ex.Message}";
                        Debug.WriteLine($"ReviewPage: Error submitting review: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка при отправке отзыва: {ex.Message}";
            Debug.WriteLine($"ReviewPage: Error submitting review: {ex.Message}");
        }
    }
}