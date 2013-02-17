using DropNet;
using DropNet.Models;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System;
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
            string toastMessage = "Upload started";
            ShellToast toast = new ShellToast();
            toast.Title = "Auto Upload";
            toast.Content = toastMessage;
            toast.Show();

            using (var library = new MediaLibrary())
            {
                var items = library.Pictures.Where(p => p.Album.Name.ToLower() == "camera roll" && !p.IsDisposed);
                var imagesToUpload = new Queue<Picture>(items);

                UploadImage(imagesToUpload);

                //foreach (var item in items)
                //{
                //    var image = item.GetImage();
                //    DropboxClient.UploadFileAsync("/test", item.Name, image, m =>
                //    {
                //        Debug.WriteLine("Uploaded: {0}", m.Path);
                //        NotifyComplete();

                //    },
                //    e =>
                //    {
                //        Debug.WriteLine("Error:{0}", item.Name);
                //        NotifyComplete();
                //    });

                //}
            }


            // If debugging is enabled, launch the agent again in one minute.
            //#if DEBUG
            //            ScheduledActionService.LaunchForTest(task.Name, TimeSpan.FromSeconds(60));
            //#endif

            // Call NotifyComplete to let the system know the agent is done working.
        }

        private void UploadImage(Queue<Picture> imagesToUpload)
        {
            var item = imagesToUpload.Dequeue();
            var image = item.GetImage();

            DropboxClient.UploadFileAsync("/test", item.Name, image, m =>
            {
                if (imagesToUpload.Count > 0)
                {
                    Debug.WriteLine("Uploaded:{0}", item.Name);
                    UploadImage(imagesToUpload);
                }
                else
                {
                    NotifyComplete();
                }

                image.Dispose();
            }, e =>
            {
                image.Dispose();
                Debug.WriteLine("Error");
                NotifyComplete();
            });
        }


        static string ApiKey
        {
            get
            {
                return "g1znvbjklqyeve8";
            }
        }

        static string AppSecret
        {
            get
            {
                return "13fvio3d1bzhkfv";
            }
        }

        DropNetClient DropboxClient
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

        UserLogin UserLogin
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

        UserLogin _userToken;
        DropNetClient _dropboxClient;
    }
}