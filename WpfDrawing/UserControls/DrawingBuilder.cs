using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace WpfDrawing
{
    public partial class DrawingFactory
    {

    }
    public class RectangleBuilder
    {
        public Point Start { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Angle { get; set; }

    }
    public class LineBuilder : IDrawingBuilder
    {
        public double Angle { get; set; }
        public double LineLength { get; set; }
        public bool IsClosed { get; set; } = true;
        public Point Start { get; set; }
        public bool Reset { get; set; } = false;
        public Point End { get; set; }

        public void Build(StreamGeometryContext ctx)
        {
            this.ContinueDrawingLine(ctx);
        }


        //public Pen Pen { get; set; }
        public Point GetEndPoint()
        {
            var angle = Angle * Math.PI / 180;

            var y = Start.Y + LineLength * Math.Sin(angle);
            var x = Start.X + LineLength * Math.Cos(angle);

            Point end_point = new Point(x, y);
            return end_point;
        }

    }

}
