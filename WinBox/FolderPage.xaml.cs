using System;
using DropNet.Models;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Telerik.Windows.Controls;
using WinBox.Model;
using WinBox.Resources;
using WinBox.Utility;
using WinBox.ViewModels;

namespace WinBox
{
    public partial class FolderPage : PhoneApplicationPage
    {
        readonly FolderViewModel _viewModel;
        RadContextMenu _tempContextMenu;

        public FolderPage()
        {
            InitializeComponent();
            BuildApplicationBar();
            _viewModel = DataContext as FolderViewModel;

            _filesList.SetValue(InteractionEffectManager.IsInteractionEnabledProperty, true);
            InteractionEffectManager.AllowedTypes.Add(typeof(RadDataBoundListBoxItem));
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

            var pinButton = new ApplicationBarIconButton(new Uri("/icons/appbar_pin_remove.png", UriKind.Relative)) { Text = Labels.Pin };
            pinButton.Click += _pin_Click;
            var uploadButton = new ApplicationBarIconButton(new Uri("/icons/appbar.upload.png", UriKind.Relative)) { Text = Labels.Upload };
            uploadButton.Click += _upload_Click;
            var refresh = new ApplicationBarIconButton(new Uri("/icons/appbar.refresh.rest.png", UriKind.Relative)) { Text = Labels.Refresh };
            refresh.Click += _refresh_Click;

            ApplicationBar.Buttons.Add(pinButton);
            ApplicationBar.Buttons.Add(uploadButton);
            ApplicationBar.Buttons.Add(refresh);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (RemoveBackEntry)
            {
                NavigationService.RemoveBackEntry();
            }

            Messenger.Default.Register<MetaDataViewModel>(this, MessengerToken.GetLink, GetLinkPage);

            var data = new MetaData
                           {
                               Path = GetPath(),
                               Icon = GetIcon()
                           };

            PageTitle.Text = data.Name.ToLower();
            _viewModel.RegisterMessengers.Execute(null);
            _viewModel.LoadItems.Execute(data);

            SetPinIcon(data);

            if (RemoveBackEntry)
            {
                App.NeedDataRefresh = true;
            }

            base.OnNavigatedTo(e);
        }

        void SetPinIcon(MetaData data)
        {
            if (ApplicationBar.Buttons.Count < 1)
            {
                return;
            }

            try
            {
                var pin = (ApplicationBarIconButton)ApplicationBar.Buttons[0];

                if (TileHelper.IsPinned(data))
                {
                    pin.IconUri = new Uri("icons/appbar_pin_remove.png", UriKind.Relative);
                    pin.Text = Labels.Unpin;
                }
                else
                {
                    pin.IconUri = new Uri("icons/appbar_pin.png", UriKind.Relative);
                    pin.Text = Labels.Pin;
                }
            }
            catch 
            {
                //ignore the exception
            }
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            Messenger.Default.Unregister<MetaDataViewModel>(this, MessengerToken.GetLink, GetLinkPage);

            _viewModel.UnregisterMessengers.Execute(null);
            base.OnNavigatingFrom(e);
        }

        string GetPath()
        {
            return NavigationContext.QueryString["path"];
        }

        string GetIcon()
        {
            if (NavigationContext.QueryString.ContainsKey("icon"))
            {
                return NavigationContext.QueryString["icon"];
            }

            return "folder";
        }

        bool RemoveBackEntry
        {
            get { return NavigationContext.QueryString.Keys.Contains("removebackentry"); }
        }

        private void _settings_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.Relative));
        }

        private void _refresh_Click(object sender, EventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.RefreshItemsCommand.Execute(null);
            }
        }

        private void _creatFolder_Click(object sender, EventArgs e)
        {
            string path = string.Format("/CreateFolderPage.xaml?path={0}", GetPath());
            NavigationService.Navigate(new Uri(path, UriKind.Relative));
        }

        private void _upload_Click(object sender, EventArgs e)
        {
            string path = string.Format("/UploadPage.xaml?path={0}", GetPath());
            NavigationService.Navigate(new Uri(path, UriKind.Relative));
        }

        private void GetLinkPage(MetaDataViewModel response)
        {
            App.MetadataViewModel = response;
            NavigationService.Navigate(new Uri("/FileSharePage.xaml", UriKind.Relative));
        }

        private void _about_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/YourLastAboutDialog;component/AboutPage.xaml", UriKind.Relative));
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (_tempContextMenu != null && _tempContextMenu.IsOpen)
            {
                _tempContextMenu.IsOpen = false;
                e.Cancel = true;
            }

            base.OnBackKeyPress(e);
        }

        private void _contextMenu_Opened(object sender, EventArgs e)
        {
            _tempContextMenu = sender as RadContextMenu;
        }

        private void _pin_Click(object sender, EventArgs e)
        {
            var data = new MetaData
                           {
                               Path = GetPath(),
                               Icon = GetIcon()
                           };

            if (TileHelper.IsPinned(data))
            {
                TileHelper.RemoveTile(data);
            }
            else
            {
                TileHelper.CreateTile(data);
            }

            SetPinIcon(data);
        }
    }
}