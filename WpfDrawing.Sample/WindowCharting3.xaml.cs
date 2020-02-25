using HevoDrawing;
using HevoDrawing.Abstractions;
using HevoDrawing.Interactions;
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

namespace HevoDrawing.Sample
{
    /// <summary>
    /// WindowCharting3.xaml 的交互逻辑
    /// </summary>
    public partial class WindowCharting3 : Window
    {
        ChartVisual chart = new ChartVisual();
        GenericCanvasContainer canvasContainer = new GenericCanvasContainer(true) { };
        DiscreteAxis axisX = new DateTimeAxis(AxisPosition.Buttom) { Name = "时间", ValueFormat = "yyyyMMdd", SplitValueFormat = "yyyy/MM", IsInterregional = true, ShowGridLine = false, IsGridLineClose = true };

        StraightLineSeriesVisual lineSeries = new StraightLineSeriesVisual() { Name = "概念关注度", LinePen = new Pen(Brushes.OrangeRed, 5) };
        StraightLineSeriesVisual lineSeries2 = new StraightLineSeriesVisual() { Name = "A股平均关注度", LinePen = new Pen(Brushes.Red, 1) };

        public WindowCharting3()
        {
            Background = new SolidColorBrush() { Color = ColorHelper.StringToColor("#FF1C1D21") };
            var uri = $"/wallpaper_mikael_gustafsson.png";

            InitializeComponent();
            Grid grid = new Grid() { };

            axisX.IsInterregional = false;

            ContinuousAxis axisY = new ContinuousAxis(AxisPosition.Left) { ValueFormat = "G4", SplitValueFormat = "G4", ShowGridLine = true, AxisPen = new Pen(Brushes.Green, 1), Unit = "万" };
            //axisY.Range = new Range() { Max = new Value<double>(5000000), Min = new Value<double>(40000) };
            chart.Offsets = new PaddingOffset(60, 20, 10, 20);

            canvasContainer.DrawingCanvas.AddChild(chart);
            canvasContainer.DrawingCanvas.DataSource = chart.DataSource;
            AxisInteractionCanvas interaction = new AxisInteractionCanvas();

            RectInteractionGroup container = new RectInteractionGroup(interaction, 1, 1, canvasContainer);
            //container.Background = Brushes.GreenYellow;
            chart.AddAsixY(axisY);
            chart.AddAsixX(axisX);

            chart.AddSeries(lineSeries);
            chart.AddSeries(lineSeries2);

            grid.Children.Add(container);
            canvasContainer.DrawingCanvas.Visuals.Add(CreateDrawingImage());

            BlurryUserControl b = new BlurryUserControl() { Background = new SolidColorBrush(ColorHelper.StringToColor("#BE323337")).OfStrength(0.2d) };
            b.BlurContainer = canvasContainer;
            b.Magnification = 0.25;
            b.BlurRadius = 30;

            interaction.Tip.Layers.Children.Insert(0, b);
            interaction.Tip.TextContainer.Margin = new Thickness(10);
            interaction.Tip.FontSize = 11;
            interaction.Tip.Border.BorderThickness = new Thickness(1);
            interaction.Tip.Border.Padding = new Thickness(0);
            interaction.Tip.Border.BorderBrush = Brushes.Black;
            interaction.Tip.Foreground = Brushes.White;
            //interaction.Tip.Border.BorderBrush = Brushes.White;
            //interaction.Tip.Border.BorderThickness = new Thickness(2);
            Content = grid;
            IsVisibleChanged += WindowCharting3_IsVisibleChanged;
            SizeChanged += WindowCharting3_SizeChanged;
            //StartDataFeed();
            //Timer timer = new Timer(200);
            //timer.Elapsed += Timer_Elapsed;
            //timer.Start();
        }

        private void WindowCharting3_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            canvasContainer.Replot();
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
            if ((bool)e.NewValue)
            {
                StartDataFeed();
            }
        }

        private async Task Render(string blockId, string marketId)
        {
            var dic = await Request(blockId);
            var dic2 = await Request(marketId, true);

            var group = dic.GroupBy(it => it.Key.ToString("yyyyMM")).Select(it => it.ElementAt(0).Key).ToList();
            group.AddRange(dic2.GroupBy(it => it.Key.ToString("yyyyMM")).Select(it => it.ElementAt(0).Key).ToList());
            group = group.Distinct().OrderBy(it => it).ToList();
            axisX.SplitValues = group.Select(it => it.ToFormatVisualData()).ToList();

            lineSeries2.VisualData = dic2.ToFormatVisualData();
            lineSeries.VisualData = dic.ToFormatVisualData();
            canvasContainer.Replot();
        }
        public async void StartDataFeed()
        {
            await Render("300843", "000001");
            //await Task.Delay(6000);
            //await Render("300843", "000001");
        }
        bool isFirst = true;
        public async Task<Dictionary<DateTime, double>> Request(string blockId, bool isMarket = false)
        {
            var str = $"http://zx.10jqka.com.cn/hotevent/api/getselfstocknum?blockid={blockId}";
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(str);
                var content = await response.Content.ReadAsStringAsync();
                Dictionary<DateTime, double> dic;
#if DEBUG
                if (isFirst && blockId == "300843")
                {
                    content = @"{""errorcode"":0,""errormsg"":"""",""result"":{""20191107"":{""market"":1220922,""300843"":""""},""20191108"":{""market"":1395440,""300843"":1559686},""20191111"":{""market"":1095570,""300843"":973335},""20191112"":{""market"":1853666,""300843"":1615285},""20191113"":{""market"":1831204,""300843"":1893740},""20191114"":{""market"":1894441,""300843"":3001139},""20191115"":{""market"":1881099,""300843"":2168200},""20191118"":{""market"":2473021,""300843"":2857082},""20191119"":{""market"":2840414,""300843"":3371114},""20191120"":{""market"":2545418,""300843"":2956473},""20191121"":{""market"":2078763,""300843"":2394988},""20191122"":{""market"":2152260,""300843"":2972098},""20191125"":{""market"":2045025,""300843"":2212477},""20191126"":{""market"":725091,""300843"":810178},""20191127"":{""market"":2325209,""300843"":3306730},""20191128"":{""market"":2299273,""300843"":2303376},""20191129"":{""market"":2569900,""300843"":2676095},""20191202"":{""market"":1816521,""300843"":2136884},""20191203"":{""market"":2169489,""300843"":3055999},""20191204"":{""market"":1401311,""300843"":1502723},""20191205"":{""market"":2058801,""300843"":2784810},""20191206"":{""market"":1628176,""300843"":1942092},""20191209"":{""market"":2048284,""300843"":2281765},""20191210"":{""market"":2049793,""300843"":2153557},""20191211"":{""market"":2353469,""300843"":2868384},""20191212"":{""market"":2451059,""300843"":3622954},""20191213"":{""market"":1978221,""300843"":2381510},""20191216"":{""market"":2072249,""300843"":2680473},""20191217"":{""market"":2490681,""300843"":3240636},""20191218"":{""market"":1663759,""300843"":2000437},""20191219"":{""market"":2402602,""300843"":2943444},""20191220"":{""market"":2609730,""300843"":2479694},""20191223"":{""market"":2993228,""300843"":3421838},""20191224"":{""market"":2481527,""300843"":2555660},""20191225"":{""market"":2824943,""300843"":3015789},""20191226"":{""market"":3473285,""300843"":3698115},""20191227"":{""market"":2605087,""300843"":2375348},""20191230"":{""market"":2924813,""300843"":2779798},""20191231"":{""market"":1961475,""300843"":1956245},""20200102"":{""market"":2270549,""300843"":2545689},""20200103"":{""market"":2264138,""300843"":3373872},""20200106"":{""market"":1805448,""300843"":2462621},""20200107"":{""market"":2106266,""300843"":2390839},""20200108"":{""market"":2417253,""300843"":2527448},""20200109"":{""market"":1955393,""300843"":2249123},""20200110"":{""market"":2059266,""300843"":2438323},""20200113"":{""market"":1942617,""300843"":2764259},""20200114"":{""market"":2358335,""300843"":3176796},""20200115"":{""market"":2462698,""300843"":2618859},""20200116"":{""market"":1534428,""300843"":1755601},""20200117"":{""market"":2568594,""300843"":3525349},""20200120"":{""market"":1865553,""300843"":2510250},""20200121"":{""market"":1415054,""300843"":1797661},""20200122"":{""market"":2325162,""300843"":3168782},""20200123"":{""market"":2719654,""300843"":3632904},""20200203"":{""market"":1922227,""300843"":2210350},""20200204"":{""market"":2225673,""300843"":2252039},""20200205"":{""market"":1822513,""300843"":2045597},""20200206"":{""market"":1779576,""300843"":1903632},""20200207"":{""market"":1798294,""300843"":2086841}}}";
                    isFirst = false;
                }
#endif
                Console.WriteLine($"Request {blockId}");

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
