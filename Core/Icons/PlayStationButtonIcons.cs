using System.Windows.Media;
namespace DriftLock.Core.Icons
{
    public static class PlayStationButtonIcons
    {
        public static readonly Brush CrossBrush = new SolidColorBrush(Color.FromRgb(52, 152, 219));   
        public static readonly Brush CircleBrush = new SolidColorBrush(Color.FromRgb(255, 23, 68));   
        public static readonly Brush SquareBrush = new SolidColorBrush(Color.FromRgb(255, 105, 180)); 
        public static readonly Brush TriangleBrush = new SolidColorBrush(Color.FromRgb(46, 204, 113)); 
        public const string CrossPath = "M3,3 L17,17 M17,3 L3,17";
        public const string CirclePath = "M10,0 A10,10 0 1,1 10,0.01";
        public const string SquarePath = "M2,2 L18,2 L18,18 L2,18 Z";
        public const string TrianglePath = "M10,2 L18,18 L2,18 Z";
        public const string DPadPath = "M6,0 L14,0 L14,6 L20,6 L20,14 L14,14 L14,20 L6,20 L6,14 L0,14 L0,6 L6,6 Z";
        public const string BumperPath = "M4,0 L16,0 A4,4 0 0,1 20,4 L20,8 L0,8 L0,4 A4,4 0 0,1 4,0 Z";
    }
}
