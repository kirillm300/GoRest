using RecreationBookingApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace RecreationBookingApp.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
        var services = MauiProgram.CreateMauiApp().Services;
        BindingContext = services.GetRequiredService<LoginViewModel>();
    }

    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}