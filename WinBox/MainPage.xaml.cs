using DropNet.Models;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System;
using System.ComponentModel;
using System.Windows;
using Telerik.Windows.Controls;
using WinBox.Model;
using WinBox.Resources;
using WinBox.Utility;
using WinBox.ViewModels;

namespace WinBox
{
    public partial class MainPage
    {

        private readonly MainViewModel _viewModel;
        private RadContextMenu _tempContextMenu;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            BuildApplicationBar();
            _viewModel = DataContext as MainViewModel;

            _filesList.SetValue(InteractionEffectManager.IsInteractionEnabledProperty, true);
            InteractionEffectManager.AllowedTypes.Add(typeof(RadDataBoundListBoxItem));
            Loaded += MainPage_Loaded;
        }

        void BuildApplicationBar()
        {
            var createFolder = new ApplicationBarMenuItem(Labels.CreateNewFolder);
            createFolder.Click += _creatFolder_Click;
            ApplicationBar.MenuItems.Add(createFolder);
            var settingsMenu = new ApplicationBarMenuItem(Labels.Settings);
            settingsMenu.Click += _settings_Click;
            ApplicationBar.MenuItems.Add(settingsMenu);
            var about = new ApplicationBarMenuItem(Labels.About);
            about.Click += _about_Click;
            ApplicationBar.MenuItems.Add(about);

            var uploadButton = new ApplicationBarIconButton(new Uri("/icons/appbar.upload.png", UriKind.Relative)) { Text = Labels.Upload };
            uploadButton.Click += _upload_Click;
            var refresh = new ApplicationBarIconButton(new Uri("/icons/appbar.refresh.rest.png", UriKind.Relative)) { Text = Labels.Refresh };
            refresh.Click += _refresh_Click;
            var search = new ApplicationBarIconButton(new Uri("/icons/appbar.feature.search.rest.png", UriKind.Relative)) { Text = Labels.Search };
            search.Click += _search_Click;

            ApplicationBar.Buttons.Add(uploadButton);
            ApplicationBar.Buttons.Add(refresh);
            ApplicationBar.Buttons.Add(search);

        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            var rateReminder = new RadRateApplicationReminder
            {
                RecurrencePerTimePeriod = TimeSpan.FromDays(3),
                AllowUsersToSkipFurtherReminders = true
            };

            rateReminder.Notify();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {

            while (NavigationService.CanGoBack)
            {
                NavigationService.RemoveBackEntry();
            }

            Messenger.Default.Register<MetaDataViewModel>(this, MessengerToken.GetLink, GetLinkPage);

            _viewModel.RegisterMessengers.Execute(null);
            _viewModel.LoadItems.Execute(new MetaData { Path = "/" });

            if (App.NeedDataRefresh || NavigationContext.QueryString.ContainsKey("refresh"))
            {
                ForceRefresh();
            }

            base.OnNavigatedTo(e);
        }

        private void GetLinkPage(MetaDataViewModel response)
        {
            App.MetadataViewModel = response;
            NavigationService.Navigate(new Uri("/FileSharePage.xaml", UriKind.Relative));
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            Messenger.Default.Unregister<MetaDataViewModel>(this, MessengerToken.GetLink, GetLinkPage);

            if (_viewModel != null)
            {
                _viewModel.UnregisterMessengers.Execute(null);
            }

            base.OnNavigatingFrom(e);
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (_tempContextMenu != null && _tempContextMenu.IsOpen)
            {
                _tempContextMenu.IsOpen = false;
                e.Cancel = true;
                return;
            }

            if (NavigationService.CanGoBack)
            {
                return;
            }

#if !DEBUG
            if (AppSettings.ExitConfirmation && MessageBox.Show(Labels.ExitConfirmation, Labels.ExitTitle,
                                    MessageBoxButton.OKCancel) != MessageBoxResult.OK)
            {
                e.Cancel = true;
            }
#endif
        }

        private void _settings_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.Relative));
        }

        private void _refresh_Click(object sender, EventArgs e)
        {
            ForceRefresh();
        }

        private void ForceRefresh()
        {
            if (_viewModel != null)
            {
                _viewModel.RefreshItemsCommand.Execute(null);
            }
        }

        private void _creatFolder_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/CreateFolderPage.xaml?path=/", UriKind.Relative));
        }

        private void _upload_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/UploadPage.xaml?path=/", UriKind.Relative));
        }

        private void _search_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SearchPage.xaml", UriKind.Relative));
        }

        private void _about_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/YourLastAboutDialog;component/AboutPage.xaml", UriKind.Relative));
        }

        private void _contextMenu_Opened(object sender, EventArgs e)
        {
            _tempContextMenu = sender as RadContextMenu;
        }
    }
}