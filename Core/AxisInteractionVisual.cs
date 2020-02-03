﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WPFAnimation
{
    /// <summary>
    /// 取消所有外部plot
    /// </summary>
    public class AxisInteractionVisual : InteractionCanvas
        , ICrossConfiguaration
        , IToolTipConfiguaration
        , IIntersectable
    {
        public event IntersectChanged IntersectChangedHandler;

        private Point LastHitPoint;
        public AxisInteractionVisual(RectDrawingVisual visual, ChartVisualCollection dataSource) : base(visual, dataSource)
        {
            Cross = new CrossVisual(this);
            DataToolTip = new ToolTipVisual(this);

            MouseMove += AxisInteractionVisual_MouseMove;
            IsCrossShow = true;
            IsToolTipShow = true;
        }

        private void AxisInteractionVisual_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (ParentElement == null || !ParentElement.IsMouseOver)
            {
                return;
            }
            var point = e.GetPosition(this);
            Plot(point, EventMessage.MouseOn);

            Cross.VisualData = VisualData;
            DataToolTip.VisualData = VisualData;
            Cross.PlotToParent(point, EventMessage.MouseOn);
            DataToolTip.PlotToParent(point, EventMessage.MouseOn);
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
        public CrossVisual Cross { get; private set; }
        public ToolTipVisual DataToolTip { get; private set; }
        public Line X => Cross.X;
        public Line Y => Cross.Y;

        public bool IsXDataAttract { get => Cross.IsXDataAttract; set => Cross.IsXDataAttract = value; }
        public bool IsYDataAttract { get => Cross.IsYDataAttract; set => Cross.IsYDataAttract = value; }

        public override RectVisualContextData DefaultData => RectChartVisualData.Empty;

        public bool IsLabelShow { get => Cross.IsLabelShow; set => Cross.IsLabelShow = value; }
        public bool IsYShow { get => Cross.IsYShow; set => Cross.IsYShow = value; }
        public bool IsXShow { get => Cross.IsXShow; set => Cross.IsXShow = value; }
        public ToolTip Tip { get => DataToolTip.Tip; set => DataToolTip.Tip = value; }

        public override void Plot(Point point, EventMessage @event)
        {
            var vdata = VisualData.TransformVisualData<RectVisualContextData>();
            if (vdata.IsBad)
            {
                return;
            }
            //series上的悬浮控件 标记当前位置
            var seriesHitList = new List<ElementPosition>();
            var seriesDatas = new List<SeriesData>();

            VisualData.Items[ContextDataItem.SeriesData] = seriesDatas;

            var coms = DataSource as ChartVisualCollection;
            var currentPoint = point;
            var axisxs = coms.AxisXVisuals;
            var series = coms.SeriesVisuals;

            var nearestX = currentPoint.X;
            var nearestY = currentPoint.Y;

            var plotArea = axisxs.PlotArea;

            if (plotArea.Contains(currentPoint))
            {
                foreach (SeriesVisual series_item in series.Visuals)
                {
                    if (!series_item.IsVisualEnable)
                    {
                        continue;
                    }
                    var series_plot = series_item.PlotArea;
                    var xAxis = axisxs.FindById(series_item.XAxisId) as DiscreteAxis;
                    var value = xAxis.GetValue(xAxis.OffsetPostion(currentPoint.X));
                    if (value.IsBad())
                    {
                        continue;
                    }
                    if (series_item.HitElement.Content != null
                        && coms.GetMappingAxisY(series_item.Id) is ContinuousAxis yAxis
                        && series_item.VisualData is RectChartVisualData series_data
                        && series_data.Data.ContainsKey(value))
                    {
                        var x = xAxis.GetPosition(value).X + xAxis.Start.X;
                        var y = yAxis.GetPosition(series_data.Data[value]).Y + xAxis.Start.Y;
                        seriesDatas.Add(new SeriesData() { Color = series_item.Color, Id = series_item.Id, Name = series_item.Name, XValue = value, YValue = series_data.Data[value], AxisX = xAxis, AxisY = (AxisVisual<IVariable>)yAxis });
                        //获取y坐标

                        nearestX = x;

                        if (!series_item.HitElement.IsAdded)
                        {
                            AddElement(series_item.HitElement.Content);
                            series_item.HitElement.IsAdded = true;
                        }
                        var leftTop = new Point(x - series_item.HitElement.Width / 2, y - series_item.HitElement.Height / 2);

                        if (series_plot.Contains(new Point(x, y)))
                        {
                            seriesHitList.Add(new ElementPosition(series_item.HitElement.Content, true, leftTop.X, leftTop.Y, series_item.HitElement.ZIndex));
                        }
                        else
                        {
                            seriesHitList.Add(new ElementPosition(series_item.HitElement.Content));
                        }

                        if (IsYDataAttract)
                        {
                            nearestY = y;
                        }
                    }
                    else
                    {
                        seriesHitList.Add(new ElementPosition(series_item.HitElement.Content));
                    }

                }
            }
            else
            {
                foreach (SeriesVisual series_item in series.Visuals)
                {
                    seriesHitList.Add(new ElementPosition(series_item.HitElement.Content));
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
                foreach (var item in seriesHitList)
                {
                    item.Render();
                }
            }
            IntersectChangedHandler?.Invoke(seriesDatas);
        }

    }
}
