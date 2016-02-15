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
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Threading;

namespace Commuter
{
    public partial class ResolveAddress : CustomPhoneApplicationPage
    {
        public static event EventHandler Closed;
        public static event EventHandler AddressChanged;

        private AsyncTask _executeTask;
        private const string BingApiKey = "A0DFD5468B4C061E90C036F542C1A621B91596B6";
        private const string BingMapsApiKey = "AviYccEd2qfMoBG8NMmsl4F3LCL7Q25GPxS4AcwUcg6kFyraTzUvBsBdDYNxa9zV";
        private MobileSrc.Services.GeocodeServices.GeocodeServiceClient _geocodeClient = new MobileSrc.Services.GeocodeServices.GeocodeServiceClient("BasicHttpBinding_IGeocodeService");
        private string _lastCheck = string.Empty;
        System.Device.Location.GeoCoordinateWatcher _watcher;

        public ResolveAddress()
        {
            InitializeComponent();

            _geocodeClient.GeocodeCompleted += new EventHandler<MobileSrc.Services.GeocodeServices.GeocodeCompletedEventArgs>(_geocodeClient_GeocodeCompleted);
            textBox.TextChanged += new TextChangedEventHandler(textBox_TextChanged);
            listBox.SelectionChanged += new SelectionChangedEventHandler(listBox_SelectionChanged);

            _watcher = new System.Device.Location.GeoCoordinateWatcher(System.Device.Location.GeoPositionAccuracy.Default);
            _watcher.PositionChanged += new EventHandler<System.Device.Location.GeoPositionChangedEventArgs<System.Device.Location.GeoCoordinate>>(a_PositionChanged);
            _watcher.Start();

            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = false;
            SelectedGeocodeResult = null;

            this.Loaded += new RoutedEventHandler(ResolveAddress_Loaded);
        }

        void ResolveAddress_Loaded(object sender, RoutedEventArgs e)
        {
            textBox.SelectAll();
            textBox.Focus();
        }

        void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = (listBox.SelectedIndex >= 0);
            SelectedGeocodeResult = ((SearchResult)listBox.SelectedItem).Result;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            PageTitle.Text = NavigationContext.QueryString["title"];
            textBox.Text = NavigationContext.QueryString["address"];
        }

        public static MobileSrc.Services.GeocodeServices.GeocodeResult SelectedGeocodeResult
        {
            get;
            set;
        }

        void a_PositionChanged(object sender, System.Device.Location.GeoPositionChangedEventArgs<System.Device.Location.GeoCoordinate> e)
        {
            _watcher.Stop();
        }

        void _geocodeClient_GeocodeCompleted(object sender, MobileSrc.Services.GeocodeServices.GeocodeCompletedEventArgs e)
        {
            //listBox.ItemsSource = null;
            if (e.Error != null)
            {
                Utils.DisplayNetworkError();
                return;
            }

            System.Collections.ObjectModel.ObservableCollection<SearchResult> results = new System.Collections.ObjectModel.ObservableCollection<SearchResult>();

            foreach (MobileSrc.Services.GeocodeServices.GeocodeResult result in e.Result.Results)
            {
                results.Add(new SearchResult(result, _watcher.Position.Location.Latitude, _watcher.Position.Location.Longitude));
            }

            if (results.Count > 0)
            {
                listBox.ItemsSource = results;
                listBox.Visibility = System.Windows.Visibility.Visible;
                loadingLabel.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                listBox.Visibility = System.Windows.Visibility.Collapsed;
                loadingLabel.Visibility = System.Windows.Visibility.Visible;
                loadingLabel.Text = "No Results Found.";
            }
        }

        void LaunchTask()
        {
            if (textBox.Text.Length > 5)
            {
                string searchString = textBox.Text.Trim();

                if (!string.Equals(_lastCheck, searchString, StringComparison.InvariantCultureIgnoreCase))
                {
                    _lastCheck = searchString;

                    if (null != _executeTask)
                    {
                        _executeTask.Cancel();
                        _executeTask = null;
                    }

                    listBox.Visibility = System.Windows.Visibility.Collapsed;
                    loadingLabel.Visibility = System.Windows.Visibility.Visible;
                    loadingLabel.Text = "Loading Results...";

                    _executeTask = new AsyncTask(this.Dispatcher, () => WaitXSeconds(1), (ex) => GetMatchingAddress(ex));
                    _executeTask.Execute();
                }
            }
        }

        void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LaunchTask();
        }

        void WaitXSeconds(int delay)
        {
            Thread.Sleep(1000 * delay);
        }

        void GetMatchingAddress(Exception ex)
        {
            string searchString = textBox.Text.Trim();

            if (searchString.Length > 5)
            {
                _lastCheck = searchString;

                MobileSrc.Services.GeocodeServices.GeocodeRequest req = new MobileSrc.Services.GeocodeServices.GeocodeRequest();
                req.Credentials = new MobileSrc.Services.GeocodeServices.Credentials();
                req.Credentials.ApplicationId = BingMapsApiKey;
                req.Query = searchString;

                req.Options = new MobileSrc.Services.GeocodeServices.GeocodeOptions();
                req.Options.Count = 15;
                req.Options.Filters = new List<MobileSrc.Services.GeocodeServices.FilterBase>();

                MobileSrc.Services.GeocodeServices.ConfidenceFilter filter = new MobileSrc.Services.GeocodeServices.ConfidenceFilter();
                filter.MinimumConfidence = MobileSrc.Services.GeocodeServices.Confidence.Low;
                req.Options.Filters.Add(filter);

                _geocodeClient.GeocodeAsync(req);
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (null != AddressChanged)
            {
                AddressChanged(this, null);
            }
            if (null != Closed)
            {
                Closed(this, null);
            }
            this.NavigationService.GoBack();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            if (null != Closed)
            {
                Closed(this, null);
            }
            this.NavigationService.GoBack();
        }
    }

    public class SearchResult
    {
        public MobileSrc.Services.GeocodeServices.GeocodeResult Result
        {
            get;
            set;
        }
        public double Distance
        {
            get;
            set;
        }
        public string DisplayDistance
        {
            get;
            set;
        }
        public string DisplayName
        {
            get;
            set;
        }

        public SearchResult()
        {
        }

        public SearchResult(MobileSrc.Services.GeocodeServices.GeocodeResult result, double lat, double lon)
        {
            this.Result = result;
            this.DisplayName = result.DisplayName;

            this.Distance = Utils.CalculateDistance(result.Locations[0].Latitude, result.Locations[0].Longitude, lat, lon) * 0.62137119;
            this.DisplayDistance = string.Format("{0:0.00} miles", this.Distance);
        }
    }
}