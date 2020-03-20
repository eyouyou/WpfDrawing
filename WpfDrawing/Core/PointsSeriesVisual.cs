using HevoDrawing.Abstractions;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

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
        public Func<TwoDimensionalContextData, Range> RangeCalculator { get; set; }

        public override Func<IVariable, Value<double>, Brush> Color { get; set; }

        /// <summary>
        /// 独立获取range
        /// </summary>
        /// <returns></returns>
        public Range GetRange()
        {
            if (!(VisualData is TwoDimensionalContextData data))
            {
                return default;
            }
            if (RangeCalculator != null)
            {
                return RangeCalculator(data);
            }
            return data.YContextData.Range;
        }
        public override ContextData DefaultData => Chart2DContextData.Empty;

        /// <summary>
        /// TODO 所有点都需要固定
        /// </summary>
        public bool PointsShow { get; set; }
        public List<MarkLineVisual> MarkLineCollection { get; } = new List<MarkLineVisual>();

        protected void PlotMarkLineToDc(DrawingContext dc)
        {
            foreach (var item in MarkLineCollection)
            {
                item.PlotToDc(dc);
            }
        }
        internal virtual List<ChartCrood> Croods
        {
            get
            {
                var list = new List<ChartCrood>();
                if (!VisualData.TryTransformVisualData<TwoDimensionalContextData>(out var visual_data))
                {
                    return list;
                }
                var coms = Assembly as ChartAssembly;

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
                foreach (var item in visual_data.ChartCroods)
                {
                    var vector = x.GetPosition(item.X.ValueData(Name) as IVariable);
                    if (vector.IsBad())
                    {
                        continue;
                    }
                    var current = new Point(offsetx + vector.X, offsety + y.GetPosition(item.Y).Y);
                    var crood = new ChartCrood(item.X, item.Y, current);
                    list.Add(crood);
                    index++;
                }
                return list;
            }
        }
    }

}
