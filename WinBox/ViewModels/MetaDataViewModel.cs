using DropNet.Exceptions;
using DropNet.Models;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework.Media;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WinBox.Model;
using WinBox.Resources;
using WinBox.Utility;
using System.Linq;

namespace WinBox.ViewModels
{
    public class MetaDataViewModel : WinBoxViewModel
    {
        public MetaDataViewModel(MetaData metadata)
        {
            _metadata = metadata;
            LoadThumbnail();
        }

        public MetaData MetaData
        {
            get
            {
                return _metadata;
            }
        }

        public string Description
        {
            get
            {
                return string.Format("{0} · {1}", Utilities.RelativeDate(MetaData.ModifiedDate),
                                     Utilities.FormatBytes(MetaData.Bytes));
            }
        }

        public Visibility NoImage
        {
            get { return _noImage; }
            set
            {
                _noImage = value;
                RaisePropertyChanged("NoImage");
            }
        }

        public string ParentDirectory
        {
            get { return Path.GetDirectoryName(_metadata.Path); }
        }

        public string ShareLink
        {
            get
            {
                if (IsInDesignModeStatic)
                {
                    return "http://goo.gl/file.txt";
                }

                return _sharelink;
            }
            
            set { _sharelink = value; }
        }

        public BitmapImage Thumb
        {
            get
            {
                if (_thumb == null)
                {
                    _thumb = new BitmapImage(new Uri(string.Format("/dpi/{0}.png", _metadata.Icon), UriKind.Relative));
                }

                return _thumb;
            }
            set
            {
                _thumb = value; RaisePropertyChanged("Thumb");
            }
        }

        public BitmapImage Image { get; private set; }

        public string LargeIcon
        {
            get
            {
                return string.Format("/dpibig/{0}.png", _metadata.Icon);
            }
        }

        public ICommand Download
        {
            get
            {
                return _download ??
                    (_download = new RelayCommand(DownloadHandler));
            }
        }

        public ICommand AddToFavourite
        {
            get
            {
                return _addToFavourite ??
                    (_addToFavourite = new RelayCommand(AddToFavouriteHandler));
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

        public ICommand Share
        {
            get
            {
                return _share ?? (_share = new RelayCommand<ShareOption>(ShareHandler));
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

        public ICommand PinToStart
        {
            get
            {
                return _pinToStart ??
                       (_pinToStart = new RelayCommand(PinToStartAction, IsPinned));
            }
        }

        public ICommand GetImage
        {
            get { return _getImageCommand ?? (_getImageCommand = new RelayCommand(GetImageHandler)); }
        }

        private void GetImageHandler()
        {
            if (!IsNetworkAvaialble)
            {
                MessageBox.Show(Labels.ConnectionErrorMessage, Labels.ConnectionErrorTitle, MessageBoxButton.OK);
                return;
            }

           //var image = App.LruCache[_metadata.Path];
            Utilities.ShowProgressIndicator(true, "Loading the image...");
            App.DropboxClient.GetFileAsync(_metadata.Path,
                                           r =>
                                               {   
                                                   using (var stream = new MemoryStream(r.RawBytes))
                                                   {
                                                       var bitmapImage = new BitmapImage();
                                                       bitmapImage.SetSource(stream);
                                                       Image = bitmapImage;
                                                      // App.LruCache[_metadata.Path] = new LRUWrapper<BitmapImage>(bitmapImage);
                                                       RaisePropertyChanged("Image");
                                                       Utilities.ShowProgressIndicator(false);
                                                   }
                                               },
                                           e =>
                                               {
                                                   NoImage = Visibility.Collapsed;
                                                   App.NeedDataRefresh = true;
                                                   Utilities.ShowProgressIndicator(false);
                                                   MessageBox.Show("Unable to get the image.", Labels.ErrorTitle,
                                                                   MessageBoxButton.OK);
                                               });
        }

        private void SaveImageAction()
        {
            if (Image == null)
            {
                return;
            }

            using (var memoryStream = new MemoryStream())
            {
                var writeableBitmap = new WriteableBitmap(Image);
                writeableBitmap.SaveJpeg(memoryStream, writeableBitmap.PixelWidth, writeableBitmap.PixelHeight, 0, 100);
                memoryStream.Seek(0L, 0);

                using (var mediaLibrary = new MediaLibrary())
                {
                    mediaLibrary.SavePicture(_metadata.Name, memoryStream);
                }

                MessageBox.Show(Labels.ImageSaved, Labels.SuccessTitle, MessageBoxButton.OK);
            }
        }

        public Visibility DownloadVisible
        {
            get { return _metadata.Is_Dir ? Visibility.Collapsed : Visibility.Visible; }
        }

        public Visibility PinVisible
        {
            get { return _metadata.Is_Dir ? Visibility.Visible : Visibility.Collapsed; }
        }
       
        public string AddFavouriteHeader
        {
            get
            {
                return "add to favourite";
            }
        }

        public List<SlideViewItem> Images
        {
            get 
            {
                string key = Utilities.GetCacheKeyName(Path.GetDirectoryName(MetaData.Path));
                var parent = Cache.Current.Get<MetaData>(key);

                return parent.Contents.Where(p => p.Thumb_Exists).
                    Select(p => new SlideViewItem(p.Path)).ToList();
            }
        }

        void LoadThumbnail()
        {
            if (_metadata.Thumb_Exists)
            {
                using (IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    string filename = Utilities.GetThumbnailFileName(_metadata.Path);

                    if (file.FileExists(filename))
                    {
                        var thumb = new BitmapImage();
                        thumb.SetSource(file.OpenFile(filename, FileMode.Open, FileAccess.Read));
                        Thumb = thumb;
                        return;
                    }
                }

                App.DropboxClient.GetThumbnailAsync(_metadata.Path, ThumbnailSize.Medium, GetThumbnail, o => Debug.WriteLine(o));
            }
        }

        void GetThumbnail(byte[] obj)
        {
            using (IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication())
            {
                try
                {
                    string filename = Utilities.GetThumbnailFileName(_metadata.Path);
                    var stream = file.CreateFile(filename);

                    using (var imageStream = new MemoryStream(obj))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.SetSource(imageStream);

                        var wb = new WriteableBitmap(bitmap);
                        wb.SaveJpeg(stream, wb.PixelWidth, wb.PixelHeight, 0, 100);
                        stream.Close();

                        Thumb = bitmap;
                    }

                    RaisePropertyChanged("SmallIcon");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        void ShareHandler(ShareOption option)
        {
            string body = string.Format("I am sharing this link with you {0}", ShareLink);

            if (option == ShareOption.Social)
            {
                var task = new ShareLinkTask
                               {
                                   LinkUri = new Uri(ShareLink),
                                   Title = MetaData.Name,
                                   Message = "I'm sharing this with you"
                               };

                task.Show();
            }
            else if (option == ShareOption.Text)
            {
                var task = new SmsComposeTask { Body = body };
                task.Show();
            }
            else
            {
                var task = new EmailComposeTask { Body = body };
                task.Show();
            }
        }

        public bool IsPinned()
        {
            return !TileHelper.IsPinned(_metadata);
        }

        void PinToStartAction()
        {
            TileHelper.CreateTile(_metadata);
        }

        void DownloadHandler()
        {
            if (!IsNetworkAvaialble)
            {
                MessageBox.Show(Labels.ConnectionErrorMessage, Labels.ConnectionErrorTitle, MessageBoxButton.OK);
                return;
            }

            App.DropboxClient.GetMediaAsync(MetaData.Path, GetMediaHandler, OnDownloadFailed);
        }

        void GetMediaHandler(ShareResponse data)
        {
            var task = new WebBrowserTask { Uri = new Uri(data.Url) };
            task.Show();
        }

        void GetLinkHandler()
        {
            if (!IsNetworkAvaialble)
            {
                MessageBox.Show(Labels.ConnectionErrorMessage, Labels.ConnectionErrorTitle, MessageBoxButton.OK);
                return;
            }

            Utilities.ShowProgressIndicator(true, "Generating the link");
            App.DropboxClient.GetShareAsync(_metadata.Path, OnShareSuccess, OnShareFailed);
        }

        void OnDownloadFailed(DropboxException ex)
        {
            Utilities.ShowProgressIndicator(false);
            
            switch (ex.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    MessageBox.Show(Labels.FileNotFoundShare, Labels.ErrorTitle, MessageBoxButton.OK);
                    break;
                default:
                    MessageBox.Show(ex.StatusCode.ToString());
                    break;
            }
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
            Messenger.Default.Send(this, MessengerToken.GetLink);
        }

        void AddToFavouriteHandler()
        {
            //TODO: implement the favourites
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
                App.DropboxClient.DeleteAsync(MetaData.Path, DeleteSuccessHandler, DeleteFailed);
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

            Messenger.Default.Send(response, MessengerToken.Deleted);
            Messenger.Default.Send(response, MessengerToken.Refresh);
        }

        readonly MetaData _metadata;
        ICommand _download;
        ICommand _addToFavourite;
        ICommand _getLink;
        ICommand _delete;
        ICommand _share;
        ICommand _saveImage;
        BitmapImage _thumb;
        string _sharelink;
        Visibility _noImage = Visibility.Visible;
        private ICommand _pinToStart;
        private ICommand _getImageCommand;
    }
}