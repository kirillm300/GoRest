using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RecreationBookingApp.Models;
using RecreationBookingApp.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using RecreationBookingApp.Views;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace RecreationBookingApp.ViewModels;

public partial class PlacesViewModel : ObservableObject
{
    private readonly IPlaceRepository _placeRepository;
    private List<Category> _cachedCategories;

    [ObservableProperty]
    private ObservableCollection<Place> places;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string errorMessage;

    [ObservableProperty]
    private string selectedStatusFilter;

    [ObservableProperty]
    private ObservableCollection<string> statusFilters;

    [ObservableProperty]
    private string selectedCategoryFilter;

    [ObservableProperty]
    private ObservableCollection<string> categoryFilters;

    [ObservableProperty]
    private string selectedCategoryId;

    [ObservableProperty]
    private Place selectedPlace;

    public PlacesViewModel(IPlaceRepository placeRepository)
    {
        _placeRepository = placeRepository;
        Places = new ObservableCollection<Place>();
        StatusFilters = new ObservableCollection<string> { "All", "active", "pending", "archived" };
        CategoryFilters = new ObservableCollection<string> { "All" };
        SelectedStatusFilter = "All";
        SelectedCategoryFilter = "All";
        _cachedCategories = new List<Category>();
        InitializeAsync().GetAwaiter().GetResult();
    }

    private async Task InitializeAsync()
    {
        await LoadCategoriesAsync();
        await LoadPlacesAsync();
    }

    partial void OnSelectedStatusFilterChanged(string oldValue, string newValue)
    {
        if (oldValue != newValue)
        {
            LoadPlacesAsync();
        }
    }

    partial void OnSelectedCategoryFilterChanged(string oldValue, string newValue)
    {
        if (oldValue != newValue)
        {
            LoadPlacesAsync();
        }
    }

    [RelayCommand]
    private async Task LoadCategoriesAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var categories = await _placeRepository.GetCategoriesAsync();
            _cachedCategories.Clear();
            _cachedCategories.AddRange(categories);
            Debug.WriteLine($"PlacesViewModel: Fetched {_cachedCategories.Count} categories from repository.");

            CategoryFilters.Clear();
            CategoryFilters.Add("All");
            foreach (var category in _cachedCategories)
            {
                if (!string.IsNullOrWhiteSpace(category.Name))
                {
                    CategoryFilters.Add(category.Name);
                    Debug.WriteLine($"PlacesViewModel: Added category to filters: {category.Name} (ID: {category.CategoryId})");
                }
                else
                {
                    Debug.WriteLine($"PlacesViewModel: Skipping category with ID {category.CategoryId} due to empty Name.");
                }
            }
            Debug.WriteLine($"PlacesViewModel: Total categories in filter: {CategoryFilters.Count}");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка загрузки категорий: {ex.Message}";
            Debug.WriteLine($"PlacesViewModel: Error loading categories: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoadPlacesAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            if (!_cachedCategories.Any())
            {
                Debug.WriteLine("PlacesViewModel: Categories not loaded yet, loading now...");
                await LoadCategoriesAsync();
            }

            var placeList = await _placeRepository.GetAllAsync();
            if (placeList == null)
            {
                Debug.WriteLine("PlacesViewModel: GetAllAsync returned null, initializing empty list.");
                placeList = new List<Place>();
            }
            Places.Clear();

            var filteredPlaces = placeList.AsQueryable();
            if (SelectedStatusFilter != "All")
            {
                filteredPlaces = filteredPlaces?.Where(p => p.Status == SelectedStatusFilter) ?? new List<Place>().AsQueryable();
            }
            if (SelectedCategoryFilter != "All")
            {
                var category = _cachedCategories.FirstOrDefault(c => c.Name == SelectedCategoryFilter);
                if (category != null)
                {
                    SelectedCategoryId = category.CategoryId;
                    filteredPlaces = filteredPlaces?.Where(p => p.CategoryId == SelectedCategoryId) ?? new List<Place>().AsQueryable();
                    Debug.WriteLine($"PlacesViewModel: Filtering places by category: {SelectedCategoryFilter} (ID: {SelectedCategoryId})");
                }
                else
                {
                    Debug.WriteLine($"PlacesViewModel: Category {SelectedCategoryFilter} not found in cached categories.");
                }
            }

            var placesList = filteredPlaces?.ToList() ?? new List<Place>();

            foreach (var place in placesList)
            {
                place.AverageRating = await _placeRepository.GetAverageRatingAsync(place.PlaceId);
                Debug.WriteLine($"PlacesViewModel: Fetched average rating for placeId={place.PlaceId}: {place.AverageRating}");

                place.MainImageUrl = await _placeRepository.GetMainImageUrlAsync(place.PlaceId);
                Debug.WriteLine($"PlacesViewModel: Fetched main image URL for placeId={place.PlaceId}: {place.MainImageUrl ?? "No main image"}");

                Places.Add(place);
            }

            Debug.WriteLine($"PlacesViewModel: Loaded {Places.Count} places after applying filters.");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка загрузки мест: {ex.Message}";
            Debug.WriteLine($"PlacesViewModel: Error loading places: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void RefreshPlaces()
    {
        LoadPlacesAsync();
    }

    [RelayCommand]
    private async Task PlaceSelected()
    {
        if (SelectedPlace == null)
        {
            Debug.WriteLine("PlaceSelected: SelectedPlace is null.");
            return;
        }

        Debug.WriteLine($"PlaceSelected: Navigating to PlaceDetailPage with placeId={SelectedPlace.PlaceId}");
        var navigationParameters = new Dictionary<string, object>
        {
            { "placeId", SelectedPlace.PlaceId }
        };
        await Shell.Current.GoToAsync($"{nameof(PlaceDetailPage)}", navigationParameters);
        SelectedPlace = null;
    }
}