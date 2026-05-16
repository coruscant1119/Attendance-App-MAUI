using Microsoft.Maui.Storage;

namespace Project
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // 判断是否已登录
            if (Preferences.Get("isLoggedIn", false))
            {
               
                return new Window(new AppShell());
            }
            else
            {
                return new Window(new NavigationPage(new LoginPage()));
            }
        }
    }
}