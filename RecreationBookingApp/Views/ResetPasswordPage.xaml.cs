using RecreationBookingApp.ViewModels;

namespace RecreationBookingApp.Views;

public partial class ResetPasswordPage : ContentPage
{
	public ResetPasswordPage()
	{
		InitializeComponent();
        BindingContext = new ResetPasswordViewModel();
    }
}
