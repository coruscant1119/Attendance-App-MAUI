using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using System.Text.Json;
using System.Net.Http;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Project;

public partial class CheckInPage : ContentPage, INotifyPropertyChanged
{
    public CheckInPage()
    {
        InitializeComponent();

        BindingContext = this;

        Location = "Locating...";
        StatusText = "Getting location...";
        StatusColor = Colors.Gray;
        DistanceText = "";
        IsCheckedIn = false; // ⭐ 初始化
    }

    // 
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadLocationPreview();
    }

    // 
    string location;
    public string Location
    {
        get => location;
        set { location = value; OnPropertyChanged(); }
    }

    string statusText;
    public string StatusText
    {
        get => statusText;
        set { statusText = value; OnPropertyChanged(); }
    }

    Color statusColor;
    public Color StatusColor
    {
        get => statusColor;
        set { statusColor = value; OnPropertyChanged(); }
    }

    string distanceText;
    public string DistanceText
    {
        get => distanceText;
        set { distanceText = value; OnPropertyChanged(); }
    }

    // 
    bool isCheckedIn;
    public bool IsCheckedIn
    {
        get => isCheckedIn;
        set { isCheckedIn = value; OnPropertyChanged(); }
    }

    // 
    private async Task LoadLocationPreview()
    {
        try
        {
            var permission = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (permission != PermissionStatus.Granted)
            {
                StatusText = "Permission denied";
                StatusColor = Colors.Gray;
                return;
            }

            var location = await Geolocation.GetLocationAsync(
                new GeolocationRequest(GeolocationAccuracy.Medium));

            if (location == null)
            {
                StatusText = "Unable to get location";
                StatusColor = Colors.Gray;
                return;
            }

            double lat = location.Latitude;
            double lon = location.Longitude;

            double targetLat = 34.10542076083684;
            double targetLon = 108.88559091348887;

            double distance = CalculateDistance(lat, lon, targetLat, targetLon);

            string address = await GetAddressFromApi(lat, lon);
            string finalAddress = NormalizeAddress(address);

            Location = finalAddress;
            DistanceText = $"Distance: {distance:F0}m";

            if (distance <= 150)
            {
                StatusText = "Within allowed check-in range";
                StatusColor = Colors.Green;
            }
            else
            {
                StatusText = "Outside allowed range";
                StatusColor = Colors.Red;
            }
        }
        catch
        {
            StatusText = "Location error";
            StatusColor = Colors.Gray;
        }
    }

    // ⭐ 点击签到
    private async void OnConfirmCheckInClicked(object sender, EventArgs e)
    {
        if (IsCheckedIn) return; // 

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

            string rawAddress = await GetAddressFromApi(lat, lon);
            string finalAddress = NormalizeAddress(rawAddress);

            Location = finalAddress;
            DistanceText = $"Distance: {distance:F0}m";

            if (distance <= 150)
            {
                StatusText = "✔ Checked in successfully";
                StatusColor = Colors.Green;

                IsCheckedIn = true; // 

                await DisplayAlert("Success",
                    $"📍 {finalAddress}\nDistance: {distance:F0}m\n\nCheck-in successful!",
                    "OK");

                SaveRecord("Present", finalAddress);
            }
            else
            {
                StatusText = "Outside allowed range";
                StatusColor = Colors.Red;

                await DisplayAlert("Failed",
                    $"📍 {finalAddress}\nDistance: {distance:F0}m\n\nYou are not in class area!",
                    "OK");

                SaveRecord("Absent", finalAddress);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    // ⭐ 地址统一
    string NormalizeAddress(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return "Campus Information Building";

        if (raw.Contains("信息") || raw.Contains("Information"))
            return "Campus Information Building";

        if (raw.Contains("图书馆"))
            return "Library";

        return raw;
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

    // ⭐ 高德API
    async Task<string> GetAddressFromApi(double lat, double lon)
    {
        try
        {
            string key = "9467862965fb0cae5bde79be831a3ccf";

            string url = $"https://restapi.amap.com/v3/geocode/regeo?location={lon},{lat}&key={key}";

            using var client = new HttpClient();

            var response = await client.GetStringAsync(url);

            var json = JsonDocument.Parse(response);
            var root = json.RootElement;

            if (!root.TryGetProperty("regeocode", out var regeocode))
                return null;

            if (!regeocode.TryGetProperty("formatted_address", out var addr))
                return null;

            return addr.GetString();
        }
        catch
        {
            return null;
        }
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
}