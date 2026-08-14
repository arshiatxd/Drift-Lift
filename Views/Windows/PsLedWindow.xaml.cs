using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using DriftLift.Core.Input;

namespace DriftLift.Views.Windows
{

    public partial class PsLedWindow : Window
    {
        private readonly IPhysicalController? _controller;
        private bool _isInitialized = false;

        public byte SelectedR { get; private set; } = 255;
        public byte SelectedG { get; private set; } = 0;
        public byte SelectedB { get; private set; } = 0;
        public double Brightness { get; private set; } = 1.0;

        public PsLedWindow(IPhysicalController? controller, byte r = 255, byte g = 0, byte b = 0, double brightness = 1.0)
        {
            InitializeComponent();
            _controller = controller;
            SelectedR = r;
            SelectedG = g;
            SelectedB = b;
            Brightness = brightness;

            _isInitialized = true;

            ColorWheel.SelectedColor = Color.FromRgb(r, g, b);
            ColorWheel.ColorChanged += ColorWheel_ColorChanged;

            if (BrightnessSlider != null) BrightnessSlider.Value = brightness * 100.0;
            UpdateUiAndController();
        }

        private void ColorWheel_ColorChanged(object? sender, Color color)
        {
            SelectedR = color.R;
            SelectedG = color.G;
            SelectedB = color.B;
            UpdateUiAndController();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Opacity = 0;
            var scaleXAnim = new DoubleAnimation(0.95, 1.0, new Duration(TimeSpan.FromMilliseconds(180)))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            var scaleYAnim = new DoubleAnimation(0.95, 1.0, new Duration(TimeSpan.FromMilliseconds(180)))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            var fadeAnim = new DoubleAnimation(0, 1.0, new Duration(TimeSpan.FromMilliseconds(160)))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            ScaleTrans.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnim);
            ScaleTrans.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnim);
            BeginAnimation(OpacityProperty, fadeAnim);
        }

        private void ColorSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_isInitialized || BrightnessSlider == null) return;
            Brightness = BrightnessSlider.Value / 100.0;
            UpdateUiAndController();
        }

        private void Swatch_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement el && el.Tag is string hex)
            {
                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(hex);
                    SelectedR = color.R;
                    SelectedG = color.G;
                    SelectedB = color.B;
                    ColorWheel.SelectedColor = color;
                    UpdateUiAndController();
                }
                catch { }
            }
        }

        private void UpdateUiAndController()
        {
            if (!_isInitialized || BrightnessValLbl == null) return;
            
            int bInt = (int)(Brightness * 100);
            BrightnessValLbl.Text = $"{bInt}%";
            BrightnessStatusText.Text = bInt == 0 ? "0% (OFF)" : (bInt == 100 ? "100% (Brightest)" : $"{bInt}% Brightness");

            byte finalR = (byte)(SelectedR * Brightness);
            byte finalG = (byte)(SelectedG * Brightness);
            byte finalB = (byte)(SelectedB * Brightness);

            var previewColor = Color.FromRgb(finalR, finalG, finalB);
            PreviewFill.Background = new SolidColorBrush(previewColor);
            PreviewBorder.BorderBrush = new SolidColorBrush(previewColor);

            _controller?.SetLedColor(finalR, finalG, finalB);
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            AnimateClose(true);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            AnimateClose(false);
        }

        private void AnimateClose(bool dialogResult)
        {
            var scaleXAnim = new DoubleAnimation(1.0, 0.95, new Duration(TimeSpan.FromMilliseconds(120)))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };
            var scaleYAnim = new DoubleAnimation(1.0, 0.95, new Duration(TimeSpan.FromMilliseconds(120)))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };
            var fadeAnim = new DoubleAnimation(1.0, 0, new Duration(TimeSpan.FromMilliseconds(120)))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            fadeAnim.Completed += (s, e) =>
            {
                DialogResult = dialogResult;
                Close();
            };

            ScaleTrans.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnim);
            ScaleTrans.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnim);
            BeginAnimation(OpacityProperty, fadeAnim);
        }
    }
}