using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Advertising.Mobile.UI;
using System.Threading;

namespace Commuter
{
    public class CustomPhoneApplicationPage : PhoneApplicationPage
    {
        private static Popup _splashPopup = null;
        private static Popup _adPopup = new Popup();
        private static Image splashImage = null;
        private static AdControl _adControl = null;
        private static Thread _adThread = null;
        private static bool _isLoading = false;

        public CustomPhoneApplicationPage()
        {
            InitializeChildComponents();
            OnSplashBegin();

            if (App.IsTrial)
            {
                if (null == _adControl)
                {
                    PrepareAdSplash();

                    _adThread = new Thread(new ThreadStart(AdThread));
                    _adThread.IsBackground = true;
                    _adThread.Start();
                }
            }

            if (App.DisplaySplash)
            {
                _isLoading = true;
                DisplayIntroSplash();
            }
            else
            {
                BindData();
                OnSplashEnd();
            }
        }

        AutoResetEvent waitEvent = new AutoResetEvent(false);

        private void AdThread()
        {
            while (_isLoading || DataContextManager.Settings.IsFirstRun)
            {
                Thread.Sleep(1000 * 1);
            }
            Thread.Sleep(1000 * 5);
            while (App.IsTrial)
            {
                this.Dispatcher.BeginInvoke(DisplayAdSplash);
                // show the ad
                while (waitEvent.WaitOne(15 * 1000))
                {
                    Thread.Sleep(100);
                }
                // hide the ad
                this.Dispatcher.BeginInvoke(HideAdSplash);

                // only show the add once
                return;
            }
        }

        private void PrepareAdSplash()
        {
            Grid grid = new Grid();
            StackPanel panel = new StackPanel();
            StackPanel horizPanel = new StackPanel();
            horizPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
            horizPanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;

            AdControl.TestMode = false;
            _adControl = new AdControl("5ed86f2d-b6c0-44bf-b0bb-d06753de9b91", "24629", AdModel.Contextual, false);
            _adControl.Width = 480;
            _adControl.Height = 80;
            _adControl.BorderThickness = new Thickness(0);

            TextBlock purchaseText = new TextBlock();
            purchaseText.Padding = new Thickness(10);
            purchaseText.Text = "The trial version of Commuter is ad-supported.  Upgrade to the full version and get push-notifications and live-tile updates.";
            purchaseText.TextWrapping = TextWrapping.Wrap;
            purchaseText.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            purchaseText.FontSize = 30;
            purchaseText.Foreground = new SolidColorBrush(Colors.White);

            HyperlinkButton buyCommuterButton = new HyperlinkButton();
            buyCommuterButton.Content = "Purchase Commuter";
            buyCommuterButton.Click += new RoutedEventHandler(buyCommuterButton_Click);
            buyCommuterButton.FontSize = 30;
            buyCommuterButton.Padding = new Thickness(5, 15, 5, 5);
            buyCommuterButton.Foreground = new SolidColorBrush(Colors.White);

            HyperlinkButton nextAdButton = new HyperlinkButton();
            nextAdButton.Content = "View Next Ad";
            nextAdButton.Click += new RoutedEventHandler(nextAdButton_Click);
            nextAdButton.FontSize = 20;
            nextAdButton.Padding = new Thickness(5, 5, 5, 5);
            nextAdButton.Foreground = new SolidColorBrush(Colors.White);

            HyperlinkButton closeAdButton = new HyperlinkButton();
            closeAdButton.Content = "Close Ad";
            closeAdButton.Click += new RoutedEventHandler(closeAdButton_Click);
            closeAdButton.FontSize = 20;
            closeAdButton.Padding = new Thickness(5, 5, 5, 5);
            closeAdButton.Foreground = new SolidColorBrush(Colors.White);

            horizPanel.Children.Add(nextAdButton);
            horizPanel.Children.Add(closeAdButton);

            panel.Children.Add(_adControl);
            panel.Children.Add(horizPanel);
            panel.Children.Add(buyCommuterButton);
            panel.Children.Add(purchaseText);

            Border border = new Border();
            border.Opacity = 0.85;
            border.Width = 480;
            border.Height = 800;
            border.Background = new SolidColorBrush(Colors.Black);

            Border frontBorder = new Border();
            frontBorder.Width = 480;
            frontBorder.Child = panel;
            frontBorder.VerticalAlignment = System.Windows.VerticalAlignment.Top;

            grid.Children.Add(border);
            grid.Children.Add(frontBorder);

            _adPopup = new Popup();
            _adPopup.Child = grid;
        }

        void closeAdButton_Click(object sender, RoutedEventArgs e)
        {
            _adPopup.IsOpen = false;
        }

        private void DisplayAdSplash()
        {
            _adPopup.IsOpen = true;
        }

        private void HideAdSplash()
        {
            _adPopup.IsOpen = false;
        }

        void buyCommuterButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Phone.Tasks.MarketplaceDetailTask searchTask = new Microsoft.Phone.Tasks.MarketplaceDetailTask();
            searchTask.Show();
        }

        void nextAdButton_Click(object sender, RoutedEventArgs e)
        {
            _adControl.RequestNextAd();
            waitEvent.Set();
        }

        private void DisplayIntroSplash()
        {
            App.DisplaySplash = false;
            this.Loaded += new RoutedEventHandler(CustomPhoneApplicationPage_Loaded);

            splashImage = new Image();
            splashImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("Themes/SplashScreenImage.jpg", UriKind.Relative));

            _splashPopup = new Popup();
            _splashPopup.Child = splashImage;
            _splashPopup.IsOpen = true;
        }

        protected virtual void InitializeChildComponents()
        {
            //
        }

        protected virtual UIElement MainElement
        {
            get { return this.RootElement.Children[0]; }
        }

        protected virtual Grid RootElement
        {
            get { return this.Content as Grid; }
        }

        private void CustomPhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            _splashPopup.Child = null;
            this.RootElement.Children.Add(splashImage);
            _splashPopup.IsOpen = false;
            if (null != this.MainElement)
            {
                this.MainElement.Visibility = System.Windows.Visibility.Visible;
            }

            this.Loaded -= new RoutedEventHandler(CustomPhoneApplicationPage_Loaded);
            AsyncTask.Execute(this.Dispatcher, () => HandleAsyncLoad(), (ex) => HandleAsyncLoadComplete(ex));
        }

        protected virtual void HandleAsyncLoad()
        {
            //
        }

        protected virtual void BindData()
        {
            //
        }

        private void HandleAsyncLoadComplete(Exception ex)
        {
            BindData();

            Storyboard board = new Storyboard();
            board.AutoReverse = false;
            board.BeginTime = TimeSpan.FromSeconds(0.25);
            board.Duration = new Duration(TimeSpan.FromSeconds(0.5));

            CompositeTransform transform = new CompositeTransform();
            transform.CenterX = 240;
            transform.CenterY = 180;

            splashImage.RenderTransform = transform;

            DoubleAnimation scaleYAnim = GetAnimation(splashImage.RenderTransform, CompositeTransform.TranslateYProperty, 0, 800);
            DoubleAnimation fadeOutAnim = GetAnimation(splashImage, UIElement.OpacityProperty, 1, 0.0);

            if (null != this.MainElement)
            {
                DoubleAnimation fadeInAnim = GetAnimation(this.MainElement, UIElement.OpacityProperty, 0, 1.0);
                board.Children.Add(fadeInAnim);
            }

            board.Children.Add(scaleYAnim);

            board.Completed += new EventHandler(board_Completed);
            board.Begin();
            System.Diagnostics.Debug.WriteLine("HandleAsyncLoadComplete");
        }

        private DoubleAnimation GetAnimation(DependencyObject image, DependencyProperty property, double from, double to)
        {
            DoubleAnimation animation = new DoubleAnimation();
            animation.BeginTime = TimeSpan.FromSeconds(0);
            animation.SpeedRatio = 2;

            Storyboard.SetTarget(animation, image);
            Storyboard.SetTargetProperty(animation, new PropertyPath(property));

            animation.From = from;
            animation.To = to;

            return animation;
        }

        private void board_Completed(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("board_Completed");
            if (null != this.MainElement)
            {
                System.Diagnostics.Debug.WriteLine("board_Completed1");
                this.MainElement.Visibility = System.Windows.Visibility.Visible;
            }
            System.Diagnostics.Debug.WriteLine("board_Completed2");
            OnSplashEnd();
            System.Diagnostics.Debug.WriteLine("board_Completed3");
        }

        protected virtual void OnSplashBegin()
        {
            //
        }

        protected virtual void OnSplashEnd()
        {
            _isLoading = false;
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (null != App.TransitionControl)
            {
                if (e.NavigationMode == System.Windows.Navigation.NavigationMode.Back)
                {
                    App.TransitionControl.Transition = "RightTransition";
                }
                else
                {
                    App.TransitionControl.Transition = "SwingInTransition";
                }
            }
            else
            {
            }
            base.OnNavigatingFrom(e);
        }
    }
}
