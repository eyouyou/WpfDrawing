using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace HevoDrawing.Abstractions
{
    public class DiscreteAxisContextData : ContextData
    {
        public static DiscreteAxisContextData Empty => new DiscreteAxisContextData(new List<IVariable>());

        public DiscreteAxisContextData(List<IVariable> data)
        {
            Data = data;
        }
        public List<IVariable> Data { get; set; }

        public override bool IsEmpty => Data.Count == 0;

        public override ContextData Copy()
        {
            return new DiscreteAxisContextData(Data);
        }
    }

    public abstract class DiscreteAxis : AxisVisual<IVariable>
    {
        public DiscreteAxis(AxisPosition position)
        {
            Position = position;
        }
        public override ContextData DefaultData => DiscreteAxisContextData.Empty;
        /// <summary>
        /// 是否区间
        /// </summary>
        public bool IsInterregional { get; set; } = true;

        public List<IVariable> Data { get; set; }
        public override string GetStringValue(IVariable value)
        {
            return value.ToString(ValueFormat, FormatProvider);
        }
        public override Vector GetPosition(IVariable value)
        {
            if (!(VisualData.Items[ContextDataItem.IsInterregional] is bool isInterregional && VisualData.Items[ContextDataItem.ValueRatios] is List<double> valueRatios))
            {
                return new Vector();
            }
            var index = Data.IndexOf(value);
            if (index == -1)
            {
                return new Vector();
            }
            var ratio = valueRatios[index];
            //计算相对比例
            return Vector.Multiply(End - Start, ratio);
        }
        public override IVariable GetValue(double offsetPosition)
        {
            if (!(VisualData.Items[ContextDataItem.IsInterregional] is bool isInterregional && VisualData.Items[ContextDataItem.ValueRatioCoordinate] is List<double> ratiosCoordinate))
            {
                return default;
            }
            if (Data.Count == 0)
            {
                return default;
            }
            var percent = offsetPosition / (End - Start).Length;
            if (percent > 1 || percent < 0)
            {
                return default;
            }
            //isInterregional 决定往那里靠 默认是isInterregional = true 
            var blockIndex = Tools.BinaryCompare(ratiosCoordinate, 0, ratiosCoordinate.Count, percent, isInterregional);
            //var index = (int)Math.Round();
            if (blockIndex < 0)
            {
                return default;
            }
            if (blockIndex >= Data.Count)
            {
                blockIndex = Data.Count - 1;
            }
            return Data[blockIndex];
        }

        /// <summary>
        /// TODO 需要优化
        /// </summary>
        /// <param name="diff">差</param>
        /// <param name="start">起点</param>
        /// <param name="data">数据</param>
        /// <param name="splitValues">切割点的值</param>
        /// <param name="splitRatios">切割点比例 总和为1</param>
        /// <param name="isInterregional">是否区间</param>
        /// <param name="splitRatiosNum">切割点比例 递增最后一个1</param>
        /// <param name="valueRatios">每个值的比例 递增最后一个1 值的点</param>
        /// <param name="valueRatioCoordinate">比例区间</param>
        /// <param name="points">切割点位置</param>
        /// <returns></returns>
        public bool CalculateDrawingParams(Vector diff, Vector start, List<IVariable> data, List<double> splitRatios, ref bool isInterregional,
            ref List<IVariable> splitValues, out List<double> splitRatiosNum, out List<double> valueRatios, out List<double> valueRatioCoordinate, out List<Point> points)
        {
            splitRatiosNum = new List<double>();
            points = new List<Point>();
            valueRatios = new List<double>();
            valueRatioCoordinate = new List<double>();
            //ratios = isInterregional ? Tools.GetAverageRatios(data.Count) : Tools.GetAverageRatiosWithZero(data.Count);
            var isMapping = false;

            var splitRatiosPlus = new List<double>();
            //按照比例平均 
            /////startwithzero: 平分除第一个0之外的
            var isStartWithZero = false;

            var sum_ratio = 0.0;

            ////没有Ratios传入的时候使用计算的range 计算逻辑在别处
            if (splitRatios == null)
            {
                splitRatios = isInterregional ? Tools.GetAverageRatios(data.Count) : Tools.GetAverageRatiosWithZero(data.Count);
                isStartWithZero = isInterregional ? false : true;
            }
            ////坐标轴平移，平分坐标轴
            else if (splitRatios.Count >= 2 && splitRatios[0] == 0)
            {
                var temp = new List<double>(splitRatios);
                //TODO 有点奇怪
                temp.RemoveAt(1);
                temp.RemoveAll(it => it == 0.0);
                temp.Insert(0, 0.0);
                splitRatios = temp;
                isStartWithZero = true;
            }
            //TODO 这个是否一定要均分 存不存在不均分的情况
            if (splitRatios.Sum() is double sum && (sum < 0.9999 || sum > 1))
            {
                splitRatios = Tools.GetAverageRatios(splitRatios, isStartWithZero);
            }
            if (isStartWithZero)
            {
                isInterregional = false;
            }
            isMapping = splitRatios.Count >= data.Count;

            if (splitValues == null)
            {
                splitValues = new List<IVariable>();

                var gIndex = -1;
                foreach (var item in splitRatios)
                {
                    sum_ratio += item;
                    gIndex++;
                    splitRatiosPlus.Add(sum_ratio);
                    if (isMapping)
                    {
                        splitValues.Add(data[gIndex]);
                        continue;
                    }
                    //TODO 这个是否需要修正
                    var index = (int)Math.Floor(data.Count * sum_ratio);
                    if (index != 0)
                    {
                        index -= 1;
                    }
                    splitValues.Add(data[index]);
                }
            }

            isMapping = splitRatiosPlus.Count >= data.Count;

            var index2 = 0;

            //数据和x轴能对应上的情况
            if (isMapping)
            {
                foreach (var item in splitRatiosPlus)
                {
                    var offset = -(index2 < splitRatios.Count ? splitRatios[index2] : 0) / 2;
                    var offset2 = (index2 + 1 < splitRatios.Count ? splitRatios[index2 + 1] : 0) / 2;
                    valueRatios.Add(isInterregional ? item + offset : item);
                    valueRatioCoordinate.Add(isInterregional ? item : item + offset2);
                    index2++;
                }
                //一一对应上了
                splitRatiosNum = splitRatiosPlus;
                foreach (var item in splitRatiosPlus)
                {
                    points.Add(Tools.GetPointByRatio(diff, start, item));
                }
            }
            else
            {
                for (int i = 0; i < data.Count; i++)
                {
                    var average = isInterregional ? 1.0 / data.Count : 1.0 / (data.Count - 1);
                    double ratio = isInterregional ? 1.0 * (i + 1) / (data.Count) : 1.0 * i / (data.Count - 1);
                    var offset = -(average) / 2;
                    var offset2 = (i + 1 < data.Count ? average : 0) / 2;
                    valueRatios.Add(isInterregional ? ratio + offset : ratio);
                    valueRatioCoordinate.Add(isInterregional ? ratio : ratio + offset2);

                    if (splitValues.IndexOf(data[i]) >= 0)
                    {
                        splitRatiosNum.Add(ratio);
                        points.Add(Tools.GetPointByRatio(diff, start, ratio));
                    }

                }
            }

            return true;
        }

        public override void CalculateRequireData()
        {
            var splitValue = SplitValues;
            var isInterregional = IsInterregional;

            var splitRatios = Ratios;
            var isNotSatisfied = CalculateDrawingParams(End - Start, Start, Data, splitRatios, ref isInterregional, ref splitValue,
                out var splitRatioNum, out var valueRatios, out var valueRatioCoordinate, out var points);
            if (!isNotSatisfied)
            {
                return;
            }

            //区间和点
            VisualData.Items.Add(ContextDataItem.ValueRatioCoordinate, valueRatioCoordinate);
            VisualData.Items.Add(ContextDataItem.ValueRatios, valueRatios);
            VisualData.Items.Add(ContextDataItem.SortedSplitRatios, splitRatioNum);
            VisualData.Items.Add(ContextDataItem.SortedSplitPoints, points);
            VisualData.Items.Add(ContextDataItem.Ratios, splitRatios);
            VisualData.Items.Add(ContextDataItem.IsInterregional, isInterregional);
            VisualData.Items.Add(ContextDataItem.SplitValues, splitValue); 
        }
        public override void PlotToDc(DrawingContext dc)
        {
            if (!(VisualData is DiscreteAxisContextData contextData))
            {
                return;
            }

            var plotArea = PlotArea;
            Freeze();

            var endPoint = new Point(End.X, End.Y);

            dc.DrawLine(AxisPen, new Point(Start.X, Start.Y), endPoint);

            //名称设置
            //if (!string.IsNullOrEmpty(Name))
            //{
            //    var endPointCopy = new Point(endPoint.X, endPoint.Y);
            //    FormattedText name_text = new FormattedText(
            //        Name,
            //        CultureInfo.InvariantCulture,
            //        FlowDirection.LeftToRight,
            //        new Typeface("Microsoft YaHei"),
            //        ChartFontSize,
            //        Brushes.Black);

            //    switch (Position)
            //    {
            //        case AxisPosition.Left:
            //        case AxisPosition.Right:
            //            endPointCopy.X -= name_text.Width / 2;
            //            //此处假设是正方向；
            //            endPointCopy.Y += name_text.Height
            //            break;
            //        case AxisPosition.Buttom:
            //        case AxisPosition.Top:
            //            break;
            //    }
            //    dc.DrawText(name_text, endPointCopy);
            //}
            var point = new Point();
            var index = 0;
            var points = VisualData.Items[ContextDataItem.SortedSplitPoints] as List<Point>;

            if (points == null)
            {
                CalculateRequireData();
            }

            var ratios = VisualData.Items[ContextDataItem.SortedSplitRatios] as List<double>;
            var splitValues = VisualData.Items[ContextDataItem.SplitValues] as List<IVariable>;

            foreach (var item in points)
            {
                if (index >= splitValues.Count)
                {
                    continue;
                }
                var offset = GetPosition(splitValues[index].ValueData(Name) as IVariable).X;
                FormattedText formatted_text = new FormattedText(
                    $"{splitValues[index].ToString(SplitValueFormat, FormatProvider)}{SplitUnit}",
                    CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Microsoft YaHei"),
                    ChartFontSize,
                    Brushes.Black);

                var margin = ShowValueMargin.GetActualLength(plotArea.Height);
                var jjEnd = Start.Y;

                switch (Position)
                {
                    case AxisPosition.Top:
                        {
                            point.X = Start.X + offset - formatted_text.Width / 2;
                            point.Y = Start.Y - margin - formatted_text.Height;
                            jjEnd = Start.Y - margin;
                        }
                        break;
                    case AxisPosition.Buttom:
                        {
                            point.X = Start.X + offset - formatted_text.Width / 2;
                            point.Y = Start.Y + margin;
                            jjEnd = Start.Y + margin;
                        }
                        break;
                }

                dc.DrawText(formatted_text, point);
                //小尖尖
                dc.DrawLine(AxisPen, new Point(item.X, Start.Y), new Point(item.X, jjEnd));
                index++;
            }
        }

    }
}
