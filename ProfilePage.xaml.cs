using Microsoft.Maui.Controls;

namespace Project
{
    public partial class ProfilePage : ContentPage
    {
        public ProfilePage()
        {
            InitializeComponent();
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Logout", "Are you sure you want to logout?", "Yes", "No");
            if (!confirm) return;

            
            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }
    }
}