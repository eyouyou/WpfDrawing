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

namespace WpfDrawing
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

        Dictionary<int, AxisVisual> CurrentAll = new Dictionary<int, AxisVisual>();
        public override void PlotToParent(Point point, EventMessage @event)
        {
            var vdata = VisualData.TransformVisualData<RectVisualContextData>();
            if (vdata.IsBad)
            {
                return;
            }

            if (!IsCrossShow)
            {
                X.Visibility = Visibility.Collapsed;
                Y.Visibility = Visibility.Collapsed;
                return;
            }


            var data = vdata.Value;
            var coms = DataSource.ElementAt(0).Value as ChartDataSource;

            var axisxs = coms.AxisXCollection;
            var axisys = coms.AxisYCollection;
            var series = coms.SeriesCollection;
            var plotArea = coms.ConnectChart.PlotArea;
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
            if (!data.Current.Items.ContainsKey(ContextDataItem.HitPointer))
            {
                ParentCanvas.Plot(point, @event);
            }
            var hitPointer = (Point)data.Current.Items[ContextDataItem.HitPointer];
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

        public override void Hide()
        {
            foreach (var item in CurrentAll)
            {
                item.Value.AxisLabel.Visibility = Visibility.Collapsed;
            }

            X.Visibility = Visibility.Collapsed;
            Y.Visibility = Visibility.Collapsed;
        }

        public override void Clear()
        {
        }
    }
}
