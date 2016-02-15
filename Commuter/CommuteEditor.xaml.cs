using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.Phone.Controls;
using MobileSrc.Services.RouteServices;
using MobileSrc.Services.GeocodeServices;

namespace Commuter
{
    public partial class CommuteEditor : CustomPhoneApplicationPage
    {
        public static event EventHandler Closing;
        private const string BingApiKey = "A0DFD5468B4C061E90C036F542C1A621B91596B6";
        private const string BingMapsApiKey = "AviYccEd2qfMoBG8NMmsl4F3LCL7Q25GPxS4AcwUcg6kFyraTzUvBsBdDYNxa9zV";

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(
                    "Source",
                    typeof(MobileSrc.Commuter.Shared.CommuteDefinition),
                    typeof(CommuteEditor),
                    new PropertyMetadata(OnSourceChanged));

        public MobileSrc.Commuter.Shared.CommuteDefinition Source
        {
            get { return (MobileSrc.Commuter.Shared.CommuteDefinition)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public CommuteEditor()
        {
            routeList.ItemsSource = DataContextManager.SelectedCommute.Routes;
            //DepartDatePicker.Value = DataContextManager.SelectedCommute.DepartureTime;
            //ReturnDatePicker.Value = DataContextManager.SelectedCommute.ReturnTime;
            //weekDaysButton.Content = DataContextManager.SelectedCommute.DaysOfWeekString;

            Utils.SetBinding(DepartDatePicker, Microsoft.Phone.Controls.TimePicker.ValueProperty, DataContextManager.SelectedCommute, "DepartureTime", BindingMode.OneWay);
            Utils.SetBinding(ReturnDatePicker, Microsoft.Phone.Controls.TimePicker.ValueProperty, DataContextManager.SelectedCommute, "ReturnTime", BindingMode.OneWay);
            Utils.SetBinding(weekDaysButton, Button.ContentProperty, DataContextManager.SelectedCommute, "DaysOfWeekString", BindingMode.OneWay);

            if (string.IsNullOrEmpty(DataContextManager.SelectedCommute.StartPoint.Address) || string.IsNullOrEmpty(DataContextManager.SelectedCommute.EndPoint.Address))
            {
                DataContextManager.SelectedCommute.StartPoint.Address = Convert.ToString(startTextBox.Tag);
                DataContextManager.SelectedCommute.EndPoint.Address = Convert.ToString(endTextBox.Tag);

                DataContextManager.SelectedCommute.Name = "untitled";
                this.Loaded += new RoutedEventHandler(CommuteEditor_Loaded);
            }

            Utils.SetBinding(startTextBox, Button.ContentProperty, DataContextManager.SelectedCommute, "StartPoint.Address", BindingMode.OneWay);
            Utils.SetBinding(endTextBox, Button.ContentProperty, DataContextManager.SelectedCommute, "EndPoint.Address", BindingMode.OneWay);

            if (string.Equals("untitled", DataContextManager.SelectedCommute.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                DataContextManager.SelectedCommute.Name = "enter commute name";
            }

            Utils.SetBinding(nameTextBox, TextBox.TextProperty, DataContextManager.SelectedCommute, "Name", BindingMode.TwoWay);
            Utils.SetBinding(pivotControl, Pivot.TitleProperty, DataContextManager.SelectedCommute, "Name", BindingMode.TwoWay);

            SetEnabled(true);

            DataContextManager.SelectedCommute.LastUpdated = DateTime.MinValue;
            DataContextManager.SelectedCommute.LastUpdatedRet = DateTime.MinValue;
        }

        private bool _focusNameText = true;
        protected override void OnSplashEnd()
        {
            pivotControl.Visibility = System.Windows.Visibility.Visible;
            if (DataContextManager.Settings.IsFirstRun)
            {
                _focusNameText = false;
                firstRunPopup.IsOpen = true;
                closePopupButton.Focus();
            }
        }

        void CommuteEditor_Loaded(object sender, RoutedEventArgs e)
        {
            if (_focusNameText)
            {
                FocusNameText();
            }
            this.Loaded -= new RoutedEventHandler(CommuteEditor_Loaded);
        }

        void FocusNameText()
        {
            nameTextBox.SelectAll();
            nameTextBox.Focus();
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (firstRunPopup.IsOpen)
            {
                e.Cancel = true;
                firstRunPopup.IsOpen = false;
                DataContextManager.Settings.IsFirstRun = false;
                DataContextManager.Save();

                FocusNameText();
            }
            else
            {
                base.OnBackKeyPress(e);
            }
        }

        private void closePopupButton_Click(object sender, RoutedEventArgs e)
        {
            firstRunPopup.IsOpen = false;
            DataContextManager.Settings.IsFirstRun = false;
            DataContextManager.Save();

            FocusNameText();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            routeList.ItemsSource = null;
            routeList.ItemsSource = DataContextManager.SelectedCommute.Routes;
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            if (e.NavigationMode == System.Windows.Navigation.NavigationMode.Back)
            {
                if (null != Closing)
                {
                    Closing(this, null);
                }
                Utils.RegisterDevice();
                DataContextManager.Save();
            }
        }

        private Button _selectedButton = null;

        void addressButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            _selectedButton = button;
            string value = button.Content.ToString();

            if (string.Equals(value, button.Tag))
            {
                value = "";
            }

            NavigationService.Navigate(new Uri(string.Format("/ResolveAddress.xaml?title={0}&address={1}", (sender == startTextBox) ? "start address" : "end address", System.Net.HttpUtility.UrlEncode(value)), UriKind.Relative));
            ResolveAddress.AddressChanged += new EventHandler(ResolveAddress_AddressChanged);
            ResolveAddress.Closed += new EventHandler(ResolveAddress_Closed);
        }

        void ResolveAddress_Closed(object sender, EventArgs e)
        {
            ResolveAddress.AddressChanged -= new EventHandler(ResolveAddress_AddressChanged);
            ResolveAddress.Closed -= new EventHandler(ResolveAddress_Closed);
        }

        void ResolveAddress_AddressChanged(object sender, EventArgs e)
        {
            if (_selectedButton == startTextBox)
            {
                DataContextManager.SelectedCommute.StartPoint = Utils.CreateRouteLocation(ResolveAddress.SelectedGeocodeResult);
            }
            else if (_selectedButton == endTextBox)
            {
                DataContextManager.SelectedCommute.EndPoint = Utils.CreateRouteLocation(ResolveAddress.SelectedGeocodeResult);
            }
            SetEnabled(false);
        }

        protected override void InitializeChildComponents()
        {
            InitializeComponent();
        }

        void SetEnabled(bool isLoad)
        {
            bool isComplete = false;

            if (!string.IsNullOrEmpty(DataContextManager.SelectedCommute.StartPoint.Address) && !string.IsNullOrEmpty(DataContextManager.SelectedCommute.EndPoint.Address))
            {
                if (!string.Equals(Convert.ToString(startTextBox.Tag), DataContextManager.SelectedCommute.StartPoint.Address, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (!string.Equals(Convert.ToString(endTextBox.Tag), DataContextManager.SelectedCommute.EndPoint.Address, StringComparison.InvariantCultureIgnoreCase))
                    {
                        isComplete = true;
                    }
                }
            }

            routePivot.IsEnabled = weekDaysButton.IsEnabled = addButton.IsEnabled = routeList.IsEnabled = DepartDatePicker.IsEnabled = ReturnDatePicker.IsEnabled = isComplete;

            if (routePivot.IsEnabled && !isLoad)
            {
                if (DataContextManager.SelectedCommute.Routes.Count == 0)
                {
                    MobileSrc.Commuter.Shared.RouteDefinition routeHighway = new MobileSrc.Commuter.Shared.RouteDefinition();
                    routeHighway.Name = "highway";
                    routeHighway.AvoidanceMeasures = MobileSrc.Commuter.Shared.RouteServices.Rest.RouteAvoid.None;

                    MobileSrc.Commuter.Shared.RouteDefinition routeBackroads = new MobileSrc.Commuter.Shared.RouteDefinition();
                    routeBackroads.Name = "backroads";
                    routeBackroads.AvoidanceMeasures = MobileSrc.Commuter.Shared.RouteServices.Rest.RouteAvoid.AvoidHighways;

                    DataContextManager.SelectedCommute.Routes.Add(routeHighway);
                    DataContextManager.SelectedCommute.Routes.Add(routeBackroads);
                }
                //pivotControl.SelectedItem = routePivot;
            }
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            MobileSrc.Commuter.Shared.RouteDefinition route = new MobileSrc.Commuter.Shared.RouteDefinition();
            DataContextManager.SelectedCommute.Routes.Add(route);

            DataContextManager.SelectedRoute = route;

            Uri uri = new Uri("/RouteEditor.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }

        private void deleteRouteLink_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            MobileSrc.Commuter.Shared.RouteDefinition definition = (MobileSrc.Commuter.Shared.RouteDefinition)button.Tag;

            string szTitle = definition.Name;
            string szMessage = "Are you sure you want to delete this route?";

            if (MessageBox.Show(szMessage, szTitle, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                DataContextManager.SelectedCommute.Routes.Remove(definition);
            }

            routeList.ItemsSource = null;
            routeList.ItemsSource = DataContextManager.SelectedCommute.Routes;
        }

        private void editRouteLink_Click(object sender, RoutedEventArgs e)
        {
            MobileSrc.Commuter.Shared.RouteDefinition route = (MobileSrc.Commuter.Shared.RouteDefinition)((Control)sender).Tag;
            DataContextManager.SelectedRoute = route;

            Uri uri = new Uri("/RouteEditor.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);

            routeList.ItemsSource = null;
            routeList.ItemsSource = DataContextManager.SelectedCommute.Routes;
        }

        private void weekDaysButton_Click(object sender, RoutedEventArgs e)
        {
            DayPicker.SelectedDays = DataContextManager.SelectedCommute.DaysOfWeek;
            DayPicker.Closing += new EventHandler(DayPicker_Closing);

            Uri uri = new Uri("/DayPicker.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }

        void DayPicker_Closing(object sender, EventArgs e)
        {
            DataContextManager.SelectedCommute.DaysOfWeek = DayPicker.SelectedDays;
            weekDaysButton.Content = DataContextManager.SelectedCommute.DaysOfWeekString;
            DayPicker.Closing -= new EventHandler(DayPicker_Closing);
        }

        private void ReturnDatePicker_ValueChanged(object sender, DateTimeValueChangedEventArgs e)
        {
            DataContextManager.SelectedCommute.ReturnTime = e.NewDateTime.Value;
        }

        private void DepartDatePicker_ValueChanged(object sender, DateTimeValueChangedEventArgs e)
        {
            DataContextManager.SelectedCommute.DepartureTime = e.NewDateTime.Value;
        }
    }
}