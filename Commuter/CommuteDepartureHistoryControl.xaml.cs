using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Controls.DataVisualization.Charting;
using System.Xml.Serialization;
using System.IO;

namespace Commuter
{
    public partial class CommuteDepartureHistoryControl : UserControl, ICommutePivotControl
    {
        private static MobileSrc.Services.CommuterServices.CommuterSoapClient _commuterServiceClient;
        public event EventHandler RefreshComplete;
        private Dictionary<Guid, BarSeries> departureRouteSeries = new Dictionary<Guid, BarSeries>();
        private Dictionary<Guid, BarSeries> returnRouteSeries = new Dictionary<Guid, BarSeries>();

        public CommuteDepartureHistoryControl()
        {
            InitializeComponent();

            _commuterServiceClient = new MobileSrc.Services.CommuterServices.CommuterSoapClient("CommuterSoap");
            _commuterServiceClient.GetCommuteHistoryCompleted += new EventHandler<MobileSrc.Services.CommuterServices.GetCommuteHistoryCompletedEventArgs>(_commuterServiceClient_GetCommuteHistoryCompleted);

            this.Loaded += new RoutedEventHandler(CommuteDepartureHistoryControl_Loaded);
        }

        void CommuteDepartureHistoryControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (null != DataContextManager.SelectedCommute && null != DataContextManager.SelectedCommute.History)
            {
                ProcessHistory(DataContextManager.SelectedCommute.History);
            }
        }

        void _commuterServiceClient_GetCommuteHistoryCompleted(object sender, MobileSrc.Services.CommuterServices.GetCommuteHistoryCompletedEventArgs e)
        {
            if (null != e.Error)
            {
                Utils.DisplayNetworkError();
            }
            else
            {
                XmlSerializer xsTo = new XmlSerializer(typeof(MobileSrc.Commuter.Shared.CommuteHistory));
                XmlSerializer xsFrom = new XmlSerializer(typeof(MobileSrc.Services.CommuterServices.CommuteHistory));

                using (StringWriter writer = new StringWriter())
                {
                    xsFrom.Serialize(writer, e.Result);
                    using (StringReader reader = new StringReader(writer.ToString()))
                    {
                        DataContextManager.SelectedCommute.History = (MobileSrc.Commuter.Shared.CommuteHistory)xsTo.Deserialize(reader);
                    }
                }
            }

            if (null != RefreshComplete)
            {
                RefreshComplete(this, null);
            }

            if (_isSelected)
            {
                ProcessHistory(DataContextManager.SelectedCommute.History);
            }
        }

        void ProcessHistory(MobileSrc.Commuter.Shared.CommuteHistory commuteHistory)
        {
            departureRouteSeries.Clear();
            returnRouteSeries.Clear();

            foreach (MobileSrc.Commuter.Shared.RouteDefinition route in DataContextManager.SelectedCommute.Routes)
            {
                ObservableCollection<KeyValuePair<string, int>> depSeriesData = new ObservableCollection<KeyValuePair<string, int>>();
                ObservableCollection<KeyValuePair<string, int>> retSeriesData = new ObservableCollection<KeyValuePair<string, int>>();

                BarSeries retSeries = new BarSeries();
                retSeries.Title = route.Name;
                retSeries.ItemsSource = retSeriesData;
                retSeries.IndependentValueBinding = new System.Windows.Data.Binding("Key");
                retSeries.DependentValueBinding = new System.Windows.Data.Binding("Value");

                BarSeries depSeries = new BarSeries();
                depSeries.Title = route.Name;
                depSeries.ItemsSource = depSeriesData;
                depSeries.IndependentValueBinding = new System.Windows.Data.Binding("Key");
                depSeries.DependentValueBinding = new System.Windows.Data.Binding("Value");

                departureRouteSeries.Add(route.Id, depSeries);
                returnRouteSeries.Add(route.Id, retSeries);
            }

            foreach (MobileSrc.Commuter.Shared.RouteHistory history in commuteHistory.Routes)
            {
                if (departureRouteSeries.ContainsKey(history.RouteId))
                {
                    ObservableCollection<KeyValuePair<string, int>> depSeriesData = (ObservableCollection<KeyValuePair<string, int>>)departureRouteSeries[history.RouteId].ItemsSource;
                    ObservableCollection<KeyValuePair<string, int>> retSeriesData = (ObservableCollection<KeyValuePair<string, int>>)returnRouteSeries[history.RouteId].ItemsSource;

                    Dictionary<DayOfWeek, double> depDayValues = new Dictionary<DayOfWeek, double>();
                    Dictionary<DayOfWeek, double> retDayValues = new Dictionary<DayOfWeek, double>();

                    foreach (MobileSrc.Commuter.Shared.RouteHistory.RouteHistoryDay historyDay in history.DepartureAverages)
                    {
                        if (!depDayValues.ContainsKey((DayOfWeek)historyDay.Day))
                        {
                            depDayValues.Add((DayOfWeek)historyDay.Day, historyDay.Minutes);
                        }
                    }
                    foreach (MobileSrc.Commuter.Shared.RouteHistory.RouteHistoryDay historyDay in history.ReturnAverages)
                    {
                        if (!retDayValues.ContainsKey((DayOfWeek)historyDay.Day))
                        {
                            retDayValues.Add((DayOfWeek)historyDay.Day, historyDay.Minutes);
                        }
                    }
                    Dictionary<DayOfWeek, double> dayValues = new Dictionary<DayOfWeek, double>();

                    for (int i = 0; i <= (int)DayOfWeek.Saturday; ++i)
                    {
                        int value = (int)Math.Pow(2, i);

                        if (0 != ((int)DataContextManager.SelectedCommute.DaysOfWeek & value))
                        {
                            double depTimeValue = 0;
                            double retTimeValue = 0;

                            if (depDayValues.ContainsKey((DayOfWeek)i))
                            {
                                depTimeValue = depDayValues[(DayOfWeek)i];
                            }
                            if (retDayValues.ContainsKey((DayOfWeek)i))
                            {
                                retTimeValue = retDayValues[(DayOfWeek)i];
                            }

                            string dayName = System.Globalization.DateTimeFormatInfo.CurrentInfo.DayNames[i].Substring(0, 3);
                            depSeriesData.Add(new KeyValuePair<string, int>(dayName, (int)depTimeValue));
                            retSeriesData.Add(new KeyValuePair<string, int>(dayName, (int)retTimeValue));
                        }
                    }
                }
            }
            BindRouteData();
        }

        private void BindRouteData()
        {
            bool foundAnything = false;
            Dictionary<Guid, BarSeries> seriesList = (!_isReturn) ? departureRouteSeries : returnRouteSeries;
            List<Series> removeSeries = new List<Series>();

            foreach (BarSeries series in departureChart.Series)
            {
                bool found = false;

                foreach (BarSeries newSeries in seriesList.Values)
                {
                    if (string.Equals(Convert.ToString(newSeries.Title), Convert.ToString(series.Title), StringComparison.InvariantCultureIgnoreCase))
                    {
                        ObservableCollection<KeyValuePair<string, int>> seriesData = (ObservableCollection<KeyValuePair<string, int>>)series.ItemsSource;
                        ObservableCollection<KeyValuePair<string, int>> newSeriesData = (ObservableCollection<KeyValuePair<string, int>>)newSeries.ItemsSource;
                        seriesData.Clear();

                        foreach (KeyValuePair<string, int> dataPoint in newSeriesData)
                        {
                            foundAnything = true;
                            seriesData.Add(dataPoint);
                        }

                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    removeSeries.Add(series);
                }
            }

            foreach (BarSeries newSeries in seriesList.Values)
            {
                bool found = false;
                foreach (BarSeries series in departureChart.Series)
                {
                    if (string.Equals(Convert.ToString(newSeries.Title), Convert.ToString(series.Title), StringComparison.InvariantCultureIgnoreCase))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    ObservableCollection<KeyValuePair<string, int>> addSource = new ObservableCollection<KeyValuePair<string, int>>();
                    BarSeries addSeries = new BarSeries();
                    addSeries.Title = newSeries.Title;
                    addSeries.ItemsSource = addSource;
                    addSeries.IndependentValueBinding = newSeries.IndependentValueBinding;
                    addSeries.DependentValueBinding = newSeries.DependentValueBinding;

                    foreach (KeyValuePair<string, int> dataPoint in newSeries.ItemsSource)
                    {
                        foundAnything = true;
                        addSource.Add(dataPoint);
                    }
                    departureChart.Series.Add(addSeries);
                }

            }

            if (foundAnything)
            {
                loadingText.Visibility = System.Windows.Visibility.Collapsed;
                departureChart.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                loadingText.Text = "No Historical Data Available";
                loadingText.Visibility = System.Windows.Visibility.Visible;
                departureChart.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        public void Refresh()
        {
            loadingText.Text = ResourceStrings.CommuteDepartureHistoryControl_LoadingStatistics;
            loadingText.Visibility = System.Windows.Visibility.Visible;
            departureChart.Visibility = System.Windows.Visibility.Collapsed;

            _commuterServiceClient.GetCommuteHistoryAsync(Utils.DeviceId, DataContextManager.SelectedCommute.Id);
        }

        private bool _isSelected = false;
        public void Select()
        {
            _isSelected = true;
            ProcessHistory(DataContextManager.SelectedCommute.History);
        }

        public void Unselect()
        {
            _isSelected = false;
        }

        private bool _isReturn = false;
        public bool SetDirection(bool isReturn)
        {
            _isReturn = isReturn;
            if (null != departureChart)
            {
                BindRouteData();
            }

            return false;
        }
    }
}
