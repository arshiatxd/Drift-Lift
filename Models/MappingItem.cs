using System;
using System.Windows.Media;
namespace DriftLift.Models
{
    public class MappingItem
    {
        public string ButtonName { get; set; } = "";
        public string MappedValueText { get; set; } = "";
        public ushort SourceBit { get; set; }
        public ushort TargetBit { get; set; }
        public string XboxText { get; set; } = "";
        public string XboxPathData { get; set; } = "";
        public Brush XboxColor { get; set; } = Brushes.White;
        public string PsText { get; set; } = "";
        public string PsPathData { get; set; } = "";
        public Brush PsColor { get; set; } = Brushes.White;
        public Geometry? XboxIconGeometry => string.IsNullOrEmpty(XboxPathData) ? null : Geometry.Parse(XboxPathData);
        public Geometry? PsIconGeometry => string.IsNullOrEmpty(PsPathData) ? null : Geometry.Parse(PsPathData);
    }
}
