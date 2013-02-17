using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinBox.Utility
{
    public static class AppSettings
    {
        static  string ExitConfirm = "ExitConfirmation";

        public static bool ExitConfirmation
        {
            get
            {
                var appSettings = IsolatedStorageSettings.ApplicationSettings;
                return appSettings.Contains(ExitConfirm) && (bool)appSettings[ExitConfirm];
            }
            set
            {
                var appSettings = IsolatedStorageSettings.ApplicationSettings;
                if (appSettings.Contains(ExitConfirm))
                {
                    appSettings[ExitConfirm] = value;
                }
                else
                {
                    appSettings.Add(ExitConfirm, value);
                }

                appSettings.Save();
            }
        }

        static string ShowFilesFirstName = "ShowFilesFirst";

        public static bool ShowFilesFirst
        {
            get
            {
                var appSettings = IsolatedStorageSettings.ApplicationSettings;
                return appSettings.Contains(ShowFilesFirstName) && (bool)appSettings[ShowFilesFirstName];
            }
            set
            {
                var appSettings = IsolatedStorageSettings.ApplicationSettings;
                if (appSettings.Contains(ShowFilesFirstName))
                {
                    appSettings[ShowFilesFirstName] = value;
                }
                else
                {
                    appSettings.Add(ShowFilesFirstName, value);
                }

                appSettings.Save();
            }
        }
    }
}
