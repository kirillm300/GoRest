using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls;
using RecreationBookingApp.Data;
using RecreationBookingApp.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

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

public partial class ReviewPage : ContentPage
{
    private readonly AppDbContext _dbContext;
    private string _errorMessage;

    public ObservableCollection<ReviewViewModel> UserReviews { get; } = new ObservableCollection<ReviewViewModel>();

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
        InitializeComponent();
        BindingContext = this;
        LoadUserReviewsAsync();
    }

    private async void LoadUserReviewsAsync()
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

                // Загружаем отзывы пользователя
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
}