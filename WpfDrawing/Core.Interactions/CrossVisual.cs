using HevoDrawing.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace HevoDrawing.Interactions
{
    public class CrossVisual : InteractionLayer, ICrossConfiguaration
    {
        /// <summary>
        /// 存储之前的画布,一致就不重画
        /// </summary>
        Rect LastPlotArea;
        Point LastPoint;
        public Line X { get; } = new Line() { Stroke = Brushes.Black, StrokeThickness = 1, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
        public Line Y { get; } = new Line() { Stroke = Brushes.Black, StrokeThickness = 1, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
        public bool IsCrossShow { get; set; } = true;
        public bool IsXDataAttract { get; set; } = true;
        public bool IsYDataAttract { get; set; } = false;

        public bool IsLabelShow { get; set; } = true;
        public bool IsYShow { get; set; } = true;
        public bool IsXShow { get; set; } = true;

        public CrossVisual(InteractionCanvas parent) : base(parent)
        {
            X.IsHitTestVisible = false;
            Y.IsHitTestVisible = false;
            parent.AddElement(X);
            parent.AddElement(Y);
        }

        Dictionary<ComponentKey, AxisVisual> AxisVisuals = new Dictionary<ComponentKey, AxisVisual>();
        public override void PlotToParent(Point point, EventMessage @event)
        {
            if (!VisualData.TryTransformVisualData<ContextData>(out var visual_data))
            {
                return;
            }

            if (!IsCrossShow)
            {
                X.Visibility = Visibility.Collapsed;
                Y.Visibility = Visibility.Collapsed;
                return;
            }

            if (!visual_data.Current.Items.ContainsKey(ContextDataItem.HitPointer))
            {
                return;
            }
            var hitPointer = (Point)visual_data.Current.Items[ContextDataItem.HitPointer];
            //var is_hit_data = (bool)data.Current.Items[ContextDataItem.IsHintData];
            //if (!is_hit_data)
            //{
            //    return;
            //}
            //优化 是否无效
            //bool isXInvariant = false;
            //bool isYInvariant = false;
            //if (hitPointer.X == LastPoint.X)
            //{
            //    isXInvariant = true;
            //}
            //if (hitPointer.Y == LastPoint.Y)
            //{
            //    isYInvariant = true;
            //}
            //LastPoint = hitPointer;

            var isHint = false;
            var hitCanvasId = -1;
            foreach (var item in DataSources)
            {
                if (item.Value is ChartAssembly dataSource)
                {
                    var plotArea = dataSource.ConnectVisual.ParentCanvas.InteractionCanvasPlotArea;
                    bool canHint = !isHint && plotArea.Contains(hitPointer);
                    //进入当前画布
                    if (canHint)
                    {
                        //用来计算放置十字线的画版
                        var area = dataSource.ConnectVisual.PlotArea;
                        var interactionVisualArea = dataSource.ConnectVisual.InteractionPlotArea;
                        var offset = dataSource.ConnectVisual.ParentCanvas.Offset;
                        hitCanvasId = dataSource.ConnectVisual.ParentCanvas.Id;

                        var axisxs = dataSource.AxisXCollection;
                        var axisys = dataSource.AxisYCollection;
                        var series = dataSource.SeriesCollection;

                        //重置十字线大小
                        X.X1 = Y.X1 = interactionVisualArea.Location.X;
                        X.Y1 = Y.Y1 = interactionVisualArea.Location.Y;
                        X.X2 = interactionVisualArea.Location.X + area.Width;
                        X.Y2 = interactionVisualArea.Location.Y;
                        Y.X2 = interactionVisualArea.Location.X;
                        Y.Y2 = interactionVisualArea.Location.Y + area.Height;

                        //放置到Top和Left
                        Canvas.SetLeft(X, 0);
                        Canvas.SetTop(X, 0);
                        Canvas.SetTop(Y, 0);
                        Canvas.SetLeft(Y, 0);

                        if (interactionVisualArea.Contains(hitPointer))
                        {
                            if (IsXShow)
                            {
                                X.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                X.Visibility = Visibility.Collapsed;
                            }
                            Canvas.SetTop(X, hitPointer.Y - interactionVisualArea.Top);

                            if (IsYShow)
                            {
                                Y.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                Y.Visibility = Visibility.Collapsed;
                            }
                            Canvas.SetLeft(Y, hitPointer.X - interactionVisualArea.Left);

                            //if (IsLabelShow)
                            //{
                            //    var nearestX = point.X;
                            //    var nearestY = point.Y;
                            //    //var nearestY = point.Y;
                            //    //找最小距离
                            //    var axisLabelList = new List<ElementPosition>();

                            //    foreach (DiscreteAxis axis in axisxs)
                            //    {
                            //        var value = axis.GetValue(axis.OffsetPostion(point.X));
                            //        if (!value.IsBad() && axis.IsAxisLabelShow)
                            //        {
                            //            var key = new ComponentKey(axis.ParentCanvas.Id, axis.Id);
                            //            if (!AxisVisuals.ContainsKey(key))
                            //            {
                            //                ParentCanvas.AddElement(axis.AxisLabel);
                            //                AxisVisuals[key] = axis;
                            //            }

                            //            axis.AxisLabel.Text = axis.GetStringValue(value);
                            //            var labelData = axis.GetAxisLabelData(value);

                            //            axisLabelList.Add(new ElementPosition(axis.AxisLabel, true, labelData.Left, labelData.Top, axis.AxisLabel.ZIndex));
                            //        }
                            //        else
                            //        {
                            //            axisLabelList.Add(new ElementPosition(axis.AxisLabel));
                            //        }
                            //    }
                            //    foreach (ContinuousAxis axis in axisys)
                            //    {
                            //        var value = axis.GetValue(axis.OffsetPostion(nearestY));
                            //        if (axis.IsAxisLabelShow)
                            //        {
                            //            var key = new ComponentKey(axis.ParentCanvas.Id, axis.Id);

                            //            if (!AxisVisuals.ContainsKey(key))
                            //            {
                            //                ParentCanvas.Children.Add(axis.AxisLabel);
                            //                AxisVisuals[key] = axis;
                            //            }

                            //            axis.AxisLabel.Text = $"{axis.GetStringValue(value)}{axis.Unit}";
                            //            var labelData = axis.GetAxisLabelData(value);

                            //            axisLabelList.Add(new ElementPosition(axis.AxisLabel, true, labelData.Left, labelData.Top));

                            //        }
                            //        else
                            //        {
                            //            axisLabelList.Add(new ElementPosition(axis.AxisLabel));
                            //        }
                            //    }

                            //    foreach (var labelItem in axisLabelList)
                            //    {
                            //        labelItem.Render();
                            //    }

                            //}

                        }
                        else
                        {
                            foreach (var visual in AxisVisuals)
                            {
                                visual.Value.AxisLabel.Visibility = Visibility.Hidden;
                            }

                            X.Visibility = Visibility.Collapsed;
                            Y.Visibility = Visibility.Collapsed;
                        }
                        isHint = true;
                    }

                }
            }
            foreach (var visual in AxisVisuals)
            {
                if (visual.Key.CanvasId != hitCanvasId)
                {
                    visual.Value.AxisLabel.Visibility = Visibility.Hidden;
                }
            }


            //X.Visibility = Visibility.Collapsed;
            //Y.Visibility = Visibility.Collapsed;


        }

        public override void Hide()
        {
            foreach (var item in AxisVisuals)
            {
                //这个一定要hidden 不然没法计算大小
                item.Value.AxisLabel.Visibility = Visibility.Hidden;
            }

            X.Visibility = Visibility.Collapsed;
            Y.Visibility = Visibility.Collapsed;
        }

        public override void Clear()
        {
        }

        public override void PlotToParentStandalone(Point point, EventMessage @event)
        {
            if (!VisualData.TryTransformVisualData<ContextData>(out var visual_data))
            {
                return;
            }

            if (!IsCrossShow)
            {
                X.Visibility = Visibility.Collapsed;
                Y.Visibility = Visibility.Collapsed;
                return;
            }

            var coms = DataSources.ElementAt(0).Value as ChartAssembly;

            var axisxs = coms.AxisXCollection;
            var axisys = coms.AxisYCollection;
            var series = coms.SeriesCollection;
            var plotArea = coms.ConnectVisual.PlotArea;
            if (LastPlotArea != plotArea)
            {
                X.X1 = Y.X1 = 0;
                X.Y1 = Y.X1 = 0;
                X.X2 = plotArea.Width;
                X.Y2 = 0;
                Y.X2 = 0;
                Y.Y2 = plotArea.Height;

                Canvas.SetLeft(X, plotArea.Location.X);
                Canvas.SetTop(X, plotArea.Location.Y);
                Canvas.SetTop(Y, plotArea.Location.Y);
                Canvas.SetLeft(Y, plotArea.Location.X);
            }

            if (!visual_data.Current.Items.ContainsKey(ContextDataItem.HitPointer))
            {
                return;
            }
            var hitPointer = (Point)visual_data.Current.Items[ContextDataItem.HitPointer];
            //优化
            bool isXInvariant = false;
            bool isYInvariant = false;
            if (hitPointer.X == LastPoint.X)
            {
                isXInvariant = true;
            }
            if (hitPointer.Y == LastPoint.Y)
            {
                isYInvariant = true;
            }
            LastPoint = hitPointer;
            if (plotArea.Contains(point))
            {
                if (IsLabelShow)
                {
                    var nearestX = point.X;
                    var nearestY = point.Y;
                    //var nearestY = point.Y;
                    //找最小距离
                    var axisLabelList = new List<ElementPosition>();

                    if (!isXInvariant)
                    {
                        foreach (DiscreteAxis axis in axisxs)
                        {
                            var value = axis.GetValue(axis.OffsetPostion(point.X));
                            if (!value.IsBad() && axis.IsAxisLabelShow)
                            {
                                if (!CurrentAll.ContainsKey(axis.Id))
                                {
                                    ParentCanvas.AddElement(axis.AxisLabel);
                                    CurrentAll[axis.Id] = axis;
                                }

                                axis.AxisLabel.Text = axis.GetStringValue(value);
                                var labelData = axis.GetAxisLabelData(value);
                                if (labelData.IsBad)
                                {
                                    continue;
                                }
                                axisLabelList.Add(new ElementPosition(axis.AxisLabel, true, labelData.Left, labelData.Top, axis.AxisLabel.ZIndex));

                            }
                            else
                            {
                                axisLabelList.Add(new ElementPosition(axis.AxisLabel));
                            }
                        }
                    }

                    if (!isYInvariant)
                    {
                        foreach (ContinuousAxis axis in axisys)
                        {
                            var value = axis.GetValue(axis.OffsetPostion(nearestY));
                            if (axis.IsAxisLabelShow)
                            {
                                if (!CurrentAll.ContainsKey(axis.Id))
                                {
                                    ParentCanvas.Children.Add(axis.AxisLabel);
                                    CurrentAll[axis.Id] = axis;
                                }

                                axis.AxisLabel.Text = $"{axis.GetStringValue(value)}{axis.Unit}";
                                var labelData = axis.GetAxisLabelData(value);

                                axisLabelList.Add(new ElementPosition(axis.AxisLabel, true, labelData.Left, labelData.Top));

                            }
                            else
                            {
                                axisLabelList.Add(new ElementPosition(axis.AxisLabel));
                            }
                        }
                    }

                    foreach (var item in axisLabelList)
                    {
                        item.Render();
                    }

                }

                LastPlotArea = plotArea;

                if (!isYInvariant)
                {
                    if (IsXShow)
                    {
                        X.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        X.Visibility = Visibility.Collapsed;
                    }
                    Canvas.SetTop(X, hitPointer.Y);
                }
                if (!isXInvariant)
                {
                    if (IsYShow)
                    {
                        Y.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        Y.Visibility = Visibility.Collapsed;
                    }
                    Canvas.SetLeft(Y, hitPointer.X);
                }
            }
            else
            {
                foreach (var item in CurrentAll)
                {
                    item.Value.AxisLabel.Visibility = Visibility.Collapsed;
                }

                X.Visibility = Visibility.Collapsed;
                Y.Visibility = Visibility.Collapsed;
            }
        }
        Dictionary<int, AxisVisual> CurrentAll = new Dictionary<int, AxisVisual>();
    }
}
