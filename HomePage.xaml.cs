using Microsoft.Maui.Storage;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.ApplicationModel;
using System.Text.Json;
using System.Collections.Generic;

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

        // ⭐ Data Binding
        BindingContext = new UserViewModel
        {
            Username = $"👋 Welcome, {user}"
        };
    }

    // ⭐ Check-in（升级版）
    private async void OnCheckInClicked(object sender, EventArgs e)
    {
        try
        {
            // 权限
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Error", "Location permission denied", "OK");
                return;
            }

            // 获取位置
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

                double schoolLat = 34.10542076083684;
                double schoolLon = 108.88559091348887;

                // ⭐ 用真实距离（米）
                double distance = CalculateDistance(latitude, longitude, schoolLat, schoolLon);

                await DisplayAlert("Your Location",
                    $"Lat: {latitude}\nLon: {longitude}\nDistance: {distance:F2} meters",
                    "OK");

                if (distance <= 150) // 150米范围
                {
                    await DisplayAlert("Success", "Check-in successful!", "OK");

                    // ⭐ 保存成功记录
                    SaveRecord("Success");
                }
                else
                {
                    await DisplayAlert("Failed", "You are not in campus area", "OK");

                    // ⭐ 保存失败记录
                    SaveRecord("Failed");
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

    // ⭐ 距离函数
    double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var R = 6371000; // 地球半径（米）

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
    void SaveRecord(string status)
    {
        var username = Preferences.Get("currentUser", "Unknown");

        var record = new AttendanceRecord
        {
            Username = username,
            CourseName = "Mobile Application Development", // ⭐ 课程名
            Time = DateTime.Now.ToString("dd MMM yyyy • HH:mm"),
            Status = status
        };

        var json = Preferences.Get("records", "[]");
        var list = JsonSerializer.Deserialize<List<AttendanceRecord>>(json);

        list.Add(record);

        Preferences.Set("records", JsonSerializer.Serialize(list));
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