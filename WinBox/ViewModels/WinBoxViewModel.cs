using GalaSoft.MvvmLight;
using Microsoft.Phone.Net.NetworkInformation;

namespace WinBox.ViewModels
{
    public class WinBoxViewModel : ViewModelBase
    {
        private bool _isLoading;

        protected bool IsNetworkAvaialble
        {
            get { return NetworkInterface.GetIsNetworkAvailable(); }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                RaisePropertyChanged("IsLoading");
            }
        }
    }
}