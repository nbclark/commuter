using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.Phone.Controls;
using MobileSrc.Services.RouteServices;

namespace Commuter
{
    public partial class RouteEditor : CustomPhoneApplicationPage
    {
        private const string BingApiKey = "A0DFD5468B4C061E90C036F542C1A621B91596B6";
        private const string BingMapsApiKey = "AviYccEd2qfMoBG8NMmsl4F3LCL7Q25GPxS4AcwUcg6kFyraTzUvBsBdDYNxa9zV";
        private Microsoft.Phone.Controls.Maps.MapLayer layer;
        private bool _setViewPort = true;
        private ContextMenu menu;
        private MobileSrc.Commuter.Shared.RouteLocation _routeLocation = null;

        private class RouteAvoidPair
        {
            public MobileSrc.Commuter.Shared.RouteServices.Rest.RouteAvoid Avoid;

            public RouteAvoidPair(MobileSrc.Commuter.Shared.RouteServices.Rest.RouteAvoid avoid)
            {
                this.Avoid = avoid;
            }

            public override string ToString()
            {
                return Regex.Replace(this.Avoid.ToString(), "(.)([A-Z])", "$1 $2");
            }
        }

        public RouteEditor()
        {
            // remove the navigation visibility.  
            // This control is redundant with the multi-touch capabilities of the phone.
            //this.BingMap.NavigationVisibility = System.Windows.Visibility.Collapsed;
            // removes the copyright note
            this.BingMap.CopyrightVisibility = System.Windows.Visibility.Collapsed;
            // removes the Bing logo
            this.BingMap.LogoVisibility = System.Windows.Visibility.Collapsed;
            //this.BingMap.NavigationVisibility = System.Windows.Visibility.Collapsed;

            this.BingMap.Mode = new Microsoft.Phone.Controls.Maps.RoadMode();
            //this.BingMap.MouseDoubleClick += new EventHandler<Microsoft.Phone.Controls.Maps.MapMouseEventArgs>(BingMap_MouseDoubleClick);
           // this.BingMap.MousePan += new EventHandler<Microsoft.Phone.Controls.Maps.MapMouseDragEventArgs>(BingMap_MousePan);
            //this.BingMap.MapZoom += new EventHandler<Microsoft.Phone.Controls.Maps.MapZoomEventArgs>(BingMap_MapZoom);
            //this.BingMap.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(BingMap_MouseLeftButtonUp);

            layer = new Microsoft.Phone.Controls.Maps.MapLayer();
            this.BingMap.Children.Add(layer);

            List<RouteAvoidPair> avoids = new List<RouteAvoidPair>();

            int i = 0;
            int selectedIndex = 0;
            while (Enum.IsDefined(typeof(MobileSrc.Commuter.Shared.RouteServices.Rest.RouteAvoid), i))
            {
                if ((int)DataContextManager.SelectedRoute.AvoidanceMeasures == i)
                {
                    selectedIndex = i;
                }
                avoids.Add(new RouteAvoidPair((MobileSrc.Commuter.Shared.RouteServices.Rest.RouteAvoid)i));
                i++;
            }

            highwayToggle.ItemsSource = avoids;
            highwayToggle.SelectedIndex = selectedIndex;

            Binding highwayBinding = new Binding();
            highwayBinding.Source = DataContextManager.SelectedRoute;
            highwayBinding.Path = new PropertyPath("AvoidanceMeasures");
            highwayBinding.Mode = BindingMode.TwoWay;

            highwayToggle.SelectionChanged += new SelectionChangedEventHandler(highwayToggle_SelectionChanged);

            if (DataContextManager.SelectedRoute.RoutePoints.Count > 0)
            {
                RenderRoute();
            }
            else
            {
                // We have a new route
                PerformRouting(true);
            }

            if (DataContextManager.SelectedRoute.Name == null)
            {
                DataContextManager.SelectedRoute.Name = "untitled";
                this.Loaded += new RoutedEventHandler(RouteEditor_Loaded);
            }

            wayPointList.ItemsSource = DataContextManager.SelectedRoute.WayPoints;
            nameTextBox.Text = DataContextManager.SelectedRoute.Name;

            menu = new ContextMenu();
            MenuItem routeMenu = new MenuItem();
            routeMenu.Header = "Route through here...";
            routeMenu.Click += new RoutedEventHandler(routeMenu_Click);

            MenuItem headerMenu = new MenuItem();
            headerMenu.Header = "Route through...";
            headerMenu.FontSize = 20;
            headerMenu.IsEnabled = false;
            headerMenu.FontStretch = FontStretches.UltraExpanded;

            MenuItem customMenu = new MenuItem();
            customMenu.Header = "Specify Custom Address...";
            customMenu.IsEnabled = true;
            customMenu.Click += new RoutedEventHandler(customMenu_Click);

            menu.Items.Add(headerMenu);
            menu.Items.Add(new Separator());
            menu.Items.Add(routeMenu);
            menu.Items.Add(customMenu);

            GestureListener listener = GestureService.GetGestureListener(this.BingMap);
            listener.Hold += new EventHandler<GestureEventArgs>(listener_Hold);
        }

        void highwayToggle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RouteAvoidPair pair = (RouteAvoidPair)highwayToggle.SelectedItem;
            DataContextManager.SelectedRoute.AvoidanceMeasures = pair.Avoid;

            PerformRouting(true);
        }

        void customMenu_Click(object sender, RoutedEventArgs e)
        {
            ResolveAddress.AddressChanged += new EventHandler(ResolveAddress_AddressChanged);
            ResolveAddress.Closed += new EventHandler(ResolveAddress_Closed);

            NavigationService.Navigate(new Uri(string.Format("/ResolveAddress.xaml?title={0}&address={1}", "route address", (_routeLocation == null) ? "" : _routeLocation.Address), UriKind.Relative));
        }

        void ResolveAddress_Closed(object sender, EventArgs e)
        {
            ResolveAddress.AddressChanged -= new EventHandler(ResolveAddress_AddressChanged);
            ResolveAddress.Closed -= new EventHandler(ResolveAddress_Closed);
        }

        void ResolveAddress_AddressChanged(object sender, EventArgs e)
        {
            DataContextManager.SelectedRoute.WayPoints.Add(Utils.CreateRouteLocation(ResolveAddress.SelectedGeocodeResult));

            wayPointList.ItemsSource = null;
            wayPointList.ItemsSource = DataContextManager.SelectedRoute.WayPoints;

            PerformRouting(false);
        }

        void listener_Hold(object sender, GestureEventArgs e)
        {
            ((MenuItem)menu.Items[0]).Header = "Identifying Location...";
            menu.IsOpen = true;

            System.Device.Location.GeoCoordinate location = this.BingMap.ViewportPointToLocation(e.GetPosition(this.BingMap));

            MobileSrc.Services.GeocodeServices.GeocodeServiceClient rclient = new MobileSrc.Services.GeocodeServices.GeocodeServiceClient("BasicHttpBinding_IGeocodeService");
            MobileSrc.Services.GeocodeServices.ReverseGeocodeRequest req = new MobileSrc.Services.GeocodeServices.ReverseGeocodeRequest();
            req.Credentials = new MobileSrc.Services.GeocodeServices.Credentials();
            req.Credentials.ApplicationId = BingMapsApiKey;
            req.Location = new MobileSrc.Services.GeocodeServices.Location();
            req.Location.Latitude = location.Latitude;
            req.Location.Longitude = location.Longitude;
            req.ExecutionOptions = new MobileSrc.Services.GeocodeServices.ExecutionOptions();

            rclient.ReverseGeocodeCompleted += new EventHandler<MobileSrc.Services.GeocodeServices.ReverseGeocodeCompletedEventArgs>(client_ReverseGeocodeCompleted);
            rclient.ReverseGeocodeAsync(req);
        }

        void client_ReverseGeocodeCompleted(object sender, MobileSrc.Services.GeocodeServices.ReverseGeocodeCompletedEventArgs e)
        {
            if (null != e.Error)
            {
                Utils.DisplayNetworkError();
                return;
            }
            _routeLocation = Utils.CreateRouteLocation(e.Result.Results[0]);
            ((MenuItem)menu.Items[0]).Header = string.Format("{0}", _routeLocation.Address);
        }

        void routeMenu_Click(object sender, RoutedEventArgs e)
        {
            DataContextManager.SelectedRoute.WayPoints.Add(_routeLocation);

            wayPointList.ItemsSource = null;
            wayPointList.ItemsSource = DataContextManager.SelectedRoute.WayPoints;

            PerformRouting(false);
        }

        protected override void InitializeChildComponents()
        {
            InitializeComponent();
        }

        void BingMap_MapZoom(object sender, Microsoft.Phone.Controls.Maps.MapZoomEventArgs e)
        {
            e.Handled = true;
            System.Device.Location.GeoCoordinate location = this.BingMap.ViewportPointToLocation(e.ViewportPoint);

            MobileSrc.Services.GeocodeServices.GeocodeServiceClient rclient = new MobileSrc.Services.GeocodeServices.GeocodeServiceClient("BasicHttpBinding_IGeocodeService");
            MobileSrc.Services.GeocodeServices.ReverseGeocodeRequest req = new MobileSrc.Services.GeocodeServices.ReverseGeocodeRequest();
            req.Credentials = new MobileSrc.Services.GeocodeServices.Credentials();
            req.Credentials.ApplicationId = BingMapsApiKey;
            req.Location = new MobileSrc.Services.GeocodeServices.Location();
            req.Location.Latitude = location.Latitude;
            req.Location.Longitude = location.Longitude;
            req.ExecutionOptions = new MobileSrc.Services.GeocodeServices.ExecutionOptions();

            rclient.ReverseGeocodeCompleted += new EventHandler<MobileSrc.Services.GeocodeServices.ReverseGeocodeCompletedEventArgs>(client_ReverseGeocodeCompleted);
            rclient.ReverseGeocodeAsync(req);
        }

        void RouteEditor_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= new RoutedEventHandler(RouteEditor_Loaded);
            nameTextBox.SelectAll();
            nameTextBox.Focus();
        }

        private MobileSrc.Commuter.Shared.RouteServices.Rest.Route _loadedRoute = null;
        void PerformRouting(bool setViewPort)
        {
            _setViewPort = setViewPort;

            AsyncTask.Execute(this.Dispatcher, delegate()
            {
                _loadedRoute = Utils.RefreshRoute(DataContextManager.SelectedCommute.StartPoint, DataContextManager.SelectedCommute.EndPoint, DataContextManager.SelectedRoute, false);

                /*
                RouteServiceClient client = new RouteServiceClient("BasicHttpBinding_IRouteService");
                RouteRequest request = new RouteRequest();
                request.Waypoints = new List<Waypoint>();
                request.Waypoints.Add(new Waypoint());

                request.Credentials = new Credentials();
                request.Credentials.ApplicationId = BingMapsApiKey;
                request.Waypoints[0].Location = new Location();

                request.Waypoints[0].Location.Latitude = DataContextManager.SelectedCommute.StartPoint.Location.Latitude;
                request.Waypoints[0].Location.Longitude = DataContextManager.SelectedCommute.StartPoint.Location.Longitude;

                foreach (MobileSrc.Commuter.Shared.RouteLocation wayPoint in DataContextManager.SelectedRoute.WayPoints)
                {
                    request.Waypoints.Add(new Waypoint());
                    request.Waypoints[request.Waypoints.Count - 1].Location = new Location();
                    request.Waypoints[request.Waypoints.Count - 1].Location.Latitude = wayPoint.Location.Latitude;
                    request.Waypoints[request.Waypoints.Count - 1].Location.Longitude = wayPoint.Location.Longitude;
                }

                request.Waypoints.Add(new Waypoint());
                request.Waypoints[request.Waypoints.Count - 1].Location = new Location();
                request.Waypoints[request.Waypoints.Count - 1].Location.Latitude = DataContextManager.SelectedCommute.EndPoint.Location.Latitude;
                request.Waypoints[request.Waypoints.Count - 1].Location.Longitude = DataContextManager.SelectedCommute.EndPoint.Location.Longitude;

                // Only accept results with high confidence.
                request.Options = new RouteOptions();
                request.Options.RoutePathType = RoutePathType.Points;
                request.Options.TrafficUsage = TrafficUsage.TrafficBasedTime;
                 */
            },
                (ex) => RouteComplete(ex)
                );
        }

        void RouteComplete(Exception ex)
        {
            if (null != _loadedRoute)
            {
                DataContextManager.SelectedRoute.RoutePoints.Clear();

                foreach (MobileSrc.Commuter.Shared.RouteServices.Rest.Point location in _loadedRoute.RoutePath.Line.Point)
                {
                    DataContextManager.SelectedRoute.RoutePoints.Add(Utils.CreateGpsLocation(location.Latitude, location.Longitude));
                }

                DataContextManager.SelectedRoute.EstimatedDistance = _loadedRoute.TravelDistance;
                DataContextManager.SelectedRoute.EstimatedDuration = TimeSpan.FromSeconds(_loadedRoute.TravelDuration);
                DataContextManager.SelectedRoute.LastUpdated = DateTime.Now;

                RenderRoute();
            }
        }

        void RenderRoute()
        {
            layer.Children.Clear();

            Microsoft.Phone.Controls.Maps.MapPolyline line = new Microsoft.Phone.Controls.Maps.MapPolyline();
            line.Locations = new Microsoft.Phone.Controls.Maps.LocationCollection();

            double maxLat = DataContextManager.SelectedRoute.RoutePoints[0].Latitude;
            double minLat = DataContextManager.SelectedRoute.RoutePoints[0].Latitude;
            double maxLon = DataContextManager.SelectedRoute.RoutePoints[0].Longitude;
            double minLon = DataContextManager.SelectedRoute.RoutePoints[0].Longitude;

            foreach (MobileSrc.Commuter.Shared.GpsLocation location in DataContextManager.SelectedRoute.RoutePoints)
            {
                line.Locations.Add(new System.Device.Location.GeoCoordinate(location.Latitude, location.Longitude, location.Altitude));

                maxLat = Math.Max(maxLat, location.Latitude);
                minLat = Math.Min(minLat, location.Latitude);
                maxLon = Math.Max(maxLon, location.Longitude);
                minLon = Math.Min(minLon, location.Longitude);
            }
            Microsoft.Phone.Controls.Maps.LocationRect rect = new Microsoft.Phone.Controls.Maps.LocationRect(maxLat, minLon, minLat, maxLon);

            line.Opacity = 0.65;
            line.StrokeThickness = 5;
            line.Visibility = System.Windows.Visibility.Visible;
            line.Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Blue);
            layer.Children.Add(line);

            if (_setViewPort)
            {
                BingMap.SetView(rect);
            }
        }

        private void Route_Click(object sender, RoutedEventArgs e)
        {
            LayoutRoot.RowDefinitions[1].Height = new GridLength(LayoutRoot.RowDefinitions[1].MinHeight);

            /*
            RouteServiceClient client = new RouteServiceClient("BasicHttpBinding_IRouteService");
            RouteRequest request = new RouteRequest();
            request.Waypoints = new List<Waypoint>();
            request.Waypoints.Add(new Waypoint());
            request.Waypoints.Add(new Waypoint());

            request.Credentials = new Credentials();
            request.Credentials.ApplicationId = BingMapsApiKey;
            request.Waypoints[0].Location = new Location();
            request.Waypoints[1].Location = new Location();

            request.Waypoints[0].Location.Latitude = DataContextManager.SelectedCommute.StartPoint.Location.Latitude;
            request.Waypoints[0].Location.Longitude = DataContextManager.SelectedCommute.StartPoint.Location.Longitude;
            request.Waypoints[1].Location.Latitude = DataContextManager.SelectedCommute.EndPoint.Location.Latitude;
            request.Waypoints[1].Location.Longitude = DataContextManager.SelectedCommute.EndPoint.Location.Longitude;

            // Only accept results with high confidence.
            request.Options = new RouteOptions();
            request.Options.RoutePathType = RoutePathType.Points;

            client.CalculateRouteCompleted += new EventHandler<CalculateRouteCompletedEventArgs>(client_CalculateRouteCompleted);
            client.CalculateRouteAsync(request);
            */

            AsyncTask.Execute(this.Dispatcher, delegate()
            {
                _loadedRoute = Utils.RefreshRoute(DataContextManager.SelectedCommute.StartPoint, DataContextManager.SelectedCommute.EndPoint, DataContextManager.SelectedRoute, false);
            }, (ex) => RouteComplete(ex));
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;

            if (string.Equals(tb.Text, tb.Tag))
            {
                tb.Text = "";
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;

            if (string.IsNullOrEmpty(tb.Text))
            {
                tb.Text = Convert.ToString(tb.Tag);
            }
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            DataContextManager.SelectedRoute.WayPoints.Remove((MobileSrc.Commuter.Shared.RouteLocation)((HyperlinkButton)sender).Tag);
            wayPointList.ItemsSource = null;
            wayPointList.ItemsSource = DataContextManager.SelectedRoute.WayPoints;

            PerformRouting(false);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            DataContextManager.SelectedRoute.Name = nameTextBox.Text;
        }

        private void zoomInButton_Click(object sender, RoutedEventArgs e)
        {
            this.BingMap.ZoomLevel = this.BingMap.ZoomLevel + 1;
        }

        private void zoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            this.BingMap.ZoomLevel = this.BingMap.ZoomLevel - 1;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (menu.IsOpen)
            {
                menu.IsOpen = false;
            }
            else
            {
                base.OnBackKeyPress(e);
            }
        }
    }
}