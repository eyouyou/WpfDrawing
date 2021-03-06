﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using HevoDrawing.Abstractions;

namespace HevoDrawing.Interactions
{
    /// <summary>
    /// 取消所有外部plot
    /// </summary>
    public class AxisInteractionCanvas : InteractionCanvas
    {
        ComponentId IdGenerator = new ComponentId();

        /// <summary>
        /// 所有超出边界的数据都自动在边界内选择最近
        /// </summary>
        public bool TryDataInBounds { get; set; } = true;
        private Point LastHitPoint;
        public AxisInteractionCanvas()
        {
            //Background = Brushes.LightCyan;
            Cross = new CrossVisual(this);
            DataToolTip = new ToolTipVisual(this);

            MouseMove += AxisInteractionVisual_MouseMove;
            MouseLeave += AxisInteractionCanvas_MouseLeave;
            IsCrossShow = true;
            IsToolTipShow = true;
        }

        private void AxisInteractionCanvas_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var point = new Point(double.NegativeInfinity, double.NegativeInfinity);
            if (!Standalone)
            {
                Plot(point, EventMessage.MouseOn);
                Cross.PlotToParent(point, EventMessage.MouseOn);
                DataToolTip.PlotToParent(point, EventMessage.MouseOn);
            }
            else
            {
                PlotStandalone(point, EventMessage.MouseOn);
                Cross.PlotToParentStandalone(point, EventMessage.MouseOn);
                DataToolTip.PlotToParentStandalone(point, EventMessage.MouseOn);
            }

        }

        private void AxisInteractionVisual_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (ParentElement == null || !ParentElement.IsMouseOver)
            {
                return;
            }
            var point = e.GetPosition(this);

            Cross.VisualData = VisualData;
            DataToolTip.VisualData = VisualData;

            if (!Standalone)
            {
                Plot(point, EventMessage.MouseOn);
                Cross.PlotToParent(point, EventMessage.MouseOn);
                DataToolTip.PlotToParent(point, EventMessage.MouseOn);
            }
            else
            {
                PlotStandalone(point, EventMessage.MouseOn);
                Cross.PlotToParentStandalone(point, EventMessage.MouseOn);
                DataToolTip.PlotToParentStandalone(point, EventMessage.MouseOn);
            }

        }

        private bool _isCrossShow = false;
        public bool IsCrossShow
        {
            get => _isCrossShow;
            set
            {
                if (value && !_isCrossShow)
                {
                    Cross.IsVisualEnable = true;
                }
                else if (!value && _isCrossShow)
                {
                    Cross.IsVisualEnable = false;
                }
                _isCrossShow = value;
            }
        }
        private bool _isToolTipShow = false;

        public bool IsToolTipShow
        {
            get => _isToolTipShow;
            set
            {
                if (value && !_isToolTipShow)
                {
                    DataToolTip.IsVisualEnable = true;
                }
                else if (!value && _isToolTipShow)
                {
                    DataToolTip.IsVisualEnable = false;
                }
                _isToolTipShow = value;
            }
        }

        /// <summary>
        /// 是否独立/公用
        /// </summary>
        public CrossVisual Cross { get; private set; }
        public ToolTipVisual DataToolTip { get; private set; }

        public override ContextData DefaultData => Chart2DContextData.Empty;

        public ToolTip Tip { get => DataToolTip.Tip; set => DataToolTip.Tip = value; }

        Dictionary<ComponentKey, ElementPosition> SeriesHitList = new Dictionary<ComponentKey, ElementPosition>();
        List<double> NearestXs = new List<double>();
        List<double> NearestYs = new List<double>();
        public override void Plot(Point point, EventMessage @event)
        {
            if (!VisualData.TryTransformVisualData<ContextData>(out var visual_data))
            {
                return;
            }
            //series上的悬浮控件 标记当前位置
            SeriesHitList.Clear();
            NearestXs.Clear();
            NearestYs.Clear();

            var hitSeriesDatas = new List<SeriesData>();

            VisualData.Items[ContextDataItem.SeriesData] = hitSeriesDatas;

            var currentPoint = point;

            var nearestX = currentPoint.X;
            var nearestY = currentPoint.Y;

            //一个hit到了其他的都不要了
            var isHint = false;
            var isHintData = false;
            foreach (var item in DataSources)
            {
                var seriesData = new List<SeriesData>();
                if (item.Value is ChartDataSource dataSource)
                {
                    var plotArea = dataSource.ConnectVisual.ParentCanvas.InteractionCanvasPlotArea;
                    var series = dataSource.SeriesCollection;
                    var offset = dataSource.ConnectVisual.ParentCanvas.Offset;
                    bool canHint = !isHint && plotArea.Contains(currentPoint) && dataSource.IsDataComplete && dataSource.ConnectVisual.ParentCanvas.IsPloted;
                    //进入当前画布
                    if (canHint)
                    {
                        foreach (SeriesVisual series_item in series)
                        {
                            if (!series_item.IsVisualEnable)
                            {
                                continue;
                            }

                            var series_plot = series_item.InteractionPlotArea;
                            var xAxis = dataSource.FindXById(series_item.XAxisId) as DiscreteAxis;
                            var axisx_offset = xAxis.OffsetPostion(currentPoint.X - offset.X);
                            var value = xAxis.GetValue(axisx_offset);
                            var key = new ComponentKey(series_item.ParentCanvas.Id, series_item.Id);

                            //HoverElement LineSeriesVisual 
                            LineSeriesVisual lineSeries = series_item.GetInterectHoverableLineSeriesVisual();

                            if (series_plot.Contains(currentPoint))
                            {
                                //验证数据是否包含等
                                if (!value.IsBad()
                                    && dataSource.GetMappingAxisY(series_item.Id) is ContinuousAxis yAxis
                                    && series_item.VisualData is TwoDimensionalContextData series_data
                                    && series_data.TryGetValue(value, out var yValue))
                                {
                                    isHintData = true;
                                    //获取当前值对应的x、y 进行十字轴的定位
                                    var x = xAxis.GetPosition(value).X + xAxis.Start.X + offset.X;
                                    var y = yAxis.GetPosition(yValue).Y + xAxis.Start.Y + offset.Y;
                                    hitSeriesDatas.Add(new SeriesData()
                                    {
                                        Color = series_item.Color(value, yValue),
                                        Id = series_item.Id,
                                        Name = series_item.Name,
                                        XValue = value,
                                        YValue = yValue,
                                        AxisX = xAxis,
                                        AxisY = yAxis
                                    });
                                    nearestX = x;

                                    if (Cross.IsYDataAttract)
                                    {
                                        nearestY = y;
                                    }

                                    if (lineSeries != null)
                                    {
                                        if (!lineSeries.HoverElement.IsAdded)
                                        {
                                            AddElement(lineSeries.HoverElement.Content);
                                            lineSeries.HoverElement.IsAdded = true;
                                        }
                                        var leftTop = new Point(x - lineSeries.HoverElement.Width / 2, y - lineSeries.HoverElement.Height / 2);

                                        if (series_plot.Contains(new Point(x, y)))
                                        {
                                            SeriesHitList.Add(key, new ElementPosition(lineSeries.HoverElement.Content, true, leftTop.X, leftTop.Y, lineSeries.HoverElement.ZIndex));
                                        }
                                        else
                                        {
                                            SeriesHitList.Add(key, new ElementPosition(lineSeries.HoverElement.Content));
                                        }
                                    }
                                }
                                else
                                {
                                    if (lineSeries != null)
                                    {
                                        SeriesHitList.Add(key, new ElementPosition(lineSeries.HoverElement.Content));
                                    }
                                }
                            }

                            else
                            {
                                if (lineSeries != null)
                                {
                                    SeriesHitList.Add(key, new ElementPosition(lineSeries.HoverElement.Content));
                                }
                            }
                            seriesData = hitSeriesDatas;
                        }
                        isHint = true;

                    }
                    else
                    {
                        foreach (SeriesVisual series_item in series)
                        {
                            var line = series_item.GetInterectHoverableLineSeriesVisual();
                            if (line != null)
                            {
                                SeriesHitList.Add(new ComponentKey(series_item.ParentCanvas.Id, series_item.Id), new ElementPosition(line.HoverElement.Content));
                            }
                        }
                    }

                    NearestXs.Add(nearestX);
                    NearestYs.Add(nearestY);

                    //优化
                    //if (canHint && LastHitPoint.X == nearestX)
                    //{
                    //    continue;
                    //}
                    //LastHitPoint = hitPoint;

                    //这么调用是否合适
                    (dataSource.ConnectVisual as ChartVisual)?.TriggerIntersectChanged(seriesData.ToDictionary(it => it.Name ?? $"{nameof(AxisInteractionCanvas)}_{IdGenerator.GenerateId().ToString()}", it => it));
                }
            }
            //记录触点坐标
            //TODO 日后需要多chart公用大十字线
            var hitPoint = Standalone ? new Point(nearestX, nearestY) : new Point(NearestXs.OrderBy(it => it - currentPoint.X).FirstOrDefault(), NearestYs.OrderBy(it => it - currentPoint.X).FirstOrDefault());
            VisualData.Items[ContextDataItem.HitPointer] = hitPoint;
            VisualData.Items[ContextDataItem.IsHintData] = isHintData;

            if (IsCrossShow)
            {
                foreach (var seriesHitItem in SeriesHitList)
                {
                    seriesHitItem.Value.Render();
                }
            }
        }
        public override void Hide()
        {
            Cross.Hide();
            DataToolTip.Hide();

            if (SeriesHitList == null)
            {
                return;
            }
            foreach (var item in SeriesHitList)
            {
                item.Value.Render(true);
            }
        }

        public override void PlotStandalone(Point point, EventMessage @event)
        {
            if (!VisualData.TryTransformVisualData<ContextData>(out var visual_data))
            {
                return;
            }
            //series上的悬浮控件 标记当前位置
            SeriesHitList = new Dictionary<ComponentKey, ElementPosition>();

            var seriesDatas = new List<SeriesData>();

            VisualData.Items[ContextDataItem.SeriesData] = seriesDatas;

            var coms = DataSources.ElementAt(0).Value as ChartDataSource;

            var currentPoint = point;

            var nearestX = currentPoint.X;
            var nearestY = currentPoint.Y;

            var plotArea = coms.ConnectVisual.PlotArea;

            var series = coms.SeriesCollection;

            var is_data_hit = false;

            if (plotArea.Contains(currentPoint) && coms.IsDataComplete && coms.ConnectVisual.ParentCanvas.IsPloted)
            {
                foreach (SeriesVisual series_item in series)
                {
                    var key = new ComponentKey(series_item.ParentCanvas.Id, series_item.Id);

                    if (!series_item.IsVisualEnable)
                    {
                        continue;
                    }
                    var series_plot = series_item.PlotArea;
                    var xAxis = coms.FindXById(series_item.XAxisId) as DiscreteAxis;
                    var value = xAxis.GetValue(xAxis.OffsetPostion(currentPoint.X), true);
                    if (value.IsBad())
                    {
                        continue;
                    }

                    var line_series_item = series_item.GetInterectHoverableLineSeriesVisual();

                    if (coms.GetMappingAxisY(series_item.Id) is ContinuousAxis yAxis
                        && series_item.VisualData is TwoDimensionalContextData series_data
                        && series_data.TryGetValue(value, out var yValue))
                    {
                        is_data_hit = true;

                        var x = xAxis.GetPosition(value).X + xAxis.Start.X;
                        var y = yAxis.GetPosition(yValue).Y + xAxis.Start.Y;
                        seriesDatas.Add(new SeriesData() { Color = series_item.Color(value, yValue), Id = series_item.Id, Name = series_item.Name, XValue = value, YValue = yValue, AxisX = xAxis, AxisY = yAxis });
                        //获取y坐标
                        nearestX = x;

                        if (line_series_item != null)
                        {
                            if (!line_series_item.HoverElement.IsAdded)
                            {
                                AddElement(line_series_item.HoverElement.Content);
                                line_series_item.HoverElement.IsAdded = true;
                            }
                            var leftTop = new Point(x - line_series_item.HoverElement.Width / 2, y - line_series_item.HoverElement.Height / 2);

                            if (series_plot.Contains(new Point(x, y)))
                            {
                                SeriesHitList.Add(key, new ElementPosition(line_series_item.HoverElement.Content, true, leftTop.X, leftTop.Y, line_series_item.HoverElement.ZIndex));
                            }
                            else
                            {
                                SeriesHitList.Add(key, new ElementPosition(line_series_item.HoverElement.Content));
                            }
                        }

                        if (Cross.IsYDataAttract)
                        {
                            nearestY = y;
                        }
                    }
                    else
                    {
                        if (line_series_item != null)
                        {
                            SeriesHitList.Add(key, new ElementPosition(line_series_item.HoverElement.Content));
                        }
                    }

                }
            }
            else
            {
                foreach (SeriesVisual series_item in series)
                {
                    var line_series_item = series_item.GetInterectHoverableLineSeriesVisual();
                    if (line_series_item != null)
                    {
                        var key = new ComponentKey(series_item.ParentCanvas.Id, series_item.Id);
                        SeriesHitList.Add(key, new ElementPosition(line_series_item.HoverElement.Content));
                    }
                }
            }

            var hitPoint = new Point(nearestX, nearestY);
            VisualData.Items[ContextDataItem.HitPointer] = hitPoint;
            //优化
            if (LastHitPoint.X == nearestX)
            {
                return;
            }
            LastHitPoint = hitPoint;

            if (IsCrossShow)
            {
                foreach (var item in SeriesHitList)
                {
                    item.Value.Render();
                }
            }
            (coms.ConnectVisual as ChartVisual)?.TriggerIntersectChanged(seriesDatas.ToDictionary(it => it.Name ?? $"{nameof(AxisInteractionCanvas)}_{IdGenerator.GenerateId().ToString()}", it => it));

        }
    }
}
