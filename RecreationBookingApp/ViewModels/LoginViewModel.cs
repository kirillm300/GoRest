using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RecreationBookingApp.Services;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace RecreationBookingApp.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IUserService _userService;

    [ObservableProperty]
    private string email;

    [ObservableProperty]
    private string password;

    [ObservableProperty]
    private string fullName;

    [ObservableProperty]
    private string phone;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string errorMessage;

    [ObservableProperty]
    private bool isRegisterMode;

    public string ToggleModeText => IsRegisterMode ? "Перейти ко входу" : "Перейти к регистрации";

    public LoginViewModel(IUserService userService)
    {
        _userService = userService;
        IsRegisterMode = false;
    }

    partial void OnEmailChanged(string value)
    {
        LoginCommand.NotifyCanExecuteChanged();
        RegisterCommand.NotifyCanExecuteChanged();
    }

    partial void OnPasswordChanged(string value)
    {
        LoginCommand.NotifyCanExecuteChanged();
        RegisterCommand.NotifyCanExecuteChanged();
    }

    partial void OnFullNameChanged(string value) => RegisterCommand.NotifyCanExecuteChanged();
    partial void OnPhoneChanged(string value) => RegisterCommand.NotifyCanExecuteChanged();
    partial void OnIsBusyChanged(bool value) => RegisterCommand.NotifyCanExecuteChanged();

    [RelayCommand(CanExecute = nameof(CanExecuteLogin))]
    private async Task LoginAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var user = await _userService.LoginAsync(Email, Password);
            if (user == null)
            {
                ErrorMessage = "Неправильный email или пароль.";
                return;
            }

            Preferences.Set("UserId", user.UserId);
            await Shell.Current.GoToAsync("//main");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка при входе: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanExecuteLogin()
    {
        return !IsBusy && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
    }

    [RelayCommand(CanExecute = nameof(CanExecuteRegister))]
    private async Task RegisterAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var user = new Models.User
            {
                UserId = Guid.NewGuid().ToString(),
                Email = Email,
                FullName = FullName,
                Phone = Phone,
                Role = "client",
                CreatedAt = DateTime.UtcNow,
                Verified = false
            };

            var success = await _userService.RegisterUserAsync(user, Password);
            if (!success)
            {
                ErrorMessage = "Пользователь с таким email уже существует.";
                return;
            }

            await Shell.Current.GoToAsync("//main");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanExecuteRegister()
    {
        return !IsBusy && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password) &&
               !string.IsNullOrWhiteSpace(FullName) && !string.IsNullOrWhiteSpace(Phone);
    }

    [RelayCommand]
    private void ToggleMode()
    {
        IsRegisterMode = !IsRegisterMode;
        ErrorMessage = string.Empty;
        Email = string.Empty;
        Password = string.Empty;
        FullName = string.Empty;
        Phone = string.Empty;
    }
}