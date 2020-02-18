using HevoDrawing.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace HevoDrawing
{
    public class ContinuousAxisContextData : ContextData
    {
        public Range Range { get; set; }

        public override bool IsEmpty => Range.Min.Data == 0 && double.IsNaN(Range.Max.Data);

        public static ContinuousAxisContextData Empty =>
            new ContinuousAxisContextData(new Range() { Max = new Value<double>(double.NaN), Min = new Value<double>(0) });


        public ContinuousAxisContextData(Range range)
        {
            Range = range;
        }
        public ContinuousAxisContextData(List<double> values)
        {
            Range = new Range() { Max = new Value<double>(values.Max()), Min = new Value<double>(values.Min()) };
        }
        public ContinuousAxisContextData(List<Value<double>> values)
        {
            Range = new Range() { Max = values.Max(), Min = values.Min() };
        }
        public override ContextData Copy()
        {
            return new ContinuousAxisContextData(Range);
        }
    }

    /// <summary>
    /// 可能存在特殊需求对Value<>进行统一修改
    /// </summary>
    public class Range
    {
        public Value<double> Min { get; set; } = Value<double>.Bad;
        public Value<double> Max { get; set; } = Value<double>.Bad;
        public double Sum => Max.Data - Min.Data;

        public bool IsEmpty => Min.IsBad || Max.IsBad;
    }
    /// <summary>
    /// 连续 是否需要再进行抽象？
    /// 若需要复用配置，修改plot
    /// </summary>
    public class ContinuousAxis : AxisVisual<IVariable>
    {
        public Range Range { get; set; }
        public double Base { get; set; }
        public int SplitNum { get; set; }

        public override ContextData DefaultData => ContinuousAxisContextData.Empty;

        public override IFormatProvider FormatProvider => null;

        public ContinuousAxis(AxisPosition position)
        {
            //TODO 是否没用
            Position = position;
            Base = 0;
            SplitNum = 5;
        }

        public override void PlotToDc(DrawingContext dc)
        {
            var vData = VisualData.TransformVisualData<ContinuousAxisContextData>();
            if (vData.IsBad)
            {
                return;
            }

            Freeze();

            dc.DrawLine(AxisPen, new Point(Start.X, Start.Y), new Point(Start.X, End.Y));

            var points = SortedSplitPoints;
            if (points == null)
            {
                CalculateRequireData();
            }
            var point = new Point();
            foreach (var item in points)
            {
                var height = item.Y;
                var value = GetValue(Math.Abs(height - Start.Y));

                FormattedText formatted_text = new FormattedText(
                    $"{value.ToString(SplitValueFormat, null)}",
                    CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Microsoft YaHei"),
                    ChartFontSize,
                    Brushes.Black);

                var margin = ShowValueMargin.GetActualLength(PlotArea.Width);
                var end = 0.0;
                switch (Position)
                {
                    case AxisPosition.Left:
                        {
                            point.X = Start.X - formatted_text.Width - margin;
                            point.Y = height - formatted_text.Height / 2;
                            end = Start.X - margin;
                        }
                        break;
                    case AxisPosition.Buttom:
                        break;
                    case AxisPosition.Right:
                        {
                            point.X = Start.X + margin;
                            point.Y = height - formatted_text.Height / 2;
                            end = Start.X + margin;
                        }
                        break;
                    case AxisPosition.Top:
                        break;
                    default:
                        break;
                }

                dc.DrawText(formatted_text, point);

                //画小尖尖
                dc.DrawLine(AxisPen, new Point(Start.X, height), new Point(end, height));
            }
        }

        public override IVariable GetValue(double offsetPosition)
        {
            if (!(VisualData is ContinuousAxisContextData context))
            {
                return default;
            }

            var length = (End - Start).Length;
            return new Value<double>(offsetPosition / length * context.Range.Sum + context.Range.Min.Data);
        }

        public override string GetStringValue(IVariable value)
        {
            return value.ToString(ValueFormat, null);
        }

        public override Vector GetPosition(IVariable value)
        {
            if (!(VisualData is ContinuousAxisContextData context))
            {
                return new Vector();
            }
            if (!(value.ValueData("") is Value<double> @double && !@double.IsBad))
            {
                return new Vector();
            }
            return Vector.Multiply(End - Start, (@double.Data - context.Range.Min.Data) / context.Range.Sum);
        }

        public List<Point> SortedSplitPoints { get; private set; }
        public override void CalculateRequireData()
        {
            if (!(VisualData is ContinuousAxisContextData context))
            {
                return;
            }

            var isNotSatisfied = CalculateDrawingParams(End - Start, Start, context.Range, SplitValues, Ratios,
                out var splitRatioNum, out var coordinateRatios, out var ratios, out var points);

            if (!isNotSatisfied)
            {
                return;
            }

            SortedSplitPoints = points;

            VisualData.Items.Add(ContextDataItem.Ratios, ratios);
        }


        /// <summary>
        /// todo 优化 
        /// </summary>
        /// <param name="diff"></param>
        /// <param name="start"></param>
        /// <param name="range"></param>
        /// <param name="splitValues"></param>
        /// <param name="splitRatios"></param>
        /// <param name="splitRatiosNum"></param>
        /// <param name="coordinateRatios"></param>
        /// <param name="ratios"></param>
        /// <param name="points"></param>
        /// <returns></returns>
        public bool CalculateDrawingParams(Vector diff, Vector start, Range range, List<IVariable> splitValues, List<double> splitRatios,
            out List<double> splitRatiosNum, out List<double> coordinateRatios, out List<double> ratios, out List<Point> points)
        {
            ratios = new List<double>();
            coordinateRatios = new List<double>();
            points = new List<Point>();
            splitRatiosNum = new List<double>();

            if (splitRatios == null)
            {
                splitRatios = Tools.GetAverageRatios(SplitNum);
                splitRatios.Insert(0, 0.0);
            }

            var sum_ratio = 0.0;
            for (int i = 0; i < splitRatios.Count; i++)
            {
                var item = splitRatios[i];
                //去除中间无用的点和比例
                if (i != 0 && item == 0.0)
                {
                    continue;
                }
                sum_ratio += item;

                if (sum_ratio >= 0.999999)
                {
                    sum_ratio = 1;
                }
                //区间和点
                coordinateRatios.Add(sum_ratio + (i + 1 < ratios.Count ? ratios[i + 1] : 0) / 2);
                ratios.Add(sum_ratio);
                points.Add(Tools.GetPointByRatio(diff, start, sum_ratio));
            }
            points = points.ToList();
            return true;
        }

    }
}
