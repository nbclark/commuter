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
using System.Windows.Controls.Primitives;

namespace Commuter
{
    public partial class CommuteOverviewControl : UserControl, ICommutePivotControl
    {
        public event EventHandler RefreshComplete;

        static Color[] RouteColors = new Color[] { Colors.Blue, Colors.Red, Colors.Green, Colors.Yellow, Colors.White, Colors.Orange, Colors.Purple };
        Microsoft.Phone.Controls.Maps.LocationRect _finalRect = null;
        private Microsoft.Phone.Controls.Maps.MapLayer _routeLayer;
        private Popup _routeInfoPopup = new Popup();

        public CommuteOverviewControl()
        {
            InitializeComponent();
            // remove the navigation visibility.  
            // This control is redundant with the multi-touch capabilities of the phone.
            // removes the copyright note
            this.BingMap.CopyrightVisibility = System.Windows.Visibility.Collapsed;
            // removes the Bing logo
            this.BingMap.LogoVisibility = System.Windows.Visibility.Visible;
            this.BingMap.ScaleVisibility = System.Windows.Visibility.Collapsed;
            this.BingMap.Mode = new Microsoft.Phone.Controls.Maps.RoadMode();

            this.BingMap.MapZoom += new EventHandler<Microsoft.Phone.Controls.Maps.MapZoomEventArgs>(BingMap_MapZoom);
            this.BingMap.MapPan += new EventHandler<Microsoft.Phone.Controls.Maps.MapDragEventArgs>(BingMap_MapPan);
            this.BingMap.ManipulationStarted += new EventHandler<ManipulationStartedEventArgs>(BingMap_ManipulationStarted);
            this.BingMap.IsEnabled = false;

            this.BingMap.Mode.AnimationLevel = Microsoft.Phone.Controls.Maps.AnimationLevel.None;
            this.BingMap.AnimationLevel = Microsoft.Phone.Controls.Maps.AnimationLevel.None;

            _routeLayer = new Microsoft.Phone.Controls.Maps.MapLayer();
            this.BingMap.Children.Insert(0, _routeLayer);

            this.BingMap.Visibility = System.Windows.Visibility.Collapsed;
        }

        void BingMap_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            e.Handled = true;
        }

        void BingMap_MapZoom(object sender, Microsoft.Phone.Controls.Maps.MapZoomEventArgs e)
        {
            e.Handled = true;
        }

        void BingMap_MapPan(object sender, Microsoft.Phone.Controls.Maps.MapDragEventArgs e)
        {
            e.Handled = true;
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(
                    "Source",
                    typeof(MobileSrc.Commuter.Shared.CommuteDefinition),
                    typeof(CommuteOverviewControl),
                    new PropertyMetadata(OnSourceChanged));

        public MobileSrc.Commuter.Shared.CommuteDefinition Source
        {
            get { return (MobileSrc.Commuter.Shared.CommuteDefinition)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CommuteOverviewControl ctrl = (CommuteOverviewControl)d;
            ctrl.OnSourceChanged(e.OldValue, e.NewValue);
        }

        protected virtual void OnSourceChanged(object oldHeader, object newHeader)
        {
            if (null != oldHeader)
            {
                ((MobileSrc.Commuter.Shared.CommuteDefinition)oldHeader).Updated -= new EventHandler(Source_Updated);
            }
            if (null != this.Source)
            {
                this.Source.Updated += new EventHandler(Source_Updated);
            }
        }

        void AddRoutes()
        {
            pushpinCanvas.Children.Clear();
            _routeLayer.Children.Clear();

            _finalRect = null;
            
            for (int i = 0; i < this.Source.Routes.Count; ++i)
            {
                Microsoft.Phone.Controls.Maps.MapPolyline line = new Microsoft.Phone.Controls.Maps.MapPolyline();
                line.Locations = new Microsoft.Phone.Controls.Maps.LocationCollection();
                Random rng = new Random();

                if (this.Source.Routes[i].RoutePoints.Count > 0)
                {
                    double maxLat = this.Source.Routes[i].RoutePoints[0].Latitude;
                    double minLat = this.Source.Routes[i].RoutePoints[0].Latitude;
                    double maxLon = this.Source.Routes[i].RoutePoints[0].Longitude;
                    double minLon = this.Source.Routes[i].RoutePoints[0].Longitude;

                    foreach (MobileSrc.Commuter.Shared.GpsLocation location in this.Source.Routes[i].RoutePoints)
                    {
                        line.Locations.Add(new System.Device.Location.GeoCoordinate(location.Latitude, location.Longitude, location.Altitude));

                        maxLat = Math.Max(maxLat, location.Latitude);
                        minLat = Math.Min(minLat, location.Latitude);
                        maxLon = Math.Max(maxLon, location.Longitude);
                        minLon = Math.Min(minLon, location.Longitude);
                    }

                    Color color = RouteColors[i % RouteColors.Count()];

                    line.Opacity = 0.65;
                    line.StrokeThickness = 5;
                    line.Visibility = System.Windows.Visibility.Visible;
                    line.Stroke = new System.Windows.Media.SolidColorBrush(color);
                    _routeLayer.Children.Add(line);
                    Microsoft.Phone.Controls.Maps.LocationRect rect = new Microsoft.Phone.Controls.Maps.LocationRect(maxLat, minLon, minLat, maxLon);

                    if (null == _finalRect)
                    {
                        _finalRect = rect;
                    }
                    else
                    {
                        _finalRect.West = Math.Min(_finalRect.West, rect.West);
                        _finalRect.North = Math.Max(_finalRect.North, rect.North);

                        _finalRect.East = Math.Max(_finalRect.East, rect.East);
                        _finalRect.South = Math.Min(_finalRect.South, rect.South);
                    }
                }
            }

            if (null != _finalRect)
            {
                AsyncTask executeTask = new AsyncTask(this.Dispatcher, () => WaitForStopDownloading(), (ex) => WaitForStopDownloadingComplete(ex));
                executeTask.Execute();
            }

            for (int i = 0; i < this.Source.Routes.Count; ++i)
            {
                if (this.Source.Routes[i].RoutePoints.Count > 0)
                {
                    Color color = RouteColors[i % RouteColors.Count()];

                    Microsoft.Phone.Controls.Maps.Pushpin pushPin = new Microsoft.Phone.Controls.Maps.Pushpin();
                    pushPin.Background = new SolidColorBrush(color);
                    pushPin.Content = this.Source.Routes[i].Name;

                    MobileSrc.Commuter.Shared.GpsLocation prevPushpinLoc = this.Source.Routes[i].RoutePoints[this.Source.Routes[i].RoutePoints.Count / 2 - 5];
                    MobileSrc.Commuter.Shared.GpsLocation pushpinLoc = this.Source.Routes[i].RoutePoints[this.Source.Routes[i].RoutePoints.Count / 2];
                    pushPin.Location = new System.Device.Location.GeoCoordinate(pushpinLoc.Latitude, pushpinLoc.Longitude);

                    string name = this.Source.Routes[i].Name;

                    Border border = new Border();
                    border.BorderBrush = new SolidColorBrush(Colors.Orange);
                    border.BorderThickness = new Thickness(0);
                    border.Background = new SolidColorBrush(Colors.White);
                    border.Width = 21;
                    border.Height = 21;
                    border.Opacity = .5;
                    border.CornerRadius = new CornerRadius(10.5);

                    pushPin.Opacity = .60;

                    Button tooltipBorder = new Button();
                    tooltipBorder.Style = (Style)LayoutRoot.Resources["PhoneTransparentButton"];
                    tooltipBorder.DataContext = this.Source.Routes[i];
                    tooltipBorder.Background = new SolidColorBrush(color);
                    tooltipBorder.FontSize = 16;
                    tooltipBorder.Foreground = new SolidColorBrush(Colors.White);
                    tooltipBorder.Content = name;
                    tooltipBorder.Padding = new Thickness(5);
                    tooltipBorder.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                    tooltipBorder.Margin = new Thickness(4);
                    tooltipBorder.BorderBrush = new SolidColorBrush(Colors.White);
                    tooltipBorder.BorderThickness = new Thickness(2);

                    pushpinCanvas.Children.Add(tooltipBorder);
                }
            }
        }
        private void WaitForStopDownloadingComplete(Exception ex)
        {
            if (null != _finalRect)
            {
                BingMap.SetView(_finalRect);
            }
        }

        private void WaitForStopDownloading()
        {
            bool isIdle = false;
            while (!isIdle)
            {
                System.Threading.Thread.Sleep(500);
                this.Dispatcher.BeginInvoke(() =>
                {
                    isIdle = this.BingMap.IsIdle;
                });
                System.Threading.Thread.Sleep(500);
            }
        }

        public void Unselect()
        {
        }

        public void Select()
        {
            this.BingMap.Visibility = System.Windows.Visibility.Visible;
            MobileSrc.Commuter.Shared.CommuteDefinition definition = this.Source;

            if (!_isRefreshing)
            {
                SetRouteRecommendation(false);
            }
            else
            {
            }
            AddRoutes();
        }

        private bool _isRefreshing = false;
        public void Refresh()
        {
            _isRefreshing = true;

            statusLabel.Text = "Updating Route Estimates...";
            durationLabel.Text = "-";
            Utils.RefreshRoutes(this.Source, _isReturn);
        }

        void Source_Updated(object sender, EventArgs e)
        {
            _isRefreshing = false;

            this.Dispatcher.BeginInvoke(() =>
            {
                SetRouteRecommendation(true);
                AddRoutes();
            });
        }

        void SetRouteRecommendation(bool triggerRefresh)
        {
            if (this.Source.Routes.Count > 0)
            {
                MobileSrc.Commuter.Shared.RouteDefinition bestRoute = this.Source.Routes[0];

                foreach (MobileSrc.Commuter.Shared.RouteDefinition route in this.Source.Routes)
                {
                    TimeSpan durA = (_isReturn) ? route.EstimatedRetDuration : route.EstimatedDuration;
                    TimeSpan durB = (_isReturn) ? bestRoute.EstimatedRetDuration : bestRoute.EstimatedDuration;
                    if (durA < durB)
                    {
                        bestRoute = route;
                    }
                }
                // we got our new data here...
                statusLabel.Text = "Recommended Route: " + bestRoute.Name;

                if (_isReturn)
                {
                    durationLabel.Text = ((int)bestRoute.EstimatedRetDuration.TotalMinutes).ToString();
                }
                else
                {
                    durationLabel.Text = ((int)bestRoute.EstimatedDuration.TotalMinutes).ToString();
                }
            }
            else
            {
                statusLabel.Text = "No Routes Defined";
                durationLabel.Text = "-";
            }

            if (triggerRefresh)
            {
                if (null != RefreshComplete)
                {
                    RefreshComplete(this, null);
                }
            }
        }

        private bool _isReturn = false;
        public bool SetDirection(bool isReturn)
        {
            bool needsRefresh = false;
            _isReturn = isReturn;

            if (isReturn && DateTime.Now.Subtract(DataContextManager.SelectedCommute.LastUpdatedRet).TotalHours > 4)
            {
                needsRefresh = true;
            }
            else if (!isReturn && DateTime.Now.Subtract(DataContextManager.SelectedCommute.LastUpdated).TotalHours > 4)
            {
                needsRefresh = true;
            }

            if (!needsRefresh)
            {
                if (this.BingMap.Visibility == System.Windows.Visibility.Visible)
                {
                    this.Select();
                }
            }

            this.directionStart.Text = (isReturn) ? "End" : "Start";
            this.directionEnd.Text = (isReturn) ? "Start" : "End";

            return needsRefresh;
        }
    }
}
