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

namespace Commuter
{
    public partial class RouteViewer : CustomPhoneApplicationPage
    {
        public static MobileSrc.Commuter.Shared.RouteDefinition SelectedDefinition
        {
            get;
            set;
        }

        public List<IndexWrapper<MobileSrc.Commuter.Shared.RouteDirection>> RoutePoints
        {
            get;
            set;
        }

        public RouteViewer()
        {
            RoutePoints = new List<IndexWrapper<MobileSrc.Commuter.Shared.RouteDirection>>();
            InitializeComponent();

            foreach (MobileSrc.Commuter.Shared.RouteDirection route in SelectedDefinition.Directions)
            {
                RoutePoints.Add(new IndexWrapper<MobileSrc.Commuter.Shared.RouteDirection>(RoutePoints, route));
            }
            directionList.ItemsSource = RoutePoints;
        }
    }
}