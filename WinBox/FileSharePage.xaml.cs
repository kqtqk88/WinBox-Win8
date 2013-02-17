using System;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using WinBox.Model;
using WinBox.Resources;
using WinBox.ViewModels;

namespace WinBox
{
    public partial class FileSharePage : PhoneApplicationPage
    {
        public FileSharePage()
        {
            InitializeComponent();
            var cancelButton = new ApplicationBarIconButton(new Uri("/Toolkit.Content/ApplicationBar.Cancel.png", UriKind.Relative)) { Text = Labels.Cancel };
            cancelButton.Click += _cancel_Click;

            ApplicationBar.Buttons.Add(cancelButton);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            DataContext = App.MetadataViewModel;
            base.OnNavigatedTo(e);
        }

        private void _cancel_Click(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }

        private void _email_Click(object sender, RoutedEventArgs e)
        {
            ShareCommand(ShareOption.Email);
        }

        private void _social_Click(object sender, RoutedEventArgs e)
        {
            ShareCommand(ShareOption.Social);
        }

        private void _text_Click(object sender, RoutedEventArgs e)
        {
            ShareCommand(ShareOption.Text);
        }

        void ShareCommand(ShareOption option)
        {
            var vm = (MetaDataViewModel)DataContext;
            vm.Share.Execute(option);
        }
    }
}