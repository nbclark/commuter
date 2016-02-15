using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Controls;
using MobileSrc.Commuter.Shared;

namespace Commuter
{
    public partial class IntroPage : CustomPhoneApplicationPage
    {
        private MobileSrc.Commuter.Shared.CommuteDefinition _showCommute = null;

        public IntroPage()
        {
        }


        protected override void InitializeChildComponents()
        {
            InitializeComponent();
        }

        protected override Grid RootElement
        {
            get
            {
                return this.LayoutRoot;
            }
        }

        protected override UIElement MainElement
        {
            get
            {
                return this.pivotControl;
            }
        }

        protected override void BindData()
        {
            this.commuterList.ItemsSource = DataContextManager.Commutes;

            Binding tileBinding = new Binding();
            tileBinding.Source = DataContextManager.Settings;
            tileBinding.Path = new PropertyPath("EnableTileNotifications");
            tileBinding.Mode = BindingMode.TwoWay;

            enableTileSwitch.SetBinding(ToggleSwitch.IsCheckedProperty, tileBinding);

            Binding toastBinding = new Binding();
            toastBinding.Source = DataContextManager.Settings;
            toastBinding.Path = new PropertyPath("EnableToastNotifications");
            toastBinding.Mode = BindingMode.TwoWay;
            enableToastSwitch.SetBinding(ToggleSwitch.IsCheckedProperty, toastBinding);

            if (App.IsTrial)
            {
                enableTileSwitch.IsChecked = enableToastSwitch.IsChecked = false;
                enableTileSwitch.IsEnabled = enableToastSwitch.IsEnabled = false;
            }
        }

        protected override void HandleAsyncLoad()
        {
        }

        protected override void OnSplashBegin()
        {
            pivotControl.Visibility = System.Windows.Visibility.Collapsed;
        }

        protected override void OnSplashEnd()
        {
            pivotControl.Visibility = System.Windows.Visibility.Visible;

            if (DataContextManager.Commutes.Count > 0)
            {
                foreach (CommuteDefinition commute in DataContextManager.Commutes)
                {
                    if (0 != (commute.DaysOfWeek & (int)DateTime.Now.DayOfWeek))
                    {
                        DataContextManager.SelectedCommute = commute;
                        Uri uri = new Uri("/CommuteViewer.xaml", UriKind.Relative);
                        //NavigationService.Navigate(uri);

                        break;
                    }
                }
            }

            base.OnSplashEnd();
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            if (e.NavigationMode == System.Windows.Navigation.NavigationMode.Back)
            {
                if (DataContextManager.SelectedCommute == null || !DataContextManager.Commutes.Contains(DataContextManager.SelectedCommute))
                {
                    if (DataContextManager.Commutes.Count == 0)
                    {
                        DataContextManager.Commutes.Add(new CommuteDefinition());
                        DataContextManager.Commutes[0].Name = "untitled";
                    }
                    DataContextManager.SelectedCommute = DataContextManager.Commutes[0];
                }
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (DataContextManager.Commutes == null)
            {
                return;
            }
            DataContextManager.Commutes.Refresh();
            base.OnNavigatedTo(e);
        }

        private void commuterList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (commuterList.SelectedIndex >= 0)
            {
                _showCommute = null;

                // selected weather data
                DataContextManager.SelectedCommute = DataContextManager.Commutes[commuterList.SelectedIndex];
                NavigationService.GoBack();
            }
        }

        private void addItemLink_Click(object sender, RoutedEventArgs e)
        {
        }

        private void addCommuteButton_Click(object sender, RoutedEventArgs e)
        {
            // selected weather data
            MobileSrc.Commuter.Shared.CommuteDefinition commuter = new MobileSrc.Commuter.Shared.CommuteDefinition();
            commuter.Name = "untitled";
            commuter.IsNew = true;

            _showCommute = commuter;

            DataContextManager.Commutes.Add(commuter);
            DataContextManager.Save();

            DataContextManager.SelectedCommute = commuter;
            NavigationService.GoBack();
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            CommuteDefinition definition = (CommuteDefinition)button.Tag;

            string szTitle = definition.Name;
            string szMessage = "Are you sure you want to delete this commute?";

            if (MessageBox.Show(szMessage, szTitle, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                DataContextManager.Commutes.Remove(definition);
                DataContextManager.Save();
            }
        }

        void definition_Updated(object sender, EventArgs e)
        {
            CommuteDefinition definition = (CommuteDefinition)sender;
            definition.Updated -= new EventHandler(definition_Updated);

            definition.Updated += new EventHandler(definitionReturn_Updated);
            Utils.RefreshRoutes(definition, true);
        }

        void definitionReturn_Updated(object sender, EventArgs e)
        {
            CommuteDefinition definition = (CommuteDefinition)sender;
            definition.Updated -= new EventHandler(definitionReturn_Updated);

            this.Dispatcher.BeginInvoke(() =>
            {
                this.commuterList.ItemsSource = null;
                this.commuterList.ItemsSource = DataContextManager.Commutes;
            });

            DataContextManager.Save();
        }


        private void mobilesrcLink_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask task = new WebBrowserTask();
            task.URL = "http://www.mobilesrc.com";
            task.Show();
        }
    }

    /// <summary>
    /// A type converter for visibility and boolean values.
    /// </summary>
    public class CommuterOverviewConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isDeparture = System.Convert.ToBoolean(parameter);
            CommuteDefinition definition = value as CommuteDefinition;

            RouteDefinition bestRoute = null;
            foreach (RouteDefinition route in definition.Routes)
            {
                if (bestRoute == null)
                {
                    bestRoute = route;
                }
                else
                {
                    TimeSpan durA = (!isDeparture) ? route.EstimatedRetDuration : route.EstimatedDuration;
                    TimeSpan durB = (!isDeparture) ? bestRoute.EstimatedRetDuration : bestRoute.EstimatedDuration;
                    if (durA < durB)
                    {
                        bestRoute = route;
                    }
                }
            }

            if (null != bestRoute)
            {
                if (isDeparture)
                {
                    return string.Format("Departure: {1}, {0:0.00} min, {2:0.00} mile", bestRoute.EstimatedDurationMinutes, bestRoute.Name, bestRoute.EstimatedDistance);
                }
                else
                {
                    return string.Format("Return: {1}, {0:0.00} min, {2:0.00} mile", bestRoute.EstimatedDurationRetMinutes, bestRoute.Name, bestRoute.EstimatedRetDistance);
                }
            }
            else
            {
                return string.Format("no routes defined");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    /// <summary>
    /// A type converter for visibility and boolean values.
    /// </summary>
    public class CommuterTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            CommuteDefinition definition = value as CommuteDefinition;
            return string.Format("{0} @ {1}{2}-{3}{4}", definition.DaysOfWeekString.Replace(" ", ""), definition.DepartureTime.ToString("h:mm"), definition.DepartureTime.Hour < 12 ? "am" : "pm", definition.ReturnTime.ToString("h:mm"), definition.ReturnTime.Hour < 12 ? "am" : "pm");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}