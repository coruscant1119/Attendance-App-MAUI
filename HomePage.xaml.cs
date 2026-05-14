using Microsoft.Maui.Storage;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.ApplicationModel;
namespace Project;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        
        if (!Preferences.Get("isLoggedIn", false))
        {
            await DisplayAlert("Error", "Please login first", "OK");

            Application.Current.MainPage = new NavigationPage(new LoginPage());
            return;
        }

       
        string user = Preferences.Get("currentUser", "Guest");
        WelcomeLabel.Text = $"👋 Welcome, {user}";
    }

    // 👉 Check-in
    

private async void OnCheckInClicked(object sender, EventArgs e)
{
    try
    {
        
        var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

        if (status != PermissionStatus.Granted)
        {
            await DisplayAlert("Error", "Location permission denied", "OK");
            return;
        }

       
        var location = await Geolocation.GetLastKnownLocationAsync();

        if (location == null)
        {
            location = await Geolocation.GetLocationAsync(
                new GeolocationRequest(GeolocationAccuracy.High));
        }

        if (location != null)
        {
            double latitude = location.Latitude;
            double longitude = location.Longitude;

            
            await DisplayAlert("Your Location",
                $"Lat: {latitude}\nLon: {longitude}",
                "OK");

            
            double schoolLat = 34.10542076083684;
            double schoolLon = 108.88559091348887;

           
            double distance = Math.Sqrt(
                Math.Pow(latitude - schoolLat, 2) +
                Math.Pow(longitude - schoolLon, 2)
            );

            if (distance < 0.01)
            {
                await DisplayAlert("Success", "Check-in successful!", "OK");
            }
            else
            {
                await DisplayAlert("Failed", "You are not in campus area", "OK");
            }
        }
        else
        {
            await DisplayAlert("Error", "Unable to get location", "OK");
        }
    }
    catch (Exception ex)
    {
        await DisplayAlert("Error", ex.Message, "OK");
    }
}

//  History
private async void OnHistoryClicked(object sender, EventArgs e)
    {
        await DisplayAlert("History", "View attendance history", "OK");
    }

    //  Profile
    private async void OnProfileClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Profile", "User profile page", "OK");
    }
}