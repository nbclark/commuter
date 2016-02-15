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
    public partial class TimePicker : CustomPhoneApplicationPage
    {
        public static event EventHandler Closing;

        public static DateTime SelectedDate
        {
            get;
            set;
        }

        public TimePicker()
        {
            BindControls();
        }

        protected override void InitializeChildComponents()
        {
            InitializeComponent();
        }

        private void BindControls()
        {
            hourLabel.Text = SelectedDate.ToString("hh");
            minuteTenLabel.Text = (SelectedDate.Minute / 10).ToString();
            minuteOneLabel.Text = (SelectedDate.Minute % 10).ToString();
            ampmLabel.Text = SelectedDate.ToString("tt").ToUpper();
        }

        private void upbutton_Click(object sender, RoutedEventArgs e)
        {
            int hour = SelectedDate.Hour;
            int minuteTen = SelectedDate.Minute / 10;
            int minuteOne = SelectedDate.Minute % 10;
            bool isAmPm = SelectedDate.Hour >= 12;

            if (hour > 12)
            {
                hour -= 12;
            }
            if (hour == 0)
            {
                hour = 12;
            }

            switch (((Button)sender).Tag.ToString().ToLower())
            {
                case "hour":
                    {
                        hour = (hour + 1) % 12;
                    }
                    break;
                case "minuteten":
                    {
                        minuteTen = (minuteTen + 1) % 6;
                    }
                    break;
                case "minuteone":
                    {
                        minuteOne = (minuteOne + 5) % 10;
                    }
                    break;
                case "ampm":
                    {
                        isAmPm = !isAmPm;
                    }
                    break;
            }

            if (hour == 12)
            {
                if (!isAmPm)
                {
                    hour = 0;
                }
            }
            else if (isAmPm)
            {
                hour += 12;
            }

            SelectedDate = new DateTime(SelectedDate.Year, SelectedDate.Month, SelectedDate.Day, hour, (minuteTen * 10) + minuteOne, 0);
            BindControls();
        }

        private void downbutton_Click(object sender, RoutedEventArgs e)
        {
            int hour = SelectedDate.Hour;
            int minuteTen = SelectedDate.Minute / 10;
            int minuteOne = SelectedDate.Minute % 10;
            bool isAmPm = SelectedDate.Hour >= 12;

            if (hour > 12)
            {
                hour -= 12;
            }
            if (hour == 0)
            {
                hour = 12;
            }

            switch (((Button)sender).Tag.ToString().ToLower())
            {
                case "hour":
                    {
                        hour = (hour + 11) % 12;
                    }
                    break;
                case "minuteten":
                    {
                        minuteTen = (minuteTen + 5) % 6;
                    }
                    break;
                case "minuteone":
                    {
                        minuteOne = (minuteOne + 5) % 10;
                    }
                    break;
                case "ampm":
                    {
                        isAmPm = !isAmPm;
                    }
                    break;
            }

            if (hour == 12)
            {
                if (!isAmPm)
                {
                    hour = 0;
                }
            }
            else if (isAmPm)
            {
                hour += 12;
            }

            SelectedDate = new DateTime(SelectedDate.Year, SelectedDate.Month, SelectedDate.Day, hour, (minuteTen * 10) + minuteOne, 0);
            BindControls();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
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
