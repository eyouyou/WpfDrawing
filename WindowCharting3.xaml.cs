using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WPFAnimation
{
    /// <summary>
    /// WindowCharting3.xaml 的交互逻辑
    /// </summary>
    public partial class WindowCharting3 : Window
    {
        RectChartVisual chart = new RectChartVisual();
        RectDrawingCanvas chartCanvas = new RectDrawingCanvas(true) { };
        DiscreteAxis axisX = new DateTimeAxis(AxisPosition.Buttom) { Name = "时间", ValueFormat = "yyyyMMdd", SplitValueFormat = "yyyy/MM", IsInterregional = true, ShowGridLine = false, IsGridLineClose = true };

        StraightLineSeriesVisual lineSeries = new StraightLineSeriesVisual() { Name = "概念关注度" };
        StraightLineSeriesVisual lineSeries2 = new StraightLineSeriesVisual() { Name = "A股平均关注度", LinePen = new Pen(Brushes.OrangeRed, 1) };
        public WindowCharting3()
        {
            InitializeComponent();
            DockPanel grid = new DockPanel() { };

            ContinuousAxis axisY = new ContinuousAxis(AxisPosition.Left) { ValueFormat = "0.00万", SplitValueFormat = "0万", ShowGridLine = true, AxisPen = new Pen(Brushes.Green, 1) };
            //axisY.Range = new Range() { Max = new Value<double>(5000000), Min = new Value<double>(40000) };
            chart.Offsets.Left = new GridLength(50);
            chart.Offsets.Buttom = new GridLength(20);
            chart.Offsets.Right = new GridLength(10);
            chart.Offsets.Top = new GridLength(20);

            RectInteractionContainer container = new RectInteractionContainer(chart, chartCanvas);

            chart.AddAsixX(axisX);
            chart.AddAsixY(axisY);

            chart.AddSeries(lineSeries);
            chart.AddSeries(lineSeries2);

            //chart.CrossOption.IsLabelShow = false;
            //chart.CrossOption.IsXShow = false;

            chart.ToolTipOption.Tip.FontSize = 11;
            chart.ToolTipOption.Tip.Border.Padding = new Thickness(1);


            grid.AddChild(container, Dock.Left);
            Content = grid;
            IsVisibleChanged += WindowCharting3_IsVisibleChanged;
            SizeChanged += WindowCharting3_SizeChanged;
            StartDataFeed();
            //Timer timer = new Timer(200);
            //timer.Elapsed += Timer_Elapsed;
            //timer.Start();
        }

        private void WindowCharting3_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            chartCanvas.Plot();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() =>
           {
               StartDataFeed();
           }));

        }

        private void WindowCharting3_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            StartDataFeed();
        }

        public async void StartDataFeed()
        {
            var dic = await Request("881151");
            var dic2 = await Request("000001", true);
            lineSeries2.VisualData = dic2.ToVisualData();
            lineSeries.VisualData = dic.ToVisualData();
            chart.Plot();
        }
        public async Task<Dictionary<DateTime, double>> Request(string blockId, bool isMarket = false)
        {
            var str = $"http://zx.10jqka.com.cn/hotevent/api/getselfstocknum?blockid={blockId}";
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(str);
                var content = await response.Content.ReadAsStringAsync();
                Dictionary<DateTime, double> dic;
                JObject @object = JObject.Parse(content);
                if (@object.TryGetValue("result", out var resultToken))
                {
                    if (isMarket)
                    {
                        dic = resultToken.AsParallel().ToDictionary(it => GetKeyTime(it), it => GetValue<double>(it, "market") / 10000);
                    }
                    else
                    {
                        dic = resultToken.AsParallel().ToDictionary(it => GetKeyTime(it), it => GetValue<double>(it, blockId) / 10000);
                    }

                    dic = dic.OrderBy(it => it.Key).ToDictionary(it => it.Key, it => it.Value);
                    axisX.IsInterregional = false;
                    var group = dic.GroupBy(it => it.Key.ToString("yyyyMM")).Select(it => it.ElementAt(0).Key);
                    axisX.SplitValues = group.Select(it => it.ToVisualData()).ToList();
                    return dic;
                }
                return new Dictionary<DateTime, double>();
            }


        }

        private T GetValue<T>(JToken it, string v)
        {
            if (it is JProperty value && value.Value[v] != null && !value.Value[v].HasValues && value.Value[v] is JValue trueValue && !string.IsNullOrEmpty(trueValue.Value.ToString()))
            {
                return value.Value[v].Value<T>();
            }
            return default;
        }

        private DateTime GetKeyTime(JToken it)
        {
            if (it is JProperty value && !string.IsNullOrEmpty(value.Name))
            {
                return DateTime.ParseExact(value.Name, "yyyyMMdd", CultureInfo.InvariantCulture);
            }
            return DateTime.MinValue;
        }
    }
}
