using HevoDrawing.Abstractions;
using HevoDrawing.Interactions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HevoDrawing.Sample
{
    /// <summary>
    /// WindowCharting.xaml 的交互逻辑
    /// </summary>
    public partial class WindowCharting : Window
    {
        ChartVisual chart = new ChartVisual();
        GenericCanvasContainer chartCanvas = new GenericCanvasContainer(true) { };

        ContinuousAxis axisY = new ContinuousAxis(AxisPosition.Left) { ValueFormat = "F1", SplitValueFormat = "F1", ShowGridLine = true, AxisPen = new Pen(Brushes.Green, 1) };
        ContinuousAxis axisY2 = new ContinuousAxis(AxisPosition.Right) { ValueFormat = "F1", SplitValueFormat = "F1", ShowGridLine = true, AxisPen = new Pen(Brushes.Purple, 1) };
        ContinuousAxis axisY3 = new ContinuousAxis(AxisPosition.Right) { ValueFormat = "F1", SplitValueFormat = "F1", ShowGridLine = true, AxisPen = new Pen(Brushes.DarkKhaki, 1) };

        DiscreteAxis axisX = new DateTimeAxis(AxisPosition.Buttom) { ValueFormat = "yyyyMMdd", SplitValueFormat = "yyyyMMdd" };
        DiscreteAxis axisX2 = new DateTimeAxis(AxisPosition.Top) { ValueFormat = "yyyyMMdd", SplitValueFormat = "yyyyMMdd" };
        DiscreteAxis axisX3 = new DateTimeAxis(AxisPosition.Buttom) { ValueFormat = "yyyyMMdd", SplitValueFormat = "yyyyMMdd" };

        public WindowCharting()
        {
            chartCanvas.Background = Brushes.White;
            Timer timer = new Timer(1000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            DockPanel grid = new DockPanel() { };

            axisY3.Offsets = new PaddingOffset(0, 0, 50, 0);
            axisX3.Offsets = new PaddingOffset(0, 0, 0, 60);
            var ratios = new List<double>() { 0.2, 0.3, 0.3, 0.2 };
            ratios.Insert(0, 0);
            axisX.Ratios = ratios;
            axisX2.Ratios = ratios;
            axisX3.Ratios = ratios;

            axisX.IsInterregional = false;
            axisX.AxisPen = new Pen(Brushes.Red, 1);
            axisX2.AxisPen = new Pen(Brushes.PeachPuff, 1);
            axisX3.AxisPen = new Pen(Brushes.DarkCyan, 1);

            AxisInteractionCanvas interaction = new AxisInteractionCanvas();
            chartCanvas.Canvas.AddChild(chart); chartCanvas.Canvas.DataSource = chart.DataSource;
            RectInteractionGroup container = new RectInteractionGroup(interaction, 1, 1, chartCanvas);

            BlurryUserControl b = new BlurryUserControl() { Background = new SolidColorBrush(ColorHelper.StringToColor("#BE323337")).OfStrength(0.2d) };
            b.BlurContainer = container.DrawingGrid;
            b.Magnification = 0.25;
            b.BlurRadius = 30;

            interaction.Tip.Layers.Children.Insert(0, b);

            interaction.Cross.IsCrossShow = true;
            chart.AddAsixX(axisX);
            chart.AddAsixX(axisX2);
            chart.AddAsixX(axisX3);
            //ValueAxis axisY2 = new ValueAxis(AxisPosition.Left) { ValueFormat = "F1" };
            //axisY2.Offsets.Left = new GridLength(50);
            //asix.AddAsixY(axisY2);
            chart.AddAsixY(axisY);
            chart.AddAsixY(axisY2);
            chart.AddAsixY(axisY3);

            StraightLineSeriesVisual lineSeries = new StraightLineSeriesVisual() { };
            StraightLineSeriesVisual lineSeries2 = new StraightLineSeriesVisual() { };

            chart.AddSeries(lineSeries);
            chart.AddSeries(lineSeries2);
            //asix.PaddingOffset.Left = new GridLength(0.1, GridUnitType.Star);
            //asix.PaddingOffset.Top = new GridLength(0.1, GridUnitType.Star);
            //asix.PaddingOffset.Right = new GridLength(0.1, GridUnitType.Star);
            //asix.PaddingOffset.Buttom = new GridLength(0.1, GridUnitType.Star);
            chart.Offsets = new PaddingOffset(50);

            var now = DateTime.Now;
            Random random = new Random();
            var max = 1000;
            Dictionary<DateTime, double> dic = new Dictionary<DateTime, double>();
            Dictionary<DateTime, double> dic2 = new Dictionary<DateTime, double>();
            for (int i = 0; i < 4; i++)
            {
                dic.Add(now.AddDays(i), random.NextDouble() * max);
            }
            for (int i = 0; i < 3; i++)
            {
                dic2.Add(now.AddDays(i), random.NextDouble() * max);
            }

            lineSeries2.LinePen.Brush = Brushes.Red;
            lineSeries2.VisualData = dic2.ToFormatVisualData();
            lineSeries.VisualData = dic.ToFormatVisualData();


            //Dictionary<DateTime, double> dic3 = new Dictionary<DateTime, double>();
            chart.Plot();

            //TestVisual test = new TestVisual(axisRect);
            //chartCanvas.AddChild(test);
            //test.VisualData = null;

            grid.AddChild(container, Dock.Left);
            //grid.Children.Add(new TextBlock() { Text = "123"});
            Content = grid;

            this.SizeChanged += WindowCharting_SizeChanged;
        }

        private void WindowCharting_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            chartCanvas.Replot();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
            {
                //chart.Plot();
            }));
        }
    }
}
