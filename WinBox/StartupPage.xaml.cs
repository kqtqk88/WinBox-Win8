using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Tasks;
using WinBox.Resources;

namespace WinBox
{
    public partial class StartupPage
    {
        public StartupPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            
            while (NavigationService.CanGoBack)
            {
                NavigationService.RemoveBackEntry();
            }

            IDictionary<string, string> qs = NavigationContext.QueryString;

            if (CanNavigateToMainScreen(qs))
            {
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                return;
            }

            const string location = "/UploadPage.xaml?FileId={0}&path=/";

            //Share
            if (qs.ContainsKey("FileId"))
            {
                string loc = string.Format(location, qs["FileId"]);
                NavigationService.Navigate(new Uri(loc, UriKind.Relative));
            }

            //Apps
            if (qs.ContainsKey("token"))
            {
                string loc = string.Format(location, qs["token"]);
                NavigationService.Navigate(new Uri(loc, UriKind.Relative));
            }

        }

        static bool CanNavigateToMainScreen(IDictionary<string, string> qs)
        {
            return App.UserLogin != null && !qs.ContainsKey("token") && !qs.ContainsKey("FileId");
        }

        private void _login_Click(object sender, RoutedEventArgs e)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                MessageBox.Show(Labels.ConnectionErrorMessage, Labels.ConnectionErrorTitle, MessageBoxButton.OK);
                return;
            }

            NavigationService.Navigate(new Uri("/LoginView.xaml", UriKind.Relative));
        }

        private void _createAccount_Click(object sender, RoutedEventArgs e)
        {
            var web = new WebBrowserTask {Uri = new Uri("http://db.tt/Du53F7jS")};
            web.Show();
        }
    }
}