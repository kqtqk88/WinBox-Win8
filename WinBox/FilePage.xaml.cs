using System;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using RestSharp;
using WinBox.Model;
using WinBox.Resources;
using WinBox.ViewModels;

namespace WinBox
{
    public partial class FilePage : PhoneApplicationPage
    {
        MetaDataViewModel _viewModel;

        public FilePage()
        {
            InitializeComponent();
            CreateApplicationBar();
        }

        void CreateApplicationBar()
        {
            var download = new ApplicationBarIconButton(new Uri("/icons/appbar.download.png", UriKind.Relative)) { Text = Labels.Download };
            download.Click += _download_Click;
            var getlink = new ApplicationBarIconButton(new Uri("/icons/appbar.link.png", UriKind.Relative)) { Text = Labels.GetLink };
            getlink.Click += _getLink_Click;
            var delete = new ApplicationBarIconButton(new Uri("/Toolkit.Content/ApplicationBar.Delete.png", UriKind.Relative)) { Text = Labels.Delete };
            delete.Click += _delete_Click;

            ApplicationBar.Buttons.Add(download);
            ApplicationBar.Buttons.Add(getlink);
            ApplicationBar.Buttons.Add(delete);
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            Messenger.Default.Unregister<MetaDataViewModel>(this, MessengerToken.GetLink, GetLinkPage);
            Messenger.Default.Unregister<IRestResponse>(this, MessengerToken.Deleted, NavigateBack);
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            string path = App.FileMetaData.Path;

            if (DataContext == null)
            {
                _viewModel = SimpleIoc.Default.GetInstance<MetaDataViewModel>(path) ??
                             new MetaDataViewModel(App.FileMetaData);

                DataContext = _viewModel;
            }

            Messenger.Default.Register<MetaDataViewModel>(this, MessengerToken.GetLink, GetLinkPage);
            Messenger.Default.Register<IRestResponse>(this, MessengerToken.Deleted, NavigateBack);
            base.OnNavigatedTo(e);
        }

        void NavigateBack(IRestResponse restResponse)
        {
            App.NeedDataRefresh = true;
            NavigationService.GoBack();
        }

        private void _download_Click(object sender, EventArgs e)
        {
            _viewModel.Download.Execute(null);
        }

        private void _delete_Click(object sender, EventArgs e)
        {
            _viewModel.Delete.Execute(null);
        }

        private void _getLink_Click(object sender, EventArgs e)
        {
            _viewModel.GetLink.Execute(null);
        }

        private void GetLinkPage(MetaDataViewModel response)
        {
            App.MetadataViewModel = response;
            NavigationService.Navigate(new Uri("/FileSharePage.xaml", UriKind.Relative));
        }

        private void _back_Click(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}