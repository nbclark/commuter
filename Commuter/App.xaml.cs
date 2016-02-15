using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Reflection;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Shell;
using MobileSrc.Commuter.Shared;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Marketplace;

namespace Commuter
{
    public partial class App : Application
    {
        public static string AccentColor;
        public static TransitioningContentControl TransitionControl;
        public static bool DisplaySplash = false;
        public static bool FirstLoad = false;
        public static LicenseInformation License = null;

        public static bool IsTrial
        {
            get
            {
                if (null == License)
                {
                    License = new LicenseInformation();
                }
                return License.IsTrial();
            }
        }

        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        public App()
        {
            AccentColor = ((Color)Application.Current.Resources["PhoneAccentColor"]).ToString().Replace("#", "");

            UnhandledException += new EventHandler<ApplicationUnhandledExceptionEventArgs>(Application_UnhandledException);
            Startup += new StartupEventHandler(App_Startup);

            InitializeComponent();

            RootFrame = new PhoneApplicationFrame();
            var ct = (ControlTemplate)this.Resources["TransitioningFrame"];
            RootFrame.Template = ct;
            RootFrame.Navigated += new System.Windows.Navigation.NavigatedEventHandler(RootFrame_Navigated);
            // debugging

            foreach (PropertyInfo property in typeof(ResourceStrings).GetProperties(BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static))
            {
                if (property.PropertyType == typeof(string))
                {
                    Application.Current.Resources.Add(property.Name, property.GetValue(null, null));
                }
            }
        }

        void RootFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
            {
                RootVisual = RootFrame;

                // Set the root visual to use the transitioning control.
                var ct = (ControlTemplate)this.Resources["TransitioningFrame"];
                (RootVisual as Microsoft.Phone.Controls.PhoneApplicationFrame).Template = ct;
                TiltEffect.SetIsTiltEnabled(this.RootVisual, true);
            }

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= new System.Windows.Navigation.NavigatedEventHandler(RootFrame_Navigated);
        }

        void Current_Launching(object sender, Microsoft.Phone.Shell.LaunchingEventArgs e)
        {
            DisplaySplash = true;
            FirstLoad = true;

            DataContextManager.HandleLaunch();
            Utils.InitializeNotifications();
        }

        void Current_Activated(object sender, Microsoft.Phone.Shell.ActivatedEventArgs e)
        {
            DisplaySplash = false;
            DataContextManager.HandleActivated();
            TransitionControl = new TransitioningContentControl();
        }

        void Current_Deactivated(object sender, Microsoft.Phone.Shell.DeactivatedEventArgs e)
        {
            DataContextManager.HandleDeactivated();
        }

        void Current_Closing(object sender, Microsoft.Phone.Shell.ClosingEventArgs e)
        {
            DataContextManager.HandleClosing();
        }

        void App_Startup(object sender, StartupEventArgs e)
        {
            Microsoft.Phone.Shell.PhoneApplicationService.Current.Activated += new EventHandler<Microsoft.Phone.Shell.ActivatedEventArgs>(Current_Activated);
            Microsoft.Phone.Shell.PhoneApplicationService.Current.Launching += new EventHandler<Microsoft.Phone.Shell.LaunchingEventArgs>(Current_Launching);
            Microsoft.Phone.Shell.PhoneApplicationService.Current.Deactivated += new EventHandler<Microsoft.Phone.Shell.DeactivatedEventArgs>(Current_Deactivated);
            Microsoft.Phone.Shell.PhoneApplicationService.Current.Closing += new EventHandler<Microsoft.Phone.Shell.ClosingEventArgs>(Current_Closing);
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                e.Handled = true;
                // An unhandled exception has occurred, break in the debugger
                System.Diagnostics.Debugger.Break();
            }
            else
            {
                // By default show the error
                e.Handled = true;
                MessageBox.Show("Commuter encountered an error. Please restart.", "Error Encountered", MessageBoxButton.OK);
            }
        }

        private void TransitioningContentControl_TransitionCompleted(object sender, RoutedEventArgs e)
        {
            //// Set the root visual to use the transitioning control.
            TransitioningContentControl transitionControl = (TransitioningContentControl)sender;
            //transitionControl.Transition = "RightTransition";

            TransitionControl = transitionControl;
        }
    }
}