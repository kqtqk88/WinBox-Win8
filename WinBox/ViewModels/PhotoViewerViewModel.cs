using DropNet.Exceptions;
using DropNet.Models;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Xna.Framework.Media;
using RestSharp;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WinBox.Model;
using WinBox.Resources;
using WinBox.Utility;

namespace WinBox.ViewModels
{
    public class PhotoViewerViewModel : WinBoxViewModel
    {
        private MetaData _metadata;
        ObservableCollection<SlideViewItem> _images;

        public PhotoViewerViewModel(MetaData metadata)
        {
            _metadata = metadata;
        }

        public MetaData MetaData
        {
            get
            {
                return _metadata;
            }
        }

        public ObservableCollection<SlideViewItem> Images
        {
            get
            {
                string path = MetaData.Path;
                if (_images == null)
                {
                    string key = Utilities.GetCacheKeyName(Path.GetDirectoryName(MetaData.Path));
                    var parent = Cache.Current.Get<MetaData>(key);

                    var images =  parent.Contents.Where(p => p.Thumb_Exists).
                        Select(p => new SlideViewItem(p.Path));

                    _images = new ObservableCollection<SlideViewItem>(images);
                }

                Application.Current.RootVisual.Dispatcher.BeginInvoke(new System.Action(
                    () =>
                    {
                        SelectedImage = _images.Where(p => p.ImagePath == path).FirstOrDefault();
                    }));

                return _images;
            }
        }

        public SlideViewItem SelectedImage
        {
            get { return _selectedImage; }
            set
            {
                if (value == null)
                {
                    return;
                }
                _selectedImage = value;
                RaisePropertyChanged("SelectedImage");
            }
        }

        public ICommand GetLink
        {
            get
            {
                return _getLink ??
                    (_getLink = new RelayCommand(GetLinkHandler));
            }
        }

        public ICommand Delete
        {
            get
            {
                return _delete ??
                    (_delete = new RelayCommand(DeleteHandler));
            }
        }

        public ICommand SaveImage
        {
            get
            {
                return _saveImage ??
                (_saveImage = new RelayCommand(SaveImageAction));
            }
        }

        void DeleteHandler()
        {
            if (!IsNetworkAvaialble)
            {
                MessageBox.Show(Labels.ConnectionErrorMessage, Labels.ConnectionErrorTitle, MessageBoxButton.OK);
                return;
            }

            var result = MessageBox.Show(Labels.DeleteConfirmation, Labels.DeleteConfirmationTitle, MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                Utilities.ShowProgressIndicator(true, "deleting...");
                App.DropboxClient.DeleteAsync(SelectedImage.ImagePath, DeleteSuccessHandler, DeleteFailed);
            }
        }

        void DeleteFailed(DropboxException ex)
        {
            Utilities.ShowProgressIndicator(false);
            switch (ex.StatusCode)
            {
                case HttpStatusCode.Forbidden:
                    MessageBox.Show(Labels.DeleteFileError, Labels.ErrorTitle, MessageBoxButton.OK);
                    return;
                case HttpStatusCode.NotFound:
                    MessageBox.Show(Labels.FileNotFoundDelete, Labels.ErrorTitle, MessageBoxButton.OK);
                    return;
                case HttpStatusCode.NotAcceptable:
                    MessageBox.Show(Labels.ServerError, Labels.ErrorTitle, MessageBoxButton.OK);
                    return;
                default:
                    MessageBox.Show(ex.StatusCode.ToString());
                    break;
            }

        }

        void DeleteSuccessHandler(IRestResponse response)
        {
            Utilities.ShowProgressIndicator(false);
            App.NeedDataRefresh = true;

            TileHelper.RemoveTile(_metadata);

            Images.Remove(SelectedImage);
          }

        void GetLinkHandler()
        {
            if (!IsNetworkAvaialble)
            {
                MessageBox.Show(Labels.ConnectionErrorMessage, Labels.ConnectionErrorTitle, MessageBoxButton.OK);
                return;
            }

            Utilities.ShowProgressIndicator(true, "Generating the link");
            App.DropboxClient.GetShareAsync(SelectedImage.ImagePath, OnShareSuccess, OnShareFailed);
        }

        void OnShareFailed(DropboxException ex)
        {
            Utilities.ShowProgressIndicator(false);
            switch (ex.StatusCode)
            {
                case HttpStatusCode.Forbidden:
                    MessageBox.Show(Labels.ShareFileError, Labels.ErrorTitle, MessageBoxButton.OK);
                    break;
                case HttpStatusCode.NotFound:
                    MessageBox.Show(Labels.FileNotFoundShare, Labels.ErrorTitle, MessageBoxButton.OK);
                    break;
                case HttpStatusCode.NotAcceptable:
                    MessageBox.Show(Labels.ServerError, Labels.ErrorTitle, MessageBoxButton.OK);
                    break;
                default:
                    MessageBox.Show(ex.StatusCode.ToString());
                    break;
            }
        }

        void OnShareSuccess(ShareResponse response)
        {
            Utilities.ShowProgressIndicator(false);
            ShareLink = response.Url;
            var vm = SimpleIoc.Default.GetInstance<MetaDataViewModel>(SelectedImage.ImagePath);
            Messenger.Default.Send(vm, MessengerToken.GetLink);
        }

        private void SaveImageAction()
        {
            if (!SelectedImage.ImageAvailable && SelectedImage.Image == null)
            {
                return;
            }

            using (var memoryStream = new MemoryStream())
            {
                var writeableBitmap = new WriteableBitmap(SelectedImage.Image);
                writeableBitmap.SaveJpeg(memoryStream, writeableBitmap.PixelWidth, writeableBitmap.PixelHeight, 0, 100);
                memoryStream.Seek(0L, 0);

                using (var mediaLibrary = new MediaLibrary())
                {
                    mediaLibrary.SavePicture(_metadata.Name, memoryStream);
                }

                MessageBox.Show(Labels.ImageSaved, Labels.SuccessTitle, MessageBoxButton.OK);
            }
        }

        ICommand _getLink;
        ICommand _delete;
        ICommand _saveImage;
        private SlideViewItem _selectedImage;

        public string ShareLink { get; set; }
    }
}
