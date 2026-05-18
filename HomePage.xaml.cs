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
            Application.Current.MainPage = new NavigationPage(new LoginPage());
            return;
        }

        string user = Preferences.Get("currentUser", "Guest");

        BindingContext = new UserViewModel
        {
            Username = $"👋 Welcome, {user}"
        };
    }

    // ⭐  Check-in
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

                // ⭐ API地址（辅助）
                string apiAddress = await GetAddressFromApi(latitude, longitude);

                // ⭐ 信息大厦坐标（主）
                double infoLat = 34.10542076083684;
                double infoLon = 108.88559091348887;

                // ⭐ 图书馆坐标（辅助）
                double libLat = 34.1048;
                double libLon = 108.8852;

                // ⭐ 距离计算
                double dInfo = CalculateDistance(latitude, longitude, infoLat, infoLon);
                double dLib = CalculateDistance(latitude, longitude, libLat, libLon);

                double minDistance = Math.Min(dInfo, dLib);

                // ⭐ 优先判断区域
                string finalArea;

                if (dInfo <= 400)
                    finalArea = "Campus Information Building";
                else if (dLib <= 200)
                    finalArea = "RiXin Building";
                else
                    finalArea = "Outside Campus";

                // ⭐ 显示
                await DisplayAlert("Your Location",
                    $"📍 Detected Area: {finalArea}\n" +
                    $"📍 API Address: {apiAddress}\n\n" +
                    $"Distance: {minDistance:F2} meters",
                    "OK");

                // ⭐ 签到判断
                if (minDistance <= 400)
                {
                    await DisplayAlert("Success",
                        $"✔ Check-in successful\n📍 {finalArea}",
                        "OK");

                    SaveRecord("Present", $"{finalArea} ({apiAddress})");
                }
                else
                {
                    await DisplayAlert("Failed",
                        $"✖ Not in campus area\nDistance: {minDistance:F2}m",
                        "OK");

                    SaveRecord("Absent", apiAddress);
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
        var list = JsonSerializer.Deserialize<List<AttendanceRecord>>(json) ?? new List<AttendanceRecord>();

        list.Add(record);

        Preferences.Set("records", JsonSerializer.Serialize(list));
    }

    // ⭐ 高德API
    async Task<string> GetAddressFromApi(double lat, double lon)
    {
        try
        {
            string apiKey = "9467862965fb0cae5bde79be831a3ccf"; 

            string url = $"https://restapi.amap.com/v3/geocode/regeo?location={lon},{lat}&key={apiKey}";

            using var client = new HttpClient();

            var response = await client.GetStringAsync(url);

            var json = JsonDocument.Parse(response);

            var address = json.RootElement
                .GetProperty("regeocode")
                .GetProperty("formatted_address")
                .GetString();

            return address ?? "Unknown location";
        }
        catch
        {
            return "Location unavailable";
        }
    }

    // 👉 History
    private async void OnHistoryClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AttendanceHistoryPage());
    }

    // 👉 Profile
    private async void OnProfileClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Profile", "User profile page", "OK");
    }
}