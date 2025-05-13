using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RecreationBookingApp.Models;
using RecreationBookingApp.Repositories;
using RecreationBookingApp.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace RecreationBookingApp.ViewModels;

public partial class PlaceDetailViewModel : ObservableObject
{
    private readonly IPlaceRepository _placeRepository;
    private readonly IUserService _userService;
    private readonly IRepository<Booking> _bookingRepository;
    private readonly Place _place;
    private readonly string _connectionString;

    [ObservableProperty]
    private string errorMessage;

    [ObservableProperty]
    private ObservableCollection<string> categories;

    [ObservableProperty]
    private ObservableCollection<string> images;

    [ObservableProperty]
    private ObservableCollection<string> rooms;

    [ObservableProperty]
    private ObservableCollection<string> roomFeatures;

    public Place Place => _place;

    public PlaceDetailViewModel(IPlaceRepository placeRepository, IUserService userService, IRepository<Booking> bookingRepository, Place place, string connectionString)
    {
        _placeRepository = placeRepository;
        _userService = userService;
        _bookingRepository = bookingRepository;
        _place = place ?? throw new ArgumentNullException(nameof(place));
        _connectionString = connectionString;

        // Добавление тестовых данных
        Categories = new ObservableCollection<string> { "Тестовая категория 1", "Тестовая категория 2" };
        Images = new ObservableCollection<string> { "http://example.com/image1.jpg", "http://example.com/image2.jpg" };
        Rooms = new ObservableCollection<string> { "Тестовая комната 1", "Тестовая комната 2" };
        RoomFeatures = new ObservableCollection<string> { "Тестовая особенность 1", "Тестовая особенность 2" };

        Debug.WriteLine($"PlaceDetailViewModel initialized for placeId={_place.PlaceId}");
        Task.Run(async () => await LoadPlaceDetailsAsync());
    }

    private async Task LoadPlaceDetailsAsync()
    {
        Debug.WriteLine($"Starting LoadPlaceDetailsAsync for placeId={_place.PlaceId}");
        try
        {
            ErrorMessage = string.Empty;

            // Получаем полные данные через репозиторий
            var fullPlace = await _placeRepository.GetFullPlaceAsync(_place.PlaceId);
            if (fullPlace == null)
            {
                ErrorMessage = "Место не найдено.";
                Debug.WriteLine("Full place data is null.");
                return;
            }

            // Загружаем категории
            Categories.Clear();
            if (fullPlace.Category != null)
            {
                Categories.Add(fullPlace.Category.Name);
            }
            Debug.WriteLine($"Loaded {Categories.Count} categories.");

            // Загружаем изображения
            Images.Clear();
            if (fullPlace.Images != null)
            {
                foreach (var image in fullPlace.Images)
                {
                    Images.Add(image);
                }
            }
            Debug.WriteLine($"Loaded {Images.Count} images.");

            // Загружаем комнаты
            Rooms.Clear();
            if (fullPlace.Rooms != null)
            {
                foreach (var room in fullPlace.Rooms)
                {
                    Rooms.Add(room.Name);
                }
            }
            Debug.WriteLine($"Loaded {Rooms.Count} rooms.");

            // Загружаем особенности комнат
            RoomFeatures.Clear();
            if (fullPlace.Rooms != null)
            {
                foreach (var room in fullPlace.Rooms)
                {
                    if (room.Features != null)
                    {
                        foreach (var feature in room.Features)
                        {
                            if (!string.IsNullOrEmpty(feature.FeatureName))
                            {
                                RoomFeatures.Add(feature.FeatureName);
                            }
                        }
                    }
                }
            }
            Debug.WriteLine($"Loaded {RoomFeatures.Count} room features.");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка загрузки данных: {ex.Message}";
            Debug.WriteLine($"Error in LoadPlaceDetailsAsync: {ex.Message}");
        }
        Debug.WriteLine("LoadPlaceDetailsAsync completed.");
    }

    [RelayCommand]
    private async Task BookPlaceAsync()
    {
        if (string.IsNullOrWhiteSpace(Preferences.Get("UserId", null)))
        {
            ErrorMessage = "Пожалуйста, войдите в аккаунт для бронирования.";
            return;
        }

        try
        {
            ErrorMessage = string.Empty;
            var userId = Preferences.Get("UserId", null);
            var booking = new Booking
            {
                BookingId = Guid.NewGuid().ToString(),
                UserId = userId,
                PlaceId = _place.PlaceId,
                ScheduleId = null,
                PromocodeId = null,
                Status = "pending",
                TotalPrice = 0m,
                PeopleCount = 1,
                PaymentStatus = "unpaid",
                CreatedAt = DateTime.UtcNow
            };

            await _bookingRepository.AddAsync(booking);

            await Shell.Current.DisplayAlert("Успех", "Бронирование успешно создано!", "OK");
            await Shell.Current.Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка при бронировании: {ex.Message}";
        }
    }
}