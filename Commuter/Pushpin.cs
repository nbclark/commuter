namespace Microsoft.Phone.Controls.Maps
{
    using Microsoft.Phone.Controls.Maps.AutomationPeers;
    using System;
    using System.Windows;
    using System.Windows.Automation.Peers;
    using System.Windows.Browser;
    using System.Windows.Controls;

    [ScriptableType]
    public class Pushpin : ContentControl
    {
        public static readonly DependencyProperty LocationDependencyProperty = DependencyProperty.Register("Location", typeof(System.Device.Location.GeoCoordinate), typeof(Pushpin), new PropertyMetadata(new PropertyChangedCallback(null, (IntPtr) OnLocationChangedCallback)));
        public static readonly DependencyProperty PositionOriginDependencyProperty = DependencyProperty.Register("PositionOrigin", typeof(Microsoft.Phone.Controls.Maps.PositionOrigin), typeof(Pushpin), new PropertyMetadata(new PropertyChangedCallback(null, (IntPtr) OnPositionOriginChangedCallback)));

        public Pushpin()
        {
            base.set_DefaultStyleKey(typeof(Pushpin));
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new BaseAutomationPeer(this, "Pushpin");
        }

        private static void OnLocationChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs eventArgs)
        {
            MapLayer.SetPosition(d, (System.Device.Location.GeoCoordinate) eventArgs.get_NewValue());
        }

        private static void OnPositionOriginChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs eventArgs)
        {
            MapLayer.SetPositionOrigin(d, (Microsoft.Phone.Controls.Maps.PositionOrigin)eventArgs.get_NewValue());
        }

        public System.Device.Location.GeoCoordinate Location
        {
            get
            {
                return (System.Device.Location.GeoCoordinate) base.GetValue(LocationDependencyProperty);
            }
            set
            {
                base.SetValue(LocationDependencyProperty, value);
            }
        }

        public Microsoft.Phone.Controls.Maps.PositionOrigin PositionOrigin
        {
            get
            {
                return (Microsoft.Phone.Controls.Maps.PositionOrigin) base.GetValue(PositionOriginDependencyProperty);
            }
            set
            {
                base.SetValue(PositionOriginDependencyProperty, value);
            }
        }
    }
}

