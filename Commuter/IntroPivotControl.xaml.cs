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
    public partial class IntroPivotControl : UserControl, ICommutePivotControl
    {
        public IntroPivotControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(
                    "Source",
                    typeof(MobileSrc.Commuter.Shared.CommuteDefinition),
                    typeof(IntroPivotControl),
                    new PropertyMetadata(OnSourceChanged));

        public MobileSrc.Commuter.Shared.CommuteDefinition Source
        {
            get { return (MobileSrc.Commuter.Shared.CommuteDefinition)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IntroPivotControl ctrl = (IntroPivotControl)d;
            ctrl.OnSourceChanged(e.OldValue, e.NewValue);
        }

        protected virtual void OnSourceChanged(object oldHeader, object newHeader)
        {
        }

        public void Unselect()
        {
        }

        public void Select()
        {
        }

        public void Refresh()
        {
        }

        public bool SetDirection(bool isReturn)
        {

            return false;
        }
    }
}
