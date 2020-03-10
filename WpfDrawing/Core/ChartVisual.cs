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
using HevoDrawing.Abstractions;

namespace HevoDrawing
{
    public class ChartGroupContextData : ContextData
    {
        public List<ContextData> Data { get; } = new List<ContextData>();
        public List<ContextData> XData { get; } = new List<ContextData>();
        public List<ContextData> YData { get; } = new List<ContextData>();

        public ChartGroupContextData(List<ContextData> data) : this(data.Select(it => it as TwoDimensionalContextData).ToList())
        {

        }
        public ChartGroupContextData(List<TwoDimensionalContextData> data)
        {
            Data = data.Select(it => (ContextData)it).ToList();
            XData = data.Select(it => (ContextData)it.XContextData).ToList();
            YData = data.Select(it => (ContextData)it.YContextData).ToList();
        }

        public override bool IsEmpty => Data.Count == 0 || Data.Any(it => it.IsEmpty);
        public static ChartGroupContextData Empty => new ChartGroupContextData(new List<TwoDimensionalContextData>());
        public override ContextData Copy()
        {
            return new ChartGroupContextData(Data.Select(it => it.Copy()).ToList());
        }
    }
    /// <summary>
    /// 2d chart
    /// 假定x轴都是离散的 y轴都是连续的double
    /// </summary>
    /// <typeparam name="Tx"></typeparam>
    /// <typeparam name="Ty"></typeparam>
    public class ChartVisual : RectDrawingVisual
                                , IIntersectable
    {
        public event IntersectChangedHandler IntersectChanged;

        XAxisVisualGroup AxisXVisuals = new XAxisVisualGroup();
        YAxisVisualGroup AxisYVisuals = new YAxisVisualGroup();
        SeriesVisualGroup SeriesVisuals = new SeriesVisualGroup();

        private readonly ChartDataSource Data;

        internal void TriggerIntersectChanged(Dictionary<string, SeriesData> data)
        {
            IntersectChanged?.Invoke(data);
        }
        public override ContextData DefaultData => ChartGroupContextData.Empty;
        public ChartVisual()
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
            if (mainArea.Size.Height <= 0 || mainArea.Size.Width <= 0)
            {
                return;
            }


            /*
             *  目前两种形式获取数据
             *  1.从series里面推算x轴 y轴数据 大多数是这种情况
             *  2.直接送入 这个调用针对plot，数据不清理的情况，使用之前的数据
             *  TODO 这个需要调整
             *  必须CopyData
             */
            if (!VisualData.TryTransformVisualData<ChartGroupContextData>(out var inductiveData))
            {
                inductiveData = SeriesVisuals.InductiveData();
                AxisXVisuals.InductiveData(inductiveData);
                AxisYVisuals.InductiveData(inductiveData);

                //过滤数据 拷贝
                inductiveData = SeriesVisuals.FilterAndCopyData();
                VisualDataSetupTidily(inductiveData);
            }
            else
            {
                AxisXVisuals.InductiveData(inductiveData);
                AxisYVisuals.InductiveData(inductiveData);
                SeriesVisuals.InductiveData(inductiveData);

                inductiveData = SeriesVisuals.FilterAndCopyData();
            }

            AxisXVisuals.DataPush(inductiveData, inductiveData.XData);
            AxisYVisuals.DataPush(inductiveData, inductiveData.YData);
            SeriesVisuals.DataPush(inductiveData, inductiveData.Data);

            //共享数据
            if (InteractionVisuals != null)
            {
                InteractionVisuals.VisualData = VisualData;
            }

            var dc = RenderOpen();
            //画分割线
            foreach (ContinuousAxis item in Data.AxisYCollection)
            {
                if (item.ShowGridLine && item.SortedSplitPoints is List<Point> points)
                {
                    foreach (var point in points)
                    {
                        item.Freeze();
                        dc.DrawLine(item.GridLinePen, new Point(mainArea.Location.X, point.Y), new Point(mainArea.Width + mainArea.Location.X, point.Y));
                    }
                }
            }


            var xCollection = Data.AxisXCollection;
            foreach (DiscreteAxis item in xCollection)
            {
                bool isXClosed = false;
                var plotArea = item.PlotArea;

                if (item.ShowGridLine && item.SortedSplitPoints is List<Point> points)
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
