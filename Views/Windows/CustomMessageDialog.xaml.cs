using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DriftLift.Views.Windows
{

    public partial class CustomMessageDialog : Window
    {
        public CustomMessageDialog(string message, string title = "DRIFT-LIFT NOTIFICATION", bool isConfirm = false)
        {
            InitializeComponent();
            TitleTxt.Text = title.ToUpper();
            MessageTxt.Text = message;
            if (isConfirm)
            {
                CancelBtn.Visibility = Visibility.Visible;
                IconTxt.Text = "❓";
            }
        }

        public static bool Show(string message, string title = "DRIFT-LIFT NOTIFICATION", bool isConfirm = false)
        {
            var dlg = new CustomMessageDialog(message, title, isConfirm);
            if (Application.Current != null && Application.Current.MainWindow != null && Application.Current.MainWindow.IsVisible)
            {
                dlg.Owner = Application.Current.MainWindow;
            }
            return dlg.ShowDialog() == true;
        }

        public static bool Show(string message, string title, MessageBoxButton button, MessageBoxImage icon)
        {
            bool isConfirm = button == MessageBoxButton.YesNo || button == MessageBoxButton.OKCancel;
            return Show(message, title, isConfirm);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Opacity = 0;
            var scaleXAnim = new DoubleAnimation(0.95, 1.0, new Duration(TimeSpan.FromMilliseconds(160)))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            var fadeAnim = new DoubleAnimation(0, 1.0, new Duration(TimeSpan.FromMilliseconds(140)))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            ScaleTrans.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnim);
            BeginAnimation(OpacityProperty, fadeAnim);
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
