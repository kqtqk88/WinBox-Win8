using DropNet.Exceptions;
using DropNet.Models;
using GalaSoft.MvvmLight.Command;
using Microsoft.Phone.Tasks;
using System;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Input;
using WinBox.Model;
using WinBox.Resources;
using WinBox.Utility;

namespace WinBox.ViewModels
{
    public class SettingsViewModel : WinBoxViewModel
    {
        ICommand _refreshStatsCommand;
        ICommand _logOutCommand;
        ICommand _shareReferralCommand;
        AccountInfo _account;

        /// <summary>
        /// Gets the page loaded command.
        /// </summary>
        public ICommand RefreshStats
        {
            get
            {
                if (_refreshStatsCommand == null)
                {
                    _refreshStatsCommand = new RelayCommand(RefreshStatsHandler);
                }

                return _refreshStatsCommand;
            }
        }

        public bool AutoUploadEnabled { get; set; }

        public ICommand ShareReferralCommand
        {
            get
            {
                if (_shareReferralCommand == null)
                {
                    _shareReferralCommand = new RelayCommand<ShareOption>(ShareReferralHandler);
                }

                return _shareReferralCommand;
            }
        }

        public bool ExitConfirmation
        {
            get
            {
                return AppSettings.ExitConfirmation;
            }
            set
            {
                AppSettings.ExitConfirmation = value;
            }
        }

        public bool ShowFilesFirst
        {
            get
            {
                return AppSettings.ShowFilesFirst;
            }
            set
            {
                AppSettings.ShowFilesFirst = value;
                App.NeedDataRefresh = true;
            }
        }

        private void ShareReferralHandler(ShareOption option)
        {
            if (option == ShareOption.Email)
            {
                var task = new EmailComposeTask();
                task.Subject = "Get Dropbox";
                task.Body = string.Format("I would like you to join Dropbox, here is the referral link {0}",
                                          Account.referral_link);
                task.Show();
            }

            if (option == ShareOption.Social)
            {
                var task = new ShareLinkTask
                    {
                        LinkUri = new Uri(Account.referral_link),
                        Message = "Join the awesome cloud storage service.",
                        Title = "Dropbox register"
                    };
                task.Show();
            }
        }

        /// <summary>
        /// Gets the log out command.
        /// </summary>
        public ICommand LogOut
        {
            get
            {
                if (_logOutCommand == null)
                {
                    _logOutCommand = new RelayCommand(LogOutHandler);
                }

                return _logOutCommand;
            }
        }

        void LogOutHandler()
        {
            App.ClearUserLogin();
            ClearAccountInfromation();
        }

        void ClearAccountInfromation()
        {
            var settings = IsolatedStorageSettings.ApplicationSettings;
            settings.Remove("AccountInfo");
            settings.Save();
        }

        /// <summary>
        /// Gets the account information.
        /// </summary>
        public AccountInfo Account
        {
            get
            {
                if (IsInDesignMode)
                {
                    var a = new AccountInfo
                    {
                        country = "India",
                        email = "a@aa.com",
                        display_name = "Prashant",
                        referral_link = "http://prashantvc.com",
                        quota_info = new QuotaInfo { normal = 100, quota = 500, shared = 20 }
                    };

                    QuotaInformation = new Quota(a.quota_info);
                    return a;
                }

                return _account;
            }

            private set
            {
                _account = value;
                RaisePropertyChanged("Account");
                RaisePropertyChanged("QuotaInformation");
            }
        }

        /// <summary>
        /// Gets the quota information.
        /// </summary>
        public Quota QuotaInformation
        {
            get;
            private set;
        }

        void RefreshStatsHandler()
        {
            RestoreAccountInformation();

            if (Account == null && !IsNetworkAvaialble)
            {
                MessageBox.Show(Labels.ConnectionErrorMessage, Labels.ConnectionErrorTitle, MessageBoxButton.OK);
                return;
            }

            try
            {
                Utilities.ShowProgressIndicator(true, "Getting your stats...");
                App.DropboxClient.AccountInfoAsync(OnGetAccountInfo, OnFail);

            }
            catch
            {
                Utilities.ShowProgressIndicator(false);
            }
        }

        private void OnFail(DropboxException ex)
        {
            MessageBox.Show(Labels.UserInfoError, Labels.ErrorTitle, MessageBoxButton.OK);
        }

        void RestoreAccountInformation()
        {
            var account = Cache.Current.Get<AccountInfo>("AccountInfo");
            if (account != null)
            {
                QuotaInformation = new Quota(account.quota_info);
                Account = account;
                RaisePropertyChanged("Account");
                RaisePropertyChanged("QuotaInformation");
            }
        }

        void OnGetAccountInfo(AccountInfo acc)
        {
            Utilities.ShowProgressIndicator(false);
            Account = acc;
            QuotaInformation = new Quota(acc.quota_info);
            RaisePropertyChanged("QuotaInformation");

            SaveAccountInformation();
        }

        void SaveAccountInformation()
        {
            Cache.Current.Add("AccountInfo", Account, DateTime.Now.AddMonths(3), TimeSpan.Zero);
        }
    }
}
