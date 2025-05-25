using Microsoft.Maui.Storage;
using RecreationBookingApp.Data;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace RecreationBookingApp;

public partial class App : Application
{
    public App(AppDbContext dbContext)
    {
        InitializeComponent();

        Debug.WriteLine("App: Начало инициализации...");

        Task.Run(async () =>
        {
            await InitializeDatabaseAsync(dbContext);
        }).GetAwaiter().GetResult();

        Debug.WriteLine("App: Установка MainPage...");
        MainPage = new AppShell();
        Debug.WriteLine("App: Инициализация завершена.");
    }

    private async Task InitializeDatabaseAsync(AppDbContext dbContext)
    {
        Debug.WriteLine("App: Начало инициализации базы данных...");

        try
        {
            string dbName = "gorest_db.db";
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, dbName);

            // Копируем базу данных из ресурсов при каждом запуске
            Debug.WriteLine($"App: Копирование базы данных {dbName} из ресурсов...");
            using var srcStream = await FileSystem.OpenAppPackageFileAsync(dbName);
            using var destStream = File.Create(dbPath);
            await srcStream.CopyToAsync(destStream);
            Debug.WriteLine($"App: База данных {dbName} скопирована из ресурсов.");

            await dbContext.Database.EnsureCreatedAsync();
            Debug.WriteLine("App: Подключение к базе данных успешно.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"App: Ошибка при инициализации базы данных: {ex.Message}");
            throw;
        }
    }
}