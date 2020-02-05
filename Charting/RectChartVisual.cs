﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WPFAnimation
{
    public class RectChartVisualCollectionData : RectVisualContextData
    {
        public List<RectVisualContextData> Data { get; } = new List<RectVisualContextData>();
        public List<RectVisualContextData> XData { get; } = new List<RectVisualContextData>();
        public List<RectVisualContextData> YData { get; } = new List<RectVisualContextData>();

        public RectChartVisualCollectionData(List<RectChartVisualData> data)
        {
            Data = data.Select(it => (RectVisualContextData)it).ToList();
            XData = data.Select(it => (RectVisualContextData)it.XData).ToList();
            YData = data.Select(it => (RectVisualContextData)it.YData).ToList();
        }
        public RectChartVisualCollectionData(List<RectVisualContextData> data) : this(data.Select(it => it as RectChartVisualData).ToList())
        {

        }
        public override bool IsEmpty => Data.Count == 0 || Data.Any(it => it.IsEmpty);
        public static RectChartVisualCollectionData Empty => new RectChartVisualCollectionData(new List<RectChartVisualData>());
        public override RectVisualContextData Copy()
        {
            return new RectChartVisualCollectionData(Data.Select(it => it.Copy()).ToList());
        }
    }

    public class RectChartVisualData : RectVisualContextData
    {
        public RectChartVisualData(double max, double min, DiscreteAxisVisualData xs)
            : this(new Value<double>(max), new Value<double>(min), xs)
        {
        }
        public RectChartVisualData(Value<double> max, Value<double> min, List<IVariable> xs)
            : this(max, min, new DiscreteAxisVisualData(xs))
        {
        }
        public RectChartVisualData(Value<double> max, Value<double> min, DiscreteAxisVisualData xs)
            : this(new Range() { Max = max, Min = min }, xs)
        {
        }

        public RectChartVisualData(Range range, DiscreteAxisVisualData xs)
            : this(new Dictionary<IVariable, Value<double>>(), new ContinuousAxisVisualData(range), xs)
        {
        }
        public RectChartVisualData(RectChartVisualData axisVisualData)
            : this(axisVisualData.Data, axisVisualData.YData, axisVisualData.XData)
        {
        }
        public RectChartVisualData(Dictionary<IVariable, Value<double>> data)
        {
            Data = data;
            XData = new DiscreteAxisVisualData(Data.Keys.ToList());
            var ydata = Data.Values.ToList();
            YData = new ContinuousAxisVisualData(ydata);
        }
        private RectChartVisualData(Dictionary<IVariable, Value<double>> data, ContinuousAxisVisualData ydata, DiscreteAxisVisualData xdata)
        {
            Data = data;
            YData = ydata;
            XData = xdata;
            XData.Items = Items;
            YData.Items = Items;
        }
        public RectChartVisualData(List<IVariable> xData, List<double> yData)
        {
            if (xData.Count != yData.Count)
            {
                throw new ArgumentException($"{nameof(xData.Count)} != {nameof(yData.Count)}");
            }
            Data = new Dictionary<IVariable, Value<double>>();
            var index = 0;
            foreach (var item in xData)
            {
                Data.Add(item, new Value<double>(yData[index]));
                index++;
            }
            XData = new DiscreteAxisVisualData(xData);
            YData = new ContinuousAxisVisualData(yData);
            XData.Items = Items;
            YData.Items = Items;
        }
        public Dictionary<IVariable, Value<double>> Data { get; set; }
        public DiscreteAxisVisualData XData { get; set; }
        public ContinuousAxisVisualData YData { get; set; }

        public override bool IsEmpty => XData.Data.Count == 0 || YData.Range.IsEmpty;

        public static RectChartVisualData Empty => new RectChartVisualData(new Range(), DiscreteAxisVisualData.Empty);

        public override RectVisualContextData Copy()
        {
            var data = new RectChartVisualData(YData.Range.Max, YData.Range.Min, XData.Data.ToList());
            data.Data = Data;
            data.Items = new Dictionary<ContextDataItem, object>(Items);
            return data;
        }
    }

    /// <summary>
    /// 2d chart
    /// 假定x轴都是离散的 y轴都是连续的double
    /// 
    /// </summary>
    /// <typeparam name="Tx"></typeparam>
    /// <typeparam name="Ty"></typeparam>
    public class RectChartVisual : RectDrawingVisual
    {
        public override RectVisualContextData DefaultData => RectChartVisualCollectionData.Empty;

        private readonly ChartVisualCollection Components;
        public RectChartVisual()
        {
            Components = new ChartVisualCollection();
            InteractionVisuals = new AxisInteractionVisual(this, Components);

            AddSubVisual(Components.AxisXVisuals);
            AddSubVisual(Components.AxisYVisuals);
            AddSubVisual(Components.SeriesVisuals);

            DataSource = Components;
        }
        public void AddAsixX(DiscreteAxis axis)
        {
            Components.AddAxisX(axis);
        }
        public void AddAsixY(ContinuousAxis axis)
        {
            Components.AddAxisY(axis);
        }
        public void AddSeries(SeriesVisual series)
        {
            Components.AddSeries(series);
        }
        public IAxisVisualConfiguare XOption => Components.AxisXVisuals;
        public IAxisVisualConfiguare YOption => Components.AxisYVisuals;
        public ICrossConfiguaration CrossOption => InteractionVisuals as ICrossConfiguaration;
        public IToolTipConfiguaration ToolTipOption => InteractionVisuals as IToolTipConfiguaration;
        public IIntersectable Intersectable => InteractionVisuals as IIntersectable;
        public override InteractionCanvas InteractionVisuals { get; }

        public override void Plot()
        {
            var mainArea = PlotArea;
            if (mainArea.Size.Height < 0 || mainArea.Size.Width < 0)
            {
                ParentCanvas.Plot();
            }

            var data = VisualData.TransformVisualData<RectChartVisualCollectionData>();
            //从seriesvisual里面取值画坐标轴
            if (data.IsBad)
            {
                data.Value = Components.SeriesVisuals.MakeData();
                Components.AxisXVisuals.MakeData(data.Value);
                Components.AxisYVisuals.MakeData(data.Value);
                VisualDataSetupTidily(data.Value);
            }

            var dc = RenderOpen();

            Components.SeriesVisuals.DataPush(data.Value, data.Value.Data);
            Components.AxisXVisuals.DataPush(data.Value, data.Value.XData);
            Components.AxisYVisuals.DataPush(data.Value, data.Value.YData);

            //共享数据
            InteractionVisuals.VisualData = VisualData;

            //画分割线
            foreach (AxisVisual item in Components.GetAxisYCollection())
            {
                if (item.ShowGridLine && item.VisualData.Items[ContextDataItem.SortedSplitPoints] is List<Point> points)
                {
                    foreach (var point in points)
                    {
                        item.Freeze();
                        dc.DrawLine(item.GridLinePen, new Point(mainArea.Location.X, point.Y), new Point(mainArea.Width + mainArea.Location.X, point.Y));
                    }
                }
            }


            var xCollection = Components.GetAxisXCollection();
            foreach (AxisVisual item in xCollection)
            {
                bool isXClosed = false;
                var plotArea = item.PlotArea;

                if (item.ShowGridLine && item.VisualData.Items[ContextDataItem.SortedSplitPoints] is List<Point> points)
                {
                    foreach (var point in points)
                    {
                        if (point.X == item.End.X)
                        {
                            isXClosed = true;
                        }
                        item.Freeze();
                        dc.DrawLine(item.GridLinePen, new Point(point.X, plotArea.Location.Y), new Point(point.X, plotArea.Location.Y + plotArea.Height));
                    }
                }
                if (item.IsGridLineClose && !isXClosed)
                {
                    dc.DrawLine(item.GridLinePen, new Point(item.End.X, plotArea.Location.Y), new Point(item.End.X, plotArea.Location.Y + plotArea.Height));
                }
            }


            Components.AxisXVisuals.Freeze();
            Components.AxisXVisuals.PlotToDc(dc);
            Components.AxisYVisuals.Freeze();
            Components.AxisYVisuals.PlotToDc(dc);

            Components.SeriesVisuals.PlotToDc(dc);

            dc.Close();

        }
    }
}
