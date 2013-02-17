using DropNet.Models;
using GalaSoft.MvvmLight;
using WinBox.Modules;
using GalaSoft.MvvmLight.Ioc;
using WinBox.Utility;

namespace WinBox.ViewModels
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {   
            if (!ViewModelBase.IsInDesignModeStatic)
            {
                SimpleIoc.Default.Register<INavigationService, NavigationService>();
                SimpleIoc.Default.Register<IFileSytemOperations, FileSytemOperations>();

                SimpleIoc.Default.Register<FolderViewModel>();
                SimpleIoc.Default.Register<MainViewModel>();
                SimpleIoc.Default.Register<UploadViewModel>();
            }
        }

        public MainViewModel Main
        {
            get
            {
                if (_main == null)
                {
                    _main = SimpleIoc.Default.GetInstance<MainViewModel>();
                }

                return _main;
            }
        }

        public FolderViewModel FolderViewModel
        {
            get
            {
                return new FolderViewModel();
            }
        }

        public SettingsViewModel Settings
        {
            get { return _settings ?? (_settings = new SettingsViewModel()); }
        }

        public CreateFolderViewModel CreateFolderViewModel
        {
            get
            {
                return _createFolderViewModel ??
                    (_createFolderViewModel = new CreateFolderViewModel());
            }
        }

        public UploadViewModel UploadViewModel
        {
            get { return _uploadViewModel ?? (_uploadViewModel = SimpleIoc.Default.GetInstance<UploadViewModel>()); }
        }

        public MetaDataViewModel MetaDataViewModel
        {
            get
            {
                return _metaDataViewModel ??
                    (_metaDataViewModel = new MetaDataViewModel(new MetaData
                                                                    {
                                                                        Path = "/image.op",
                                                                        Icon = "page_white_compressed",
                                                                        Bytes = 1024,
                                                                        Modified = "1/1/2000"
                                                                    }));
            }
        }

        public SearchViewModel SearchViewModel
        {
            get { return _searchViewModel ?? (_searchViewModel = new SearchViewModel()); }
        }

        SettingsViewModel _settings;
        MainViewModel _main;
        CreateFolderViewModel _createFolderViewModel;
        UploadViewModel _uploadViewModel;
        MetaDataViewModel _metaDataViewModel;
        private SearchViewModel _searchViewModel;
    }
}
