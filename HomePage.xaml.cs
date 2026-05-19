using Microsoft.Maui.Storage;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.ApplicationModel;
using System.Text.Json;
using System.Collections.Generic;
using System.Net.Http;

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
            await Shell.Current.GoToAsync("//LoginPage");
            return;
        }

        string user = Preferences.Get("currentUser", "Guest");

        BindingContext = new UserViewModel
        {
            Username = $"👋 Welcome, {user}"
        };
    }

    // ⭐⭐⭐ 快捷签到（不跳转 + 真实地址）
    private async void OnCheckInClicked(object sender, EventArgs e)
    {
        try
        {
            var permission = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (permission != PermissionStatus.Granted)
            {
                await DisplayAlert("Error", "Location permission denied", "OK");
                return;
            }

            var location = await Geolocation.GetLocationAsync(
                new GeolocationRequest(GeolocationAccuracy.High));

            if (location == null)
            {
                await DisplayAlert("Error", "Unable to get location", "OK");
                return;
            }

            double lat = location.Latitude;
            double lon = location.Longitude;

            double targetLat = 34.10542076083684;
            double targetLon = 108.88559091348887;

            double distance = CalculateDistance(lat, lon, targetLat, targetLon);

            // ⭐ 获取真实地址（关键修复点）
            string address = await GetAddressFromApi(lat, lon);

            if (distance <= 150)
            {
                await DisplayAlert("Success",
                    $"✔ Quick Check-in successful\n📍 {address}",
                    "OK");

                SaveRecord("Present", address);
            }
            else
            {
                await DisplayAlert("Failed",
                    $"✖ Not in class area\n📍 {address}\nDistance: {distance:F0}m",
                    "OK");

                SaveRecord("Absent", address);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    // ⭐ History（Shell跳转）
    private async void OnHistoryClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//HistoryPage");
    }

    // ⭐ Profile
    private async void OnProfileClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//ProfilePage");
    }

    // ⭐ 距离计算
    double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var R = 6371000;

        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;

        var a =
            Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(lat1 * Math.PI / 180) *
            Math.Cos(lat2 * Math.PI / 180) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }

    // ⭐ 保存记录
    void SaveRecord(string status, string address)
    {
        var username = Preferences.Get("currentUser", "Unknown");

        var record = new AttendanceRecord
        {
            Username = username,
            CourseName = "Mobile Application Development",
            Time = DateTime.Now.ToString("dd MMM yyyy • HH:mm"),
            Status = status,
            Location = address
        };

        var json = Preferences.Get("records", "[]");

        var list = JsonSerializer.Deserialize<List<AttendanceRecord>>(json)
                   ?? new List<AttendanceRecord>();

        list.Add(record);

        Preferences.Set("records", JsonSerializer.Serialize(list));
    }

    // ⭐ 高德API（真实地址）
    async Task<string> GetAddressFromApi(double lat, double lon)
    {
        try
        {
            string apiKey = "9467862965fb0cae5bde79be831a3ccf";

            string url = $"https://restapi.amap.com/v3/geocode/regeo?location={lon},{lat}&key={apiKey}";

            using var client = new HttpClient();

            var response = await client.GetStringAsync(url);

            var json = JsonDocument.Parse(response);

            return json.RootElement
                .GetProperty("regeocode")
                .GetProperty("formatted_address")
                .GetString() ?? "Unknown location";
        }
        catch
        {
            return "Location unavailable";
        }
    }
}