using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace HevoDrawing.Abstractions
{
    public class DiscreteAxisContextData : ContextData
    {
        public static DiscreteAxisContextData Empty => new DiscreteAxisContextData(new List<IVariable>());

        public DiscreteAxisContextData(List<IVariable> data)
        {
            data = data.Distinct().ToList();
            Data = data;
        }
        public List<IVariable> Data { get; private set; }

        public override bool IsEmpty => Data.Count == 0;

        public override ContextData Copy()
        {
            return new DiscreteAxisContextData(new List<IVariable>(Data));
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
            if (!(VisualData.Items[ContextDataItem.IsInterregional] is bool isInterregional))
            {
                return Tools.BadVector;
            }
            var index = Data.IndexOf(value);
            if (index == -1)
            {
                return Tools.BadVector;
            }
            var ratio = ValueRatios[index];
            //计算相对比例
            return Vector.Multiply(End - Start, ratio);
        }
        public override IVariable GetValue(double offsetPosition)
        {
            if (!(VisualData.Items[ContextDataItem.IsInterregional] is bool isInterregional))
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
            var blockIndex = Tools.BinaryCompare(ValueRatioCoordinates, 0, ValueRatioCoordinates.Count, percent, isInterregional);
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
        /// <param name="splitValues">切割点的值</param>
        /// <param name="splitRatios">切割点比例 总和为1</param>
        /// <param name="isInterregional">是否区间</param>
        /// <param name="splitRatiosNum">切割点比例 递增最后一个1</param>
        /// <param name="valueRatios">每个值的比例 递增最后一个1 值的点</param>
        /// <param name="valueRatioCoordinate">比例区间</param>
        /// <param name="points">切割点位置</param>
        /// <returns></returns>
        public bool CalculateDrawingParams(Vector diff, Vector start, List<double> splitRatios, ref bool isInterregional,
            ref List<IVariable> splitValues, out List<double> splitRatiosNum, out List<double> valueRatios, out List<double> valueRatioCoordinate, out List<Point> points)
        {
            splitRatiosNum = new List<double>();
            points = new List<Point>();
            valueRatios = new List<double>();
            valueRatioCoordinate = new List<double>();

            var ordered_x_data = Data.OrderBy(it => it).ToList();
            var range_data = new Section() { Left = ordered_x_data.FirstOrDefault(), Right = ordered_x_data.LastOrDefault() };

            Section range_split = null;
            //在数据区间内 或者splitvalue超出区间
            if (splitValues != null)
            {
                List<IVariable> ordered_split = splitValues.OrderBy(it => it).ToList();
                range_split = new Section() { Left = ordered_split.FirstOrDefault(), Right = ordered_split.LastOrDefault() };
                //先排除边界
                if (range_split.Left.CompareTo(range_data.Left) > 0 && range_split.Right.CompareTo(range_data.Right) < 0)
                    FollowData = false;
                splitValues = ordered_split;
                Data = ordered_x_data;
            }

            /*
              1. SplitRatios 没有则平均
              2. SplitValue 没有按照data和SplitRatios推导
             */
            //！！！通过数据推导其他变量
            if (FollowData)
            {
                /*
                 * SplitRatios 无需外部传入 
                 * SplitValue 无需外部传入 
                 * 
                 * SplitValue 不为空的情况下 => 按照数据均分
                 * SplitRatios 每个数据都有分割 对应数据的比例 
                 * 对应数据的比例: 获取SplitValue对应的index 获取ratio
                 * IsDataFull 数据全 
                 * 
                 * SplitValue 为空 1.SplitRatios也为空 每个数据都有分割 2.SplitRatios不为空 推导SplitValue
                */

                var isStartWithZero = false;
                if (splitValues != null)
                {
                    /*
                     * SplitRatios 为空 排除其他数据 包含SplitValue两侧数据 总数据平均
                     * SplitRatios 不为空 区间平均
                     */
                    if (splitRatios == null || splitRatios.Count == 0)
                    {
                        var list = new List<IVariable>();
                        foreach (var item in Data)
                        {
                            if (!IsDataFull && (item.CompareTo(range_split.Left) < 0 || item.CompareTo(range_split.Right) > 0))
                            {
                                continue;
                            }
                            list.Add(item);
                        }
                        ordered_x_data = list;
                        ordered_x_data.Insert(0, range_split.Left);
                        ordered_x_data.Add(range_split.Right);
                        ordered_x_data = ordered_x_data.Distinct().ToList();
                        Data = ordered_x_data;

                        var splitIndex = new List<double>();
                        foreach (var item in splitValues)
                        {
                            splitIndex.Add(ordered_x_data.IndexOf(item));
                        }

                        var sum_ratio = 0.0;
                        var index = 0;

                        List<double> dataRatios = Tools.GetAverageRatiosWithZero(ordered_x_data.Count - 1);
                        isStartWithZero = true;
                        if (dataRatios.Sum() is double sum && (sum < 0.9999 || sum > 1))
                        {
                            dataRatios = Tools.GetAverageRatios(dataRatios, 1, isStartWithZero);
                        }

                        foreach (var item in dataRatios)
                        {
                            sum_ratio += item;
                            if (splitIndex.Contains(index))
                            {
                                splitRatiosNum.Add(sum_ratio);
                                points.Add(Tools.GetPointByRatio(diff, start, sum_ratio));
                            }
                            valueRatios.Add(sum_ratio);
                            var coord = sum_ratio + (index + 1 < dataRatios.Count ? dataRatios[index + 1] : 0) / 2;
                            valueRatioCoordinate.Add(coord);
                            index++;
                        }
                    }
                    else
                    {
                        var splitRatiosCrood = new List<double>();

                        var sum = splitRatios.Sum();
                        if (sum > 1)
                        {
                            var sum_ratio = 0.0;
                            var index = 0;
                            foreach (var item in splitRatios)
                            {
                                sum_ratio += item;
                                splitRatiosCrood.Add(sum_ratio);
                                if (sum_ratio > 1)
                                {
                                    break;
                                }
                                index++;
                            }
                            splitRatios = splitRatios.Take(index).ToList();
                            splitRatios.Add(1 - splitRatiosCrood.LastOrDefault());
                        }

                    }


                    //if (IsDataFull)
                    //{

                    //}
                    //else
                    //{
                    //    for (int i = 0; i < ordered_data.Count; i++)
                    //    {
                    //        var average = isInterregional ? 1.0 / ordered_data.Count : 1.0 / (ordered_data.Count - 1);
                    //        double ratio = isInterregional ? 1.0 * (i + 1) / (ordered_data.Count) : 1.0 * i / (ordered_data.Count - 1);
                    //        var offset = -(average) / 2;
                    //        var offset2 = (i + 1 < ordered_data.Count ? average : 0) / 2;
                    //        valueRatios.Add(isInterregional ? ratio + offset : ratio);
                    //        valueRatioCoordinate.Add(isInterregional ? ratio : ratio + offset2);
                    //    }

                    //}

                }
                else
                {
                    var splitRatiosCrood = new List<double>();
                    splitValues = new List<IVariable>();
                    if (splitRatios == null)
                    {
                        splitRatios = isInterregional ? Tools.GetAverageRatios(ordered_x_data.Count) : Tools.GetAverageRatiosWithZero(ordered_x_data.Count);
                        isStartWithZero = isInterregional ? false : true;
                    }
                    //没有Ratios传入的时候使用计算的range 计算逻辑在别处
                    //坐标轴平移，平分坐标轴
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
                        splitRatios = Tools.GetAverageRatios(splitRatios, 1, isStartWithZero);
                    }
                    if (isStartWithZero)
                    {
                        isInterregional = false;
                    }
                    var sum_ratio = 0.0;
                    var gIndex = -1;
                    foreach (var item in splitRatios)
                    {
                        sum_ratio += item;
                        gIndex++;
                        splitRatiosCrood.Add(sum_ratio);
                        splitValues.Add(Data[gIndex]);
                    }

                    var index2 = 0;
                    foreach (var item in splitRatiosCrood)
                    {
                        var offset = -(index2 < splitRatios.Count ? splitRatios[index2] : 0) / 2;
                        var offset2 = (index2 + 1 < splitRatios.Count ? splitRatios[index2 + 1] : 0) / 2;
                        valueRatios.Add(isInterregional ? item + offset : item);
                        valueRatioCoordinate.Add(isInterregional ? item : item + offset2);
                        index2++;
                    }
                    //一一对应上了
                    splitRatiosNum = splitRatiosCrood;
                    foreach (var item in splitRatiosCrood)
                    {
                        points.Add(Tools.GetPointByRatio(diff, start, item));
                    }
                }

                //if (splitValues == null)
                //{
                //    foreach (var item in dataRatios)
                //    {
                //        sum_ratio += item;
                //        gIndex++;
                //        splitRatiosCrood.Add(sum_ratio);
                //        if (isMapping)
                //        {
                //            splitValues.Add(data[gIndex]);
                //            continue;
                //        }
                //        //TODO 这个是否需要修正
                //        var index = (int)Math.Floor(data.Count * sum_ratio);
                //        if (index != 0)
                //        {
                //            index -= 1;
                //        }
                //        splitValues.Add(data[index]);
                //    }

                //}
                //else
                //{

                //}

            }
            else
            {
                /*
                 * SplitRatios 通过计算起点和终点百分比均匀分配 
                 * SplitValue 必须外部赋值 
                */

            }

            //数据和x轴能对应上的情况
            //if (true)
            //{
            //    var index2 = 0;
            //    foreach (var item in splitRatiosCrood)
            //    {
            //        var offset = -(index2 < splitRatios.Count ? splitRatios[index2] : 0) / 2;
            //        var offset2 = (index2 + 1 < splitRatios.Count ? splitRatios[index2 + 1] : 0) / 2;
            //        valueRatios.Add(isInterregional ? item + offset : item);
            //        valueRatioCoordinate.Add(isInterregional ? item : item + offset2);
            //        index2++;
            //    }
            //    //一一对应上了
            //    splitRatiosNum = splitRatiosCrood;
            //    foreach (var item in splitRatiosCrood)
            //    {
            //        points.Add(Tools.GetPointByRatio(diff, start, item));
            //    }
            //}
            //else
            //{
            //    if (FollowData)
            //    {
            //        for (int i = 0; i < data.Count; i++)
            //        {
            //            var average = isInterregional ? 1.0 / data.Count : 1.0 / (data.Count - 1);
            //            double ratio = isInterregional ? 1.0 * (i + 1) / (data.Count) : 1.0 * i / (data.Count - 1);
            //            var offset = -(average) / 2;
            //            var offset2 = (i + 1 < data.Count ? average : 0) / 2;
            //            valueRatios.Add(isInterregional ? ratio + offset : ratio);
            //            valueRatioCoordinate.Add(isInterregional ? ratio : ratio + offset2);

            //            if (splitValues.IndexOf(data[i]) >= 0)
            //            {
            //                splitRatiosNum.Add(ratio);
            //                points.Add(Tools.GetPointByRatio(diff, start, ratio));
            //            }
            //        }
            //    }
            //    //区间均分
            //    else
            //    {
            //        List<Section> sections = new List<Section>();
            //        for (int i = 0; i < splitValues.Count; i++)
            //        {
            //            if (i != 0)
            //            {
            //                sections.Add(new Section() { Left = splitValues[i], Right = SplitValues[i + 1] });
            //            }
            //        }
            //        foreach (var item in sections)
            //        {
            //            for (int i = 0; i < data.Count; i++)
            //            {
            //                if (data[i].CompareTo(item.Left) < 0 && data[i].CompareTo(item.Right) > 0)
            //                {
            //                    continue;
            //                }

            //                var average = isInterregional ? 1.0 / data.Count : 1.0 / (data.Count - 1);
            //                double ratio = isInterregional ? 1.0 * (i + 1) / (data.Count) : 1.0 * i / (data.Count - 1);
            //                var offset = -(average) / 2;
            //                var offset2 = (i + 1 < data.Count ? average : 0) / 2;
            //                valueRatios.Add(isInterregional ? ratio + offset : ratio);
            //                valueRatioCoordinate.Add(isInterregional ? ratio : ratio + offset2);

            //                if (splitValues.IndexOf(data[i]) >= 0)
            //                {
            //                    splitRatiosNum.Add(ratio);
            //                    points.Add(Tools.GetPointByRatio(diff, start, ratio));
            //                }
            //            }

            //        }
            //    }
            //}

            return true;
        }
        #region 内部计算项

        public List<double> ValueRatioCoordinates { get; private set; }
        public List<double> ValueRatios { get; private set; }
        public List<double> SortedSplitRatios { get; private set; }
        public List<Point> SortedSplitPoints { get; private set; }

        public bool FollowData { get; private set; } = true;
        #endregion

        /// <summary>
        /// <see cref="SplitValues"/>\<see cref="Ratios"/>不互斥 存在一定关联 共同作用 优先<see cref="SplitValues"/>
        /// </summary>
        public override void CalculateRequireData()
        {
            var splitValue = SplitValues;
            var isInterregional = IsInterregional;

            var splitRatios = Ratios;

            if (Data == null || Data.Count == 0)
            {
                return;
            }

            var isNotSatisfied = CalculateDrawingParams(End - Start, Start, splitRatios, ref isInterregional, ref splitValue,
                out var splitRatioNum, out var valueRatios, out var valueRatioCoordinate, out var points);
            if (!isNotSatisfied)
            {
                return;
            }

            //区间和点
            ValueRatioCoordinates = valueRatioCoordinate;
            ValueRatios = valueRatios;
            SortedSplitRatios = splitRatioNum;
            SortedSplitPoints = points;

            //暂存数据
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
            var points = SortedSplitPoints;

            if (points == null)
            {
                CalculateRequireData();
            }

            var ratios = SortedSplitRatios;
            var splitValues = VisualData.GetVisualDataItem<List<IVariable>>(ContextDataItem.SplitValues);

            if (splitValues == default)
            {
                return;
            }
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
    public class Section
    {
        public IVariable Left { get; set; }
        public IVariable Right { get; set; }
    }
}
