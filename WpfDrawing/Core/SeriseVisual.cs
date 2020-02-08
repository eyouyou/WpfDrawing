using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfDrawing
{
    public abstract class HitElement
    {
        /// <summary>
        /// TODO 需要调整 配置remove和enable
        /// </summary>
        public bool IsAdded { get; set; } = false;
        public SeriesVisual ParentSeries { get; set; }
        public double Width { get; set; } = 10;
        public double Height { get; set; } = 10;
        public Brush Color { get; set; }
        public abstract FrameworkElement Content { get; }
        public int ZIndex { get; set; } = 0;

    }
    public class EllipseSolidHitElement : HitElement
    {
        private Ellipse _ellipse = new Ellipse() { };
        public override FrameworkElement Content
        {
            get
            {
                _ellipse.Width = Width;
                _ellipse.Height = Height;
                ZIndex = 2;
                if (Color != null)
                {
                    _ellipse.Fill = Color;
                }
                else if (ParentSeries != null)
                {
                    _ellipse.Fill = ParentSeries.Color;
                }
                return _ellipse;
            }
        }
    }
    public abstract class SeriesVisual : SubRectDrawingVisual
    {
        public override RectVisualContextData DefaultData => RectChartVisualData.Empty;
        public int XAxisId { get; set; } = 0;
        public SeriesVisual()
        {
            HitElement = new EllipseSolidHitElement();
        }
        public abstract Brush Color { get; }
        private HitElement _hitElement = null;
        public HitElement HitElement
        {
            get => _hitElement;
            set
            {
                _hitElement = value;
                _hitElement.ParentSeries = this;
            }
        }

        /// <summary>
        /// 对外接口 计算range
        /// </summary>
        public Func<RectChartVisualData, Range> RangeCalculator { get; set; }

        /// <summary>
        /// 独立获取range
        /// </summary>
        /// <returns></returns>
        public Range GetRange()
        {
            if (!(VisualData is RectChartVisualData data))
            {
                return default;
            }
            if (RangeCalculator != null)
            {
                return RangeCalculator(data);
            }
            return data.YData.Range;
        }


    }
    public class StraightLineSeriesVisual : SeriesVisual
    {
        public Pen LinePen { get; set; } = new Pen(Brushes.Blue, 1);

        public override Brush Color
        {
            get => LinePen.Brush;
        }


        public override void PlotToDc(DrawingContext dc)
        {
            VisualScrollableAreaClip = PlotArea;
            var vData = VisualData.TransformVisualData<RectChartVisualData>();
            if (vData.IsBad)
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
            var x = coms.FindById(Id) as DiscreteAxis;
            if (x == null)
            {
                return;
            }

            var offsetx = x.Start.X;
            var offsety = x.Start.Y;
            var index = 0;
            var stream = new StreamGeometry();
            //stream.Bounds.Location = plotArea.Location;
            var points = new List<Point>();
            foreach (var item in vData.Value.Data)
            {
                var current = new Point(offsetx + x.GetPosition(item.Key.ValueData(Name) as IVariable).X, offsety + y.GetPosition(item.Value).Y);
                points.Add(current);
                index++;
            }
            using (var sgc = stream.Open())
            {
                sgc.BeginFigure(points[0], false, false);
                sgc.PolyLineTo(points.Skip(0).ToList(), true, true);
            }
            dc.PushClip(new RectangleGeometry() { Rect = plotArea });
            dc.DrawGeometry(Brushes.Transparent, LinePen, stream);
            dc.Pop();
        }


    }

    public class BezierLineSeriesVisual : SeriesVisual
    {
        public Pen LinePen { get; } = new Pen(Brushes.Blue, 1);

        public override Brush Color
        {
            get => LinePen.Brush;
        }

        public override RectVisualContextData DefaultData => throw new NotImplementedException();

        public override void PlotToDc(DrawingContext dc)
        {
            if (!(VisualData is RectChartVisualData data))
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
            var x = coms.FindById(Id) as DiscreteAxis;
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
                    //if(index != 0)
                    //{
                    //    points.Add(GetCenterPoint(lasted, current));
                    //}

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
