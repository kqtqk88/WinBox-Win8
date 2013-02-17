using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Navigation;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using RestSharp;
using WinBox.Model;
using WinBox.ViewModels;

namespace WinBox
{
    public partial class PhotoViewPage : PhoneApplicationPage
    {
        private PhotoViewerViewModel _viewModel;

        public PhotoViewPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _viewModel = App.PhotoViewerVM;
            DataContext = _viewModel;
            Messenger.Default.Register<MetaDataViewModel>(this, MessengerToken.GetLink, GetLinkPage);
 
            base.OnNavigatedTo(e);
        }

        private void GetLinkPage(MetaDataViewModel response)
        {
            App.MetadataViewModel = response;
            NavigationService.Navigate(new Uri("/FileSharePage.xaml", UriKind.Relative));
        }

        private void NavigateBack(IRestResponse restResponse)
        {
            NavigationService.GoBack();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Messenger.Default.Unregister<MetaDataViewModel>(this, MessengerToken.GetLink, GetLinkPage);

            base.OnNavigatedFrom(e);
        }

        private void _saveImage_Click(object sender, EventArgs e)
        {
            _viewModel.SaveImage.Execute(null);
        }

        private void _getLink_Click(object sender, EventArgs e)
        {
            _viewModel.GetLink.Execute(null);
        }

        private void _delete_Click(object sender, EventArgs e)
        {
            _viewModel.Delete.Execute(null);
        }

        private void ApplicationBar_StateChanged(object sender, ApplicationBarStateChangedEventArgs e)
        {
            ApplicationBar.Opacity = e.IsMenuVisible ? 0.8 : 0;
        }
    }
}