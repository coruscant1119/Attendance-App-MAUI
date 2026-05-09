namespace Project;

public partial class HomePage : ContentPage
{
	public HomePage ()
	{
		InitializeComponent();
	}
    private async void OnHistoryClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AttendanceHistoryPage());
    }
    private async void OnCheckInClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CheckInPage());

    }
    private async void OnProfileClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ProfilePage());
    }
}