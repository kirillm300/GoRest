using Microsoft.Maui.Controls;
using RecreationBookingApp.Data;
using RecreationBookingApp.ViewModels;

namespace RecreationBookingApp.Views;

public partial class OwnerBookingsPage : ContentPage
{
    public OwnerBookingsPage(AppDbContext dbContext)
    {
        InitializeComponent();
        BindingContext = new OwnerBookingsViewModel(dbContext);
    }

    public OwnerBookingsPage()
    {
        InitializeComponent();
    }
}