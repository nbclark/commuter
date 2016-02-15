using System;
using System.Net;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Notification;
using System.Diagnostics;

namespace Commuter
{
    public class NotificationProvider
    {
        public HttpNotificationChannel myChannel;
        public void CreatingANotificationChannel()
        {
            myChannel = HttpNotificationChannel.Find("MyChannel");

            if (myChannel == null)
            {
                myChannel = new HttpNotificationChannel("MyChannel", "www.contoso.com");

                // An application is expected to send its notification channel URI to its corresponding web service each time it launches.
                // The notification channel URI is not guaranteed to be the same as the last time the application ran.
                myChannel.ChannelUriUpdated += new EventHandler<NotificationChannelUriEventArgs>(myChannel_ChannelUriUpdated);

                myChannel.Open();
            }
            else // Found an existing notification channel.
            {
                // The URI that the application sends to its web service.
                Debug.WriteLine("Notification channel URI:" + myChannel.ChannelUri.ToString());
            }

            myChannel.HttpNotificationReceived += new EventHandler<HttpNotificationEventArgs>(myChannel_HttpNotificationReceived);
            myChannel.ShellToastNotificationReceived += new EventHandler<NotificationEventArgs>(myChannel_ShellToastNotificationReceived);
            myChannel.ErrorOccurred += new EventHandler<NotificationChannelErrorEventArgs>(myChannel_ErrorOccurred);
        }

        void myChannel_ChannelUriUpdated(object sender, NotificationChannelUriEventArgs e)
        {
            // The URI that the application will send to its cloud service.
            Debug.WriteLine("Notification channel URI:" + e.ChannelUri.ToString());
        }

        void myChannel_ErrorOccurred(object sender, NotificationChannelErrorEventArgs e)
        {
            switch (e.ErrorType)
            {
                case ChannelErrorType.ChannelOpenFailed:
                    // ...
                    break;
                case ChannelErrorType.MessageBadContent:
                    // ...
                    break;
                case ChannelErrorType.NotificationRateTooHigh:
                    // ...
                    break;
                case ChannelErrorType.PayloadFormatError:
                    // ...
                    break;
                case ChannelErrorType.PowerLevelChanged:
                    // ...
                    break;
            }
        }

        // Receiving a toast notification. 
        // Toast notifications are only delivered to the device when the application is not running in the foreground. 
        // If the application is running in the foreground, the toast notification is instead routed to the application.
        void myChannel_ShellToastNotificationReceived(object sender, NotificationEventArgs e)
        {
            if (e.Collection != null)
            {
                Dictionary<string, string> collection = (Dictionary<string, string>)e.Collection;
                System.Text.StringBuilder messageBuilder = new System.Text.StringBuilder();

                foreach (string elementName in collection.Keys)
                {
                    //...
                }
            }
        }

        // Receiving a raw notification. 
        // Raw notifications are only delivered to the application when it is running in the foreground. 
        // If the application is not running in the foreground, the raw notification message is dropped.
        void myChannel_HttpNotificationReceived(object sender, HttpNotificationEventArgs e)
        {
            if (e.Notification.Body != null && e.Notification.Headers != null)
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(e.Notification.Body);
            }
        }

        // Binding a notification channel to a tile notification. 
        // Tile notifications are always delivered to the tile, regardless of whether the application is running in the foreground.
        private void BindingANotificationsChannelToATileNotification()
        {
            myChannel.BindToShellTile();
        }

        // Binding a notification channel to a live tile notification. 
        // The web service can either push remote resources that are in the approved list or reference a local resource in the future. 
        private void BindingANotificationsChannelToALiveTileNotification()
        {
            // The approved list of URIs that will be verified on every push notification that contains a URI reference.
            Collection<Uri> ListOfAllowedDomains = new Collection<Uri> { new Uri("www.contoso.com") };
            myChannel.BindToShellTile(ListOfAllowedDomains);
        }

        // Binding a notification channel to a toast notification.
        private void BindingANotificationsChannelToAToastNotification()
        {
            myChannel.BindToShellToast();
        }
    }
}
