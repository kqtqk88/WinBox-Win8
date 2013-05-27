using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Windows;
using Microsoft.Xna.Framework.Media;
using System.Linq;
using System.Collections.Generic;

namespace WinBoxAgent
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        private static volatile bool _classInitialized;


        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        public ScheduledAgent()
        {
            Debug.WriteLine("Created");

            if (!_classInitialized)
            {
                _classInitialized = true;
                // Subscribe to the managed exception handler
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                    {
                        Application.Current.UnhandledException += ScheduledAgent_UnhandledException;
                    });
            }
        }

        /// Code to execute on Unhandled Exceptions
        private void ScheduledAgent_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
                NotifyComplete();
            }
        }

        /// <summary>
        /// Agent that runs a scheduled task
        /// </summary>
        /// <param name="task">
        /// The invoked task
        /// </param>
        /// <remarks>
        /// This method is called when a periodic or resource intensive task is invoked
        /// </remarks>
        protected override void OnInvoke(ScheduledTask task)
        {
            var h = new OAuthMessageHandler(new HttpClientHandler());
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://api-content.dropbox.com/1/dropbox/");

            string toastMessage = "Upload started";
            ShellToast toast = new ShellToast();
            toast.Title = "Auto Upload";
            toast.Content = toastMessage;
            toast.Show();

            PostFile(client);
        }

        private async void PostFile(HttpClient client)
        {
            using (var library = new MediaLibrary())
            {
                var items = library.Pictures.Where(p => p.Album.Name.ToLower() == "camera roll" && !p.IsDisposed);
                var imagesToUpload = new Queue<Picture>(items);
                using (var image = imagesToUpload.Dequeue())
                {   
                    using (var imageData = image.GetImage())
                    {
                        var content = new StreamContent(imageData);
                        var response = await client.PutAsync("files_put/" + image.Name, content);

                        Debug.WriteLine(response);
                        NotifyComplete();
                    }

                }
            }
        }
    }
}