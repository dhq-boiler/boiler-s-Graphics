using System.Windows;

namespace boilersGraphics.Helpers
{
    public enum TransformType
    {
        Move,
        Resize,
        Rotate
    }

    public class GroupTransformNotification
    {
        public TransformType Type { get; set; }
        public double LeftChange { get; set; }
        public double TopChange { get; set; }
        public Point GroupLeftTop { get; set; }
        public Point GroupCenter { get; set; }
        public double OldWidth { get; set; }
        public double OldHeight { get; set; }
        public double WidthChange { get; set; }
        public double HeightChange { get; set; }
        public double RotateAngleChange { get; set; }
    }
}
