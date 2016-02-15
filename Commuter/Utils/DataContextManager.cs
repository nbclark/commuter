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
using Microsoft.Phone.Shell;
using MobileSrc.Commuter.Shared;

namespace Commuter
{
    public static class DataContextManager
    {
        private static object _lockObject = new object();

        public static Settings Settings
        {
            get;
            set;
        }

        public static CommuteCollection Commutes
        {
            get;
            set;
        }

        public static MobileSrc.Commuter.Shared.CommuteDefinition SelectedCommute
        {
            get;
            set;
        }

        public static MobileSrc.Commuter.Shared.RouteDefinition SelectedRoute
        {
            get;
            set;
        }

        public static void HandleLaunch()
        {
            Commutes = CommuteStorage.Load();
            Settings = CommuteStorage.LoadSettings();

            if (Commutes.Count == 0)
            {
                Commutes.Add(new CommuteDefinition());
                Commutes[0].Name = "untitled";
                Commutes[0].IsNew = true;
            }

            DataContextManager.SelectedCommute = Commutes[0];

            Settings.SettingsChanged += new EventHandler(_settings_SettingsChanged);
        }

        static void _settings_SettingsChanged(object sender, EventArgs e)
        {
            Utils.RegisterDevice();
            Utils.BindNotifications();
        }

        public static void HandleActivated()
        {
            Settings = PhoneApplicationService.Current.State["Settings"] as Settings;
            Commutes = PhoneApplicationService.Current.State["Commutes"] as CommuteCollection;
            SelectedCommute = PhoneApplicationService.Current.State["SelectedCommute"] as MobileSrc.Commuter.Shared.CommuteDefinition;
            SelectedRoute = PhoneApplicationService.Current.State["SelectedRoute"] as MobileSrc.Commuter.Shared.RouteDefinition;

            Settings.SettingsChanged += new EventHandler(_settings_SettingsChanged);
        }

        public static void HandleDeactivated()
        {
            PhoneApplicationService.Current.State["Settings"] = Settings;
            PhoneApplicationService.Current.State["Commutes"] = Commutes;
            PhoneApplicationService.Current.State["SelectedCommute"] = SelectedCommute;
            PhoneApplicationService.Current.State["SelectedRoute"] = SelectedRoute;

            HandleClosing();
        }

        public static void HandleClosing()
        {
            Save();
        }

        public static void Save()
        {
            CommuteStorage.Save();
        }
    }
}
