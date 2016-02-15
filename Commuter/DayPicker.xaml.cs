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
    public partial class DayPicker : CustomPhoneApplicationPage
    {
        public static event EventHandler Closing;

        public static int SelectedDays
        {
            get;
            set;
        }

        public DayPicker()
        {
            BindControls();
        }

        protected override void InitializeChildComponents()
        {
            InitializeComponent();
        }

        private void BindControls()
        {
            for (int i = (int)DayOfWeek.Sunday; i <= (int)DayOfWeek.Saturday; ++i)
            {
                CheckBox checkBox = (CheckBox)this.FindName("day" + i);
                int pow = (int)Math.Pow(2, i);

                if (0 != (SelectedDays & pow))
                {
                    checkBox.IsChecked = true;
                }
                else
                {
                    checkBox.IsChecked = false;
                }
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            SelectedDays = 0;
            for (int i = (int)DayOfWeek.Sunday; i <= (int)DayOfWeek.Saturday; ++i)
            {
                CheckBox checkBox = (CheckBox)this.FindName("day" + i);
                int pow = (int)Math.Pow(2, i);

                if (checkBox.IsChecked.HasValue && checkBox.IsChecked.Value)
                {
                    SelectedDays += pow;
                }
            }
            if (null != Closing)
            {
                Closing(this, null);
            }
            this.NavigationService.GoBack();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.NavigationService.GoBack();
        }
    }
}
