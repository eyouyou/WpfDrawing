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
using System.Windows.Media.Imaging;

namespace WpfDrawing.Sample
{
    /// <summary>
    /// WindowCharting3.xaml 的交互逻辑
    /// </summary>
    public partial class WindowCharting3 : Window
    {
        Chart chart = new Chart();
        RectDrawingCanvas chartCanvas = new RectDrawingCanvas(true) { };
        DiscreteAxis axisX = new DateTimeAxis(AxisPosition.Buttom) { Name = "时间", ValueFormat = "yyyyMMdd", SplitValueFormat = "yyyy/MM", IsInterregional = true, ShowGridLine = false, IsGridLineClose = true };

        StraightLineSeriesVisual lineSeries = new StraightLineSeriesVisual() { Name = "概念关注度" };
        StraightLineSeriesVisual lineSeries2 = new StraightLineSeriesVisual() { Name = "A股平均关注度", LinePen = new Pen(Brushes.OrangeRed, 1) };
        public WindowCharting3()
        {
            var uri = $"/wallpaper_mikael_gustafsson.png";

            InitializeComponent();
            Grid grid = new Grid() { };

            axisX.IsInterregional = false;

            ContinuousAxis axisY = new ContinuousAxis(AxisPosition.Left) { ValueFormat = "G4", SplitValueFormat = "G4", ShowGridLine = true, AxisPen = new Pen(Brushes.Green, 1), Unit = "万" };
            //axisY.Range = new Range() { Max = new Value<double>(5000000), Min = new Value<double>(40000) };
            chart.Offsets.Left = new GridLength(60);
            chart.Offsets.Buttom = new GridLength(20);
            chart.Offsets.Right = new GridLength(10);
            chart.Offsets.Top = new GridLength(20);

            chartCanvas.AddChild(chart);
            chartCanvas.DataSource = chart.DataSource;
            AxisInteractionCanvas interaction = new AxisInteractionCanvas(chartCanvas);

            RectInteractionGroup container = new RectInteractionGroup(interaction, 1, 1, chartCanvas);
            //container.Background = Brushes.GreenYellow;
            chart.AddAsixX(axisX);
            chart.AddAsixY(axisY);

            chart.AddSeries(lineSeries);
            chart.AddSeries(lineSeries2);

            grid.Children.Add(container);
            //chartCanvas.Visuals.Add(CreateDrawingImage());

            //chart.CrossOption.IsLabelShow = false;
            //chart.CrossOption.IsXShow = false;

            BlurryUserControl b = new BlurryUserControl() { };
            b.BlurContainer = chartCanvas;
            b.Magnification = 0.25;
            b.BlurRadius = 10;

            interaction.ToolTipOption.Tip.TextContainer.Margin = new Thickness(10);
            interaction.ToolTipOption.Tip.Layers.Children.Insert(0, b);
            interaction.ToolTipOption.Tip.FontSize = 11;

            interaction.ToolTipOption.Tip.Border.Padding = new Thickness(0);
            //chart.ToolTipOption.Tip.Foreground = Brushes.White;
            //chart.ToolTipOption.Tip.Border.BorderThickness = new Thickness(2);
            //chart.ToolTipOption.Tip.Border.BorderBrush = Brushes.White;
            //chart.ToolTipOption.Tip.Background = Brushes.Black;
            //chart.ToolTipOption.Tip.Opacity = 0.5;
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
        private DrawingVisual CreateDrawingImage()
        {
            var uri = $"wallpaper_mikael_gustafsson.png";

            System.Windows.Media.DrawingVisual drawingVisual = new System.Windows.Media.DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            drawingVisual.XSnappingGuidelines = new DoubleCollection(new List<double>() { 1 });
            drawingVisual.YSnappingGuidelines = new DoubleCollection(new List<double>() { 1 });
            //Image image = new Image() { Source =  };
            var source = new BitmapImage(new Uri(uri, UriKind.Relative));
            drawingContext.DrawImage(source, new Rect(new Size(800, 450)));
            drawingContext.Close();

            return drawingVisual;

        }
        private void WindowCharting3_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            StartDataFeed();
        }

        public async void StartDataFeed()
        {
            var dic = await Request("881151");
            var dic2 = await Request("000001", true);

            var group = dic.GroupBy(it => it.Key.ToString("yyyyMM")).Select(it => it.ElementAt(0).Key).ToList();
            group.AddRange(dic2.GroupBy(it => it.Key.ToString("yyyyMM")).Select(it => it.ElementAt(0).Key).ToList());
            group = group.Distinct().OrderBy(it => it).ToList();
            axisX.SplitValues = group.Select(it => it.ToVisualData()).ToList();

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
#if DEBUG
                if (blockId == "881151")
                {
                    content = @"{ ""errorcode"":0,""errormsg"":"""",""result"":{ ""20191108"":{ ""market"":1395440,""881151"":""""},""20191111"":{ ""market"":1095570,""881151"":895019},""20191112"":{ ""market"":1853666,""881151"":2088995},""20191113"":{ ""market"":1831204,""881151"":2011745},""20191114"":{ ""market"":1894441,""881151"":1305982},""20191115"":{ ""market"":1881099,""881151"":1566144},""20191118"":{ ""market"":2473021,""881151"":1990847},""20191119"":{ ""market"":2840414,""881151"":5250764},""20191120"":{ ""market"":2545418,""881151"":3477587},""20191121"":{ ""market"":2078763,""881151"":2631980},""20191122"":{ ""market"":2152260,""881151"":2181767},""20191125"":{ ""market"":2045025,""881151"":1879575},""20191126"":{ ""market"":725091,""881151"":1057709},""20191127"":{ ""market"":2325209,""881151"":2126412},""20191128"":{ ""market"":2299273,""881151"":3177618},""20191129"":{ ""market"":2569900,""881151"":3169631},""20191202"":{ ""market"":1816521,""881151"":1673178},""20191203"":{ ""market"":2169489,""881151"":1698748},""20191204"":{ ""market"":1401311,""881151"":1603250},""20191205"":{ ""market"":2058801,""881151"":3063886},""20191206"":{ ""market"":1628176,""881151"":1755468},""20191209"":{ ""market"":2048284,""881151"":1949771},""20191210"":{ ""market"":2049793,""881151"":2210885},""20191211"":{ ""market"":2353469,""881151"":3614880},""20191212"":{ ""market"":2451059,""881151"":3748137},""20191213"":{ ""market"":1978221,""881151"":2585166},""20191216"":{ ""market"":2072249,""881151"":""""},""20191217"":{ ""market"":2490681,""881151"":""""},""20191218"":{ ""market"":1663759,""881151"":""""},""20191219"":{ ""market"":2402602,""881151"":""""},""20191220"":{ ""market"":2609730,""881151"":""""},""20191223"":{ ""market"":2993228,""881151"":""""},""20191224"":{ ""market"":2481527,""881151"":""""},""20191225"":{ ""market"":2824943,""881151"":""""},""20191226"":{ ""market"":3473285,""881151"":""""},""20191227"":{ ""market"":2605087,""881151"":""""},""20191230"":{ ""market"":2924813,""881151"":""""},""20191231"":{ ""market"":1961475,""881151"":""""},""20200102"":{ ""market"":2270549,""881151"":""""},""20200103"":{ ""market"":2264138,""881151"":""""},""20200106"":{ ""market"":1805448,""881151"":""""},""20200107"":{ ""market"":2106266,""881151"":""""},""20200108"":{ ""market"":2417253,""881151"":""""},""20200109"":{ ""market"":1955393,""881151"":""""},""20200110"":{ ""market"":2059266,""881151"":""""},""20200113"":{ ""market"":1942617,""881151"":""""},""20200114"":{ ""market"":2358335,""881151"":""""},""20200115"":{ ""market"":2462698,""881151"":""""},""20200116"":{ ""market"":1534428,""881151"":""""},""20200117"":{ ""market"":2568594,""881151"":""""},""20200120"":{ ""market"":1865553,""881151"":""""},""20200121"":{ ""market"":1415054,""881151"":""""},""20200122"":{ ""market"":2325162,""881151"":""""},""20200123"":{ ""market"":2719654,""881151"":""""},""20200203"":{ ""market"":1922227,""881151"":""""},""20200204"":{ ""market"":2225673,""881151"":""""},""20200205"":{ ""market"":1822513,""881151"":""""},""20200206"":{ ""market"":1779576,""881151"":""""},""20200207"":{ ""market"":1798294,""881151"":""""},""20200210"":{ ""market"":1687355,""881151"":""""} } }";
                }
#endif
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
