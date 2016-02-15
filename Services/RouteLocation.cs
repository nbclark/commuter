using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace MobileSrc.Commuter.Shared
{
    public class RouteHistory
    {
        public RouteHistory()
        {
            this.DepartureAverages = new List<RouteHistoryDay>();
            this.ReturnAverages = new List<RouteHistoryDay>();
        }
        public Guid RouteId
        {
            get;
            set;
        }
        public List<RouteHistoryDay> DepartureAverages
        {
            get;
            set;
        }
        public List<RouteHistoryDay> ReturnAverages
        {
            get;
            set;
        }

        public class RouteHistoryDay
        {
            public DayOfWeek Day
            {
                get;
                set;
            }
            public double Minutes
            {
                get;
                set;
            }
        }
    }

    public class CommuteHistory
    {
        public CommuteHistory()
        {
            this.Routes = new List<RouteHistory>();
        }
        public List<RouteHistory> Routes
        {
            get;
            set;
        }
    }

    public class GpsLocation
    {
        public GpsLocation()
        {

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
        public double Altitude
        {
            get;
            set;
        }
    }

    public enum RouteAction
    {
        Depart,
        Arrive,
        Turn
    }

    /// <summary>
    /// <VirtualEarth:Action>Depart</VirtualEarth:Action> <VirtualEarth:RoadName>134th Pl NE</VirtualEarth:RoadName> toward <VirtualEarth:Toward>NE 186th St</VirtualEarth:Toward>
    /// </summary>
    public class RouteDirection : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public RouteDirection()
        {
        }
        public RouteDirection(string xml)
        {
            this.Action = Regex.Replace(xml, "<.+?>", "");
        }
        public RouteDirection(MobileSrc.Commuter.Shared.RouteServices.Rest.Instruction instruction)
        {
            this.Action = instruction.Value;
        }

        private string _Action;
        public string Action
        {
            get { return _Action; }
            set
            {
                if (value != _Action)
                {
                    _Action = value;
                    FirePropertyChanged("Action");
                }
            }
        }

        private void FirePropertyChanged(string propertyName)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class RouteLocation : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public RouteLocation()
        {
            this.Address = string.Empty;
        }

        private GpsLocation _Location;
        public GpsLocation Location
        {
            get { return _Location; }
            set
            {
                if (value != _Location)
                {
                    _Location = value;
                    FirePropertyChanged("Location");
                }
            }
        }

        private string _Address;
        public string Address
        {
            get { return _Address; }
            set
            {
                if (value != _Address)
                {
                    _Address = value;
                    FirePropertyChanged("Address");
                }
            }
        }

        public override string ToString()
        {
            return this.Address;
        }

        private void FirePropertyChanged(string propertyName)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
    public class RouteDefinition : INotifyPropertyChanged
    {
        public event EventHandler Updated;
        public event PropertyChangedEventHandler PropertyChanged;

        public RouteDefinition()
        {
            this.WayPoints = new List<RouteLocation>();
            this.RoutePoints = new List<GpsLocation>();
            this.Directions = new List<RouteDirection>();
            this.EstimatedDuration = TimeSpan.FromHours(0.234);
            this.AvoidanceMeasures = RouteServices.Rest.RouteAvoid.None;

            this.Id = Guid.NewGuid();
        }

        public Guid Id
        {
            get;
            set;
        }

        public List<RouteLocation> WayPoints
        {
            get;
            set;
        }
        public List<GpsLocation> RoutePoints
        {
            get;
            set;
        }
        public List<RouteDirection> Directions
        {
            get;
            set;
        }

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (_Name != value)
                {
                    _Name = value;
                    FirePropertyChanged("Name");
                }
            }
        }

        private RouteServices.Rest.RouteAvoid _AvoidanceMeasures;
        public RouteServices.Rest.RouteAvoid AvoidanceMeasures
        {
            get { return _AvoidanceMeasures; }
            set
            {
                if (_AvoidanceMeasures != value)
                {
                    _AvoidanceMeasures = value;
                    FirePropertyChanged("AvoidanceMeasures");
                }
            }
        }

        public TimeSpan EstimatedDuration
        {
            get { return TimeSpan.FromMinutes(this.EstimatedDurationMinutes); }
            set
            {
                this.EstimatedDurationMinutes = value.TotalMinutes;
                FirePropertyChanged("EstimatedDuration");
            }
        }

        private double _EstimatedDurationMinutes;
        public double EstimatedDurationMinutes
        {
            get { return _EstimatedDurationMinutes; }
            set
            {
                if (_EstimatedDurationMinutes != value)
                {
                    _EstimatedDurationMinutes = value;
                    FirePropertyChanged("EstimatedDurationMinutes");
                }
            }
        }


        private double _EstimatedDistance;
        public double EstimatedDistance
        {
            get { return _EstimatedDistance; }
            set
            {
                if (_EstimatedDistance != value)
                {
                    _EstimatedDistance = value;
                    FirePropertyChanged("EstimatedDistance");
                }
            }
        }

        public TimeSpan EstimatedRetDuration
        {
            get { return TimeSpan.FromMinutes(this.EstimatedDurationRetMinutes); }
            set
            {
                this.EstimatedDurationRetMinutes = value.TotalMinutes;
                FirePropertyChanged("EstimatedRetDuration");
            }
        }

        private double _EstimatedDurationRetMinutes;
        public double EstimatedDurationRetMinutes
        {
            get { return _EstimatedDurationRetMinutes; }
            set
            {
                if (_EstimatedDurationRetMinutes != value)
                {
                    _EstimatedDurationRetMinutes = value;
                    FirePropertyChanged("EstimatedDurationRetMinutes");
                }
            }
        }

        private double _EstimatedRetDistance;
        public double EstimatedRetDistance
        {
            get { return _EstimatedRetDistance; }
            set
            {
                if (_EstimatedRetDistance != value)
                {
                    _EstimatedRetDistance = value;
                    FirePropertyChanged("EstimatedRetDistance");
                }
            }
        }

        public string TravelSummary
        {
            get { return string.Format("{0:0.00} miles, {1:0.00} minutes", this.EstimatedDistance, this.EstimatedDuration.TotalMinutes); }
        }
        public string TravelSummaryRet
        {
            get { return string.Format("{0:0.00} miles, {1:0.00} minutes", this.EstimatedRetDistance, this.EstimatedRetDuration.TotalMinutes); }
        }

        private DateTime _LastUpdated;
        public DateTime LastUpdated
        {
            get { return _LastUpdated; }
            set
            {
                if (_LastUpdated != value)
                {
                    _LastUpdated = value;
                    FirePropertyChanged("LastUpdated");
                }
            }
        }

        private void FirePropertyChanged(string propertyName)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public void FireUpdated()
        {
            if (null != Updated)
            {
                Updated(this, null);
            }
        }
    }
    public class CommuteDefinition : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler Updated;

        public CommuteDefinition()
        {
            this.History = null;
            this.Routes = new List<RouteDefinition>();
            this.StartPoint = new RouteLocation();
            this.EndPoint = new RouteLocation();

            this.DepartureTime = DateTime.Now.Date.AddHours(9);
            this.ReturnTime = DateTime.Now.Date.AddHours(17);

            this.LastUpdated = DateTime.MinValue;
            this.LastUpdatedRet = DateTime.MinValue;

            this.Id = Guid.NewGuid();
            this.DaysOfWeek = 0;

            this.IsNew = false;

            for (int i = (int)DayOfWeek.Monday; i <= (int)DayOfWeek.Friday; ++i)
            {
                this.DaysOfWeek += (int)Math.Pow(2, i);
            }
        }

        public Guid Id
        {
            get;
            set;
        }

        public bool IsNew
        {
            get;
            set;
        }

        private RouteLocation _StartPoint;
        public RouteLocation StartPoint
        {
            get { return _StartPoint; }
            set
            {
                if (_StartPoint != value)
                {
                    _StartPoint = value;
                    FirePropertyChanged("StartPoint");
                }
            }
        }

        private RouteLocation _EndPoint;
        public RouteLocation EndPoint
        {
            get { return _EndPoint; }
            set
            {
                if (_EndPoint != value)
                {
                    _EndPoint = value;
                    FirePropertyChanged("EndPoint");
                }
            }
        }

        public List<RouteDefinition> Routes
        {
            get;
            set;
        }

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (_Name != value)
                {
                    _Name = value;
                    FirePropertyChanged("Name");
                }
            }
        }

        private DateTime _DepartureTime;
        public DateTime DepartureTime
        {
            get { return _DepartureTime; }
            set
            {
                if (_DepartureTime != value)
                {
                    _DepartureTime = value;
                    FirePropertyChanged("DepartureTime");
                }
            }
        }

        private DateTime _ReturnTime;
        public DateTime ReturnTime
        {
            get { return _ReturnTime; }
            set
            {
                if (_ReturnTime != value)
                {
                    _ReturnTime = value;
                    FirePropertyChanged("ReturnTime");
                }
            }
        }

        public string Description
        {
            get
            {
                return string.Format("Depart {0} - Return {1}", this.DepartureTime.ToShortTimeString(), this.ReturnTime.ToShortTimeString());
            }
        }

        private int _DaysOfWeek;
        public int DaysOfWeek
        {
            get { return _DaysOfWeek; }
            set
            {
                if (_DaysOfWeek != value)
                {
                    _DaysOfWeek = value;
                    FirePropertyChanged("DaysOfWeek");
                }
            }
        }

        [XmlIgnore]
        public string DaysOfWeekString
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                for (int i = (int)DayOfWeek.Sunday; i <= (int)DayOfWeek.Saturday; ++i)
                {
                    DayOfWeek dow = (DayOfWeek)i;
                    int pow = (int)Math.Pow(2, i);

                    if (0 != (this.DaysOfWeek & pow))
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append(", ");
                        }
                        sb.Append(dow.ToString().Substring(0, 2));
                    }
                }

                return sb.ToString();
            }
        }

        private DateTime _LastUpdated;
        public DateTime LastUpdated
        {
            get { return _LastUpdated; }
            set
            {
                if (_LastUpdated != value)
                {
                    _LastUpdated = value;
                    FirePropertyChanged("LastUpdated");
                }
            }
        }

        private DateTime _LastUpdatedRet;
        public DateTime LastUpdatedRet
        {
            get { return _LastUpdatedRet; }
            set
            {
                if (_LastUpdatedRet != value)
                {
                    _LastUpdatedRet = value;
                    FirePropertyChanged("LastUpdatedRet");
                }
            }
        }
        public CommuteHistory History
        {
            get;
            set;
        }
        public void FireUpdated()
        {
            if (null != Updated)
            {
                Updated(this, null);
            }
        }
        private void FirePropertyChanged(string propertyName)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
