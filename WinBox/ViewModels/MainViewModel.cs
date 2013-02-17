using WinBox.Modules;
using WinBox.Utility;

namespace WinBox.ViewModels
{
    public class MainViewModel : FolderViewModel
    {
        public MainViewModel(IFileSytemOperations fileSytemOperations, INavigationService navigationService)
            : base(fileSytemOperations, navigationService)
        {
          
        }
    }
}
