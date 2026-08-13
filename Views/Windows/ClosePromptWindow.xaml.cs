using System;
using System.Windows;
using System.Windows.Media.Animation;
namespace DriftLift.Views.Windows
{
    public partial class ClosePromptWindow : Window
    {
        public bool ResultExitCompletely { get; private set; }
        public bool RememberChoice => RememberChoiceCb.IsChecked == true;
        public ClosePromptWindow()
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
        private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            ResultExitCompletely = false;
            AnimateClose(true);
        }
        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            ResultExitCompletely = true;
            AnimateClose(true);
        }
        private void CancelPopup_Click(object sender, RoutedEventArgs e)
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
