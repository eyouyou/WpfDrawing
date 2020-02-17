using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using WpfDrawing.Abstraction;

namespace WpfDrawing
{
    public static class LineSeriesVisualExtension
    {
        /// <summary>
        ///  需要lineSeries
        /// </summary>
        /// <param name="series"></param>
        /// <returns></returns>
        public static LineSeriesVisual GetInterectHoverableLineSeriesVisual(this SeriesVisual series)
        {
            if (series is LineSeriesVisual line && line.HoverElement.Content != null)
            {
                return line;
            }
            return null;
        }
    }
    public abstract class LineSeriesVisual : PointsSeriesVisual
    {
        public override RectVisualContextData DefaultData => RectChartContextData.Empty;

        public LineSeriesVisual()
        {
            HoverElement = new EllipseSolidHitElement();
        }

        /// <summary>
        /// 优化点 减少至一个 移动至交互层
        /// </summary>
        private HitElement _hoverElement = null;
        public HitElement HoverElement
        {
            get => _hoverElement;
            set
            {
                _hoverElement = value;
                _hoverElement.ParentSeries = this;
            }
        }

    }

    public class StraightLineSeriesVisual : LineSeriesVisual
    {
        public Pen LinePen { get; set; } = new Pen(Brushes.Blue, 1);

        public override Brush Color
        {
            get => LinePen.Brush;
        }

        public override void PlotToDc(DrawingContext dc)
        {
            var points = Points;
            if (points.Count == 0)
            {
                return;
            }
            var stream = new StreamGeometry();
            using (var sgc = stream.Open())
            {
                sgc.BeginFigure(points[0], false, false);
                sgc.PolyLineTo(points.Skip(0).ToList(), true, true);
            }
            var plotArea = PlotArea;
            dc.PushClip(new RectangleGeometry() { Rect = plotArea });
            dc.DrawGeometry(Brushes.Transparent, LinePen, stream);
            dc.Pop();
        }
    }

    public class BezierLineSeriesVisual : LineSeriesVisual
    {
        public Pen LinePen { get; } = new Pen(Brushes.Blue, 1);

        public override Brush Color
        {
            get => LinePen.Brush;
        }

        public override void PlotToDc(DrawingContext dc)
        {
            var points = Points;
            if (points.Count == 0)
            {
                return;
            }
            var index = 0;

            Point lasted = new Point(0, 0);
            var stream = new StreamGeometry();
            using (var sgc = stream.Open())
            {
                foreach (var item in points)
                {
                    if (index >= 1)
                    {
                        var center = GetCenterPoint(lasted, item);
                        sgc.BeginFigure(lasted, false, false);
                        sgc.BezierTo(new Point(center.X, lasted.Y), new Point(center.X, item.Y), item, true, false);
                        points.Add(item);
                    }
                    lasted = item;

                    index++;

                }

                //sgc.PolyQuadraticBezierTo(points, true, false);
                //sgc.BezierTo(, true, false);
            }

            dc.DrawGeometry(Brushes.OrangeRed, LinePen, stream);

        }
        public Point GetCenterPoint(Point p1, Point p2)
        {
            return new Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
        }
    }


}
