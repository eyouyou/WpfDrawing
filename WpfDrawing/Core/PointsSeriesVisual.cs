using HevoDrawing.Abstractions;
using System;
using System.Collections.Generic;
using System.Windows;

namespace HevoDrawing
{
    /// <summary>
    /// scatter\line
    /// </summary>
    public abstract class PointsSeriesVisual : SeriesVisual
    {
        /// <summary>
        /// 对外接口 计算range
        /// </summary>
        public Func<Chart2DContextData, Range> RangeCalculator { get; set; }

        /// <summary>
        /// 独立获取range
        /// </summary>
        /// <returns></returns>
        public Range GetRange()
        {
            if (!(VisualData is Chart2DContextData data))
            {
                return default;
            }
            if (RangeCalculator != null)
            {
                return RangeCalculator(data);
            }
            return data.YData.Range;
        }
        public override ContextData DefaultData => Chart2DContextData.Empty;

        /// <summary>
        /// TODO 所有点都需要固定
        /// </summary>
        public bool IsPointsFixed { get; set; }
        List<MarkLineVisual> MarkLineCollection { get; } = new List<MarkLineVisual>();

        public virtual List<Point> Points
        {
            get
            {
                var list = new List<Point>();
                var vData = VisualData.TransformVisualData<TwoDimensionalContextData>();
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
                foreach (var item in vData.Value.ChartCroods)
                {
                    var current = new Point(offsetx + x.GetPosition(item.X.ValueData(Name) as IVariable).X, offsety + y.GetPosition(item.Y).Y);
                    list.Add(current);
                    index++;
                }
                return list;
            }
        }
    }

}
