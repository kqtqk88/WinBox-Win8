using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Linq;

using DropNet;
using DropNet.Exceptions;
using DropNet.Models;

using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;

using Microsoft.Phone.Tasks;
using RestSharp;

using WinBox.Model;
using WinBox.Modules;
using WinBox.Resources;
using WinBox.Utility;

namespace WinBox.ViewModels
{
    public class FolderViewModel : WinBoxViewModel
    {
        readonly IFileSytemOperations _fileSytemOperations;
        readonly INavigationService _navigationService;

        public FolderViewModel()
        {
            _fileSytemOperations = SimpleIoc.Default.GetInstance<IFileSytemOperations>();
            _navigationService = SimpleIoc.Default.GetInstance<INavigationService>();

            if (IsInDesignMode)
            {
                LoadItemsHandler(new MetaData { Path = "/My Dropbox" });
            }
        }

        [PreferredConstructor]
        public FolderViewModel(IFileSytemOperations fileSytemOperations, INavigationService navigationService)
        {
            _fileSytemOperations = fileSytemOperations;
            _navigationService = navigationService;

            if (IsInDesignMode)
            {
                LoadItemsHandler(new MetaData { Path = "/My Dropbox" });
            }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                RaisePropertyChanged("Title");
            }
        }

        public ObservableCollection<MetaDataViewModel> ListOfFiles
        {
            get { return _files ?? (_files = new ObservableCollection<MetaDataViewModel>()); }
        }

        public ICommand LoadItems
        {
            get
            {
                return _loadItemsCommand ??
                    (_loadItemsCommand = new RelayCommand<MetaData>(LoadItemsHandler));
            }
        }

        public ICommand RefreshItemsCommand
        {
            get
            {
                return _refreshItemsCommand ??
                    (_refreshItemsCommand = new RelayCommand(RefreshItemsCommandHandler));
            }
        }

        public ICommand Delete
        {
            get
            {
                return _delete ??
                    (_delete = new RelayCommand<MetaData>(DeleteHandler));
            }
        }

        public ICommand Upload
        {
            get
            {
                return _uploadCommand ??
                (_uploadCommand = new RelayCommand(UploadCommandHandler));
            }
        }

        public ICommand NavigateCommand
        {
            get
            {
                return _navigateCommand ??
                    (_navigateCommand = new RelayCommand<MetaDataViewModel>(NavigateHandler));
            }
        }

        public ICommand RegisterMessengers
        {
            get
            {
                return _registerMessengers ??
                    (_registerMessengers = new RelayCommand(RegisterMessengersHandler));
            }
        }

        public ICommand UnregisterMessengers
        {
            get
            {
                return _unregisterMessengers ??
                    (_unregisterMessengers = new RelayCommand(UnregisterMessengersHandler));
            }
        }

        DropNetClient Client
        {
            get
            {
                return App.DropboxClient;
            }
        }

        SimpleIoc Ioc
        {
            get { return SimpleIoc.Default; }
        }

        void NavigateHandler(MetaDataViewModel data)
        {
            if (!IsNetworkAvaialble)
            {
                MessageBox.Show(Labels.ConnectionErrorMessage, Labels.ConnectionErrorTitle, MessageBoxButton.OK);
                return;
            }

            if (data.MetaData.Is_Dir)
            {
                string path = string.Format("/FolderPage.xaml?path={0}&icon={1}", data.MetaData.Path, data.MetaData.Icon);
                _navigationService.Navigate(new Uri(path, UriKind.Relative));
            }
            else if (data.MetaData.Thumb_Exists)
            {
                App.PhotoViewerVM = new PhotoViewerViewModel(data.MetaData);
                _navigationService.Navigate(new Uri("/PhotoViewPage.xaml", UriKind.Relative));
            }
            else
            {
                App.FileMetaData = data.MetaData;
                _navigationService.Navigate(new Uri("/FilePage.xaml", UriKind.Relative));
            }
        }

        void RefreshMessengerHandler(IRestResponse restResponse)
        {
            RefreshItemsCommandHandler();
        }

        void OnTaskOnCompleted(object sender, PhotoResult result)
        {
            if (result.ChosenPhoto == null)
            {
                return;
            }

            Deployment.Current.Dispatcher.BeginInvoke(() => Utilities.ShowProgressIndicator(true, "Uploading..."));
            _fileSytemOperations.UploadFile(new UploadData(_metaDataCache.Path, result.ChosenPhoto, Path.GetFileName(result.OriginalFileName)),
                OnUploadSuccess, Fail);
        }

        void OnUploadSuccess(MetaData metaData)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
                                                          {
                                                              Utilities.ShowProgressIndicator(false);
                                                              RefreshItemsCommandHandler();
                                                          });
        }

        void UploadCommandHandler()
        {
            var task = new PhotoChooserTask();
            task.Show();
            task.Completed += OnTaskOnCompleted;
        }

        void DeleteHandler(MetaData metaData)
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
                Client.DeleteAsync(metaData.Path, DeleteSuccessHandler, Fail);
            }
        }

        void DeleteSuccessHandler(IRestResponse obj)
        {
            Utilities.ShowProgressIndicator(false);
            RefreshItemsCommandHandler();
        }

        void RefreshItemsCommandHandler()
        {
            _files = null;
            LoadItemsHandler(_metaDataCache, true);
        }

        void LoadItemsHandler(MetaData metaData)
        {
            LoadItemsHandler(metaData, false);
        }

        void LoadItemsHandler(MetaData metaData, bool forceReload)
        {
            _metaDataCache = metaData;

            Title = metaData.Name;

            if (IsInDesignMode)
            {
                ListOfFiles.Add(new MetaDataViewModel(new MetaData { Path = "/Path1", Icon = "folder", Modified = "17-Apr-2012" }));
                ListOfFiles.Add(new MetaDataViewModel(new MetaData { Path = "/App", Icon = "folder_app", Modified = "17-Apr-2012" }));
                ListOfFiles.Add(new MetaDataViewModel(new MetaData { Path = "/file.cs", Icon = "page_white_csharp", Modified = "17-Apr-2012" }));
                ListOfFiles.Add(new MetaDataViewModel(new MetaData { Path = "/file.pptx", Icon = "page_white_powerpoint", Modified = "17-Apr-2012" }));
                return;
            }


            if (!forceReload && !App.NeedDataRefresh)
            {
                if (ListOfFiles.Count > 0)
                {
                    return;
                }

                string key = GetCacheKeyName(metaData.Path);
                MetaData item = Cache.Current.Get<MetaData>(key);

                if (item != null)
                {
                    AddMetadataToListOfFiles(item);
                    return;
                }
            }

            App.NeedDataRefresh = false;
            if (!IsNetworkAvaialble)
            {
                MessageBox.Show(Labels.ConnectionErrorMessage, Labels.ConnectionErrorTitle, MessageBoxButton.OK);
                return;
            }

            IsLoading = true;
            Client.GetMetaDataAsync(metaData.Path, GetMetadataHandler, Fail);
        }

        void GetMetadataHandler(MetaData metadata)
        {
            string key = GetCacheKeyName(metadata.Path);
            Cache.Current.Add(key, metadata);

            AddMetadataToListOfFiles(metadata);
            IsLoading = false;
        }

        void AddMetadataToListOfFiles(MetaData metadata)
        {
            ListOfFiles.Clear();

            bool showFilesFirst = AppSettings.ShowFilesFirst;
            var items = metadata.Contents.OrderBy(p => p.Is_Dir == showFilesFirst);

            foreach (var item in items)
            {
                var temp = item;
                var vm = Ioc.GetInstance<MetaDataViewModel>(temp.Path);
                if (vm != null)
                {
                    ListOfFiles.Add(vm);
                }
                else
                {
                    var tempVm = new MetaDataViewModel(temp);
                    ListOfFiles.Add(tempVm);
                    Ioc.Register(() => tempVm, temp.Path);
                }
            }

            RaisePropertyChanged("ListOfFiles");
        }

        protected void Fail(DropboxException ed)
        {
            IsLoading = false;
            Deployment.Current.Dispatcher.BeginInvoke(() => Utilities.ShowProgressIndicator(false));
            MessageBox.Show(Labels.GenericErrorMessage, Labels.ErrorTitle, MessageBoxButton.OK);
        }

        void RegisterMessengersHandler()
        {
            Messenger.Default.Register<IRestResponse>(this, MessengerToken.Refresh, RefreshMessengerHandler);
        }

        void UnregisterMessengersHandler()
        {
            Messenger.Default.Unregister<IRestResponse>(this, MessengerToken.Refresh, RefreshMessengerHandler);
        }

        string GetCacheKeyName(string path)
        {
            if (path == "/")
            {
                return "root";
            }

            return string.Format("MetaData{0}", path.Replace("/", "·"));
        }

        public override void Cleanup()
        {
            Messenger.Default.Unregister<IRestResponse>(this, MessengerToken.Refresh, RefreshMessengerHandler);
            base.Cleanup();
        }

        string _title;

        ICommand _registerMessengers;
        ICommand _unregisterMessengers;
        ICommand _navigateCommand;
        ICommand _refreshItemsCommand;
        ICommand _delete;
        ICommand _loadItemsCommand;
        ICommand _uploadCommand;

        MetaData _metaDataCache;
        ObservableCollection<MetaDataViewModel> _files;
    }
}