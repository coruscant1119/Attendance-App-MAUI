
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Storage;

namespace Project;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    List<User> users = new List<User>
    {
        new User { Username = "iris", Password = "123456" },
        new User { Username = "admin", Password = "admin" }
    };

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string username = UsernameEntry.Text?.Trim();
        string password = PasswordEntry.Text?.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            await DisplayAlert("Error", "Please enter Student ID and Password", "OK");
            return;
        }

        var user = users.FirstOrDefault(u =>
            u.Username == username && u.Password == password);

        if (user != null)
        {
            Preferences.Set("isLoggedIn", true);
            Preferences.Set("currentUser", username);

            await DisplayAlert("Success", "Login successful", "OK");

            Application.Current.MainPage = new AppShell();
        }
        else
        {
            await DisplayAlert("Error", "Invalid Student ID or Password", "OK");
        }
    }
}