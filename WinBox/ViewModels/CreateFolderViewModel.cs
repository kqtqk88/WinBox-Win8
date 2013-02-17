using System.Net;
using System.Windows;
using System.Windows.Input;
using DropNet.Exceptions;
using DropNet.Models;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using WinBox.Model;
using WinBox.Resources;
using WinBox.Utility;

namespace WinBox.ViewModels
{
    public class CreateFolderViewModel : WinBoxViewModel
    {
        MetaData _metadata;
        ICommand _createFolderCommand;

        public CreateFolderViewModel()
        {
            if (IsInDesignMode)
            {
                _metadata = new MetaData { Path = "/photos" };
            }
        }

        public string Parent
        {
            get
            {
                if (string.IsNullOrEmpty(_metadata.Name))
                {
                    return "root";
                }

                return _metadata.Name;
            }
        }

        public ICommand CreateFolderCommand
        {
            get
            {
                return _createFolderCommand ??
                    (_createFolderCommand = new RelayCommand<string>(CreateFolderCommandHandler));
            }
        }

        void CreateFolderCommandHandler(string foldername)
        {
            if (!IsNetworkAvaialble)
            {
                MessageBox.Show(Labels.ConnectionErrorMessage, Labels.ConnectionErrorTitle, MessageBoxButton.OK);
                return;
            }

            string folderPath = string.Format("{0}/{1}", _metadata.Path, foldername);

            Utilities.ShowProgressIndicator(true, "creating folder...");
            App.DropboxClient.CreateFolderAsync(folderPath.Replace("//", "/"), RequestHanler, Fail);
        }

        void RequestHanler(MetaData metaData)
        {
            Utilities.ShowProgressIndicator(false);
            MessageBox.Show(string.Format(Labels.FolderCreated, metaData.Name),
                            Labels.FolderCreatedTitle, MessageBoxButton.OK);
            Messenger.Default.Send(metaData, MessengerToken.Created);
        }

        void Fail(DropboxException ed)
        {
            Utilities.ShowProgressIndicator(false);
            switch ((int)ed.StatusCode)
            {
                case (int)HttpStatusCode.NotFound:
                    MessageBox.Show(Labels.ConnectionErrorMessage, Labels.ConnectionErrorTitle, MessageBoxButton.OK);
                    return;

                case (int)HttpStatusCode.Forbidden:
                    MessageBox.Show(Labels.FolderExist, Labels.ErrorTitle, MessageBoxButton.OK);
                    return;

                case 507:
                    MessageBox.Show(Labels.NoSpaceAvailable, Labels.ErrorTitle, MessageBoxButton.OK);
                    return;
            }

            MessageBox.Show(ed.StatusCode.ToString());
        }

        public void SetMetaData(MetaData metaData)
        {
            _metadata = metaData;
            RaisePropertyChanged("FolderName");
        }
    }

}