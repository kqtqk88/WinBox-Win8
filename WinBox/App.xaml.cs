using System.Windows;
using System.Windows.Navigation;
using DropNet;
using JeffWilcox.FourthAndMayor;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using DropNet.Models;
using System.IO.IsolatedStorage;
using Telerik.Windows.Controls;
using WinBox.ViewModels;
using WinBox.Utility;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace WinBox
{
    public partial class App : Application
    {
        private static DropNetClient _dropboxClient;
        private static LRUCache<string, LRUWrapper<BitmapImage>> _lruCache;

        public static MetaData FileMetaData { get; set; }


        public static string ApiKey
        {
            get
            {
                return "g1znvbjklqyeve8";
            }
        }

        public static string AppSecret
        {
            get
            {
                return "13fvio3d1bzhkfv";
            }
        }

        public static DropNetClient DropboxClient
        {
            get
            {
                if (_dropboxClient == null)
                {
                    _dropboxClient = new DropNetClient(ApiKey, AppSecret);

                    if (UserLogin != null)
                    {
                        _dropboxClient.UserLogin = UserLogin;
                    }
                }

                return _dropboxClient;
            }
        }

        public static UserLogin UserLogin
        {
            get
            {
                if (_userToken == null)
                {
                    IsolatedStorageSettings.ApplicationSettings.TryGetValue
                        ("userToken", out _userToken);
                }

                return _userToken;
            }
        }

        public static LRUCache<string, LRUWrapper<BitmapImage>> LruCache
        {
            get
            {
                if (_lruCache == null)
                {
                    _lruCache = new LRUCache<string, LRUWrapper<BitmapImage>>(50);
                }

                return _lruCache;
            }
        }

        static UserLogin _userToken;

        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        public static bool NeedDataRefresh { get; set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            ApplicationUsageHelper.Init("1.8.2.0");

            //Global exception handling
            var diagnostics = new RadDiagnostics { EmailTo = "prshntvc@gmail.com" };
            diagnostics.Init();


            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            var color = System.Windows.Media.Color.FromArgb(255, 31, 117, 204);

            ThemeManager.SetAccentColor(color);
            ThemeManager.OverrideOptions = ThemeManagerOverrideOptions.None;

            ThemeManager.ToLightTheme();

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                //TODO: Uncomment the code
                //Display the current frame rate counters.
                const bool helper = false;
                Current.Host.Settings.EnableFrameRateCounter = helper;
                MetroGridHelper.IsVisible = helper;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Disable the application idle detection by setting the UserIdleDetectionMode property of the
                // application's PhoneApplicationService object to Disabled.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {

        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            //IsolatedStorageExplorer.Explorer.RestoreFromTombstone();
            var settings = IsolatedStorageSettings.ApplicationSettings;

            if (settings.Contains("metadata"))
            {
                FileMetaData = (MetaData)settings["metadata"];
            }
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            var settings = IsolatedStorageSettings.ApplicationSettings;

            if (settings.Contains("metadata"))
            {
                settings["metadata"] = FileMetaData;
            }
            else
            {
                settings.Add("metadata", FileMetaData);
            }

            settings.Save();
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized;
        public static MetaDataViewModel MetadataViewModel;
        public static PhotoViewerViewModel PhotoViewerVM;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Assign the custom URI mapper class to the application frame.
            RootFrame.UriMapper = new CustomUriMapper();


            // Ensure we don't initialize again
            phoneApplicationInitialized = true;

            //Initialise Global loading progress
            GlobalLoading.Instance.Initialize(RootFrame);
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion

        public static void ClearUserLogin()
        {
            _userToken = null;
            IsolatedStorageSettings.ApplicationSettings.Remove("userToken");
            IsolatedStorageSettings.ApplicationSettings.Save();
        }
    }
}