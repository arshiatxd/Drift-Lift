using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DriftLift.Views.Controls
{
    public partial class ColorWheelControl : UserControl
    {
        private bool _isDragging = false;
        private const double CenterX = 90.0;
        private const double CenterY = 90.0;
        private const double MaxRadius = 82.0;
        private const double ThumbHalfSize = 8.0;

        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register(nameof(SelectedColor), typeof(Color), typeof(ColorWheelControl),
                new FrameworkPropertyMetadata(Colors.Red, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedColorChanged));

        public Color SelectedColor
        {
            get => (Color)GetValue(SelectedColorProperty);
            set => SetValue(SelectedColorProperty, value);
        }

        public event EventHandler<Color>? ColorChanged;

        public ColorWheelControl()
        {
            InitializeComponent();
            Loaded += ColorWheelControl_Loaded;
        }

        private void ColorWheelControl_Loaded(object sender, RoutedEventArgs e)
        {
            DrawSpectrum();
            UpdateThumbPosition(SelectedColor);
        }

        private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ColorWheelControl wheel && e.NewValue is Color color)
            {
                wheel.UpdateThumbPosition(color);
            }
        }

        private void DrawSpectrum()
        {
            SpectrumCanvas.Children.Clear();
            double radius = 90.0;

            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext dc = drawingVisual.RenderOpen())
            {
                Point center = new Point(radius, radius);
                for (int angle = 0; angle < 360; angle++)
                {
                    double rad1 = angle * Math.PI / 180.0;
                    double rad2 = (angle + 1.2) * Math.PI / 180.0;

                    Point p1 = new Point(radius + radius * Math.Cos(rad1), radius + radius * Math.Sin(rad1));
                    Point p2 = new Point(radius + radius * Math.Cos(rad2), radius + radius * Math.Sin(rad2));

                    PathGeometry geo = new PathGeometry();
                    PathFigure fig = new PathFigure { StartPoint = center, IsClosed = true };
                    fig.Segments.Add(new LineSegment(p1, false));
                    fig.Segments.Add(new LineSegment(p2, false));
                    geo.Figures.Add(fig);

                    Color c = HsvToRgb(angle, 1.0, 1.0);
                    dc.DrawGeometry(new SolidColorBrush(c), null, geo);
                }
            }

            RenderTargetBitmap bmp = new RenderTargetBitmap(180, 180, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(drawingVisual);
            SpectrumCanvas.Background = new ImageBrush(bmp);
        }

        private void Wheel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            WheelGrid.CaptureMouse();
            ProcessMouseInput(e.GetPosition(WheelGrid));
        }

        private void Wheel_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                ProcessMouseInput(e.GetPosition(WheelGrid));
            }
        }

        private void Wheel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                WheelGrid.ReleaseMouseCapture();
            }
        }

        private void ProcessMouseInput(Point pos)
        {
            double dx = pos.X - CenterX;
            double dy = pos.Y - CenterY;
            double dist = Math.Sqrt(dx * dx + dy * dy);

            if (dist > MaxRadius && dist > 0)
            {
                dx = (dx / dist) * MaxRadius;
                dy = (dy / dist) * MaxRadius;
                dist = MaxRadius;
            }

            double angleRad = Math.Atan2(dy, dx);
            double angleDeg = angleRad * 180.0 / Math.PI;
            if (angleDeg < 0) angleDeg += 360.0;

            double sat = Math.Clamp(dist / MaxRadius, 0.0, 1.0);
            Color selected = HsvToRgb(angleDeg, sat, 1.0);

            SelectedColor = selected;
            ColorChanged?.Invoke(this, selected);

            Canvas.SetLeft(ThumbRing, (CenterX + dx) - ThumbHalfSize);
            Canvas.SetTop(ThumbRing, (CenterY + dy) - ThumbHalfSize);
        }

        private void UpdateThumbPosition(Color c)
        {
            if (_isDragging) return;

            RgbToHsv(c, out double hue, out double sat, out _);
            double dist = Math.Clamp(sat, 0.0, 1.0) * MaxRadius;
            double rad = hue * Math.PI / 180.0;
            double x = CenterX + dist * Math.Cos(rad);
            double y = CenterY + dist * Math.Sin(rad);

            Canvas.SetLeft(ThumbRing, x - ThumbHalfSize);
            Canvas.SetTop(ThumbRing, y - ThumbHalfSize);
        }

        private static Color HsvToRgb(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60.0)) % 6;
            double f = hue / 60.0 - Math.Floor(hue / 60.0);

            value = value * 255.0;
            byte v = (byte)Math.Clamp(value, 0, 255);
            byte p = (byte)Math.Clamp(value * (1.0 - saturation), 0, 255);
            byte q = (byte)Math.Clamp(value * (1.0 - f * saturation), 0, 255);
            byte t = (byte)Math.Clamp(value * (1.0 - (1.0 - f) * saturation), 0, 255);

            return hi switch
            {
                0 => Color.FromRgb(v, t, p),
                1 => Color.FromRgb(q, v, p),
                2 => Color.FromRgb(p, v, t),
                3 => Color.FromRgb(p, q, v),
                4 => Color.FromRgb(t, p, v),
                _ => Color.FromRgb(v, p, q),
            };
        }

        private static void RgbToHsv(Color color, out double hue, out double saturation, out double value)
        {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;

            hue = 0;
            if (delta > 0.00001)
            {
                if (Math.Abs(max - r) < 0.00001) hue = (g - b) / delta + (g < b ? 6.0 : 0.0);
                else if (Math.Abs(max - g) < 0.00001) hue = (b - r) / delta + 2.0;
                else hue = (r - g) / delta + 4.0;
                hue *= 60.0;
            }

            saturation = max == 0 ? 0 : delta / max;
            value = max;
        }
    }
}
