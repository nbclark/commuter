using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Navigation;

namespace Commuter
{
    public partial class CommuteDetailControl : UserControl, ICommutePivotControl
    {
        public event EventHandler RefreshComplete;

        public List<IndexWrapper<MobileSrc.Commuter.Shared.RouteDefinition>> Routes
        {
            get;
            set;
        }

        public CommuteDetailControl()
        {
            InitializeComponent();
        }

        private void BindData()
        {
            Routes = new List<IndexWrapper<MobileSrc.Commuter.Shared.RouteDefinition>>();

            if (null != DataContextManager.SelectedCommute)
            {
                foreach (MobileSrc.Commuter.Shared.RouteDefinition route in DataContextManager.SelectedCommute.Routes)
                {
                    IndexWrapper < MobileSrc.Commuter.Shared.RouteDefinition >  obj = new IndexWrapper<MobileSrc.Commuter.Shared.RouteDefinition>(Routes, route);
                    obj.Message = (_isReturn) ? route.TravelSummaryRet : route.TravelSummary;
                    Routes.Add(obj);
                }
                Routes.Sort(new Comparison<IndexWrapper<MobileSrc.Commuter.Shared.RouteDefinition>>(SortRoutes));
                routeList.ItemsSource = null;
                routeList.ItemsSource = Routes;
            }
        }

        private int SortRoutes(IndexWrapper<MobileSrc.Commuter.Shared.RouteDefinition> a, IndexWrapper<MobileSrc.Commuter.Shared.RouteDefinition> b)
        {
            TimeSpan durA = (_isReturn) ? a.Data.EstimatedRetDuration : a.Data.EstimatedDuration;
            TimeSpan durB = (_isReturn) ? b.Data.EstimatedRetDuration : b.Data.EstimatedDuration;
            return (durA.CompareTo(durB));
        }

        public void Refresh()
        {
            BindData();

            if (null != RefreshComplete)
            {
                RefreshComplete(this, null);
            }
        }

        public void Select()
        {
            BindData();
        }

        public void Unselect()
        {
        }

        private void routeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (routeList.SelectedIndex >= 0)
            {
                IndexWrapper<MobileSrc.Commuter.Shared.RouteDefinition> item = routeList.SelectedItem as IndexWrapper<MobileSrc.Commuter.Shared.RouteDefinition>;
                if (null != item)
                {
                    RouteViewer.SelectedDefinition = item.Data;
                    Uri uri = new Uri("/RouteViewer.xaml", UriKind.Relative);

                    Page parentPage = null;
                    FrameworkElement parentControl = this.Parent as FrameworkElement;

                    while (parentControl != null)
                    {
                        parentPage = parentControl as Page;

                        if (null != parentPage)
                        {
                            break;
                        }

                        parentControl = parentControl.Parent as FrameworkElement;
                    }

                    if (null != parentPage)
                    {
                        parentPage.NavigationService.Navigate(uri);
                    }
                }
            }
            routeList.SelectedIndex = -1;
        }

        private bool _isReturn = false;
        public bool SetDirection(bool isReturn)
        {
            _isReturn = isReturn;
            Refresh();

            return false;
        }
    }

    public class IndexWrapper<T>
    {
        private List<IndexWrapper<T>> List
        {
            get;
            set;
        }
        public T Data
        {
            get;
            set;
        }
        public string Index
        {
            get { return string.Format("{0}.", List.IndexOf(this)+1); }
        }
        public string Message
        {
            get;
            set;
        }
        public IndexWrapper(List<IndexWrapper<T>> parentList, T data)
        {
            this.List = parentList;
            this.Data = data;
        }
    }

    /// <summary>
    /// A type converter for visibility and boolean values.
    /// </summary>
    public class RowNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            CollectionViewSource collectionViewSource = parameter as CollectionViewSource;

            int counter = 1;
            foreach (object item in collectionViewSource.View)
            {
                if (item == value)
                {
                    return counter.ToString();
                }
                counter++;
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
