using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

namespace Commuter
{
    public partial class CommuteViewer : CustomPhoneApplicationPage
    {
        private int _refreshCount = 0;
        private bool _skipPivotLoad = false;
        private bool _firstLoad = true;

        public static CommuteViewer Instance
        {
            get;
            set;
        }

        public CommuteViewer()
        {
            Instance = this;

            this.pivotControl.UnloadingPivotItem += new EventHandler<PivotItemEventArgs>(pivotControl_UnloadingPivotItem);
            this.pivotControl.LoadedPivotItem += new EventHandler<PivotItemEventArgs>(pivotControl_LoadedPivotItem);

            this.overviewControl.RefreshComplete += new EventHandler(control_RefreshComplete);
            this.historyControl.RefreshComplete += new EventHandler(control_RefreshComplete);
        }

        protected override void InitializeChildComponents()
        {
            InitializeComponent();
        }

        protected override void BindData()
        {
            BindCommuteData();
            _skipPivotLoad = false;
        }

        protected void BindCommuteData()
        {
            overviewControl.Source = DataContextManager.SelectedCommute;

            if (DataContextManager.SelectedCommute.IsNew)
            {
                DataContextManager.SelectedCommute.IsNew = false;
                Uri uri = new Uri("/CommuteEditor.xaml", UriKind.Relative);
                NavigationService.Navigate(uri);

                _skipPivotLoad = true;
                return;
            }
            double timeFromDeparture = Math.Abs((DateTime.Now.TimeOfDay.TotalHours - DataContextManager.SelectedCommute.DepartureTime.TimeOfDay.TotalHours));
            double timeFromReturn = Math.Abs((DateTime.Now.TimeOfDay.TotalHours - DataContextManager.SelectedCommute.ReturnTime.TimeOfDay.TotalHours));

            bool reverseRoute = (timeFromReturn < timeFromDeparture);

            if (reverseRoute)
            {
                returnCheck.IsChecked = true;
            }
            else
            {
                departureCheck.IsChecked = true;
            }


            Utils.SetBinding(pivotControl, Pivot.TitleProperty, DataContextManager.SelectedCommute, "Name", BindingMode.TwoWay);

            if (reverseRoute)
            {
                updatedLabel.Text = string.Concat("", DataContextManager.SelectedCommute.LastUpdatedRet.ToString("MM/dd @ hh:mm tt"));
            }
            else
            {
                updatedLabel.Text = string.Concat("", DataContextManager.SelectedCommute.LastUpdated.ToString("MM/dd @ hh:mm tt"));
            }
        }

        protected override UIElement MainElement
        {
            get
            {
                return null;
            }
        }

        protected override Grid RootElement
        {
            get
            {
                return this.LayoutRoot;
            }
        }

        protected override void OnSplashBegin()
        {
            this.ApplicationBar.IsVisible = false;
            this.updatedLabel.Visibility = System.Windows.Visibility.Collapsed;
            this.pivotControl.Visibility = System.Windows.Visibility.Collapsed;
        }

        protected override void OnSplashEnd()
        {
            this.ApplicationBar.IsVisible = true;
            this.updatedLabel.Visibility = System.Windows.Visibility.Visible;
            this.pivotControl.Visibility = System.Windows.Visibility.Visible;

            if (!DataContextManager.Settings.ShownRatingRequest && DataContextManager.Settings.FirstRunDate.AddDays(1) < DateTime.Now)
            {
                if (MessageBoxResult.OK == MessageBox.Show("We would love for you to help spread the word about Commuter. Would you like to review or rate Commuter now?", "Review Commuter?", MessageBoxButton.OKCancel))
                {
                    Microsoft.Phone.Tasks.MarketplaceReviewTask reviewTask = new MarketplaceReviewTask();
                    reviewTask.Show();
                }
                DataContextManager.Settings.ShownRatingRequest = true;
                DataContextManager.Save();
            }
            base.OnSplashEnd();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (_firstLoad)
            {
                _firstLoad = false;
                return;
            }
            bool isNew = DataContextManager.SelectedCommute.IsNew;
            BindCommuteData();
            if (!isNew)
            {
                pivotControl.SelectedIndex = 0;
                if (null != pivotControl.SelectedItem)
                {
                    if (!_skipPivotLoad)
                    {
                        ((ICommutePivotControl)((PivotItem)pivotControl.SelectedItem).Content).Select();
                    }
                }
                _skipPivotLoad = false;
            }
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            foreach (PivotItem pivotItem in pivotControl.Items)
            {
                ((ICommutePivotControl)pivotItem.Content).Unselect();
            }
            base.OnNavigatingFrom(e);
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            string szTitle = DataContextManager.SelectedCommute.Name;
            string szMessage = "Are you sure you want to delete this commute?";

            if (MessageBox.Show(szMessage, szTitle, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                DataContextManager.Commutes.Remove(DataContextManager.SelectedCommute);
                NavigationService.GoBack();
            }
        }

        private void listButton_Click(object sender, EventArgs e)
        {
            Uri uri = new Uri("/IntroPage.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }

        private void editButton_Click(object sender, EventArgs e)
        {
            Uri uri = new Uri("/CommuteEditor.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            Refresh();
        }

        void Refresh()
        {
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = false;
            progressBar.Visibility = System.Windows.Visibility.Visible;

            if (_refreshCount == 0)
            {
                _refreshCount = 2;

                overviewControl.Refresh();
                historyControl.Refresh();
            }
        }

        void pivotControl_LoadedPivotItem(object sender, PivotItemEventArgs e)
        {
            if (!_skipPivotLoad)
            {
                ((ICommutePivotControl)e.Item.Content).Select();
            }
        }

        void pivotControl_UnloadingPivotItem(object sender, PivotItemEventArgs e)
        {
            ((ICommutePivotControl)e.Item.Content).Unselect();
        }

        void control_RefreshComplete(object sender, EventArgs e)
        {
            _refreshCount--;

            if (_refreshCount <= 0)
            {
                detailControl.Refresh();

                if (returnCheck.IsChecked.Value)
                {
                    DataContextManager.SelectedCommute.LastUpdatedRet = DateTime.Now;
                }
                else
                {
                    DataContextManager.SelectedCommute.LastUpdated = DateTime.Now;
                }
                updatedLabel.Text = string.Concat("", DateTime.Now.ToString("MM/dd @ hh:mm tt"));

                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = true;
                progressBar.Visibility = System.Windows.Visibility.Collapsed;
                _refreshCount = 0;

                if (0 != (DataContextManager.SelectedCommute.DaysOfWeek & (int)Math.Pow(2, (int)DateTime.Now.DayOfWeek)))
                {
                    MobileSrc.Commuter.Shared.RouteDefinition bestRoute = null;
                    foreach (MobileSrc.Commuter.Shared.RouteDefinition route in DataContextManager.SelectedCommute.Routes)
                    {
                        if (bestRoute == null)
                        {
                            bestRoute = route;
                        }
                        else
                        {
                            TimeSpan durA = (returnCheck.IsChecked.Value) ? route.EstimatedRetDuration : route.EstimatedDuration;
                            TimeSpan durB = (returnCheck.IsChecked.Value) ? bestRoute.EstimatedRetDuration : bestRoute.EstimatedDuration;
                            if (durA < durB)
                            {
                                bestRoute = route;
                            }
                        }
                    }

                    if (null != bestRoute)
                    {
                        string tileUrl = string.Format(@"http://mobilesrc.com/commuter/LiveTile.aspx?commute={0}&route={1}&duration={2}&interval={3}&day={4}&color={5}", DataContextManager.SelectedCommute.Name, bestRoute.Name, (int)((returnCheck.IsChecked.Value) ? bestRoute.EstimatedRetDuration.TotalMinutes : bestRoute.EstimatedDuration.TotalMinutes), "min", DateTime.Now.ToString("dddd @ hh:mm tt"), App.AccentColor);

                        Utils.RequestTileUpdate(tileUrl);
                    }
                }
                DataContextManager.Save();
            }
        }

        private void departureCheck_Checked(object sender, RoutedEventArgs e)
        {
            bool reverseRoute = (sender == returnCheck);
            
            bool needsRefresh = this.historyControl.SetDirection(reverseRoute);
            needsRefresh = this.overviewControl.SetDirection(reverseRoute) || needsRefresh;
            needsRefresh = this.detailControl.SetDirection(reverseRoute) || needsRefresh;

            if (needsRefresh)
            {
                this.Refresh();
            }
        }
    }
}