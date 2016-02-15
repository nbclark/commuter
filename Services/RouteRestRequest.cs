﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.4952
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by xsd, Version=2.0.50727.1432.
// 
namespace MobileSrc.Commuter.Shared.RouteServices.Rest
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Text;
    using System.Collections.Generic;
    using System.Xml;

    public enum RouteAvoid
    {
        None,
        AvoidHighways,
        AvoidTolls,
        MinimizeHighways,
        MinimizeTolls
    }

    public enum RouteOptimize
    {
        None,
        Distance,
        Time,
        TimeWithTraffic
    }

    public enum RoutePathOutput
    {
        None,
        Points
    }

    public enum RouteDistanceUnit
    {
        Mile,
        Kilometer
    }

    public enum RouteTravelMode
    {
        Walking,
        Driving
    }

    public class RouteRestRequest
    {
        public RouteRestRequest()
        {
            this.Avoid = RouteAvoid.None;
            this.Optimize = RouteOptimize.TimeWithTraffic;
            this.PathOutput = RoutePathOutput.None;
            this.DistanceUnit = RouteDistanceUnit.Mile;
            this.TravelMode = RouteTravelMode.Driving;

            this.Waypoints = new List<RestWaypoint>();
            this.ApplicationId = "AviYccEd2qfMoBG8NMmsl4F3LCL7Q25GPxS4AcwUcg6kFyraTzUvBsBdDYNxa9zV";
        }

        public string ApplicationId
        {
            get;
            set;
        }

        public RouteAvoid Avoid
        {
            get;
            set;
        }

        public RouteOptimize Optimize
        {
            get;
            set;
        }

        public RoutePathOutput PathOutput
        {
            get;
            set;
        }

        public RouteDistanceUnit DistanceUnit
        {
            get;
            set;
        }

        public RouteTravelMode TravelMode
        {
            get;
            set;
        }

        public List<RestWaypoint> Waypoints
        {
            get;
            set;
        }

        public string GetRequestUri()
        {
            StringBuilder uri = new StringBuilder();
            uri.AppendFormat("http://dev.virtualearth.net/REST/V1/Routes/{0}?output=xml&key={1}", this.TravelMode, this.ApplicationId);
            
            uri.AppendFormat("&du={0}", this.DistanceUnit);
            uri.AppendFormat("&rpo={0}", this.PathOutput);
            if (this.Optimize != RouteOptimize.None)
            {
                uri.AppendFormat("&optmz={0}", this.Optimize);
            }
            if (this.Avoid != RouteAvoid.None)
            {
                uri.AppendFormat("&avoid={0}", this.Avoid.ToString().ToLower().Replace("avoid", ""));
            }

            for (int i = 0; i < this.Waypoints.Count; ++i)
            {
                if (!string.IsNullOrEmpty(this.Waypoints[i].Location.Address))
                {
                    uri.AppendFormat("&wp.{0}={1}", i, this.Waypoints[i].Location.Address);
                }
                else
                {
                    uri.AppendFormat("&wp.{0}={1},{2}", i, this.Waypoints[i].Location.Latitude, this.Waypoints[i].Location.Longitude);
                }
            }

            return uri.ToString();
        }

        public Route Execute()
        {
            string uri = this.GetRequestUri();

            System.Net.WebRequest request = System.Net.HttpWebRequest.Create(uri);

            AutoResetEvent waitEvent = new AutoResetEvent(false);
            IAsyncResult result = request.BeginGetResponse((AsyncCallback)delegate(IAsyncResult ar)
            {
                waitEvent.Set();
                Thread.Sleep(0);
            }, this);
            waitEvent.WaitOne();

            RouteRestResponse response;
            using (System.IO.StreamReader str = new System.IO.StreamReader(request.EndGetResponse(result).GetResponseStream()))
            {
                response = DeserializeResponse(str.ReadToEnd());
            }
            
            Route restRoute = null;
            if (response.ResourceSets.Length > 0 && response.ResourceSets[0].Resources.Length > 0)
            {
                restRoute = response.ResourceSets[0].Resources[0] as Route;
            }

            if (null != restRoute)
            {
                StringBuilder sb1 = new StringBuilder();
                StringBuilder sb2= new StringBuilder();

                foreach (RouteLeg leg in restRoute.RouteLeg)
                {
                    foreach (ItineraryItem item in leg.ItineraryItem)
                    {
                        sb1.AppendLine(item.Instruction.maneuverType + "\t" + item.Instruction.Value);
                    }
                    sb1.AppendLine();
                }

                // Apparently we don't get good traffic data from this call
                MobileSrc.Services.RouteServices.RouteServiceClient client = new Services.RouteServices.RouteServiceClient("BasicHttpBinding_IRouteService");
#if SERVER
                if (response.ResourceSets.Length > 0 && response.ResourceSets[0].Resources.Length > 0)
                {
                    Services.RouteServices.RouteRequest soapRequest = GetRequest(response.ResourceSets[0].Resources[0] as Route);

                    try
                    {
                        Services.RouteServices.RouteResponse soapResponse = client.CalculateRoute(soapRequest);
                        restRoute.TravelDuration = soapResponse.Result.Summary.TimeInSeconds;

                        foreach (MobileSrc.Services.RouteServices.RouteLeg leg in soapResponse.Result.Legs)
                        {
                            foreach (MobileSrc.Services.RouteServices.ItineraryItem item in leg.Itinerary)
                            {
                                sb2.AppendLine(item.ManeuverType + "\t" + item.Text);
                            }
                            sb2.AppendLine();
                        }
                    }
                    catch
                    {
                    }
                }
#else
                client.CalculateRouteCompleted += new EventHandler<Services.RouteServices.CalculateRouteCompletedEventArgs>
                (
                    delegate(object sender, Services.RouteServices.CalculateRouteCompletedEventArgs e)
                    {
                        if (null == e.Error)
                        {
                            restRoute.TravelDuration = e.Result.Result.Summary.TimeInSeconds;

                            foreach (MobileSrc.Services.RouteServices.RouteLeg leg in e.Result.Result.Legs)
                            {
                                foreach (MobileSrc.Services.RouteServices.ItineraryItem item in leg.Itinerary)
                                {
                                    sb2.AppendLine(item.ManeuverType + "\t" + item.Text);
                                }
                                sb2.AppendLine();
                            }
                        }
                        waitEvent.Set();
                    }
                );
                if (response.ResourceSets.Length > 0 && response.ResourceSets[0].Resources.Length > 0)
                {
                    Services.RouteServices.RouteRequest soapRequest = GetRequest(response.ResourceSets[0].Resources[0] as Route);
                    client.CalculateRouteAsync(soapRequest);
                }

                waitEvent.WaitOne();
#endif
            }
                    
            return restRoute;
        }

        private int CompareItinerary(ItineraryItem a, ItineraryItem b)
        {
            return b.TravelDistance.CompareTo(a.TravelDistance);
        }

        private Services.RouteServices.RouteRequest GetRequest(Route response)
        {
            Services.RouteServices.RouteRequest request = new Services.RouteServices.RouteRequest();
            request.Waypoints = new List<Services.RouteServices.Waypoint>();
            request.Waypoints.Add(new Services.RouteServices.Waypoint());

            request.Credentials = new Services.RouteServices.Credentials();
            request.Credentials.ApplicationId = this.ApplicationId;
            request.Waypoints[0].Location = new Services.RouteServices.Location();

            request.Waypoints[0].Location.Latitude = this.Waypoints[0].Location.Latitude;
            request.Waypoints[0].Location.Longitude = this.Waypoints[0].Location.Longitude;

            ItineraryItem[] items = new ItineraryItem[response.RouteLeg[0].ItineraryItem.Length];
            response.RouteLeg[0].ItineraryItem.CopyTo(items, 0);

            Array.Sort<ItineraryItem>(items, new Comparison<ItineraryItem>(CompareItinerary));

            int counter = 1;

            List<ItineraryItem> validItems = new List<ItineraryItem>();

            for (int i = 0; i < 23 && i < items.Length; ++i)
            {
                validItems.Add(items[i]);
            }

            foreach (ItineraryItem item in response.RouteLeg[0].ItineraryItem)
            {
                if (validItems.Contains(item))
                {
                    request.Waypoints.Add(new Services.RouteServices.Waypoint());
                    request.Waypoints[request.Waypoints.Count - 1].Location = new Services.RouteServices.Location();
                    request.Waypoints[request.Waypoints.Count - 1].Location.Latitude = item.ManeuverPoint.Latitude;
                    request.Waypoints[request.Waypoints.Count - 1].Location.Longitude = item.ManeuverPoint.Longitude;

                    counter++;

                    if (counter >= 23)
                    {
                        break;
                    }
                }
            }

            /*
            int itemCount = 23;
            int increment = (int)Math.Max(1, Math.Floor(response.RoutePath.Line.Point.Length / (double)itemCount));
            for (int i = 0; i < itemCount && i < response.RoutePath.Line.Point.Length; i += increment)
            {
                Point item = response.RoutePath.Line.Point[i];
                request.Waypoints.Add(new Services.RouteServices.Waypoint());
                request.Waypoints[request.Waypoints.Count - 1].Location = new Services.RouteServices.Location();
                request.Waypoints[request.Waypoints.Count - 1].Location.Latitude = item.Latitude;
                request.Waypoints[request.Waypoints.Count - 1].Location.Longitude = item.Longitude;
            }
            */
            request.Waypoints.Add(new Services.RouteServices.Waypoint());
            request.Waypoints[request.Waypoints.Count - 1].Location = new Services.RouteServices.Location();
            request.Waypoints[request.Waypoints.Count - 1].Location.Latitude = this.Waypoints[this.Waypoints.Count - 1].Location.Latitude;
            request.Waypoints[request.Waypoints.Count - 1].Location.Longitude = this.Waypoints[this.Waypoints.Count-1].Location.Longitude;

            // Only accept results with high confidence.
            request.Options = new Services.RouteServices.RouteOptions();
            request.Options.RoutePathType = Services.RouteServices.RoutePathType.None;
            request.Options.TrafficUsage = Services.RouteServices.TrafficUsage.TrafficBasedTime;

            return request;
        }

        public RouteRestResponse DeserializeResponse(string response)
        {
            System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(MobileSrc.Commuter.Shared.RouteServices.Rest.RouteRestResponse), "http://schemas.microsoft.com/search/local/ws/rest/v1");

            using (StringReader reader = new StringReader(response))
            {
                return (xs.Deserialize(reader) as RouteRestResponse);
            }
        }
    }

    public class RestWaypoint
    {
        public RestLocation Location
        {
            get;
            set;
        }
    }

    public class RestLocation
    {
        public string Address
        {
            get;
            set;
        }

        public double Altitude
        {
            get;
            set;
        }

        public double Latitude
        {
            get;
            set;
        }

        public double Longitude
        {
            get;
            set;
        }
    }
}