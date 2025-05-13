using RecreationBookingApp.ViewModels;
namespace RecreationBookingApp.Views;

public partial class PlacesPage : ContentPage
{
    public PlacesPage(PlacesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PlacesViewModel viewModel)
        {
            viewModel.LoadPlacesCommand.Execute(null);
        }
    }
}