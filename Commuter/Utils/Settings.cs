using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Commuter
{
    public class Settings
    {
        private bool _enableTileNotifications = true;
        private bool _enableToastNotifications = false;
        private bool _isFirstRun = true;
        private bool _shownRatingRequest = false;
        private DateTime _firstRunDate = DateTime.Now;
        public event EventHandler SettingsChanged;

        public Settings()
        {
            this.FirstRunDate = DateTime.Now;
            this.HasNotifiedToast = false;
        }

        public bool EnableTileNotifications
        {
            get { return _enableTileNotifications; }
            set
            {
                if (value != _enableTileNotifications)
                {
                    _enableTileNotifications = value;

                    if (null != SettingsChanged)
                    {
                        SettingsChanged(this, null);
                    }
                }
            }
        }
        public bool EnableToastNotifications
        {
            get { return _enableToastNotifications; }
            set
            {
                if (value != _enableToastNotifications)
                {
                    _enableToastNotifications = value;

                    if (null != SettingsChanged)
                    {
                        SettingsChanged(this, null);
                    }
                }
            }
        }

        public bool IsFirstRun
        {
            get { return _isFirstRun; }
            set
            {
                _isFirstRun = value;
            }
        }

        public bool HasNotifiedToast
        {
            get;
            set;
        }

        public bool ShownRatingRequest
        {
            get { return _shownRatingRequest; }
            set
            {
                _shownRatingRequest = value;
            }
        }

        public DateTime FirstRunDate
        {
            get { return _firstRunDate; }
            set
            {
                _firstRunDate = value;
            }
        }
    }
}
