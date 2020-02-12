using System;
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
using WpfDrawing.Abstraction;

namespace WpfDrawing
{
    public class RectChartGroupContextData : RectVisualContextData
    {
        public List<RectVisualContextData> Data { get; } = new List<RectVisualContextData>();
        public List<RectVisualContextData> XData { get; } = new List<RectVisualContextData>();
        public List<RectVisualContextData> YData { get; } = new List<RectVisualContextData>();

        public RectChartGroupContextData(List<RectChartContextData> data)
        {
            Data = data.Select(it => (RectVisualContextData)it).ToList();
            XData = data.Select(it => (RectVisualContextData)it.XData).ToList();
            YData = data.Select(it => (RectVisualContextData)it.YData).ToList();
        }
        public RectChartGroupContextData(List<RectVisualContextData> data) : this(data.Select(it => it as RectChartContextData).ToList())
        {

        }
        public override bool IsEmpty => Data.Count == 0 || Data.Any(it => it.IsEmpty);
        public static RectChartGroupContextData Empty => new RectChartGroupContextData(new List<RectChartContextData>());
        public override RectVisualContextData Copy()
        {
            return new RectChartGroupContextData(Data.Select(it => it.Copy()).ToList());
        }
    }

    public class RectChartContextData : RectVisualContextData
    {
        public RectChartContextData(double max, double min, DiscreteAxisContextData xs)
            : this(new Value<double>(max), new Value<double>(min), xs)
        {
        }
        public RectChartContextData(Value<double> max, Value<double> min, List<IVariable> xs)
            : this(max, min, new DiscreteAxisContextData(xs))
        {
        }
        public RectChartContextData(Value<double> max, Value<double> min, DiscreteAxisContextData xs)
            : this(new Range() { Max = max, Min = min }, xs)
        {
        }

        public RectChartContextData(Range range, DiscreteAxisContextData xs)
            : this(new Dictionary<IVariable, Value<double>>(), new ContinuousAxisContextData(range), xs)
        {
        }
        public RectChartContextData(RectChartContextData axisVisualData)
            : this(axisVisualData.Data, axisVisualData.YData, axisVisualData.XData)
        {
        }
        public RectChartContextData(Dictionary<IVariable, Value<double>> data)
        {
            Data = data;
            XData = new DiscreteAxisContextData(Data.Keys.ToList());
            var ydata = Data.Values.ToList();
            YData = new ContinuousAxisContextData(ydata);
        }
        private RectChartContextData(Dictionary<IVariable, Value<double>> data, ContinuousAxisContextData ydata, DiscreteAxisContextData xdata)
        {
            Data = data;
            YData = ydata;
            XData = xdata;
            XData.Items = Items;
            YData.Items = Items;
        }
        public RectChartContextData(List<IVariable> xData, List<double> yData)
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
            XData = new DiscreteAxisContextData(xData);
            YData = new ContinuousAxisContextData(yData);
            XData.Items = Items;
            YData.Items = Items;
        }
        public Dictionary<IVariable, Value<double>> Data { get; set; }
        public DiscreteAxisContextData XData { get; set; }
        public ContinuousAxisContextData YData { get; set; }

        public override bool IsEmpty => XData.Data.Count == 0 || YData.Range.IsEmpty;

        public static RectChartContextData Empty => new RectChartContextData(new Range(), DiscreteAxisContextData.Empty);

        public override RectVisualContextData Copy()
        {
            var data = new RectChartContextData(YData.Range.Max, YData.Range.Min, XData.Data.ToList());
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
    public class Chart : RectDrawingVisual
    {
        XAxisVisualGroup AxisXVisuals = new XAxisVisualGroup();
        YAxisVisualGroup AxisYVisuals = new YAxisVisualGroup();
        SeriesVisualGroup SeriesVisuals = new SeriesVisualGroup();

        private readonly ChartDataSource Data;

        public override RectVisualContextData DefaultData => RectChartGroupContextData.Empty;
        public Chart()
        {
            Data = new ChartDataSource(this);

            AddSubVisual(AxisXVisuals);
            AddSubVisual(AxisYVisuals);
            AddSubVisual(SeriesVisuals);

            DataSource = Data;
        }
        public void AddAsixX(DiscreteAxis axis)
        {
            AxisXVisuals.Add(axis);
            Data.AddAxisX(axis);
        }
        public void AddAsixY(ContinuousAxis axis)
        {
            AxisYVisuals.Add(axis);
            Data.AddAxisY(axis);
        }
        public void AddSeries(SeriesVisual series)
        {
            SeriesVisuals.Add(series);
            Data.AddSeries(series);
        }
        public IAxisVisualConfiguare XOption => AxisXVisuals;
        public IAxisVisualConfiguare YOption => AxisYVisuals;

        /// <summary>
        /// 赋值给Canvas.DataSource
        /// </summary>
        public override RectDrawingVisualDataSource DataSource
        {
            get => base.DataSource;
            internal set
            {
                AxisXVisuals.DataSource = value;
                AxisYVisuals.DataSource = value;
                SeriesVisuals.DataSource = value;

                base.DataSource = value;
            }
        }

        public override void Plot()
        {
            var mainArea = PlotArea;
            if (mainArea.Size.Height < 0 || mainArea.Size.Width < 0)
            {
                ParentCanvas.Plot();
                return;
            }

            var data = VisualData.TransformVisualData<RectChartGroupContextData>();
            //从seriesvisual里面取值画坐标轴
            if (data.IsBad)
            {
                var dataMade = SeriesVisuals.InductiveData();
                if (dataMade.IsEmpty)
                {
                    return;
                }
                data.Value = dataMade;
                AxisXVisuals.InductiveData(dataMade);
                AxisYVisuals.InductiveData(dataMade);
                VisualDataSetupTidily(dataMade);
            }

            SeriesVisuals.DataPush(data.Value, data.Value.Data);
            AxisXVisuals.DataPush(data.Value, data.Value.XData);
            AxisYVisuals.DataPush(data.Value, data.Value.YData);

            //共享数据
            if (InteractionVisuals != null)
            {
                InteractionVisuals.VisualData = VisualData;
            }

            var dc = RenderOpen();
            //画分割线
            foreach (AxisVisual item in Data.AxisYCollection)
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


            var xCollection = Data.AxisXCollection;
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


            AxisXVisuals.Freeze();
            AxisXVisuals.PlotToDc(dc);
            AxisYVisuals.Freeze();
            AxisYVisuals.PlotToDc(dc);

            SeriesVisuals.PlotToDc(dc);

            dc.Close();

        }
    }
}
