using WinBox.Resources;

namespace WinBox.Utility
{
    public class LocalizedStrings
    {
        static readonly Labels _localizedResources = new Labels();

        public Labels LocalizedResources
        {
            get
            {
                return _localizedResources;
            }
        }
    }
}