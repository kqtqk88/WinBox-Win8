using System;
using System.Windows.Input;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Phone.Controls;
using Telerik.Windows.Controls;
using WinBox.Model;
using WinBox.ViewModels;

namespace WinBox
{
    public partial class SearchPage : PhoneApplicationPage
    {
        public SearchPage()
        {
            InitializeComponent();

            _filesList.SetValue(InteractionEffectManager.IsInteractionEnabledProperty, true);
            InteractionEffectManager.AllowedTypes.Add(typeof(RadDataBoundListBoxItem));

            Loaded += (sender, args) =>
                          {
                              _searchString.Text = string.Empty;
                              _searchString.Focus();
                          };
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            Messenger.Default.Register<MetaDataViewModel>(this, MessengerToken.GetLink, GetLinkPage);
            _searchString.Focus();
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            Messenger.Default.Unregister<MetaDataViewModel>(this, MessengerToken.GetLink, GetLinkPage);
            base.OnNavigatingFrom(e);
        }

        private void GetLinkPage(MetaDataViewModel response)
        {
            App.MetadataViewModel = response;
            NavigationService.Navigate(new Uri("/FileSharePage.xaml", UriKind.Relative));
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

        RadContextMenu _tempContextMenu;

        private void _contextMenu_Opened(object sender, EventArgs e)
        {
            _tempContextMenu = sender as RadContextMenu;
        }

        void _searchString_ActionButtonTap(object sender, EventArgs e)
        {
            _filesList.Focus();
        }

        void _searchString_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var vm = DataContext as SearchViewModel;
                if (vm != null)
                {
                    vm.Search.Execute(_searchString.Text);
                    _filesList.Focus();
                }
            }
        }
    }
}