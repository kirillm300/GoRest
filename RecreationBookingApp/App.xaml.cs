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

            // Проверяем, существует ли файл базы данных
            if (!File.Exists(dbPath))
            {
                Debug.WriteLine($"App: Файл базы данных {dbPath} не найден. Копируем из ресурсов...");

                // Копируем базу данных из ресурсов только если файла нет
                using var srcStream = await FileSystem.OpenAppPackageFileAsync(dbName);
                using var destStream = File.Create(dbPath);
                await srcStream.CopyToAsync(destStream);
                Debug.WriteLine($"App: База данных {dbName} скопирована из ресурсов.");
            }
            else
            {
                Debug.WriteLine($"App: Файл базы данных {dbPath} уже существует. Используем существующий файл.");
            }

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