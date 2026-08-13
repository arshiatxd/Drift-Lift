using System;
using System.Windows;
using System.Windows.Media.Animation;
using DriftLift.Services;
namespace DriftLift.Views.Windows
{
    public partial class HidHidePromptWindow : Window
    {
        public bool InstalledAndConfigured { get; private set; }
        public HidHidePromptWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Opacity = 0;
            var scaleXAnim = new DoubleAnimation(0.85, 1.0, new Duration(TimeSpan.FromMilliseconds(220)))
            {
                EasingFunction = new BackEase { Amplitude = 0.35, EasingMode = EasingMode.EaseOut }
            };
            var scaleYAnim = new DoubleAnimation(0.85, 1.0, new Duration(TimeSpan.FromMilliseconds(220)))
            {
                EasingFunction = new BackEase { Amplitude = 0.35, EasingMode = EasingMode.EaseOut }
            };
            var fadeAnim = new DoubleAnimation(0, 1.0, new Duration(TimeSpan.FromMilliseconds(200)))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            ScaleTrans.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleXProperty, scaleXAnim);
            ScaleTrans.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleYProperty, scaleYAnim);
            BeginAnimation(OpacityProperty, fadeAnim);
        }
        private async void Download_Click(object sender, RoutedEventArgs e)
        {
            PromptGrid.Visibility = Visibility.Collapsed;
            ProgressGrid.Visibility = Visibility.Visible;
            var progress = new Progress<(double ProgressPercentage, string DownloadStats)>(data =>
            {
                double barTotalWidth = ProgressGrid.ActualWidth > 50 ? ProgressGrid.ActualWidth : 420;
                ProgressBarFill.Width = Math.Clamp(data.ProgressPercentage / 100.0 * barTotalWidth, 4, barTotalWidth);
                ProgressDetailsLbl.Text = $"{data.ProgressPercentage:F0}% ({data.DownloadStats})";
            });
            Action<string> statusCallback = statusMsg =>
            {
                Dispatcher.Invoke(() =>
                {
                    StatusLbl.Text = statusMsg;
                });
            };
            try
            {
                await HidHideInstallerService.DownloadAndInstallAsync(progress, statusCallback);
                InstalledAndConfigured = true;
                AnimateClose(true);
            }
            catch (Exception ex)
            {
                StatusLbl.Text = "HidHide Installation Cancelled or Failed";
                ErrorLbl.Text = ex.Message;
                ErrorLbl.Visibility = Visibility.Visible;
                ErrorContinueBtn.Visibility = Visibility.Visible;
            }
        }
        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            AnimateClose(false);
        }
        private void AnimateClose(bool dialogResult)
        {
            var scaleXAnim = new DoubleAnimation(1.0, 0.9, new Duration(TimeSpan.FromMilliseconds(130)))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };
            var scaleYAnim = new DoubleAnimation(1.0, 0.9, new Duration(TimeSpan.FromMilliseconds(130)))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };
            var fadeAnim = new DoubleAnimation(1.0, 0, new Duration(TimeSpan.FromMilliseconds(130)))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };
            fadeAnim.Completed += (s, e) =>
            {
                DialogResult = dialogResult;
                Close();
            };
            ScaleTrans.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleXProperty, scaleXAnim);
            ScaleTrans.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleYProperty, scaleYAnim);
            BeginAnimation(OpacityProperty, fadeAnim);
        }
    }
}
