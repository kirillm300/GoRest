using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RecreationBookingApp.Data;
using RecreationBookingApp.Models;
using RecreationBookingApp.Repositories;
using RecreationBookingApp.Services;
using RecreationBookingApp.ViewModels;
using RecreationBookingApp.Views;

namespace RecreationBookingApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Настройка базы данных SQLite
        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "gorest_db.db");
        string connectionString = $"Data Source={dbPath};Foreign Keys=True";
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString)
                   .LogTo(Console.WriteLine, LogLevel.Information));

        // Регистрация строки подключения как сервиса
        builder.Services.AddSingleton(connectionString);

        // Регистрация репозиториев
        builder.Services.AddScoped<IRepository<User>, Repository<User>>();
        builder.Services.AddScoped<IPlaceRepository, PlaceRepository>();
        builder.Services.AddScoped<IRepository<Place>, Repository<Place>>();
        builder.Services.AddScoped<IRepository<Booking>, Repository<Booking>>();
        builder.Services.AddScoped<IRepository<Booking>, BookingRepository>();

        // Регистрация сервисов
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IPlaceRepository, PlaceRepository>();

        // Регистрация ViewModels и Views
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<PlacesViewModel>();
        builder.Services.AddTransient<PlacesPage>();
        builder.Services.AddTransient<PlaceDetailViewModel>();
        builder.Services.AddTransient<PlaceDetailPage>();
        builder.Services.AddTransient<ReviewPage>();
        builder.Services.AddTransient<OwnerBookingsViewModel>();
        builder.Services.AddTransient<OwnerBookingsPage>(s =>
        {
            var dbContext = s.GetRequiredService<AppDbContext>();
            return new OwnerBookingsPage(dbContext);
        });

        builder.Services.AddSingleton<App>();

        return builder.Build();
    }
}