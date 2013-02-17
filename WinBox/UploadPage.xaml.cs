using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework.Media;
using WinBox.Model;
using WinBox.Resources;
using WinBox.ViewModels;

namespace WinBox
{
    public partial class UploadPage : PhoneApplicationPage
    {
        readonly UploadViewModel _vm;
        readonly PhotoChooserTask _photoChooserTask;
        PhotoResult _result;
        Picture _picture;

        public UploadPage()
        {
            InitializeComponent();
            _vm = DataContext as UploadViewModel;
            _photoChooserTask = new PhotoChooserTask { ShowCamera = true };
            _photoChooserTask.Completed += PhotoChooserTaskOnCompleted;

            Loaded += (sender, args) =>
                          {
                              _filename.Focus();
                              _filename.SelectAll();
                          };
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {

            var queryString = NavigationContext.QueryString;

            if (_result == null && !queryString.ContainsKey("FileId") && !queryString.ContainsKey("token"))
            {
                _photoChooserTask.Show();
                base.OnNavigatedTo(e);
                return;
            }

            _foldername.Text = NavigationContext.QueryString["path"];

            if (queryString.ContainsKey("FileId"))
            {
                while (NavigationService.CanGoBack)
                {
                    NavigationService.RemoveBackEntry();
                }

                var media = new MediaLibrary();
                _picture = media.GetPictureFromToken(queryString["FileId"]);
                _filename.Text = _picture.Name;
                _filename.SelectAll();

                var bitmap = new BitmapImage();
                bitmap.SetSource(_picture.GetImage());

                _test.Source = bitmap;

                return;
            }

            _filename.Focus();
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            _result = null;
            if (_picture != null)
            {
                _picture.Dispose();
            }

            base.OnNavigatingFrom(e);
        }

        private void PhotoChooserTaskOnCompleted(object sender, PhotoResult photoResult)
        {
            _result = photoResult;
            if (string.IsNullOrEmpty(photoResult.OriginalFileName))
            {
                return;
            }

            //Setup file name
            _filename.Text = Path.GetFileName(photoResult.OriginalFileName);
            _filename.SelectAll();

            _test.Source = new BitmapImage(new Uri(photoResult.OriginalFileName));
        }

        private void _filename_TextChanged(object sender, TextChangedEventArgs e)
        {
            var button = ApplicationBar.Buttons[0] as ApplicationBarIconButton;
            if (button != null)
            {
                button.IsEnabled = !string.IsNullOrWhiteSpace(_filename.Text);
            }
        }

        private void _ok_Click(object sender, EventArgs e)
        {
            Focus();

            if (_picture == null && _result.ChosenPhoto == null)
            {
                MessageBox.Show(Labels.SelectImageUpload, Labels.UploadTitle, MessageBoxButton.OK);
                return;
            }


            var imageStream = _picture != null ? _picture.GetImage() : _result.ChosenPhoto;

            try
            {
                var upload = new UploadData(_foldername.Text, imageStream, _filename.Text);
                _vm.Upload.Execute(upload);
                _result = null;
            }
            catch
            {
                NavigationService.Navigate(new Uri("MainPage.xaml", UriKind.Relative));
            }
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (_vm.Uploading && MessageBox.Show(Labels.ExitUploadConfirmation, Labels.ExitTitle,
                                    MessageBoxButton.OKCancel) != MessageBoxResult.OK)
            {
                e.Cancel = true;
            }

            base.OnBackKeyPress(e);
        }
    }
}