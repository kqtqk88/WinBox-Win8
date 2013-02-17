using System;
using System.Windows;
using DropNet;
using DropNet.Models;
using Microsoft.Phone.Controls;
using System.IO.IsolatedStorage;
using WinBox.Resources;
using WinBox.Utility;

namespace WinBox
{
    public partial class LoginView
    {
        public LoginView()
        {
            InitializeComponent();
            
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _browser.Navigating += _browser_Navigating;
            _browser.Navigated += _browser_Navigated;

            
            _client.GetTokenAsync(userLogin =>
                                      {
                                          _userLogin = userLogin;
                                          const string redirectUrl = "http://" + RedirectUrl;
                                          var url = _client.BuildAuthorizeUrl(_userLogin, redirectUrl);
                                          _browser.Navigate(new Uri(url));
                                      },
                                      error => MessageBox.Show(Labels.ConnectionErrorMessage, Labels.ConnectionErrorTitle, MessageBoxButton.OK));
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            Utilities.ShowProgressIndicator(false);
            base.OnNavigatedFrom(e);
        }

        private void _browser_Navigating(object sender, NavigatingEventArgs e)
        {
            Utilities.ShowProgressIndicator(true, Labels.Loading);

            if (e.Uri.Host.Equals(RedirectUrl))
            {
                _client.GetAccessTokenAsync(
                    user =>
                    {
                        Utilities.ShowProgressIndicator(false);
                        SaveToken(user);

                        e.Cancel = true;
                        _browser.Navigating -= _browser_Navigating;
                        _browser.Navigated -= _browser_Navigated;

                        NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                    },
                    err => MessageBox.Show(Labels.LoginNotPossible, Labels.ErrorTitle, MessageBoxButton.OK));
            }
        }

        private static void SaveToken(UserLogin user)
        {
            var appSettings = IsolatedStorageSettings.ApplicationSettings;
            const string userToken = "userToken";
            
            if (appSettings.Contains(userToken))
            {
                appSettings.Remove(userToken);
            }

            appSettings.Add(userToken, user);
            appSettings.Save();
        }

        private void _browser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            Utilities.ShowProgressIndicator(false);
        }

        private readonly DropNetClient _client = App.DropboxClient;
        private UserLogin _userLogin;
        private const string RedirectUrl = "mydomain.com";
    }
}