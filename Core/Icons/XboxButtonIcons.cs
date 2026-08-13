using System.Windows.Media;
namespace DriftLift.Core.Icons
{
    public static class XboxButtonIcons
    {
        public static readonly Brush ABrush = new SolidColorBrush(Color.FromRgb(46, 204, 113));  
        public static readonly Brush BBrush = new SolidColorBrush(Color.FromRgb(255, 23, 68));   
        public static readonly Brush XBrush = new SolidColorBrush(Color.FromRgb(52, 152, 219));  
        public static readonly Brush YBrush = new SolidColorBrush(Color.FromRgb(241, 196, 15));  
        public const string CircleOutlinePath = "M0,10 A10,10 0 1,1 0,9.99";
        public const string DPadPath = "M6,0 L14,0 L14,6 L20,6 L20,14 L14,14 L14,20 L6,20 L6,14 L0,14 L0,6 L6,6 Z";
        public const string BumperPath = "M4,0 L16,0 A4,4 0 0,1 20,4 L20,8 L0,8 L0,4 A4,4 0 0,1 4,0 Z";
        public const string TriggerPath = "M0,0 L20,0 L20,12 L16,16 L4,16 L0,12 Z";
    }
}
