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

        Dictionary<ComponentKey, AxisVisual> AxisVisuals = new Dictionary<ComponentKey, AxisVisual>();
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

            if (!data.Current.Items.ContainsKey(ContextDataItem.HitPointer))
            {
                ParentCanvas.Plot(point, @event);
            }
            var hitPointer = (Point)data.Current.Items[ContextDataItem.HitPointer];

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
                if (item.Value is ChartDataSource dataSource)
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
                        }
                        else
                        {
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
    }
}
