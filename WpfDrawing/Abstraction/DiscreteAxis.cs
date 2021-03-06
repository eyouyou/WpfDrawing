﻿using System;
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
        public List<IVariable> Data { get; internal set; }

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

        public override string GetStringValue(IVariable value)
        {
            return value.ToString(ValueFormat, FormatProvider);
        }

        /// <summary>
        /// 排除区间 排除不了边界
        /// 必须区间互相独立
        /// </summary>
        public List<ValueSection> ExceptSections { get; set; }
        /// <summary>
        /// 数据显示全 忽略x轴SplitValue的区间限制
        /// </summary>
        public bool IsDataFull { get; set; } = false;

        public List<ValueSection> GetSectionsExcept(ValueSection section)
        {
            var total = new List<ValueSection>();
            if (ExceptSections != null && ExceptSections.Count > 0)
            {
                foreach (var item in ExceptSections)
                {
                    total.AddRange(section.Except(item));
                }
            }
            else
            {
                return new List<ValueSection>() { section };
            }
            return total;
        }
        public override Vector GetPosition(IVariable value)
        {
            //该处影响比较大的是数据点
            if (!VisualData.Items.ContainsKey(ContextDataItem.IsInterregional) || !(VisualData.Items[ContextDataItem.IsInterregional] is bool isInterregional)
                || !(VisualData is DiscreteAxisContextData data))
            {
                return Tools.BadVector;
            }
            var index = data.Data.BinarySearch(value);
            if (index < 0)
            {
                return Tools.BadVector;
            }
            var ratio = ValueRatios[index];
            //计算相对比例
            return Vector.Multiply(End - Start, ratio);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="offsetPosition"></param>
        /// <param name="withOutOfBoundData">超出边界按照最近的来</param>
        /// <returns></returns>
        public override IVariable GetValue(double offsetPosition, bool withOutOfBoundData = false)
        {
            //该处影响比较大的是交互层
            if (!VisualData.Items.ContainsKey(ContextDataItem.IsInterregional) || !(VisualData.Items[ContextDataItem.IsInterregional] is bool isInterregional)
                || !(VisualData is DiscreteAxisContextData data))
            {
                return default;
            }
            if (data.Data.Count == 0)
            {
                return default;
            }
            var percent = offsetPosition / (End - Start).Length;
            if (isInterregional && (percent > 1 || percent < 0))
            {
                return default;
            }
            //isInterregional 决定往那里靠 默认是isInterregional = true 
            var blockIndex = ValueRatioCoordinates.BinarySearch(new RatioSection() { Current = percent });
            //var index = (int)Math.Round();
            if (blockIndex < 0 && !withOutOfBoundData)
            {
                return default;
            }
            else if (blockIndex < 0)
            {
                blockIndex = ~blockIndex;
            }
            if (blockIndex >= data.Data.Count)
            {
                blockIndex = data.Data.Count - 1;
            }
            return data.Data[blockIndex];
        }

        /// <summary>
        /// 最小单元格
        /// 按照各自轴的基本单位 
        /// 推算一根data的区间
        /// </summary>
        public double MinUnit { get; set; } = 1;

        /// <summary>
        /// 区间定位
        /// 推测相对位置
        /// 通过 <see cref="MinUnit"/>
        /// </summary>
        /// <param name="section"></param>
        /// <param name="variable">当前值</param>
        /// <param name="step">位移</param>
        /// <returns></returns>
        public abstract double IntervalPositioning(ValueSection section, IVariable variable, int step);

        /// <summary>
        /// TODO 需要优化
        /// TODO 还有部分followdata未实现
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
        protected bool CalculateDrawingParams(Vector diff, Vector start, List<double> splitRatios, ref bool isInterregional,
            ref List<IVariable> splitValues, out List<double> splitRatiosNum, out List<double> valueRatios, out List<RatioSection> valueRatioCoordinate, out List<Point> points)
        {
            //必须计算出来
            splitRatiosNum = new List<double>();
            points = new List<Point>();
            valueRatios = new List<double>();
            valueRatioCoordinate = new List<RatioSection>();

            if (!VisualData.TryTransformVisualData<DiscreteAxisContextData>(out var visual_data))
            {
                return false;
            }

            var followData = visual_data.Data.Count > 0 ? FollowData : false;
            var ordered_x_data = visual_data.Data.OrderBy(it => it).ToList();

            if (splitValues != null && splitValues.Count > 0)
            {
                if (splitValues.Count < 2)
                {
                    throw new ArgumentException($"{nameof(splitValues)} 必须大于等于2");
                }

                splitValues = splitValues.OrderBy(it => it).ToList();
                var list = new List<IVariable>();

                //重置followData
                followData = visual_data.Data.Count > 0 ? FollowData : false;

                var range_split = new ValueSection() { Left = splitValues.First(), Right = splitValues.Last() };
                var range_data = new ValueSection() { };
                if (ordered_x_data.Count == 0)
                {
                    range_data.Left = range_split.Left;
                    range_data.Right = range_split.Right;
                }
                else
                {
                    range_data.Left = ordered_x_data.First();
                    range_data.Right = ordered_x_data.Last();
                }
                if (range_split.LeftBiggerThan(range_data.Right) || range_split.RightLessThan(range_data.Left))
                {
                    //throw new ArgumentException($"数据在{nameof(splitValues)}以外");
                    return false;
                }
                if (range_split.LeftLessThan(range_data.Left) || range_split.RightBiggerThan(range_data.Right))
                {
                    followData = false;
                }

                //插入SplitValue数据到Data
                var splitIndex = new List<double>();
                foreach (var item in splitValues)
                {
                    var value_index = ordered_x_data.IndexOf(item);
                    if (value_index == -1)
                    {
                        for (int i = 0; i < ordered_x_data.Count; i++)
                        {
                            var data = ordered_x_data[i];
                            if (data.IsBiggerThan(item))
                            {
                                value_index = i;
                                ordered_x_data.Insert(value_index, item);
                                break;
                            }
                        }
                    }
                    splitIndex.Add(value_index);
                }

                /*
                 * 根据数据计算 接受外部传入
                 * splitRatiosNum
                 * valueRatios
                 * valueRatioCoordinate
                 */
                if (followData)
                {
                    var dataratio_index = 0;
                    var sections = Tools.GetSectionsFromData(isInterregional, ordered_x_data);
                    foreach (var item in sections)
                    {
                        if (splitIndex.Contains(dataratio_index))
                        {
                            splitRatiosNum.Add(item.Current);
                            points.Add(Tools.GetPointByRatio(diff, start, item.Current));
                        }
                        valueRatios.Add(item.Current);
                        valueRatioCoordinate.Add(item);
                        dataratio_index++;
                    }
                }
                else
                {
                    /*
                     * SplitRatios 按照SplitValue计算
                     * SplitRatios 不为空 
                     */

                    if (splitRatios == null || splitRatios.Count == 0)
                    {
                        var valueRatioCroods2 = new List<double>();
                        foreach (var item in ordered_x_data)
                        {
                            //range的相对比例
                            var rangeValueRatio = IntervalPositioning(range_split, item, 0);
                            valueRatioCroods2.Add(rangeValueRatio);
                        }

                        var sections = Tools.GetSectionsFromRatioCroodAndSection(range_split, this, isInterregional, valueRatioCroods2, ordered_x_data);
                        var dataratio_index = 0;
                        foreach (var item in sections)
                        {
                            if (splitIndex.Contains(dataratio_index))
                            {
                                splitRatiosNum.Add(item.Current);
                                points.Add(Tools.GetPointByRatio(diff, start, item.Current));
                            }
                            valueRatios.Add(item.Current);
                            valueRatioCoordinate.Add(item);
                            dataratio_index++;
                        }
                    }
                    else
                    {
                        /*
                         * splitRatios和splitValues共同作用
                         * 所有数据点根据比例缩小
                         */
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

                        sum = splitRatios.Sum();

                        var startWithZero = splitRatios.IndexOf(0) == 0;
                        splitRatios.RemoveAll(it => it == 0);

                        /**
                         * 已有ratio的splitValues 按比例计算
                         * 没有的按照section比例计算
                         * IntervalPositioning计算相对splitRatios
                         * 
                         * 如果Ratio大 按照比例计算
                         */

                        if ((startWithZero && splitValues.Count >= splitRatios.Count + 1)
                            || (!startWithZero && splitValues.Count >= splitRatios.Count))
                        {
                            var splitRatio2 = new List<double>();
                            if (startWithZero)
                            {
                                splitRatio2.Add(0);
                                splitRatiosNum.Add(0);
                            }

                            var sum_ratio = 0.0;
                            foreach (var item in splitRatios)
                            {
                                sum_ratio += item;
                                splitRatiosNum.Add(sum_ratio);
                                splitRatio2.Add(item);
                            }

                            var current_sum = splitRatio2.Sum();

                            var other_index = splitRatio2.Count - 1;
                            var section = new ValueSection() { Left = splitValues[other_index], Right = splitValues.LastOrDefault() };
                            var index = 0;
                            var current_sum_split_ratios = splitRatiosNum.LastOrDefault();
                            //剩余split的权重
                            var other_weight = 1 - current_sum_split_ratios;
                            foreach (var item in splitValues.Skip(other_index + 1))
                            {
                                var offset_ratio = IntervalPositioning(section, item, 0) * other_weight;
                                current_sum_split_ratios += offset_ratio;
                                splitRatiosNum.Add(current_sum_split_ratios);
                                splitRatio2.Add(offset_ratio + (index == 0 ? 0 : -splitRatio2.Sum()));
                                index++;
                            }
                            splitRatios = splitRatio2;
                        }
                        else
                        {
                            splitRatios = splitRatios.Take(splitValues.Count - 1).ToList();
                            splitRatios = Tools.GetAverageRatios(splitRatios, 1);
                            splitRatiosNum.Add(0);
                            var sum_ratio = 0.0;
                            foreach (var item in splitRatios)
                            {
                                sum_ratio += item;
                                splitRatiosNum.Add(sum_ratio);
                            }
                        }

                        foreach (var item in splitRatiosNum)
                        {
                            points.Add(Tools.GetPointByRatio(diff, start, item));
                        }
                        var sections = Tools.ChangeToSections(splitValues, splitRatios);

                        if (sections.Count == 0)
                        {
                            return false;
                        }

                        var data_sections = Tools.GetSectionsFromData(isInterregional, this, sections, visual_data.Data);

                        var dataratio_index = 0;
                        foreach (var item in data_sections)
                        {
                            valueRatios.Add(item.Current);
                            valueRatioCoordinate.Add(item);
                            dataratio_index++;
                        }

                    }
                }

            }
            else
            {
                if (followData)
                {
                    var splitRatiosCrood = new List<double>();
                    splitValues = new List<IVariable>();
                    var isStartWithZero = false;
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
                        splitRatios = Tools.GetAverageRatios(splitRatios, 1);
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
                        splitValues.Add(visual_data.Data[gIndex]);
                    }


                    var sections = Tools.GetSectionsFromRatioCrood(isInterregional, splitRatiosCrood, ordered_x_data);
                    var dataratio_index = 0;
                    foreach (var item in sections)
                    {
                        valueRatios.Add(item.Current);
                        valueRatioCoordinate.Add(item);
                        dataratio_index++;
                    }

                    //一一对应上了
                    splitRatiosNum = splitRatiosCrood;
                    foreach (var item in splitRatiosCrood)
                    {
                        points.Add(Tools.GetPointByRatio(diff, start, item));
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
        class CroodLocal
        {
            public IVariable Data { get; set; }
            public double ValueCrood { get; set; }
            public double ValueIntervalCrood { get; set; }
        }

        #region 内部计算项

        public List<RatioSection> ValueRatioCoordinates { get; protected set; }
        public List<double> ValueRatios { get; protected set; }
        public List<double> SortedSplitRatios { get; protected set; }
        public List<Point> SortedSplitPoints { get; protected set; }
        #endregion

        /// <summary>
        /// x轴 跟数据比例走 根据比例来计算 支持外部传入
        /// 数据相对于x轴Splitvalue不全的时候 为false 这个时候必须赋值<see cref="SplitValues"/>，<see cref="Ratios"/>无效
        /// </summary>
        public bool FollowData { get; set; } = true;

        /// <summary>
        /// <see cref="SplitValues"/>\<see cref="Ratios"/>不互斥 存在一定关联 共同作用 优先<see cref="SplitValues"/>
        /// </summary>
        public override void CalculateRequireData()
        {
            if (!VisualData.TryTransformVisualData<DiscreteAxisContextData>(out var visual_data))
            {
                return;
            }

            var splitValue = new List<IVariable>();
            if (SplitValues != null)
            {
                splitValue = new List<IVariable>(SplitValues);
            }
            var isInterregional = IsInterregional;

            List<double> splitRatios = new List<double>();
            if (Ratios != null)
            {
                splitRatios = new List<double>(Ratios);
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
            var plotArea = PlotArea;

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
                var position = GetPosition(splitValues[index].ValueData(Name) as IVariable);
                var offset = position.X;
                //如果Bad 画就完事
                if (position.IsBad())
                {
                    offset = item.X - Start.X;
                }
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
    public class ValueSection
    {
        public IVariable Left { get; set; }
        public IVariable Right { get; set; }

        public bool LeftLessThan(IVariable variable)
        {
            return Left.CompareTo(variable) < 0;
        }
        public bool LeftLessThanOrEquals(IVariable variable)
        {
            return Left.CompareTo(variable) <= 0;
        }
        public bool LeftBiggerThan(IVariable variable)
        {
            return Left.CompareTo(variable) > 0;
        }
        public bool LeftBiggerThanOrEquals(IVariable variable)
        {
            return Left.CompareTo(variable) >= 0;
        }

        public bool RightBiggerThan(IVariable variable)
        {
            return Right.CompareTo(variable) > 0;
        }
        public bool RightBiggerThanOrEquals(IVariable variable)
        {
            return Right.CompareTo(variable) >= 0;
        }
        public bool RightLessThan(IVariable variable)
        {
            return Right.CompareTo(variable) < 0;
        }
        public bool RightLessThanOrEquals(IVariable variable)
        {
            return Right.CompareTo(variable) <= 0;
        }

        public override int GetHashCode()
        {
            return Left.GetHashCode() << 32 ^ Right.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj is ValueSection section)
            {
                return section.Left.Equals(Left) && section.Right.Equals(Right);
            }
            return false;
        }
        public double SectionRatio { get; set; }
        public bool Contains(IVariable variable)
        {
            return variable.CompareTo(Left) >= 0 && variable.CompareTo(Right) <= 0;
        }
        public List<ValueSection> Merge(ValueSection section)
        {
            var sections = new List<ValueSection>();
            //完全分离
            if ((section.Right.CompareTo(Left) < 0 && section.Left.CompareTo(Left) < 0) ||
                (section.Right.CompareTo(Right) > 0 && section.Left.CompareTo(Right) > 0))
            {
                sections.Add(section);
                sections.Add(this);
                return sections;
            }
            var new_section = new ValueSection();
            sections.Add(new_section);

            if (section.Contains(Left) && section.Contains(Right))
            {
                new_section.Left = section.Left;
                new_section.Right = section.Right;
            }
            else if (Contains(section.Left) && Contains(section.Right))
            {
                new_section.Left = Left;
                new_section.Right = Right;
            }
            else if (section.Contains(Left) && section.RightLessThanOrEquals(Right))
            {
                new_section.Left = section.Left;
                new_section.Right = Right;
            }
            else if (section.Contains(Right) && section.LeftBiggerThanOrEquals(Left))
            {
                new_section.Left = section.Left;
                new_section.Right = section.Right;
            }
            return sections;
        }

        public List<ValueSection> Except(ValueSection section)
        {
            var sections = new List<ValueSection>();

            //完全分离
            if ((section.Right.CompareTo(Left) < 0 && section.Left.CompareTo(Left) < 0) ||
                (section.Right.CompareTo(Right) > 0 && section.Left.CompareTo(Right) > 0))
            {
                sections.Add(this);
                return sections;
            }

            var new_section = new ValueSection();

            if (section.Contains(Left) && section.Contains(Right))
            {
            }
            else if (Contains(section.Left) && Contains(section.Right))
            {
                sections.Add(new ValueSection() { Left = Left, Right = section.Left });
                sections.Add(new ValueSection() { Left = section.Right, Right = Right });
                return sections;
            }
            else if (section.Contains(Left) && section.RightLessThanOrEquals(Right))
            {
                new_section.Left = section.Right;
                new_section.Right = Right;
            }
            else if (section.Contains(Right) && section.LeftBiggerThanOrEquals(Left))
            {
                new_section.Left = Left;
                new_section.Right = section.Left;
            }
            sections.Add(new_section);

            return sections;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sections">各自独立section</param>
        /// <returns></returns>
        public List<ValueSection> MergeFrom(List<ValueSection> sections)
        {
            var list = new List<ValueSection>();
            foreach (var item in sections)
            {
                var merged = item.Merge(this);
                merged.RemoveAll(it => it.Equals(this));
                list.AddRange(merged);
            }
            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sections">各自独立section</param>
        /// <returns></returns>
        public List<ValueSection> ExceptFrom(List<ValueSection> sections)
        {
            var list = new List<ValueSection>();
            foreach (var item in sections)
            {
                var excepted = item.Except(this);
                list.AddRange(excepted);
            }
            return list;
        }
        public List<ValueSection> Intersect(ValueSection section)
        {
            return new List<ValueSection>();
        }
    }
}
