using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.Serialization;

namespace WinBox.Utility
{
    public class FavouritesManager
    {
        private FavouritesManager()
        {

        }

        public static FavouritesManager Instance
        {
            get
            {
                return _instance ?? (_instance = new FavouritesManager());
            }
        }

        public void Add(string key, object value)
        {
            lock (_sync)
            {
                //Removes only if exist
                Remove(key);

                NormalWrite(key, value);
            }
        }

        public void Remove(string key)
        {
            lock (_sync)
            {
                if(!Contains(key))
                {
                    return;
                }

                var file = _myStore.GetFileNames(string.Format("fav·{0}*", key)).FirstOrDefault();
                if (!string.IsNullOrEmpty(file))
                {
                    _myStore.DeleteFile(file);
                }
            }
        }

        public bool Contains(string key)
        {
            lock (_sync)
            {
                var file = _myStore.GetFileNames(string.Format("fav·{0}*", key)).FirstOrDefault();
                return (!string.IsNullOrEmpty(file));
            }
        }

        public IEnumerable<T> GetItems<T>()
        {
            var files = _myStore.GetFileNames("fav·*");

            foreach (var file in files)
            {
                yield return NormalRead<T>(file);
            }
        }

        void NormalWrite(string fileName, object value)
        {

            using (var isolatedStorageFileStream =
                 new IsolatedStorageFileStream(fileName, FileMode.OpenOrCreate, _myStore))
            {
                var s = new DataContractSerializer(value.GetType());

                s.WriteObject(isolatedStorageFileStream, value);
            }
        }        

        T NormalRead<T>(string fileName)
        {
            using (var isolatedStorageFileStream = new IsolatedStorageFileStream(fileName, FileMode.Open, _myStore))
            {
                var s = new DataContractSerializer(typeof(T));

                object value = s.ReadObject(isolatedStorageFileStream);
                isolatedStorageFileStream.Close();
                return (T)value;
            }
        }

        static FavouritesManager _instance;
        readonly object _sync = new object();
        readonly IsolatedStorageFile _myStore = IsolatedStorageFile.GetUserStoreForApplication();
    }
}