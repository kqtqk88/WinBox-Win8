using System.Collections.ObjectModel;
using DropNet.Models;
using WinBox.Utility;
using GalaSoft.MvvmLight.Ioc;

namespace WinBox.ViewModels
{
    public interface IFavouriteViewModel
    {
        void Add(MetaData metadata);
        void Remove(string path);
        bool Contains(string path);

        ObservableCollection<MetaDataViewModel> Favourites { get; }
    }

    public class FavouriteViewModel : IFavouriteViewModel
    {
        public FavouriteViewModel()
        {
            _cache = FavouritesManager.Instance;
        }

        public void Add(MetaData metadata)
        {
            string key = GetFavouriteKeyName(metadata.Path);
            _cache.Add(key, metadata);
        }

        public void Remove(string path)
        {
            string key = GetFavouriteKeyName(path);
            _cache.Remove(key);
        }

        public bool Contains(string path)
        {
            return _cache.Contains(GetFavouriteKeyName(path));
        }

        public ObservableCollection<MetaDataViewModel> Favourites
        {
            get
            {
                var items = _cache.GetItems<MetaData>();
                _favouriteItems.Clear();
                foreach (var item in items)
                {
                    var temp = item;
                    var vm = SimpleIoc.Default.GetInstance<MetaDataViewModel>(temp.Path);
                    if (vm != null)
                    {
                        _favouriteItems.Add(vm);
                    }
                    else
                    {
                        var tempVm = new MetaDataViewModel(temp);
                        _favouriteItems.Add(tempVm);
                        SimpleIoc.Default.Register(() => tempVm, temp.Path);
                    }
                }

                return _favouriteItems;
            }
        }

        string GetFavouriteKeyName(string path)
        {
            if (path == "/")
            {
                return "root";
            }

            return string.Format("fav{0}", path.Replace("/", "·"));
        }

        private readonly FavouritesManager _cache;
        readonly ObservableCollection<MetaDataViewModel> _favouriteItems 
            = new ObservableCollection<MetaDataViewModel>();
    }
}