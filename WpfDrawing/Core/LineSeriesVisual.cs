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
        public static bool IsInterectHoverable(this SeriesVisual series)
        {
            return series is LineSeriesVisual line && line.HoverElement.Content != null;
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


        public override List<Point> Points
        {
            get
            {
                var list = new List<Point>();
                var vData = VisualData.TransformVisualData<RectChartContextData>();
                if (vData.IsBad)
                {
                    return list;
                }
                var coms = DataSource as ChartDataSource;

                var axisxs = coms.AxisXCollection;

                Point lasted = new Point(double.MinValue, double.MinValue);

                var y = coms.GetMappingAxisY(Id) as ContinuousAxis;
                if (y == null)
                {
                    return list;
                }
                var x = coms.FindXById(Id) as DiscreteAxis;
                if (x == null)
                {
                    return list;
                }

                var offsetx = x.Start.X;
                var offsety = x.Start.Y;
                var index = 0;
                foreach (var item in vData.Value.Data)
                {
                    var current = new Point(offsetx + x.GetPosition(item.Key.ValueData(Name) as IVariable).X, offsety + y.GetPosition(item.Value).Y);
                    list.Add(current);
                    index++;
                }
                return list;
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
            if (!(VisualData is RectChartContextData data))
            {
                return;
            }
            var coms = DataSource as ChartDataSource;

            var axisxs = coms.AxisXCollection;

            Point lasted = new Point(double.MinValue, double.MinValue);
            var plotArea = PlotArea;

            var y = coms.GetMappingAxisY(Id) as ContinuousAxis;
            if (y == null)
            {
                return;
            }
            var x = coms.FindXById(Id) as DiscreteAxis;
            if (x == null)
            {
                return;
            }

            var offsetx = x.Start.X;
            var offsety = x.Start.Y;
            var index = 0;
            var points = new List<Point>();

            var stream = new StreamGeometry();
            using (var sgc = stream.Open())
            {
                foreach (var item in data.Data)
                {
                    var current = new Point(offsetx + x.GetPosition(item.Key.ValueData(Name) as IVariable).X, offsety + y.GetPosition(item.Value).Y);

                    if (index >= 1)
                    {
                        var center = GetCenterPoint(lasted, current);
                        sgc.BeginFigure(lasted, false, false);
                        sgc.BezierTo(new Point(center.X, lasted.Y), new Point(center.X, current.Y), current, true, false);
                        points.Add(current);
                    }
                    lasted = current;

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
