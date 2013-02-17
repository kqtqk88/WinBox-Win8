using System;
using System.Net;
using JeffWilcox.FourthAndMayor;

namespace WinBox.Utility
{
    public class Utilities
    {

        public static void ShowProgressIndicator(bool show, string message = "")
        {
            GlobalLoading.Instance.IsLoading = show;
            GlobalLoading.Instance.SetMessage(message);
        }

        public static string GetNavPath(string path)
        {
            var temp = path.Replace("/", string.Empty);
            return string.Format("/{0}", HttpUtility.UrlEncode(temp));
        }

        public static string GetCacheKeyName(string path)
        {
            path = path.Replace("\\", "/");

            if (path == "/")
            {
                return "root";
            }
            return string.Format("MetaData{0}", path.Replace("/", "·"));
        }

        public static string GetThumbnailFileName(string path)
        {
            return string.Format("Thumb{0}", path.Replace("/", "·"));
        }

        public static string FormatBytes(long bytes)
        {
            string[] suffix = { "B", "KB", "MB", "GB", "TB" };
            int i;
            double dblSByte = 0;
            for (i = 0; (int)(bytes / 1024) > 0; i++, bytes /= 1024)
            {
                dblSByte = bytes / 1024.0;
            }

            return string.Format("{0:0.00} {1}", dblSByte, suffix[i]);
        }

        public static string RelativeDate(DateTime date)
        {
            var ts = new TimeSpan(DateTime.Now.Ticks - date.Ticks);
            double delta = Math.Abs(ts.TotalSeconds);

            if (delta < 60)
            {
                return ts.Seconds == 1 ? "one second ago" : ts.Seconds + " seconds ago";
            }
            if (delta < 120)
            {
                return "a minute ago";
            }
            if (delta < 2700) // 45 * 60
            {
                return ts.Minutes + " minutes ago";
            }
            if (delta < 5400) // 90 * 60
            {
                return "an hour ago";
            }
            if (delta < 86400) // 24 * 60 * 60
            {
                return ts.Hours + " hours ago";
            }
            if (delta < 172800) // 48 * 60 * 60
            {
                return "yesterday";
            }
            if (delta < 2592000) // 30 * 24 * 60 * 60
            {
                return ts.Days + " days ago";
            }
            if (delta < 31104000) // 12 * 30 * 24 * 60 * 60
            {
                int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? "one month ago" : months + " months ago";
            }

            int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
            return years <= 1 ? "one year ago" : years + " years ago";
        }
    }
}