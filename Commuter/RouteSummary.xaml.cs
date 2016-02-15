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

namespace Commuter
{
    public partial class RouteSummary : UserControl
    {
        public RouteSummary()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(
                    "Source",
                    typeof(MobileSrc.Commuter.Shared.RouteDefinition),
                    typeof(RouteSummary),
                    new PropertyMetadata(OnSourceChanged));

        public MobileSrc.Commuter.Shared.RouteDefinition Source
        {
            get { return (MobileSrc.Commuter.Shared.RouteDefinition)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            if (null != Closed)
            {
                Closed(this, null);
            }
        }

        public event EventHandler Closed;
    }
}
