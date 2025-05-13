using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;

namespace RecreationBookingApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [RelayCommand]
    private async Task NavigateToPlacesCommand()
    {
        await Shell.Current.GoToAsync("//places");
    }
}