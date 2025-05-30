using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using RecreationBookingApp.Services;
using System.Threading.Tasks;
using System.Diagnostics;
using RecreationBookingApp.Models;

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

    public ResetPasswordViewModel(IUserService userService)
    {
        _userService = userService;
        IsEmailVerified = false;
        IsBusy = false;
    }

    partial void OnEmailChanged(string value)
    {
        Debug.WriteLine($"Email changed to: {value}, CanVerifyEmail: {CanVerifyEmail()}");
        VerifyEmailCommand?.NotifyCanExecuteChanged();
    }

    partial void OnNewPasswordChanged(string value)
    {
        Debug.WriteLine($"NewPassword changed to: {value}, CanResetPassword: {CanResetPassword()}");
        ResetPasswordCommand?.NotifyCanExecuteChanged(); // Уведомляем команду об изменении
    }

    [RelayCommand(CanExecute = nameof(CanVerifyEmail))]
    private async Task VerifyEmailAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            Debug.WriteLine($"Verifying email: {Email}");
            var user = await _userService.GetUserByEmailAsync(Email);
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
        bool canExecute = !IsBusy && !string.IsNullOrWhiteSpace(Email);
        Debug.WriteLine($"CanVerifyEmail: IsBusy={IsBusy}, Email='{Email}', Result={canExecute}");
        return canExecute;
    }

    [RelayCommand(CanExecute = nameof(CanResetPassword))]
    private async Task ResetPasswordAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            await _userService.UpdatePasswordAsync(Email, NewPassword);

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
        bool canExecute = !IsBusy && IsEmailVerified && !string.IsNullOrWhiteSpace(NewPassword);
        Debug.WriteLine($"CanResetPassword: IsBusy={IsBusy}, IsEmailVerified={IsEmailVerified}, NewPassword='{NewPassword}', Result={canExecute}");
        return canExecute;
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Application.Current.MainPage.Navigation.PopModalAsync();
    }
}