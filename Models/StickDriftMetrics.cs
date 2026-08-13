using System;
namespace DriftLift.Models
{
    public class StickDriftMetrics
    {
        public double LiveCircularityError { get; set; }
        public double AverageCircularityError { get; set; }
        public double MinCircularityError { get; set; }
        public double MaxCircularityError { get; set; }
        public double CenterOffsetX { get; set; }
        public double CenterOffsetY { get; set; }
        public double RestingNoiseVariance { get; set; }
        public int TotalSamplesCount { get; set; }
        public int JitterSpikeCount { get; set; }
    }
}
