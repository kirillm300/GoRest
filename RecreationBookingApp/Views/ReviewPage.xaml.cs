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

public class ReviewViewModel : BindableObject
{
    private string _name;
    private int _rating;
    private string _comment;
    private DateTime _createdAt;

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
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

    public DateTime CreatedAt
    {
        get => _createdAt;
        set
        {
            _createdAt = value;
            OnPropertyChanged();
        }
    }
}

public class PlaceViewModel : BindableObject
{
    private string _placeId;
    private string _name;

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
}

public partial class ReviewPage : ContentPage
{
    private readonly AppDbContext _dbContext;
    private string _errorMessage;
    private bool _isReviewFormVisible;

    public ObservableCollection<ReviewViewModel> UserReviews { get; } = new ObservableCollection<ReviewViewModel>();
    public ObservableCollection<PlaceViewModel> AvailablePlaces { get; } = new ObservableCollection<PlaceViewModel>();
    public List<int> RatingOptions { get; } = new List<int> { 1, 2, 3, 4, 5 };
    public IRelayCommand AddReviewCommand { get; }
    public IRelayCommand SubmitNewReviewCommand { get; }
    public IRelayCommand CancelReviewCommand { get; }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged();
        }
    }

    public bool IsReviewFormVisible
    {
        get => _isReviewFormVisible;
        set
        {
            _isReviewFormVisible = value;
            OnPropertyChanged();
            if (ReviewForm != null) ReviewForm.IsVisible = value; // Обновляем видимость формы
        }
    }

    public PlaceViewModel SelectedPlace
    {
        get => _selectedPlace;
        set
        {
            _selectedPlace = value;
            OnPropertyChanged();
        }
    }
    private PlaceViewModel _selectedPlace;

    public int NewRating
    {
        get => _newRating;
        set
        {
            _newRating = value;
            OnPropertyChanged();
        }
    }
    private int _newRating = 5;

    public string NewComment
    {
        get => _newComment;
        set
        {
            _newComment = value;
            OnPropertyChanged();
        }
    }
    private string _newComment;

    public ReviewPage(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext), "AppDbContext cannot be null.");
        AddReviewCommand = new RelayCommand(ShowReviewForm);
        SubmitNewReviewCommand = new RelayCommand(async () => await SubmitNewReviewAsync());
        CancelReviewCommand = new RelayCommand(HideReviewForm);
        InitializeComponent();
        BindingContext = this;
        LoadUserReviewsAsync(); // Вызов без await, так как это событие конструктора
        LoadAvailablePlacesAsync(); // Вызов без await
    }

    private async Task LoadUserReviewsAsync() // Изменён с void на Task
    {
        var userId = Preferences.Get("UserId", null);
        if (string.IsNullOrWhiteSpace(userId))
        {
            ErrorMessage = "Пожалуйста, войдите в аккаунт, чтобы увидеть отзывы.";
            return;
        }

        try
        {
            using (var connection = _dbContext.Database.GetDbConnection() as SqliteConnection)
            {
                await connection.OpenAsync();
                Debug.WriteLine("ReviewPage: Database connection opened.");

                var reviewsCommand = connection.CreateCommand();
                reviewsCommand.CommandText = @"
                    SELECT r.comment, r.rating, r.created_at, p.name
                    FROM reviews r
                    JOIN places p ON r.place_id = p.place_id
                    WHERE r.user_id = $userId";
                reviewsCommand.Parameters.AddWithValue("$userId", userId);

                UserReviews.Clear();
                using (var reader = await reviewsCommand.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var review = new ReviewViewModel
                        {
                            Comment = reader.IsDBNull(0) ? null : reader.GetString(0),
                            Rating = reader.GetInt32(1),
                            CreatedAt = reader.GetDateTime(2),
                            Name = reader.GetString(3)
                        };
                        UserReviews.Add(review);
                    }
                }

                Debug.WriteLine($"ReviewPage: Loaded {UserReviews.Count} reviews for userId={userId}");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка загрузки отзывов: {ex.Message}";
            Debug.WriteLine($"ReviewPage: Error loading reviews: {ex.Message}");
        }
    }

    private async Task LoadAvailablePlacesAsync() // Изменён с void на Task
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
                Debug.WriteLine("ReviewPage: Loading available places...");

                var placesCommand = connection.CreateCommand();
                placesCommand.CommandText = @"
                    SELECT DISTINCT p.place_id, p.name
                    FROM bookings b
                    JOIN schedules s ON b.schedule_id = s.schedule_id
                    JOIN rooms r ON s.room_id = r.room_id
                    JOIN places p ON r.place_id = p.place_id
                    WHERE b.user_id = $userId AND b.status = 'confirmed'";
                placesCommand.Parameters.AddWithValue("$userId", userId);

                AvailablePlaces.Clear();
                using (var reader = await placesCommand.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var place = new PlaceViewModel
                        {
                            PlaceId = reader.GetString(0),
                            Name = reader.GetString(1)
                        };
                        AvailablePlaces.Add(place);
                    }
                }

                Debug.WriteLine($"ReviewPage: Loaded {AvailablePlaces.Count} available places for userId={userId}");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка загрузки доступных мест: {ex.Message}";
            Debug.WriteLine($"ReviewPage: Error loading available places: {ex.Message}");
        }
    }

    private void ShowReviewForm()
    {
        IsReviewFormVisible = true;
        NewRating = 5; // Значение по умолчанию
        NewComment = string.Empty;
        SelectedPlace = AvailablePlaces.FirstOrDefault(); // Устанавливаем первое место по умолчанию
    }

    private void HideReviewForm()
    {
        IsReviewFormVisible = false;
    }

    private async Task SubmitNewReviewAsync()
    {
        var userId = Preferences.Get("UserId", null);
        if (string.IsNullOrWhiteSpace(userId))
        {
            ErrorMessage = "Пожалуйста, войдите в аккаунт, чтобы оставить отзыв.";
            return;
        }

        if (SelectedPlace == null)
        {
            ErrorMessage = "Пожалуйста, выберите место.";
            return;
        }

        if (NewRating < 1 || NewRating > 5)
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
                PlaceId = SelectedPlace.PlaceId,
                Rating = NewRating,
                Comment = string.IsNullOrWhiteSpace(NewComment) ? null : NewComment,
                CreatedAt = DateTime.UtcNow
            };

            using (var connection = _dbContext.Database.GetDbConnection() as SqliteConnection)
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var reviewCheckCommand = connection.CreateCommand();
                        reviewCheckCommand.CommandText = @"
                            SELECT COUNT(*) 
                            FROM reviews 
                            WHERE user_id = $userId AND place_id = $placeId";
                        reviewCheckCommand.Parameters.AddWithValue("$userId", userId);
                        reviewCheckCommand.Parameters.AddWithValue("$placeId", SelectedPlace.PlaceId);

                        var reviewCount = Convert.ToInt32(await reviewCheckCommand.ExecuteScalarAsync());
                        if (reviewCount > 0)
                        {
                            ErrorMessage = "Вы уже оставили отзыв на это место.";
                            return;
                        }

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

                        await LoadUserReviewsAsync();
                        HideReviewForm();
                        await DisplayAlert("Успех", "Отзыв успешно отправлен!", "OK");
                        Debug.WriteLine($"ReviewPage: Review submitted for placeId={SelectedPlace.PlaceId}, rating={NewRating}");
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