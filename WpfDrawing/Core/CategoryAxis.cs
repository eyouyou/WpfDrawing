﻿using HevoDrawing.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace HevoDrawing
{
    public class CategoryAxis : DiscreteAxis
    {
        public CategoryAxis(AxisPosition position) : base(position)
        {

        }
        public override IFormatProvider FormatProvider => null;

        public override double IntervalPositioning(ValueSection section, IVariable variable, int step)
        {
            return double.NaN;
        }
        public override void CalculateRequireData()
        {
            if (!VisualData.TryTransformVisualData<DiscreteAxisContextData>(out var visual_data))
            {
                return;
            }

            List<IVariable> splitValue = null;
            var isInterregional = IsInterregional;

            List<double> splitRatios = null;
            FollowData = true;

            if (visual_data.Data == null || visual_data.Data.Count == 0)
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
        public override Vector GetPosition(IVariable value)
        {
            if (!(VisualData.Items[ContextDataItem.IsInterregional] is bool isInterregional)
                || !VisualData.TryTransformVisualData<DiscreteAxisContextData>(out var visual_data))
            {
                return Tools.BadVector;
            }
            var index = visual_data.Data.IndexOf(value);
            if (index < 0)
            {
                return Tools.BadVector;
            }
            var ratio = ValueRatios[index];
            //计算相对比例
            return Vector.Multiply(End - Start, ratio);
        }
    }
}
