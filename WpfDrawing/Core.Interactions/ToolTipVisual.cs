using HevoDrawing.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace HevoDrawing.Interactions
{
    public enum ToolTipBehavior
    {
        FollowLeft,
    }
    public abstract class ToolTip : UserControl
    {
        public ToolTipBehavior Behavior { get; set; } = ToolTipBehavior.FollowLeft;
        public abstract void PushData(List<SeriesData> series);
        public abstract Border Border { get; }
        public abstract DockPanel TextContainer { get; }
        public abstract Grid Layers { get; }
    }
    public class NormalToolTip : ToolTip
    {
        DockPanel dock = new DockPanel();
        TextBlock XText = new TextBlock();
        DockPanel dock_content = new DockPanel();
        Border border = new Border() { CornerRadius = new CornerRadius(3), Padding = new Thickness(10) };
        Grid layers = new Grid();
        public NormalToolTip()
        {
            Visibility = Visibility.Collapsed;
            layers.Children.Add(dock);
            border.Child = layers;
            Content = border;

            dock.AddChild(XText, Dock.Top);
            dock.AddChild(dock_content, Dock.Top);
        }

        public override Border Border => border;

        public override Grid Layers => layers;

        public override DockPanel TextContainer => dock;

        private DockPanel MakeValueText(SeriesData seriesData)
        {
            DockPanel dock = new DockPanel() { LastChildFill = false, VerticalAlignment = VerticalAlignment.Center };
            var ellipse = new Ellipse() { Width = 10, Height = 10, Fill = seriesData.Color, Margin = new Thickness(0, 0, 5, 0), VerticalAlignment = VerticalAlignment.Center };

            dock.AddChild(ellipse, Dock.Left);
            if (!string.IsNullOrEmpty(seriesData.Name))
            {
                dock.AddChild(new TextBlock() { Text = $"{seriesData.Name}: " }, Dock.Left);
            }
            dock.AddChild(new TextBlock() { Text = $"{seriesData.AxisY.GetStringValue(seriesData.YValue)}{seriesData.AxisY.Unit}" }, Dock.Left);
            return dock;
        }
        public override void PushData(List<SeriesData> series)
        {
            dock_content.Children.Clear();
            //TODO 这部分后续需要支持多轴
            if (series == null || series.Count <= 0)
            {
                Visibility = Visibility.Collapsed;
                return;
            }
            Visibility = Visibility.Visible;
            var se = series[0];
            var value = se.AxisX.GetStringValue(se.XValue);
            XText.Text = !string.IsNullOrEmpty(se.AxisX.Name) ? $"{se.AxisX.Name}: {value}{se.AxisX.Unit}" : $"{value}{se.AxisX.Unit}";
            foreach (var item in series)
            {
                dock_content.AddChild(MakeValueText(item), Dock.Top);
            }
            //dock_content.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            //Layers.Width = dock_content.DesiredSize.Width + dock.Margin.Left + dock.Margin.Right + border.Padding.Left + border.Padding.Right + TextContainer.Margin.Left + TextContainer.Margin.Right;
            //Layers.Height = dock_content.DesiredSize.Height + dock.Margin.Top + dock.Margin.Bottom + border.Padding.Top + border.Padding.Bottom + TextContainer.Margin.Top + TextContainer.Margin.Bottom;
        }
    }
    public class ToolTipVisual : InteractionLayer
    {
        private ToolTip _tip = new NormalToolTip();
        public ToolTip Tip
        {
            get => _tip;
            set
            {
                if (ParentCanvas.Children.Contains(_tip))
                {
                    ParentCanvas.Children.Remove(_tip);
                }
                if (!ParentCanvas.Children.Contains(value))
                {
                    ParentCanvas.Children.Add(value);
                }
                _tip = value;
            }
        }
        public bool IsToolTipShow { get; set; } = true;

        public ToolTipVisual(InteractionCanvas parent) : base(parent)
        {
            ParentCanvas.AddElement(Tip);
            //ParentCanvas.Loaded += ParentCanvas_Loaded;
        }

        private void ParentCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            ParentCanvas.Loaded -= ParentCanvas_Loaded;
            //var comps = DataSource as ChartVisualCollection;
            //var series = comps.GetSeriesCollection();
            //foreach (SeriesVisual item in series)
            //{
            //    comps.get
            //    //item.DataSource = 
            //}
            ////Tip.Scheme = 
        }

        Point LastPoint;
        public override void PlotToParent(Point point, EventMessage @event)
        {
            var data = VisualData.GetMainVisualDataItem<List<SeriesData>>(ContextDataItem.SeriesData);
            if (data == null)
            {
                return;
            }
            if (!IsToolTipShow)
            {
                Tip.Visibility = Visibility.Collapsed;
                return;
            }

            //优化
            var hitPointer = (Point)VisualData.Current.Items[ContextDataItem.HitPointer];
            bool isXInvariant = false;
            if (hitPointer.X == LastPoint.X)
            {
                isXInvariant = true;
            }
            LastPoint = hitPointer;

            bool isHint = false;

            var is_hit_data = (bool)VisualData.Current.Items[ContextDataItem.IsHintData];

            foreach (var item in DataSources)
            {
                if (item.Value is ChartAssembly dataSource)
                {
                    var area = dataSource.ConnectVisual.InteractionPlotArea;
                    if (isHint || !is_hit_data)
                    {
                        continue;
                    }
                    if (!area.Contains(point))
                    {
                        Tip.Visibility = Visibility.Collapsed;
                        continue;
                    }
                    if (!isXInvariant)
                    {
                        Tip.PushData(data);
                    }
                    Tip.Visibility = Visibility.Visible;

                    switch (Tip.Behavior)
                    {
                        case ToolTipBehavior.FollowLeft:
                            {
                                Tip.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                                var offsetLeftTopPoint = new Point(point.X - Tip.DesiredSize.Width, point.Y);
                                var offsetLeftButtomPoint = new Point(point.X - Tip.DesiredSize.Width, point.Y + Tip.DesiredSize.Height);
                                var offsetRightButtomPoint = new Point(point.X, point.Y + Tip.DesiredSize.Height);
                                var offsetRightTopPoint = new Point(point.X, point.Y);
                                if (!area.Contains(offsetLeftTopPoint) && !area.Contains(offsetLeftButtomPoint))
                                {
                                    offsetLeftTopPoint.X += Tip.DesiredSize.Width;
                                }
                                if (!area.Contains(offsetRightButtomPoint) && !area.Contains(offsetLeftButtomPoint))
                                {
                                    offsetLeftTopPoint.Y -= Tip.DesiredSize.Height;
                                }

                                Canvas.SetTop(Tip, offsetLeftTopPoint.Y);
                                Canvas.SetLeft(Tip, offsetLeftTopPoint.X);
                            }
                            break;
                        default:
                            break;
                    }

                    isHint = true;
                }
            }



        }
        public override void Hide()
        {
            Tip.Visibility = Visibility.Collapsed;
        }

        public override void Clear()
        {
            if (ParentCanvas.Children.Contains(Tip))
            {
                ParentCanvas.Children.Remove(Tip);
            }
        }

        public override void PlotToParentStandalone(Point point, EventMessage @event)
        {
            var data = VisualData.GetMainVisualDataItem<List<SeriesData>>(ContextDataItem.SeriesData);
            if (data == null)
            {
                return;
            }
            if (!IsToolTipShow)
            {
                Tip.Visibility = Visibility.Collapsed;
                return;
            }
            //优化
            var hitPointer = (Point)VisualData.Current.Items[ContextDataItem.HitPointer];
            bool isXInvariant = false;
            if (hitPointer.X == LastPoint.X)
            {
                isXInvariant = true;
            }
            LastPoint = hitPointer;

            var coms = DataSources.ElementAt(0).Value as ChartAssembly;
            var area = coms.ConnectVisual.PlotArea;

            if (!area.Contains(point))
            {
                Tip.Visibility = Visibility.Collapsed;
                return;
            }
            if (!isXInvariant)
            {
                Tip.PushData(data);
            }
            Tip.Visibility = Visibility.Visible;

            switch (Tip.Behavior)
            {
                case ToolTipBehavior.FollowLeft:
                    {
                        Tip.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                        var offsetLeftTopPoint = new Point(point.X - Tip.DesiredSize.Width, point.Y);
                        var offsetLeftButtomPoint = new Point(point.X - Tip.DesiredSize.Width, point.Y + Tip.DesiredSize.Height);
                        var offsetRightButtomPoint = new Point(point.X, point.Y + Tip.DesiredSize.Height);
                        var offsetRightTopPoint = new Point(point.X, point.Y);
                        if (!area.Contains(offsetLeftTopPoint) && !area.Contains(offsetLeftButtomPoint))
                        {
                            offsetLeftTopPoint.X += Tip.DesiredSize.Width;
                        }
                        if (!area.Contains(offsetRightButtomPoint) && !area.Contains(offsetLeftButtomPoint))
                        {
                            offsetLeftTopPoint.Y -= Tip.DesiredSize.Height;
                        }

                        Canvas.SetTop(Tip, offsetLeftTopPoint.Y);
                        Canvas.SetLeft(Tip, offsetLeftTopPoint.X);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
