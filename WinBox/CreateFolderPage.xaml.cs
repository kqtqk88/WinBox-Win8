using System;
using System.Windows.Controls;
using DropNet.Models;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using WinBox.Model;
using WinBox.Resources;
using WinBox.ViewModels;

namespace WinBox
{
    public partial class CreateFolderPage : PhoneApplicationPage
    {
        public CreateFolderPage()
        {
            InitializeComponent();
            CreateApplicationBar();
            _viewModel = DataContext as CreateFolderViewModel;
            Loaded += (sender, args) => _folderName.Focus();
        }

        void CreateApplicationBar()
        {
            var okButton = new ApplicationBarIconButton(new Uri("/icons/appbar.check.rest.png", UriKind.Relative)) { Text = Labels.OK, IsEnabled = false};
            okButton.Click += _ok_Click;
            var cancelButton = new ApplicationBarIconButton(new Uri("/Toolkit.Content/ApplicationBar.Cancel.png", UriKind.Relative)) { Text = Labels.Cancel };
            cancelButton.Click += _cancel_Click;

            ApplicationBar.Buttons.Add(okButton);
            ApplicationBar.Buttons.Add(cancelButton);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            Messenger.Default.Register<MetaData>(this, MessengerToken.Created, NavigateToNewFolder);
            string folderPath = NavigationContext.QueryString["path"];
            if (_viewModel != null)
            {
                _viewModel.SetMetaData(new MetaData { Path = folderPath });
            }
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            Messenger.Default.Unregister<MetaData>(this, MessengerToken.Created, NavigateToNewFolder);
            base.OnNavigatingFrom(e);
        }

        void NavigateToNewFolder(MetaData metadata)
        {
            string path = string.Format("/FolderPage.xaml?path={0}&removebackentry={1}", metadata.Path, 1);
            NavigationService.Navigate(new Uri(path, UriKind.Relative));
        }

        private void _cancel_Click(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }

        private void _ok_Click(object sender, EventArgs e)
        {
            Focus();

            if (_viewModel != null)
            {
                _viewModel.CreateFolderCommand.Execute(_folderName.Text);
            }
        }

        private void _folderName_TextChanged(object sender, TextChangedEventArgs e)
        {
            var button = ApplicationBar.Buttons[0] as ApplicationBarIconButton;
            if (button != null)
            {
                button.IsEnabled = !string.IsNullOrWhiteSpace(_folderName.Text);
            }
        }

        readonly CreateFolderViewModel _viewModel;
    }
}