using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using WinBox.Resources;
using WinBox.Utility;
using WinBox.ViewModels;

namespace WinBox.Model
{
    public class SlideViewItem : WinBoxViewModel
    {
        private string _path;
        private BitmapImage _image;
        private bool _loading;

        public SlideViewItem(string path)
        {
            _path = path;
        }

        public string ImagePath
        {
            get
            {
                return _path;
            }
        }

        public string Title
        {
            get
            {
                return Path.GetFileName(_path);
            }
        }

        public string Description
        {
            get
            {
                return DateTime.Now.ToString();
            }
        }

        public bool Loading
        {
            get
            {
                return _loading;
            }
            set
            {
                _loading = value;
                RaisePropertyChanged("Loading");
            }
        }

        public bool ImageAvailable { get; private set; }

        public BitmapImage Image
        {
            get
            {
                Loading = true;

                var cache = App.LruCache[_path];
                if (cache != null)
                {
                    return cache.Value;
                }
                App.DropboxClient.GetFileAsync(_path,
                                            r =>
                                            {
                                                using (var stream = new MemoryStream(r.RawBytes))
                                                {
                                                    var bitmapImage = new BitmapImage();
                                                    bitmapImage.SetSource(stream);
                                                    _image = bitmapImage;
                                                     App.LruCache[_path] = new LRUWrapper<BitmapImage>(bitmapImage);
                                                    RaisePropertyChanged("Image");
                                                    Loading = false;
                                                }

                                                ImageAvailable = true;
                                            },
                                            e =>
                                            {
                                                ImageAvailable = false;
                                                Loading = false;
                                                App.NeedDataRefresh = true;
                                                MessageBox.Show("Unable to get the image.", Labels.ErrorTitle,
                                                                MessageBoxButton.OK);
                                            });

                return null;
            }
        }


    }
}
