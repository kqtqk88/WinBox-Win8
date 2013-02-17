using System;
using System.Net;
using System.Windows;
using System.Windows.Input;
using DropNet.Exceptions;
using DropNet.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using WinBox.Model;
using WinBox.Modules;
using WinBox.Resources;
using WinBox.Utility;
using RestSharp;

namespace WinBox.ViewModels
{
    public class UploadViewModel : ViewModelBase
    {

        public UploadViewModel(IFileSytemOperations fileSytemOperations, INavigationService navigationService)
        {
            _fileSytemOperations = fileSytemOperations;
            _navigationService = navigationService;
        }

        public ICommand Upload
        {
            get
            {
                return _upload ??
                       (_upload = new RelayCommand<UploadData>(UploadHandler));
            }
        }

        public bool Uploading { get; private set; }

        private void UploadHandler(UploadData uploadData)
        {
            Utilities.ShowProgressIndicator(true, "Uploading...");
            Uploading = true;
            _fileSytemOperations.UploadFile(uploadData, OnUploadSuccess, OnFail);
        }

        void OnUploadSuccess(MetaData metaData)
        {
            Utilities.ShowProgressIndicator(false);
            Uploading = false;

            App.NeedDataRefresh = true;
            MessageBox.Show(string.Format("'{0}' uploaded successfully", metaData.Name), Labels.SuccessTitle,
                            MessageBoxButton.OK);

            if (_navigationService != null)
            {
                try
                {
                    _navigationService.GoBack();
                }

                catch
                {
                    _navigationService.Navigate(new Uri("/MainPage.xaml?refresh=true", UriKind.Relative));
                }
            }
        }

        void OnFail(DropboxException ed)
        {

            Utilities.ShowProgressIndicator(false);
            Uploading = false;
            switch (ed.StatusCode)
            {
                case HttpStatusCode.NotFound:
                case HttpStatusCode.LengthRequired:
                    MessageBox.Show(Labels.ConnectionErrorMessage, Labels.ConnectionErrorTitle, MessageBoxButton.OK);
                    return;
                case HttpStatusCode.BadRequest:
                    MessageBox.Show(Labels.FileExtensionError, Labels.ErrorTitle, MessageBoxButton.OK);
                    return;
            }

            MessageBox.Show(Labels.GenericErrorMessage, Labels.ErrorTitle, MessageBoxButton.OK);

        }

        ICommand _upload;
        readonly IFileSytemOperations _fileSytemOperations;
        private readonly INavigationService _navigationService;
    }
}