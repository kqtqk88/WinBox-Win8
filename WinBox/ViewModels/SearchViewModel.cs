using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;
using System.Windows.Input;
using DropNet.Exceptions;
using DropNet.Models;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using WinBox.Resources;
using WinBox.Utility;

namespace WinBox.ViewModels
{
    public class SearchViewModel : WinBoxViewModel
    {
        private ObservableCollection<MetaDataViewModel> _searchResult;
        private ICommand _search;
        private bool _found;
        private bool _searchCurrentFolder;
        private ICommand _navigateCommand;
        readonly INavigationService _navigationService;

        public SearchViewModel()
        {
            _searchResult = new ObservableCollection<MetaDataViewModel>();
            _navigationService = SimpleIoc.Default.GetInstance<INavigationService>();

            if (IsInDesignMode)
            {
                _searchResult.Add(new MetaDataViewModel(new MetaData { Path = "/Path1", Icon = "folder", Modified = "17-Apr-2012" }));
                _searchResult.Add(new MetaDataViewModel(new MetaData { Path = "/App", Icon = "folder_app", Modified = "17-Apr-2012" }));
                _searchResult.Add(new MetaDataViewModel(new MetaData { Path = "/file.cs", Icon = "page_white_csharp", Modified = "17-Apr-2012" }));
                _searchResult.Add(new MetaDataViewModel(new MetaData { Path = "/file.pptx", Icon = "page_white_powerpoint", Modified = "17-Apr-2012" }));
            }
        }

        public ICommand Search
        {
            get
            {
                return _search ??
                    (_search = new RelayCommand<string>(SearchHandler));
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

        void NavigateHandler(MetaDataViewModel data)
        {
            if (data.MetaData.Is_Dir)
            {
                string path = string.Format("/FolderPage.xaml?path={0}&icon={1}", data.MetaData.Path, data.MetaData.Icon);
                _navigationService.Navigate(new Uri(path, UriKind.Relative));
            }
            else if (data.MetaData.Thumb_Exists)
            {
                App.MetadataViewModel = data;
                _navigationService.Navigate(new Uri("/PhotoViewPage.xaml", UriKind.Relative));
            }
            else
            {
                App.FileMetaData = data.MetaData;
                _navigationService.Navigate(new Uri("/FilePage.xaml", UriKind.Relative));
            }
        }

        public ObservableCollection<MetaDataViewModel> SearchResult
        {
            get { return _searchResult; }
            set
            {
                _searchResult = value;
                RaisePropertyChanged("SearchResult");
            }
        }

        public bool SearchCurrentFolder
        {
            get { return _searchCurrentFolder; }
            set
            {
                _searchCurrentFolder = value;
                RaisePropertyChanged("SearchCurrentFolder");
            }
        }

        public bool Found
        {
            get { return _found; }
            set
            {
                _found = value;
                RaisePropertyChanged("Found");
            }
        }

        SimpleIoc Ioc
        {
            get { return SimpleIoc.Default; }
        }

        private void SearchHandler(string searchString)
        {
            if (!IsNetworkAvaialble)
            {
                MessageBox.Show(Labels.ConnectionErrorMessage, Labels.ConnectionErrorTitle, MessageBoxButton.OK);
                return;
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                IsLoading = true;

                if (SearchCurrentFolder)
                {
                    App.DropboxClient.SearchAsync(searchString, "TODO:", OnSearchComplete, Fail);
                    return;
                }

                App.DropboxClient.SearchAsync(searchString, OnSearchComplete, Fail);
            }
        }

        private void Fail(DropboxException ex)
        {
            Found = false;
            IsLoading = false;

            if (ex.StatusCode == HttpStatusCode.NotFound)
            {
                MessageBox.Show(Labels.ConnectionErrorMessage, Labels.ConnectionErrorTitle, MessageBoxButton.OK);
            }

            MessageBox.Show(Labels.GenericErrorMessage, Labels.ErrorTitle, MessageBoxButton.OK);
        }

        private void OnSearchComplete(List<MetaData> results)
        {
            IsLoading = false;
            _searchResult.Clear();

            Found = results.Count > 0;

            if (results.Count < 1)
            {
                MessageBox.Show(Labels.NoResultsFound, Labels.SearchTitle, MessageBoxButton.OK);
            }

            foreach (var item in results)
            {
                var temp = item;

                var vm = Ioc.GetInstance<MetaDataViewModel>(temp.Path);
                if (vm != null)
                {
                    _searchResult.Add(vm);
                }
                else
                {
                    var tempVm = new MetaDataViewModel(temp);
                    _searchResult.Add(tempVm);
                    Ioc.Register(() => tempVm, temp.Path);
                }
            }

            RaisePropertyChanged("SearchResult");
        }
    }
}