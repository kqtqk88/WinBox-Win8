using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DropNet.Models;
using Telerik.Windows.Controls;

namespace WinBox.Utility
{
    public class TileHelper
    {
        public static void CreateTile(MetaData metaData)
        {
            ImageSource imageSource = new BitmapImage(new Uri(string.Format("/TileIcon/{0}.png", metaData.Icon), UriKind.Relative));
            var visual = new LiveTileVisual();
            visual.SetProperties(metaData.Name, imageSource);
            var data = new RadExtendedTileData
                           {
                               VisualElement = visual
                           };

            string path = string.Format("/FolderPage.xaml?path={0}&icon={1}", metaData.Path, metaData.Icon);


            LiveTileHelper.CreateTile(data, new Uri(path, UriKind.Relative));
        }

        public static bool IsPinned(MetaData metaData)
        {
            string path = string.Format("/FolderPage.xaml?path={0}&icon={1}", metaData.Path, metaData.Icon);
            var tile = LiveTileHelper.GetTile(new Uri(path, UriKind.Relative));
            
            return tile != null;
        }

        public static void RemoveTile(MetaData metaData)
        {
            string path = string.Format("/FolderPage.xaml?path={0}&icon={1}", metaData.Path, metaData.Icon);
            var tile = LiveTileHelper.GetTile(new Uri(path, UriKind.Relative));

            if (tile != null)
            {
                tile.Delete();
            }
        }
    }
}