using System;
using System.Windows;
using Microsoft.Phone.Controls;
using WinBox.Model;
using WinBox.Resources;
using WinBox.ViewModels;

namespace WinBox
{
    public partial class Settings : PhoneApplicationPage
    {
        readonly SettingsViewModel _viewModel;

        public Settings()
        {
            InitializeComponent();
            ThemeManager.OverrideOptions = ThemeManagerOverrideOptions.SystemTrayAndApplicationBars;
            _viewModel = DataContext as SettingsViewModel;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            ExecuteRefreshStatsCommand();
            base.OnNavigatedTo(e);
        }

        void ExecuteRefreshStatsCommand()
        {
            if (_viewModel != null)
            {
                _viewModel.RefreshStats.Execute(null);
            }
        }

        private void _logout_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(Labels.LogOutMessage, Labels.LogOutTitle, MessageBoxButton.OKCancel);
            
            if (result == MessageBoxResult.Cancel)
            {
                return;
            }
            
            if (_viewModel != null)
            {
                _viewModel.LogOut.Execute(null);
            }

            NavigationService.Navigate(new Uri("/StartupPage.xaml", UriKind.Relative));
        }

        private void _refresh_Click(object sender, EventArgs e)
        {
            ExecuteRefreshStatsCommand();
        }

        private void _emailShare_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ShareReferralCommand.Execute(ShareOption.Email);
        }

        private void _socialShare_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ShareReferralCommand.Execute(ShareOption.Social);
        }

        private void _about_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/YourLastAboutDialog;component/AboutPage.xaml", UriKind.Relative));

        }
    }
}