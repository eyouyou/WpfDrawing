using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WPFAnimation
{
    /// <summary>
    /// TODO 这部分可能需要调整
    /// </summary>
    public class SeriesData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Brush Color { get; set; }
        public IVariable XValue { get; set; }
        public AxisVisual<IVariable> AxisX { get; set; }
        public IVariable YValue { get; set; }
        public AxisVisual<IVariable> AxisY { get; set; }
    }
    public enum ToolTipBehavior
    {
        FollowLeft,
    }
    public abstract class ToolTip : UserControl
    {
        public ToolTipBehavior Behavior { get; set; } = ToolTipBehavior.FollowLeft;
        public abstract void PushData(List<SeriesData> series);
        public abstract Border Border { get; }
    }
    public class NormalToolTip : ToolTip
    {
        DockPanel dock = new DockPanel();
        TextBlock XText = new TextBlock();
        DockPanel dock_content = new DockPanel();
        Border border = new Border() { CornerRadius = new CornerRadius(3), Background = Brushes.Black, Padding = new Thickness(10) };

        public NormalToolTip()
        {
            Visibility = Visibility.Collapsed;
            border.Child = dock;
            Content = border;

            Foreground = Brushes.White;
            dock.AddChild(XText, Dock.Top);
            dock.AddChild(dock_content, Dock.Top);
            Opacity = 0.8;
        }

        public override Border Border => border;

        public DockPanel MakeValueText(SeriesData seriesData)
        {
            DockPanel dock = new DockPanel() { LastChildFill = false };
            var ellipse = new Ellipse() { Width = 10, Height = 10, Fill = seriesData.Color, Margin = new Thickness(0, 0, 5, 0) };

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
        }
    }
    public class ToolTipVisual : InteractionLayer, IToolTipConfiguaration
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

            var area = ParentCanvas.DependencyVisual.PlotArea;

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

                        var offsetPoint = new Point(point.X - Tip.DesiredSize.Width, point.Y);

                        if (area.Contains(offsetPoint))
                        {
                            Canvas.SetTop(Tip, offsetPoint.Y);
                            Canvas.SetLeft(Tip, offsetPoint.X);
                        }
                    }
                    break;
                default:
                    break;
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
    }
}
