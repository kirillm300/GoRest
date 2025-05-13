using RecreationBookingApp.ViewModels;

namespace RecreationBookingApp.Views;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        BindingContext = new MainViewModel(); // Инициализация ViewModel
    }
}