﻿using HevoDrawing.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace HevoDrawing
{
    public class CategoryAxis : DiscreteAxis
    {
        public CategoryAxis(AxisPosition position) : base(position)
        {

        }
        public override IFormatProvider FormatProvider => null;

        public override double IntervalPositioning(Section section, IVariable variable)
        {
            return double.NaN;
        }
        public override void CalculateRequireData()
        {
            List<IVariable> splitValue = null;
            var isInterregional = IsInterregional;

            List<double> splitRatios = null;

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
    }
}
