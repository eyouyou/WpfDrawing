using HevoDrawing.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace HevoDrawing
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
        public DataPointStyle PointStyle { get; set; }
        public override ContextData DefaultData => Chart2DContextData.Empty;

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

        protected void DrawDataPoints(DrawingContext dc, List<ChartCrood> croods)
        {
            if (PointStyle != null)
            {
                switch (PointStyle)
                {
                    case EllipsePointStyle ellipse:
                        foreach (var item in croods)
                        {
                            Brush brush = Brushes.White;
                            if (ellipse.Brush == null)
                            {
                                brush = Color(item.X, item.Y);
                            }
                            dc.DrawEllipse(brush, ellipse.Pen, item.Point, ellipse.RadiusX, ellipse.RadiusY);
                        }
                        break;
                }
            }
        }
    }

    public class StraightLineSeriesVisual : LineSeriesVisual
    {
        public Pen LinePen { get; set; } = new Pen(Brushes.Blue, 1);

        public override Func<IVariable, Value<double>, Brush> Color
        {
            get
            {
                return base.Color ?? ((xdata, ydata) => LinePen.Brush);
            }
            set => base.Color = value;
        }
        public override void PlotToDc(DrawingContext dc)
        {
            var points = Croods;
            if (points.Count == 0)
            {
                return;
            }
            var stream = new StreamGeometry();
            using (var sgc = stream.Open())
            {
                sgc.BeginFigure(points[0].Point, false, false);
                sgc.PolyLineTo(points.Skip(0).Select(it => it.Point).ToList(), true, true);
            }
            var plotArea = PlotArea;
            dc.PushClip(new RectangleGeometry() { Rect = plotArea });
            dc.DrawGeometry(Brushes.Transparent, LinePen, stream);
            DrawDataPoints(dc, points);
            dc.Pop();
        }
        public override void Freeze()
        {
            if (LinePen.CanFreeze)
            {
                LinePen.Freeze();
            }
        }
    }

    public class BezierLineSeriesVisual : LineSeriesVisual
    {
        public Pen LinePen { get; } = new Pen(Brushes.Blue, 1);
        public override Func<IVariable, Value<double>, Brush> Color
        {
            get
            {
                return base.Color ?? ((xdata, ydata) => LinePen.Brush);
            }
            set => base.Color = value;
        }

        public override void PlotToDc(DrawingContext dc)
        {
            var points = Croods;
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
                        var center = GetCenterPoint(lasted, item.Point);
                        sgc.BeginFigure(lasted, false, false);
                        sgc.BezierTo(new Point(center.X, lasted.Y), new Point(center.X, item.Point.Y), item.Point, true, false);
                        points.Add(item);
                    }
                    lasted = item.Point;

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

        public override void Freeze()
        {
            if (LinePen.CanFreeze)
            {
                LinePen.Freeze();
            }
        }
    }


}
