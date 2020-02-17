using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using WpfDrawing.Abstraction;

namespace WpfDrawing
{
    public class BarSeriesVisual : PointsSeriesVisual
    {
        public Pen Pen { get; set; } = new Pen();
        public Brush Fill { get; set; } = Brushes.Black;
        public GridLength BarWidth { get; set; } = new GridLength(1, GridUnitType.Star);
        public override Brush Color => Brushes.Black;

        public override void PlotToDc(DrawingContext dc)
        {
            var points = Points;
            if (points.Count == 0)
            {
                return;
            }
            var coms = DataSource as ChartDataSource;

            var x = coms.FindXById(Id) as DiscreteAxis;
            var plotArea = x.PlotArea;
            var startx = x.Start;
            var endx = x.End;
            var left = 0.0;
            var right = 0.0;
            var index = 0;
            foreach (var item in points)
            {
                bool isLeftBound = false;
                bool isRightBound = false;
                if (index == 0 && item.X == startx.X)
                {
                    left = 0;
                    isLeftBound = true;
                }
                else
                {
                    left = points[index - 1].X - startx.X;
                }
                if (index == points.Count - 1 && item.X == endx.X)
                {
                    right = points[index].X - startx.X;
                    isRightBound = true;
                }
                else
                {
                    right = points[index + 1].X - startx.X;
                }
                var all_width = (right - left) / 2;
                var width = BarWidth.GetActualLength(all_width);

                dc.PushClip(new RectangleGeometry() { Rect = plotArea });
                var offset = 0.0;
                if (isLeftBound)
                {
                    offset = 0;
                }
                else if (isRightBound)
                {
                    offset = width;
                }
                else
                {
                    offset = width / 2;
                }
                var leftTopX = item.X - offset;
                dc.DrawRectangle(Fill, Pen, new Rect(new Point(leftTopX, item.Y), new Size(width, Math.Abs(item.Y - startx.Y))));
                dc.Pop();
                index++;
            }
        }
    }
}
