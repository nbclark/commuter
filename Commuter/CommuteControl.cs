using System;
using System.Windows.Controls;
using System.Windows;

namespace Commuter
{
    public class CommuteControl1 : Control
    {
        public CommuteControl1()
        {
            this.DefaultStyleKey = typeof(CommuteControl1);
        }

        #region Source
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(
                    "Source",
                    typeof(MobileSrc.Commuter.Shared.CommuteDefinition),
                    typeof(CommuteControl1),
                    new PropertyMetadata(OnSourceChanged));

        public MobileSrc.Commuter.Shared.CommuteDefinition Source
        {
            get { return (MobileSrc.Commuter.Shared.CommuteDefinition)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CommuteControl1 ctrl = (CommuteControl1)d;
            ctrl.OnSourceChanged(e.OldValue, e.NewValue);
        }

        protected virtual void OnSourceChanged(object oldHeader, object newHeader)
        {
        }
        #endregion
    }
}
