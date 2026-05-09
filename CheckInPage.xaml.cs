namespace Project;

public partial class CheckInPage : ContentPage
{
	public CheckInPage()
	{
		InitializeComponent();
	}
    private async void OnConfirmCheckInClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Check-in Successful",
            "Your attendance has been recorded successfully.",
            "OK");
    }
}