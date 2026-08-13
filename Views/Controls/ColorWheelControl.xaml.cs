using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DriftLock.Views.Controls
{
    // ##== Interactive Circular RGB Color Wheel Logic ==##
    public partial class ColorWheelControl : UserControl
    {
        private bool _isDragging = false;

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
            double size = 180;
            double radius = size / 2.0;

            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext dc = drawingVisual.RenderOpen())
            {
                for (int angle = 0; angle < 360; angle += 2)
                {
                    double rad1 = angle * Math.PI / 180.0;
                    double rad2 = (angle + 3) * Math.PI / 180.0;

                    Point p1 = new Point(radius + radius * Math.Cos(rad1), radius + radius * Math.Sin(rad1));
                    Point p2 = new Point(radius + radius * Math.Cos(rad2), radius + radius * Math.Sin(rad2));
                    Point center = new Point(radius, radius);

                    PathGeometry geo = new PathGeometry();
                    PathFigure fig = new PathFigure { StartPoint = center, IsClosed = true };
                    fig.Segments.Add(new LineSegment(p1, true));
                    fig.Segments.Add(new LineSegment(p2, true));
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
            _isDragging = false;
            WheelGrid.ReleaseMouseCapture();
        }

        private void ProcessMouseInput(Point pos)
        {
            double center = 90.0;
            double dx = pos.X - center;
            double dy = pos.Y - center;
            double dist = Math.Sqrt(dx * dx + dy * dy);
            double maxR = 78.0;

            if (dist > maxR)
            {
                dx = (dx / dist) * maxR;
                dy = (dy / dist) * maxR;
                dist = maxR;
            }

            double angleRad = Math.Atan2(dy, dx);
            double angleDeg = angleRad * 180.0 / Math.PI;
            if (angleDeg < 0) angleDeg += 360;

            double sat = Math.Clamp(dist / maxR, 0.0, 1.0);
            Color selected = HsvToRgb(angleDeg, sat, 1.0);

            SelectedColor = selected;
            ColorChanged?.Invoke(this, selected);

            Canvas.SetLeft(ThumbRing, (center + dx) - 7);
            Canvas.SetTop(ThumbRing, (center + dy) - 7);
        }

        private void UpdateThumbPosition(Color c)
        {
            if (_isDragging) return;
            RgbToHsv(c, out double hue, out double sat, out _);
            double radius = sat * 78.0;
            double rad = hue * Math.PI / 180.0;
            double x = 90.0 + radius * Math.Cos(rad);
            double y = 90.0 + radius * Math.Sin(rad);

            Canvas.SetLeft(ThumbRing, x - 7);
            Canvas.SetTop(ThumbRing, y - 7);
        }

        private static Color HsvToRgb(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            byte v = (byte)Math.Clamp(value, 0, 255);
            byte p = (byte)Math.Clamp(value * (1 - saturation), 0, 255);
            byte q = (byte)Math.Clamp(value * (1 - f * saturation), 0, 255);
            byte t = (byte)Math.Clamp(value * (1 - (1 - f) * saturation), 0, 255);

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
            if (delta > 0)
            {
                if (max == r) hue = (g - b) / delta + (g < b ? 6 : 0);
                else if (max == g) hue = (b - r) / delta + 2;
                else hue = (r - g) / delta + 4;
                hue *= 60;
            }

            saturation = max == 0 ? 0 : delta / max;
            value = max;
        }
    }
}
