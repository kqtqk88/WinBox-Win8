using System;
using System.Windows.Navigation;

namespace WinBox.Utility
{
    internal class CustomUriMapper : UriMapperBase
    {
        public override Uri MapUri(Uri uri)
        {
            // The incoming URI
            string tempUri = uri.ToString();

            // Search for a specific deep link URI keyword
            if (tempUri.Contains("ConfigurePhotosUploadSettings"))
            {
                // Launch to the auto-upload settings page.
                return new Uri("/Settings.xaml", UriKind.Relative);
            }

            // Otherwise perform normal launch.
            return uri;
        }
    }
}
