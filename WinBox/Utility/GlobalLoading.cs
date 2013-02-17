using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace JeffWilcox.FourthAndMayor
{
    public class GlobalLoading : INotifyPropertyChanged
    {
        private ProgressIndicator _mangoIndicator;

        private GlobalLoading()
        {
        }

        public void Initialize(PhoneApplicationFrame frame)
        {
            // If using AgFx:
            //DataManager.Current.PropertyChanged += OnDataManagerPropertyChanged;

            _mangoIndicator = new ProgressIndicator();
            frame.Navigated += OnRootFrameNavigated;
        }

        private void OnRootFrameNavigated(object sender, NavigationEventArgs e)
        {
            // Use in Mango to share a single progress indicator instance.
            var ee = e.Content;
            _page = ee as PhoneApplicationPage;
            if (_page != null)
            {   
                SystemTray.SetBackgroundColor(_page, Color.FromArgb(255, 240,249,255));
                SystemTray.SetForegroundColor(_page, Color.FromArgb(255, 31,117,204));
                _page.SetValue(SystemTray.OpacityProperty, 0.99);
                _page.SetValue(SystemTray.ProgressIndicatorProperty, _mangoIndicator);
            }
        }

        private void OnDataManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ("IsLoading" == e.PropertyName)
            {
                //IsDataManagerLoading = DataManager.Current.IsLoading;
                NotifyValueChanged();
            }
        }

        private static GlobalLoading _in;
        public static GlobalLoading Instance
        {
            get { return _in ?? (_in = new GlobalLoading()); }
        }

        public bool IsDataManagerLoading { get; set; }

        public bool ActualIsLoading
        {
            get
            {
                return IsLoading || IsDataManagerLoading;
            }
        }

        private int _loadingCount;
        PhoneApplicationPage _page;

        public bool IsLoading
        {
            get
            {
                return _loadingCount > 0;
            }
            set
            {
                bool loading = IsLoading;
                if (value)
                {
                    ++_loadingCount;
                }
                else
                {
                    --_loadingCount;
                }

                NotifyValueChanged();
            }
        }

        private void NotifyValueChanged()
        {
            if (_page == null)
            {   
                 return;
            }

            if (_mangoIndicator != null)
            {
                if (_loadingCount < 1)
                {
                    SetMessage(string.Empty);
                    _page.SetValue(SystemTray.IsVisibleProperty, false);
                }

                _mangoIndicator.IsIndeterminate = _loadingCount > 0 || IsDataManagerLoading;

                if (_mangoIndicator.IsIndeterminate)
                {
                    _page.SetValue(SystemTray.IsVisibleProperty, true);
                }

                // for now, just make sure it's always visible.
                if (_mangoIndicator.IsVisible == false)
                {
                    _mangoIndicator.IsVisible = true;
                }
            }
        }

        public void SetMessage(string message)
        {
            _mangoIndicator.Text = message;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}