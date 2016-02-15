using System;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Microsoft.Phone.Notification;
using Microsoft.Phone.Info;
using System.Threading;
using MobileSrc.Commuter.Shared.RouteServices.Rest;
using System.IO.IsolatedStorage;
using MobileSrc.Commuter.Shared;
using Microsoft.Phone.Controls;
using System.Windows.Data;

namespace Commuter
{
    internal static class Utils
    {
        public static Guid DeviceId
        {
            get { return UniqueID; }
        }

        public static void SetBinding(FrameworkElement element, DependencyProperty property, object dataObject, string propertyPath, BindingMode mode)
        {
            Binding binding = new Binding();
            binding.Source = dataObject;
            binding.Path = new PropertyPath(propertyPath);
            binding.Mode = mode;
            binding.NotifyOnValidationError = true;
            binding.BindsDirectlyToSource = true;

            element.SetBinding(property, binding);
        }

        private static string BingMapsApiKey = "AviYccEd2qfMoBG8NMmsl4F3LCL7Q25GPxS4AcwUcg6kFyraTzUvBsBdDYNxa9zV";
        private static Guid _deviceId = Guid.Empty;
        private static HttpNotificationChannel _channel = null;
        private static MobileSrc.Services.CommuterServices.CommuterSoapClient _commuterServiceClient = new MobileSrc.Services.CommuterServices.CommuterSoapClient("CommuterSoap");

        static Utils()
        {
            _commuterServiceClient.RequestTileUpdateCompleted += new EventHandler<MobileSrc.Services.CommuterServices.RequestTileUpdateCompletedEventArgs>(_commuterServiceClient_RequestTileUpdateCompleted);
        }

        public static void _commuterServiceClient_RequestTileUpdateCompleted(object sender, MobileSrc.Services.CommuterServices.RequestTileUpdateCompletedEventArgs e)
        {
            if (null != e.Error)
            {
                MessageBox.Show("Error Updating Live Tile", e.Error.ToString(), MessageBoxButton.OK);
            }
        }

        public static GpsLocation CreateGpsLocation(double latitude, double longitude)
        {
            GpsLocation gpsLocation = new GpsLocation();

            gpsLocation.Latitude = latitude;
            gpsLocation.Longitude = longitude;

            return gpsLocation;
        }
        public static RouteLocation CreateRouteLocation(MobileSrc.Services.GeocodeServices.GeocodeResult result)
        {
            RouteLocation routeLoc = new RouteLocation();

            routeLoc.Location = new GpsLocation();
            routeLoc.Location.Latitude = result.Locations[0].Latitude;
            routeLoc.Location.Longitude = result.Locations[0].Longitude;
            routeLoc.Location.Altitude = result.Locations[0].Altitude;

            routeLoc.Address = result.Address.FormattedAddress;

            return routeLoc;
        }
        /*
        public static RouteLocation CreateRouteLocation(ItineraryItem itinerary)
        {
            RouteLocation routeLoc = new RouteLocation();

            routeLoc.Location = new GpsLocation();
            routeLoc.Location.Latitude = itinerary.Location.Latitude;
            routeLoc.Location.Longitude = itinerary.Location.Longitude;
            routeLoc.Location.Altitude = itinerary.Location.Altitude;

            try
            {
                using (XmlReader reader = XmlReader.Create(new StringReader(string.Concat("<Result>", itinerary.Text.Replace("VirtualEarth:", ""), "</Result>"))))
                {
                    // <VirtualEarth:Action>Depart</VirtualEarth:Action> <VirtualEarth:RoadName>134th Pl NE</VirtualEarth:RoadName> toward <VirtualEarth:Toward>NE 186th St</VirtualEarth:Toward>
                    if (reader.ReadToDescendant("RoadName"))
                    {
                        reader.ReadStartElement();
                        routeLoc.Address = reader.ReadContentAsString();
                    }
                }
            }
            catch
            {
                routeLoc.Address = routeLoc.Location.ToString();
            }

            return routeLoc;
        }
        */
        public static Guid UniqueID
        {
            get
            {
                if (_deviceId == Guid.Empty)
                {
                    try
                    {
                        byte[] data = (byte[])DeviceExtendedProperties.GetValue("DeviceUniqueId");
                        Int32 id = BitConverter.ToInt32(data, 0);
                        short a = BitConverter.ToInt16(data, 4);
                        short b = BitConverter.ToInt16(data, 8);

                        Guid uniqueId = new Guid(id, a, b, data[9], data[10], data[11], data[12], data[13], data[14], data[15], data[16]);
                        _deviceId = uniqueId;
                    }
                    catch
                    {
                        _deviceId = Guid.NewGuid();
                    }
                }
                return _deviceId;
            }
        }

        public static void DisplayNetworkError()
        {
            MessageBox.Show("You must have network access to use Commuter. Please establish a connection and try again");
        }

        public static void RequestTileUpdate(string imageUrl)
        {
            if (null != _channel)
            {
                _commuterServiceClient.RequestTileUpdateAsync(_channel.ChannelUri.ToString(), imageUrl);
            }
        }

        public static void InitializeNotifications()
        {
            _channel = HttpNotificationChannel.Find("Commuter");
            if (null == _channel)
            {
                _channel = new HttpNotificationChannel("Commuter", "http://www.mobilesrc.com/");
                RegisterNotifications();
                _channel.Open();
            }
            else
            {
                BindNotifications();
                RegisterNotifications();
            }
        }

        static void _channel_HttpNotificationReceived(object sender, HttpNotificationEventArgs e)
        {
        }

        static void _channel_ShellToastNotificationReceived(object sender, NotificationEventArgs e)
        {
        }

        static void _channel_ErrorOccurred(object sender, NotificationChannelErrorEventArgs e)
        {
            //
        }

        static void _channel_ChannelUriUpdated(object sender, NotificationChannelUriEventArgs e)
        {
            _channel = HttpNotificationChannel.Find("Commuter");

            if (null != _channel)
            {
                BindNotifications();
            }
        }

        internal static void BindNotifications()
        {
            if (null != _channel)
            {
                RegisterDevice();

                if (_channel.IsShellTileBound && !DataContextManager.Settings.EnableTileNotifications)
                {
                    _channel.UnbindToShellTile();
                }
                else if (!_channel.IsShellTileBound && DataContextManager.Settings.EnableTileNotifications)
                {
                    var uris = new Collection<Uri>
                    {
                        new Uri("http://mobilesrc.com")
                    };

                    _channel.BindToShellTile(uris);
                }

                if (_channel.IsShellToastBound && !DataContextManager.Settings.EnableToastNotifications)
                {
                    _channel.UnbindToShellToast();
                }
                else if (!_channel.IsShellToastBound && DataContextManager.Settings.EnableToastNotifications)
                {
                    bool shouldSet = true;
                    if (!DataContextManager.Settings.HasNotifiedToast)
                    {
                        if (MessageBoxResult.OK != MessageBox.Show("Are you sure you want to enable toast notifications?", "Enable Toasts", MessageBoxButton.OKCancel))
                        {
                            DataContextManager.Settings.EnableToastNotifications = false;
                            shouldSet = false;
                        }
                        DataContextManager.Settings.HasNotifiedToast = true;
                        DataContextManager.Save();
                    }

                    if (shouldSet)
                    {
                        _channel.BindToShellToast();
                    }
                }
            }
        }

        public static void RegisterDevice()
        {
            if (null != _channel)
            {
                List<MobileSrc.Services.CommuterServices.CommuteDefinition> definitions = new List<MobileSrc.Services.CommuterServices.CommuteDefinition>();

                foreach (MobileSrc.Commuter.Shared.CommuteDefinition definition in DataContextManager.Commutes)
                {
                    definitions.Add(CopyCommuteDefinition(definition));
                }

                DateTime now = DateTime.Now;
                DateTime nowUtc = now.ToUniversalTime();

                int timeZoneOffset = (int)(nowUtc - now).TotalHours;

                string accentColor = App.AccentColor;

                _commuterServiceClient.RegisterDeviceAsync(UniqueID, "", DataContextManager.Settings.EnableTileNotifications, DataContextManager.Settings.EnableToastNotifications, timeZoneOffset, accentColor, Convert.ToString(_channel.ChannelUri), definitions.ToArray());
            }
        }

        public static void RegisterCommute(MobileSrc.Commuter.Shared.CommuteDefinition definition)
        {
            _commuterServiceClient.AddCommuteAsync(UniqueID, CopyCommuteDefinition(definition));
        }

        static MobileSrc.Services.CommuterServices.CommuteDefinition CopyCommuteDefinition(MobileSrc.Commuter.Shared.CommuteDefinition definition)
        {
            MobileSrc.Services.CommuterServices.CommuteDefinition newDefinition = new MobileSrc.Services.CommuterServices.CommuteDefinition();

            XmlSerializer xsFrom = new XmlSerializer(typeof(MobileSrc.Commuter.Shared.CommuteDefinition));
            XmlSerializer xsTo = new XmlSerializer(typeof(MobileSrc.Services.CommuterServices.CommuteDefinition));

            using (StringWriter writer = new StringWriter())
            {
                xsFrom.Serialize(writer, definition);
                using (StringReader reader = new StringReader(writer.ToString()))
                {
                    newDefinition = (MobileSrc.Services.CommuterServices.CommuteDefinition)xsTo.Deserialize(reader);
                }
            }

            newDefinition.DepartureTime = newDefinition.DepartureTime.ToUniversalTime();
            newDefinition.ReturnTime = newDefinition.ReturnTime.ToUniversalTime();

            foreach (MobileSrc.Services.CommuterServices.RouteDefinition route in newDefinition.Routes)
            {
                route.RoutePoints = new MobileSrc.Services.CommuterServices.GpsLocation[0];
            }

            return newDefinition;
        }

        static void RegisterNotifications()
        {
            _channel.ChannelUriUpdated += new EventHandler<NotificationChannelUriEventArgs>(_channel_ChannelUriUpdated);
            _channel.ErrorOccurred += new EventHandler<NotificationChannelErrorEventArgs>(_channel_ErrorOccurred);
            _channel.ShellToastNotificationReceived += new EventHandler<NotificationEventArgs>(_channel_ShellToastNotificationReceived);
            _channel.HttpNotificationReceived += new EventHandler<HttpNotificationEventArgs>(_channel_HttpNotificationReceived);
        }

        public static void RefreshRoutes(CommuteDefinition definition, bool reverseRoute)
        {
            Thread thread = new Thread(new ParameterizedThreadStart(_RefreshRoutesAsync));
            thread.IsBackground = true;
            thread.Start(new object[] { definition, reverseRoute });
        }

        public static void _RefreshRoutesAsync(object param)
        {
            object[] objs = (object[])param;
            CommuteDefinition definition = (CommuteDefinition)objs[0];
            bool reverseRoute = (bool)objs[1];
            
            double timeFromDeparture = Math.Abs((DateTime.Now.TimeOfDay - definition.DepartureTime.TimeOfDay).TotalMinutes);
            double timeFromReturn = Math.Abs((DateTime.Now.TimeOfDay - definition.ReturnTime.TimeOfDay).TotalMinutes);

            int updatedRoutes = definition.Routes.Count;
            foreach (RouteDefinition route in definition.Routes)
            {
                route.Updated += new EventHandler((object sender, EventArgs e) =>
                {
                    updatedRoutes--;
                });
                RefreshRoute(definition, route, reverseRoute);
            }

            int counter = 30;
            while (updatedRoutes > 0)
            {
                Thread.Sleep(500);

                if (counter-- <= 0)
                {
                    break;
                }
            }

            if (reverseRoute)
            {
                definition.LastUpdatedRet = DateTime.Now;
            }
            else
            {
                definition.LastUpdated = DateTime.Now;
            }
            definition.FireUpdated();
        }

        public static void RefreshRoute(CommuteDefinition definition, RouteDefinition route, bool reverseRoute)
        {
            Route restRoute = null;

            try
            {
                restRoute = RefreshRoute(definition.StartPoint, definition.EndPoint, route, reverseRoute);
            }
            catch
            {
            }

            if (null == restRoute)
            {
            }
            else
            {
                if (reverseRoute)
                {
                    route.EstimatedRetDistance = restRoute.TravelDistance;
                    route.EstimatedRetDuration = TimeSpan.FromSeconds(restRoute.TravelDuration);
                }
                else
                {
                    route.EstimatedDistance = restRoute.TravelDistance;
                    route.EstimatedDuration = TimeSpan.FromSeconds(restRoute.TravelDuration);
                }

                route.Directions.Clear();

                foreach (RouteLeg leg in restRoute.RouteLeg)
                {
                    foreach (ItineraryItem item in leg.ItineraryItem)
                    {
                        route.Directions.Add(new RouteDirection(item.Instruction));
                    }
                }

                route.RoutePoints.Clear();

                foreach (MobileSrc.Commuter.Shared.RouteServices.Rest.Point location in restRoute.RoutePath.Line.Point)
                {
                    route.RoutePoints.Add(Utils.CreateGpsLocation(location.Latitude, location.Longitude));
                }

                route.LastUpdated = DateTime.Now;
            }
            route.FireUpdated();
        }

        public static Route RefreshRoute(RouteLocation start, RouteLocation end, RouteDefinition route, bool reverseRoute)
        {
            RouteRestRequest request = new RouteRestRequest();
            
            request.Waypoints = new List<RestWaypoint>();
            request.Waypoints.Add(new RestWaypoint());

            request.ApplicationId = BingMapsApiKey;
            request.Waypoints[0].Location = new RestLocation();

            request.Waypoints[0].Location.Latitude = start.Location.Latitude;
            request.Waypoints[0].Location.Longitude = start.Location.Longitude;

            foreach (MobileSrc.Commuter.Shared.RouteLocation wayPoint in route.WayPoints)
            {
                request.Waypoints.Add(new RestWaypoint());
                request.Waypoints[request.Waypoints.Count - 1].Location = new RestLocation();
                request.Waypoints[request.Waypoints.Count - 1].Location.Latitude = wayPoint.Location.Latitude;
                request.Waypoints[request.Waypoints.Count - 1].Location.Longitude = wayPoint.Location.Longitude;
            }

            request.Waypoints.Add(new RestWaypoint());
            request.Waypoints[request.Waypoints.Count - 1].Location = new RestLocation();
            request.Waypoints[request.Waypoints.Count - 1].Location.Latitude = end.Location.Latitude;
            request.Waypoints[request.Waypoints.Count - 1].Location.Longitude = end.Location.Longitude;

            if (route.AvoidanceMeasures != RouteAvoid.None)
            {
                request.Avoid = route.AvoidanceMeasures;
            }

            if (reverseRoute)
            {
                System.Collections.Generic.List<RestWaypoint> reversed = new System.Collections.Generic.List<RestWaypoint>();

                for (int i = request.Waypoints.Count - 1; i >= 0; --i)
                {
                    reversed.Add(request.Waypoints[i]);
                }

                request.Waypoints = reversed;
            }

            request.Optimize = RouteOptimize.TimeWithTraffic;
            request.PathOutput = RoutePathOutput.Points;

            return request.Execute();
        }

        public static double CalculateDistance(double Lat1, double Long1, double Lat2, double Long2)
        {
            /*
                The Haversine formula according to Dr. Math.
                http://mathforum.org/library/drmath/view/51879.html
                
                dlon = lon2 - lon1
                dlat = lat2 - lat1
                a = (sin(dlat/2))^2 + cos(lat1) * cos(lat2) * (sin(dlon/2))^2
                c = 2 * atan2(sqrt(a), sqrt(1-a)) 
                d = R * c
                
                Where
                    * dlon is the change in longitude
                    * dlat is the change in latitude
                    * c is the great circle distance in Radians.
                    * R is the radius of a spherical Earth.
                    * The locations of the two points in 
                        spherical coordinates (longitude and 
                        latitude) are lon1,lat1 and lon2, lat2.
            */
            double dDistance = Double.MinValue;
            double dLat1InRad = Lat1 * (Math.PI / 180.0);
            double dLong1InRad = Long1 * (Math.PI / 180.0);
            double dLat2InRad = Lat2 * (Math.PI / 180.0);
            double dLong2InRad = Long2 * (Math.PI / 180.0);

            double dLongitude = dLong2InRad - dLong1InRad;
            double dLatitude = dLat2InRad - dLat1InRad;

            // Intermediate result a.

            double a = Math.Pow(Math.Sin(dLatitude / 2.0), 2.0) +
                       Math.Cos(dLat1InRad) * Math.Cos(dLat2InRad) *
                       Math.Pow(Math.Sin(dLongitude / 2.0), 2.0);

            // Intermediate result c (great circle distance in Radians).

            double c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));

            // Distance.

            // const Double kEarthRadiusMiles = 3956.0;

            const Double kEarthRadiusKms = 6376.5;
            dDistance = kEarthRadiusKms * c;

            return dDistance;
        }

    }
}
