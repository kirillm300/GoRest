using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using RecreationBookingApp.Services;
using System.Threading.Tasks;

namespace RecreationBookingApp.ViewModels;

public partial class ResetPasswordViewModel : ObservableObject
{
    private readonly IUserService _userService;

    [ObservableProperty]
    private string email;

    [ObservableProperty]
    private string newPassword;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string errorMessage;

    [ObservableProperty]
    private bool isEmailVerified;

    public ResetPasswordViewModel()
    {
        // Получаем IUserService через DI (предполагается, что он зарегистрирован)
        _userService = Application.Current.MainPage.Handler.MauiContext.Services.GetService<IUserService>();
        IsEmailVerified = false;
    }

    [RelayCommand(CanExecute = nameof(CanVerifyEmail))]
    private async Task VerifyEmailAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            // Проверяем, существует ли пользователь с таким email
            var user = await _userService.LoginAsync(Email, ""); // Используем пустой пароль, чтобы просто проверить email
            if (user == null)
            {
                ErrorMessage = "Пользователь с таким email не найден.";
                return;
            }

            IsEmailVerified = true;
            ErrorMessage = "Email найден. Введите новый пароль.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка проверки email: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanVerifyEmail()
    {
        return !IsBusy && !string.IsNullOrWhiteSpace(Email);
    }

    [RelayCommand(CanExecute = nameof(CanResetPassword))]
    private async Task ResetPasswordAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            // Обновляем пароль
            await _userService.UpdatePasswordAsync(Email, NewPassword);

            // Закрываем модальное окно
            await Application.Current.MainPage.Navigation.PopModalAsync();
            await Application.Current.MainPage.DisplayAlert("Успех", "Пароль успешно сброшен. Войдите с новым паролем.", "OK");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка сброса пароля: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanResetPassword()
    {
        return !IsBusy && IsEmailVerified && !string.IsNullOrWhiteSpace(NewPassword);
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Application.Current.MainPage.Navigation.PopModalAsync();
    }
}