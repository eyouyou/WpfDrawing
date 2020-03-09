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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfDrawing.Sample
{
    /// <summary>
    /// MultiChartings2.xaml 的交互逻辑
    /// </summary>
    public partial class MultiChartings2 : Window
    {
        RectInteractionGroup container;
        public MultiChartings2()
        {
            InitializeComponent();

            container = new RectInteractionGroup(null, 2, 2
                , new ChartItem() { /*Background = Brushes.LightPink*/ /*Background = Brushes.LightGreen */ }
                , new ChartItem2() {/*Background = Brushes.LightPink*//*Background = Brushes.LightPink  */}
                , new Strength() { /*Background = Brushes.LightPink*/ /*Background = Brushes.LightSalmon*/ }
                , new ChartItem() { /*Background = Brushes.LightPink*/ /*Background = Brushes.LightGreen */}
                );

            Content = container;

            SizeChanged += MultiChartings_SizeChanged;
        }
        const int minHeight = 180;
        const double width_height_ratio = 1.6;
        const int default_min_width = 230;

        private void MultiChartings_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var act_wid = e.NewSize.Width;
            var act_hei = e.NewSize.Height;

            var row_count = (int)act_hei / minHeight;
            if (row_count == 0)
            {
                row_count = 1;
            }
            var min_wid = act_hei / row_count * width_height_ratio;
            if (min_wid < default_min_width)
            {
                min_wid = default_min_width;
            }
            var total_count = (int)Math.Min((int)act_wid / min_wid * row_count, container.Containers.Count);
            if (total_count == 0)
            {
                total_count = 1;
            }
            var col_count = total_count / row_count + total_count % row_count > 0 ? 1 : 0;
            if (col_count == 0)
            {
                col_count = 1;
            }
            if (col_count * row_count > total_count)
            {
                var extra = col_count * row_count - total_count;
                for (int i = 0; i < extra / col_count; i++)
                {
                    row_count--;
                }
            }
            container.Resize(col_count, row_count);
            container.Replot();
        }


        public class ChartItem : RectDrawingCanvasContainer
        {
            ChartVisual chart = new ChartVisual();
            DiscreteAxis axisX = new DateTimeAxis(AxisPosition.Buttom) { Name = "时间", ValueFormat = "yyyyMMdd", SplitValueFormat = "yyyy/MM", IsInterregional = true, ShowGridLine = false, IsGridLineClose = true };

            StraightLineSeriesVisual lineSeries = new StraightLineSeriesVisual() { Name = "概念关注度", LinePen = new Pen(Brushes.SpringGreen, 1) };
            StraightLineSeriesVisual lineSeries2 = new StraightLineSeriesVisual() { Name = "A股平均关注度", LinePen = new Pen(Brushes.Red, 1) };

            ContinuousAxis axisY = new ContinuousAxis(AxisPosition.Left) { ValueFormat = "G4", SplitValueFormat = "G4", ShowGridLine = true, AxisPen = new Pen(Brushes.Green, 1), Unit = "万" };

            RectDrawingCanvas rectDrawingCanvas = new RectDrawingCanvas();

            TextBlock text1 = new TextBlock();
            TextBlock text2 = new TextBlock();

            AxisInteractionCanvas interaction = new AxisInteractionCanvas();
            public ChartItem()
            {
                BlurryUserControl b = new BlurryUserControl() { Background = new SolidColorBrush(ColorHelper.StringToColor("#BE323337")).OfStrength(0.2d) };
                b.BlurContainer = rectDrawingCanvas;
                b.Magnification = 0.15;
                b.BlurRadius = 25;

                interaction.Tip.TextContainer.Margin = new Thickness(10);
                interaction.Tip.Layers.Children.Insert(0, b);
                interaction.Tip.FontSize = 11;
                interaction.Tip.Border.BorderThickness = new Thickness(1);
                interaction.Tip.Border.Padding = new Thickness(0);
                interaction.Tip.Border.BorderBrush = Brushes.Black;
                interaction.Tip.Foreground = Brushes.White;

                BorderThickness = new Thickness(1);
                BorderBrush = Brushes.Black;

                chart.IntersectChanged += Chart_IntersectChanged;

                chart.Offsets = new PaddingOffset(20);

                axisX.IsInterregional = false;

                chart.AddAsixY(axisY);
                chart.AddAsixX(axisX);

                chart.AddSeries(lineSeries);
                chart.AddSeries(lineSeries2);

                rectDrawingCanvas.AddChild(chart);

                DrawingCanvas.DataSource = chart.DataSource;

                IsVisibleChanged += ChartItem_IsVisibleChanged;

                DockPanel dock = new DockPanel();
                DockPanel title = new DockPanel();
                title.VerticalAlignment = VerticalAlignment.Center;
                title.AddChild(new TextBlock() { Height = 20, Text = "MultiCharting" }, Dock.Left);
                title.AddChild(text1, Dock.Left);
                text2.Margin = new Thickness(10, 0, 0, 0);
                title.AddChild(text2, Dock.Right);
                dock.AddChild(title, Dock.Top);
                dock.AddChild(DrawingCanvasArea, Dock.Top);

                Content = dock;


            }

            private void Chart_IntersectChanged(Dictionary<string, SeriesData> data)
            {
                if (data.ContainsKey("概念关注度") && data["概念关注度"].YValue.ValueData("") is Value<double> currentValue
                    && !currentValue.IsBad)
                {
                    text1.Text = currentValue.Data.ToString("G4");
                }
                else
                {
                    text1.Text = "--";
                }

                if (data.ContainsKey("A股平均关注度") && data["A股平均关注度"].YValue.ValueData("") is Value<double> currentValue2
                    && !currentValue2.IsBad)
                {
                    text2.Text = currentValue2.Data.ToString("G4");
                }
                else
                {
                    text2.Text = "--";
                }

            }

            public override RectDrawingCanvas DrawingCanvas => rectDrawingCanvas;

            public override InteractionCanvas InteractionCanvas => interaction;

            private void ChartItem_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                if ((bool)e.NewValue)
                {
                    StartDataFeed();
                }
            }
            public async void StartDataFeed()
            {
                await Render("300843", "000001");
            }
            private async Task Render(string blockId, string marketId)
            {
                var dic = await Request(blockId);
                var dic2 = await Request(marketId, true);

                var all_x = dic.ToList();
                all_x.AddRange(dic2.ToList());
                var a = all_x.GroupBy(it => it.Key.ToString("yyyyMM")).Select(it => it.ElementAt(0).Key).Distinct().OrderBy(it => it).ToList();
                axisX.SplitValues = a.Select(it => it.ToFormatVisualData()).ToList();
                //axisX.Ratios = new List<double>() { 0.1, 0.1, 0.1 };

                lineSeries2.VisualData = dic2.ToFormatVisualData();
                lineSeries.VisualData = dic.ToFormatVisualData();
                DrawingCanvas.Replot();
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
        public class Strength : RectDrawingCanvasContainer
        {
            ChartVisual chart = new ChartVisual();
            DiscreteAxis axisX = new DateTimeAxis(AxisPosition.Buttom) { Name = "时间", ValueFormat = "t", SplitValueFormat = "t", IsInterregional = true, ShowGridLine = false, IsGridLineClose = true };

            LineSeriesVisual lineSeries = new StraightLineSeriesVisual() { Name = "涨停" };
            StraightLineSeriesVisual lineSeries2 = new StraightLineSeriesVisual() { Name = "跌停", LinePen = new Pen(Brushes.Red, 1) };
            StraightLineSeriesVisual lineSeries3 = new StraightLineSeriesVisual() { Name = "一字涨停", LinePen = new Pen(Brushes.Black, 1) };
            StraightLineSeriesVisual lineSeries4 = new StraightLineSeriesVisual() { Name = "非一字涨停", LinePen = new Pen(Brushes.AliceBlue, 1) };

            ContinuousAxis axisY = new ContinuousAxis(AxisPosition.Left) { ShowGridLine = true, AxisPen = new Pen(Brushes.Green, 1), SplitValueFormat = "F2" };

            RectDrawingCanvas rectDrawingCanvas = new RectDrawingCanvas();

            TextBlock text1 = new TextBlock();
            TextBlock text2 = new TextBlock();

            public Strength()
            {
                axisX.MinUnit = 600000;

                BorderThickness = new Thickness(1);
                BorderBrush = Brushes.Black;

                chart.IntersectChanged += Chart_IntersectChanged;

                chart.Offsets = new PaddingOffset(50, 20, 20, 20);

                axisX.IsInterregional = false;

                chart.AddAsixY(axisY);
                chart.AddAsixX(axisX);

                chart.AddSeries(lineSeries);
                chart.AddSeries(lineSeries2);
                chart.AddSeries(lineSeries3);
                chart.AddSeries(lineSeries4);

                rectDrawingCanvas.AddChild(chart);
                DrawingCanvas.DataSource = chart.DataSource;
                IsVisibleChanged += ChartItem_IsVisibleChanged;

                DockPanel dock = new DockPanel();
                DockPanel title = new DockPanel();
                title.VerticalAlignment = VerticalAlignment.Center;
                title.AddChild(new TextBlock() { Text = "强弱评级" }, Dock.Left);
                title.AddChild(text1, Dock.Left);
                text2.Margin = new Thickness(10, 0, 0, 0);
                title.AddChild(text2, Dock.Right);
                dock.AddChild(title, Dock.Top);
                dock.AddChild(DrawingCanvasArea, Dock.Top);
                Content = dock;
            }

            private void Chart_IntersectChanged(Dictionary<string, SeriesData> data)
            {
                if (data.ContainsKey("涨停") && data["涨停"].YValue.ValueData("") is Value<double> currentValue
                    && !currentValue.IsBad)
                {
                    text1.Text = currentValue.Data.ToString("G4");
                }
                else
                {
                    text1.Text = "--";
                }

                if (data.ContainsKey("跌停") && data["跌停"].YValue.ValueData("") is Value<double> currentValue2
                    && !currentValue2.IsBad)
                {
                    text2.Text = currentValue2.Data.ToString("G4");
                }
                else
                {
                    text2.Text = "--";
                }
            }

            public override RectDrawingCanvas DrawingCanvas => rectDrawingCanvas;

            private void ChartItem_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                if ((bool)e.NewValue)
                {
                    StartDataFeed();
                }
            }
            public async void StartDataFeed()
            {
                var datetime = await RequestTradeTime();
                var str_datetime = datetime.ToString("yyyyMMdd");

                var list = await RequestList(str_datetime);

                Render(list, datetime);
            }
            private void Render(List<StrengthItem> list, DateTime tradeDay)
            {
                tradeDay = DateTime.Now.Date;
                var day1 = tradeDay.AddHours(9).AddMinutes(30);
                var day2 = tradeDay.AddHours(10).AddMinutes(30);
                var day3 = tradeDay.AddHours(11).AddMinutes(30);

                var except_left = tradeDay.AddHours(11).AddMinutes(30);
                var except_right = tradeDay.AddHours(13);

                var day4 = tradeDay.AddHours(14);
                var day5 = tradeDay.AddHours(15);
                var axes = new List<DateTime> { day1, day2, day3, day4, day5 };

                axisX.ExceptSections = new List<Section>() { new Section() { Left = except_left.ToFormatVisualData(), Right = except_right.ToFormatVisualData() } };
                axisX.SplitValues = axes.Select(it => it.ToFormatVisualData()).ToList();
                axisX.Ratios = Tools.GetAverageRatiosWithZero(axes.Count - 1);

                var filterList = list.Where(it => it.zt != -1 && it.dt != -1).ToList();
                //filterList.ForEach(it => it.time = DateTime.Now.Date.AddHours(8));
                var value1 = filterList.Select(it => new CroodData<DateTime>(it.time, it.zt * 1.0));
                axisY.Range = new Range(0, value1.Select(it => it.YData).Max());
                lineSeries.VisualData = filterList.Select(it => new CroodData<DateTime>(it.time, it.zt * 1.0)).ToFormatVisualData();
                lineSeries2.VisualData = filterList.Select(it => new CroodData<DateTime>(it.time, it.dt * 1.0)).ToFormatVisualData();
                lineSeries3.VisualData = filterList.Select(it => new CroodData<DateTime>(it.time, it.yzzt * 1.0)).ToFormatVisualData();
                lineSeries4.VisualData = filterList.Select(it => new CroodData<DateTime>(it.time, it.fyzzt * 1.0)).ToFormatVisualData();

                rectDrawingCanvas.Replot();
            }
            //private async Task Render()
            //{
            //    var dic = await Request();

            //    //var group = dic.GroupBy(it => it.Key.ToString("yyyyMM")).Select(it => it.ElementAt(0).Key).ToList();
            //    //group.AddRange(dic2.GroupBy(it => it.Key.ToString("yyyyMM")).Select(it => it.ElementAt(0).Key).ToList());
            //    //group = group.Distinct().OrderBy(it => it).ToList();
            //    //axisX.SplitValues = group.Select(it => it.ToVisualData()).ToList();

            //    //lineSeries2.VisualData = dic2.ToVisualData();
            //    //lineSeries.VisualData = dic.ToVisualData();
            //    //Canvas.Replot();
            //}
            bool isFirst = true;
            public async Task<DateTime> RequestTradeTime()
            {
                var str = $"http://sp.10jqka.com.cn/api/tradeday/get/";
                using (HttpClient client = new HttpClient())
                {
                    var str_reply = await client.GetStringAsync(str);
                    var index = str_reply.IndexOf("\"result\":[\"");
                    if (index > 0)
                    {
                        var str_data = str_reply.Substring(index + 11, 8);
                        if (str_data.Length == 8)
                        {
                            return DateTime.ParseExact(str_data, "yyyyMMdd", CultureInfo.CurrentCulture);
                        }
                    }
                }
                return DateTime.Now;
            }
            class StrengthItem
            {
                public DateTime time { get; set; }
                public int zt { get; set; }
                public int dt { get; set; }
                public int fyzzt { get; set; }
                public int yzzt { get; set; }

                public override int GetHashCode()
                {
                    return time.GetHashCode();
                }
                public override bool Equals(object obj)
                {
                    if (obj is StrengthItem item && item.time == this.time)
                    {
                        return true;
                    }
                    return false;
                }
            }

            private async Task<List<StrengthItem>> RequestList(string str_data)
            {
                var items = new List<StrengthItem>();
                var str = $"http://hqstats.10jqka.com.cn/fyzzt.php?date={str_data}";
                using (HttpClient client = new HttpClient())
                {
                    await Task.Delay(200);
                    var response = await client.GetStringAsync(str);
                    //    var response = @"
                    //fyzzt({""update_time"":""202002201404"",""data"":{""09:30"":{""zt"":16,""dt"":2,""yzzt"":12,""fyzzt"":4},""09:31"":{""zt"":19,""dt"":1,""yzzt"":12,""fyzzt"":7},""09:32"":{""zt"":21,""dt"":1,""yzzt"":12,""fyzzt"":9},""09:33"":{""zt"":20,""dt"":2,""yzzt"":12,""fyzzt"":8},""09:34"":{""zt"":22,""dt"":2,""yzzt"":12,""fyzzt"":10},""09:35"":{""zt"":22,""dt"":2,""yzzt"":12,""fyzzt"":10},""09:36"":{""zt"":22,""dt"":2,""yzzt"":12,""fyzzt"":10},""09:37"":{""zt"":24,""dt"":2,""yzzt"":12,""fyzzt"":12},""09:38"":{""zt"":24,""dt"":2,""yzzt"":12,""fyzzt"":12},""09:39"":{""zt"":27,""dt"":2,""yzzt"":12,""fyzzt"":15},""09:40"":{""zt"":28,""dt"":2,""yzzt"":12,""fyzzt"":16},""09:41"":{""zt"":29,""dt"":2,""yzzt"":12,""fyzzt"":17},""09:42"":{""zt"":29,""dt"":2,""yzzt"":12,""fyzzt"":17},""09:43"":{""zt"":29,""dt"":2,""yzzt"":12,""fyzzt"":17},""09:44"":{""zt"":28,""dt"":2,""yzzt"":12,""fyzzt"":16},""09:45"":{""zt"":30,""dt"":2,""yzzt"":12,""fyzzt"":18},""09:46"":{""zt"":31,""dt"":3,""yzzt"":12,""fyzzt"":19},""09:47"":{""zt"":29,""dt"":3,""yzzt"":12,""fyzzt"":17},""09:48"":{""zt"":33,""dt"":2,""yzzt"":12,""fyzzt"":21},""09:49"":{""zt"":31,""dt"":2,""yzzt"":12,""fyzzt"":19},""09:50"":{""zt"":31,""dt"":2,""yzzt"":12,""fyzzt"":19},""09:51"":{""zt"":32,""dt"":2,""yzzt"":12,""fyzzt"":20},""09:52"":{""zt"":33,""dt"":2,""yzzt"":12,""fyzzt"":21},""09:53"":{""zt"":32,""dt"":2,""yzzt"":12,""fyzzt"":20},""09:54"":{""zt"":34,""dt"":2,""yzzt"":12,""fyzzt"":22},""09:55"":{""zt"":36,""dt"":3,""yzzt"":12,""fyzzt"":24},""09:56"":{""zt"":34,""dt"":3,""yzzt"":12,""fyzzt"":22},""09:57"":{""zt"":32,""dt"":3,""yzzt"":12,""fyzzt"":20},""09:58"":{""zt"":30,""dt"":3,""yzzt"":12,""fyzzt"":18},""09:59"":{""zt"":33,""dt"":3,""yzzt"":12,""fyzzt"":21},""10:00"":{""zt"":32,""dt"":3,""yzzt"":12,""fyzzt"":20},""10:01"":{""zt"":35,""dt"":2,""yzzt"":12,""fyzzt"":23},""10:02"":{""zt"":35,""dt"":1,""yzzt"":12,""fyzzt"":23},""10:03"":{""zt"":35,""dt"":1,""yzzt"":12,""fyzzt"":23},""10:04"":{""zt"":35,""dt"":2,""yzzt"":12,""fyzzt"":23},""10:05"":{""zt"":35,""dt"":2,""yzzt"":12,""fyzzt"":23},""10:06"":{""zt"":37,""dt"":1,""yzzt"":12,""fyzzt"":25},""10:07"":{""zt"":36,""dt"":2,""yzzt"":12,""fyzzt"":24},""10:08"":{""zt"":38,""dt"":2,""yzzt"":12,""fyzzt"":26},""10:09"":{""zt"":40,""dt"":2,""yzzt"":12,""fyzzt"":28},""10:10"":{""zt"":40,""dt"":2,""yzzt"":12,""fyzzt"":28},""10:11"":{""zt"":41,""dt"":1,""yzzt"":12,""fyzzt"":29},""10:12"":{""zt"":40,""dt"":1,""yzzt"":12,""fyzzt"":28},""10:13"":{""zt"":40,""dt"":2,""yzzt"":12,""fyzzt"":28},""10:14"":{""zt"":40,""dt"":1,""yzzt"":12,""fyzzt"":28},""10:15"":{""zt"":39,""dt"":2,""yzzt"":12,""fyzzt"":27},""10:16"":{""zt"":40,""dt"":2,""yzzt"":12,""fyzzt"":28},""10:17"":{""zt"":40,""dt"":2,""yzzt"":12,""fyzzt"":28},""10:18"":{""zt"":42,""dt"":2,""yzzt"":12,""fyzzt"":30},""10:19"":{""zt"":42,""dt"":2,""yzzt"":12,""fyzzt"":30},""10:20"":{""zt"":42,""dt"":2,""yzzt"":12,""fyzzt"":30},""10:21"":{""zt"":43,""dt"":2,""yzzt"":12,""fyzzt"":31},""10:22"":{""zt"":42,""dt"":2,""yzzt"":12,""fyzzt"":30},""10:23"":{""zt"":42,""dt"":2,""yzzt"":12,""fyzzt"":30},""10:24"":{""zt"":42,""dt"":3,""yzzt"":12,""fyzzt"":30},""10:25"":{""zt"":40,""dt"":3,""yzzt"":12,""fyzzt"":28},""10:26"":{""zt"":41,""dt"":3,""yzzt"":12,""fyzzt"":29},""10:27"":{""zt"":42,""dt"":3,""yzzt"":12,""fyzzt"":30},""10:28"":{""zt"":42,""dt"":3,""yzzt"":12,""fyzzt"":30},""10:29"":{""zt"":43,""dt"":3,""yzzt"":12,""fyzzt"":31},""10:30"":{""zt"":44,""dt"":2,""yzzt"":12,""fyzzt"":32},""10:31"":{""zt"":46,""dt"":2,""yzzt"":12,""fyzzt"":34},""10:32"":{""zt"":45,""dt"":2,""yzzt"":12,""fyzzt"":33},""10:33"":{""zt"":45,""dt"":2,""yzzt"":12,""fyzzt"":33},""10:34"":{""zt"":44,""dt"":2,""yzzt"":12,""fyzzt"":32},""10:35"":{""zt"":45,""dt"":2,""yzzt"":12,""fyzzt"":33},""10:36"":{""zt"":45,""dt"":2,""yzzt"":12,""fyzzt"":33},""10:37"":{""zt"":45,""dt"":2,""yzzt"":12,""fyzzt"":33},""10:38"":{""zt"":43,""dt"":2,""yzzt"":12,""fyzzt"":31},""10:39"":{""zt"":44,""dt"":2,""yzzt"":12,""fyzzt"":32},""10:40"":{""zt"":44,""dt"":2,""yzzt"":12,""fyzzt"":32},""10:41"":{""zt"":44,""dt"":2,""yzzt"":12,""fyzzt"":32},""10:42"":{""zt"":44,""dt"":3,""yzzt"":12,""fyzzt"":32},""10:43"":{""zt"":44,""dt"":3,""yzzt"":12,""fyzzt"":32},""10:44"":{""zt"":45,""dt"":3,""yzzt"":12,""fyzzt"":33},""10:45"":{""zt"":45,""dt"":2,""yzzt"":12,""fyzzt"":33},""10:46"":{""zt"":45,""dt"":2,""yzzt"":12,""fyzzt"":33},""10:47"":{""zt"":45,""dt"":2,""yzzt"":12,""fyzzt"":33},""10:48"":{""zt"":45,""dt"":2,""yzzt"":12,""fyzzt"":33},""10:49"":{""zt"":45,""dt"":2,""yzzt"":12,""fyzzt"":33},""10:50"":{""zt"":46,""dt"":2,""yzzt"":12,""fyzzt"":34},""10:51"":{""zt"":45,""dt"":2,""yzzt"":12,""fyzzt"":33},""10:52"":{""zt"":45,""dt"":2,""yzzt"":12,""fyzzt"":33},""10:53"":{""zt"":45,""dt"":2,""yzzt"":12,""fyzzt"":33},""10:54"":{""zt"":45,""dt"":2,""yzzt"":12,""fyzzt"":33},""10:55"":{""zt"":45,""dt"":2,""yzzt"":12,""fyzzt"":33},""10:56"":{""zt"":46,""dt"":2,""yzzt"":12,""fyzzt"":34},""10:57"":{""zt"":46,""dt"":2,""yzzt"":12,""fyzzt"":34},""10:58"":{""zt"":46,""dt"":2,""yzzt"":12,""fyzzt"":34},""10:59"":{""zt"":46,""dt"":2,""yzzt"":12,""fyzzt"":34},""11:00"":{""zt"":46,""dt"":2,""yzzt"":12,""fyzzt"":34},""11:01"":{""zt"":47,""dt"":1,""yzzt"":12,""fyzzt"":35},""11:02"":{""zt"":48,""dt"":1,""yzzt"":12,""fyzzt"":36},""11:03"":{""zt"":49,""dt"":1,""yzzt"":12,""fyzzt"":37},""11:04"":{""zt"":48,""dt"":1,""yzzt"":12,""fyzzt"":36},""11:05"":{""zt"":48,""dt"":1,""yzzt"":12,""fyzzt"":36},""11:06"":{""zt"":48,""dt"":1,""yzzt"":12,""fyzzt"":36},""11:07"":{""zt"":48,""dt"":1,""yzzt"":12,""fyzzt"":36},""11:08"":{""zt"":48,""dt"":1,""yzzt"":12,""fyzzt"":36},""11:09"":{""zt"":49,""dt"":1,""yzzt"":12,""fyzzt"":37},""11:10"":{""zt"":50,""dt"":1,""yzzt"":12,""fyzzt"":38},""11:11"":{""zt"":51,""dt"":1,""yzzt"":12,""fyzzt"":39},""11:12"":{""zt"":52,""dt"":1,""yzzt"":12,""fyzzt"":40},""11:13"":{""zt"":52,""dt"":1,""yzzt"":12,""fyzzt"":40},""11:14"":{""zt"":52,""dt"":1,""yzzt"":12,""fyzzt"":40},""11:15"":{""zt"":51,""dt"":1,""yzzt"":12,""fyzzt"":39},""11:16"":{""zt"":52,""dt"":1,""yzzt"":12,""fyzzt"":40},""11:17"":{""zt"":54,""dt"":1,""yzzt"":12,""fyzzt"":42},""11:18"":{""zt"":54,""dt"":1,""yzzt"":12,""fyzzt"":42},""11:19"":{""zt"":55,""dt"":1,""yzzt"":12,""fyzzt"":43},""11:20"":{""zt"":57,""dt"":1,""yzzt"":12,""fyzzt"":45},""11:21"":{""zt"":58,""dt"":1,""yzzt"":12,""fyzzt"":46},""11:22"":{""zt"":59,""dt"":1,""yzzt"":12,""fyzzt"":47},""11:23"":{""zt"":59,""dt"":1,""yzzt"":12,""fyzzt"":47},""11:24"":{""zt"":60,""dt"":1,""yzzt"":12,""fyzzt"":48},""11:25"":{""zt"":62,""dt"":1,""yzzt"":12,""fyzzt"":50},""11:26"":{""zt"":64,""dt"":1,""yzzt"":12,""fyzzt"":52},""11:27"":{""zt"":64,""dt"":1,""yzzt"":12,""fyzzt"":52},""11:28"":{""zt"":64,""dt"":1,""yzzt"":12,""fyzzt"":52},""11:29"":{""zt"":67,""dt"":1,""yzzt"":12,""fyzzt"":55},""11:30"":{""zt"":66,""dt"":1,""yzzt"":12,""fyzzt"":54},""13:00"":{""zt"":74,""dt"":1,""yzzt"":12,""fyzzt"":62},""13:01"":{""zt"":76,""dt"":1,""yzzt"":12,""fyzzt"":64},""13:02"":{""zt"":74,""dt"":1,""yzzt"":12,""fyzzt"":62},""13:03"":{""zt"":75,""dt"":1,""yzzt"":12,""fyzzt"":63},""13:04"":{""zt"":76,""dt"":1,""yzzt"":12,""fyzzt"":64},""13:05"":{""zt"":75,""dt"":1,""yzzt"":12,""fyzzt"":63},""13:06"":{""zt"":75,""dt"":1,""yzzt"":12,""fyzzt"":63},""13:07"":{""zt"":74,""dt"":1,""yzzt"":12,""fyzzt"":62},""13:08"":{""zt"":77,""dt"":1,""yzzt"":12,""fyzzt"":65},""13:09"":{""zt"":77,""dt"":1,""yzzt"":12,""fyzzt"":65},""13:10"":{""zt"":78,""dt"":1,""yzzt"":12,""fyzzt"":66},""13:11"":{""zt"":77,""dt"":1,""yzzt"":12,""fyzzt"":65},""13:12"":{""zt"":77,""dt"":1,""yzzt"":12,""fyzzt"":65},""13:13"":{""zt"":76,""dt"":1,""yzzt"":12,""fyzzt"":64},""13:14"":{""zt"":76,""dt"":1,""yzzt"":12,""fyzzt"":64},""13:15"":{""zt"":76,""dt"":1,""yzzt"":12,""fyzzt"":64},""13:16"":{""zt"":76,""dt"":1,""yzzt"":12,""fyzzt"":64},""13:17"":{""zt"":77,""dt"":1,""yzzt"":12,""fyzzt"":65},""13:18"":{""zt"":77,""dt"":1,""yzzt"":12,""fyzzt"":65},""13:19"":{""zt"":77,""dt"":1,""yzzt"":12,""fyzzt"":65},""13:20"":{""zt"":80,""dt"":1,""yzzt"":12,""fyzzt"":68},""13:21"":{""zt"":79,""dt"":1,""yzzt"":12,""fyzzt"":67},""13:22"":{""zt"":79,""dt"":1,""yzzt"":12,""fyzzt"":67},""13:23"":{""zt"":79,""dt"":1,""yzzt"":12,""fyzzt"":67},""13:24"":{""zt"":79,""dt"":1,""yzzt"":12,""fyzzt"":67},""13:25"":{""zt"":80,""dt"":1,""yzzt"":12,""fyzzt"":68},""13:26"":{""zt"":80,""dt"":1,""yzzt"":12,""fyzzt"":68},""13:27"":{""zt"":79,""dt"":1,""yzzt"":12,""fyzzt"":67},""13:28"":{""zt"":81,""dt"":1,""yzzt"":12,""fyzzt"":69},""13:29"":{""zt"":80,""dt"":1,""yzzt"":12,""fyzzt"":68},""13:30"":{""zt"":80,""dt"":1,""yzzt"":12,""fyzzt"":68},""13:31"":{""zt"":80,""dt"":1,""yzzt"":12,""fyzzt"":68},""13:32"":{""zt"":81,""dt"":1,""yzzt"":12,""fyzzt"":69},""13:33"":{""zt"":79,""dt"":1,""yzzt"":12,""fyzzt"":67},""13:34"":{""zt"":80,""dt"":1,""yzzt"":12,""fyzzt"":68},""13:35"":{""zt"":82,""dt"":1,""yzzt"":12,""fyzzt"":70},""13:36"":{""zt"":82,""dt"":1,""yzzt"":12,""fyzzt"":70},""13:37"":{""zt"":84,""dt"":1,""yzzt"":12,""fyzzt"":72},""13:38"":{""zt"":86,""dt"":1,""yzzt"":12,""fyzzt"":74},""13:39"":{""zt"":86,""dt"":1,""yzzt"":12,""fyzzt"":74},""13:40"":{""zt"":86,""dt"":1,""yzzt"":12,""fyzzt"":74},""13:41"":{""zt"":85,""dt"":1,""yzzt"":12,""fyzzt"":73},""13:42"":{""zt"":87,""dt"":1,""yzzt"":12,""fyzzt"":75},""13:43"":{""zt"":88,""dt"":1,""yzzt"":12,""fyzzt"":76},""13:44"":{""zt"":88,""dt"":1,""yzzt"":12,""fyzzt"":76},""13:45"":{""zt"":86,""dt"":1,""yzzt"":12,""fyzzt"":74},""13:46"":{""zt"":88,""dt"":1,""yzzt"":12,""fyzzt"":76},""13:47"":{""zt"":90,""dt"":1,""yzzt"":12,""fyzzt"":78},""13:48"":{""zt"":91,""dt"":1,""yzzt"":12,""fyzzt"":79},""13:49"":{""zt"":91,""dt"":1,""yzzt"":12,""fyzzt"":79},""13:50"":{""zt"":91,""dt"":1,""yzzt"":12,""fyzzt"":79},""13:51"":{""zt"":90,""dt"":1,""yzzt"":12,""fyzzt"":78},""13:52"":{""zt"":90,""dt"":1,""yzzt"":12,""fyzzt"":78},""13:53"":{""zt"":91,""dt"":1,""yzzt"":12,""fyzzt"":79},""13:54"":{""zt"":92,""dt"":1,""yzzt"":12,""fyzzt"":80},""13:55"":{""zt"":93,""dt"":1,""yzzt"":12,""fyzzt"":81},""13:56"":{""zt"":94,""dt"":1,""yzzt"":12,""fyzzt"":82},""13:57"":{""zt"":94,""dt"":1,""yzzt"":12,""fyzzt"":82},""13:58"":{""zt"":94,""dt"":1,""yzzt"":12,""fyzzt"":82},""13:59"":{""zt"":93,""dt"":1,""yzzt"":12,""fyzzt"":81},""14:00"":{""zt"":92,""dt"":1,""yzzt"":12,""fyzzt"":80},""14:01"":{""zt"":95,""dt"":1,""yzzt"":12,""fyzzt"":83},""14:02"":{""zt"":95,""dt"":1,""yzzt"":12,""fyzzt"":83},""14:03"":{""zt"":96,""dt"":1,""yzzt"":12,""fyzzt"":84},""14:04"":{""zt"":94,""dt"":1,""yzzt"":12,""fyzzt"":82}},""stocklist"":{""09:30"":{""zt"":[600416,600555,603290,603893,""002125"",""002255"",""002359"",""002455"",""002458"",""002516"",""002805"",300169,300220,300769,300817,300820],""dt"":[600083,300155],""yzzt"":[600416,600555,603290,603893,""002125"",""002255"",""002455"",""002516"",300169,300769,300817,300820],""fyzzt"":[""002359"",""002458"",""002805"",300220]},""10:00"":{""zt"":[600221,600237,600365,600416,600555,603011,603189,603200,603290,603533,603838,603893,""000616"",""000953"",""002028"",""002125"",""002255"",""002359"",""002455"",""002516"",""002571"",""002639"",""002805"",""002823"",""002905"",300029,300048,300102,300169,300769,300817,300820],""dt"":[600083,300155,300805],""yzzt"":[600416,600555,603290,603893,""002125"",""002255"",""002455"",""002516"",300169,300769,300817,300820],""fyzzt"":[600221,600237,600365,603011,603189,603200,603533,603838,""000616"",""000953"",""002028"",""002359"",""002571"",""002639"",""002805"",""002823"",""002905"",300029,300048,300102]},""11:00"":{""zt"":[600221,600237,600365,600367,600416,600476,600509,600555,600774,603011,603189,603200,603290,603528,603533,603838,603893,""000585"",""000616"",""000953"",""000993"",""002028"",""002125"",""002160"",""002255"",""002359"",""002418"",""002455"",""002481"",""002516"",""002571"",""002639"",""002805"",""002824"",""002837"",""002865"",""002905"",300029,300032,300048,300102,300169,300706,300769,300817,300820],""dt"":[600083,300805],""yzzt"":[600416,600555,603290,603893,""002125"",""002255"",""002455"",""002516"",300169,300769,300817,300820],""fyzzt"":[600221,600237,600365,600367,600476,600509,600774,603011,603189,603200,603528,603533,603838,""000585"",""000616"",""000953"",""000993"",""002028"",""002160"",""002359"",""002418"",""002481"",""002571"",""002639"",""002805"",""002824"",""002837"",""002865"",""002905"",300029,300032,300048,300102,300706]},""11:30"":{""zt"":[600221,600237,600318,600365,600367,600416,600476,600499,600509,600555,600621,600774,603011,603033,603105,603189,603200,603290,603528,603533,603838,603893,""000016"",""000585"",""000616"",""000796"",""000936"",""000953"",""000993"",""002028"",""002125"",""002160"",""002255"",""002359"",""002369"",""002418"",""002455"",""002481"",""002516"",""002571"",""002639"",""002656"",""002805"",""002823"",""002824"",""002837"",""002865"",""002905"",300029,300032,300048,300057,300102,300160,300169,300248,300317,300321,300493,300558,300706,300707,300769,300817,300820,600209],""dt"":[600083],""yzzt"":[600416,600555,603290,603893,""002125"",""002255"",""002455"",""002516"",300169,300769,300817,300820],""fyzzt"":[600221,600237,600318,600365,600367,600476,600499,600509,600621,600774,603011,603033,603105,603189,603200,603528,603533,603838,""000016"",""000585"",""000616"",""000796"",""000936"",""000953"",""000993"",""002028"",""002160"",""002359"",""002369"",""002418"",""002481"",""002571"",""002639"",""002656"",""002805"",""002823"",""002824"",""002837"",""002865"",""002905"",300029,300032,300048,300057,300102,300160,300248,300317,300321,300493,300558,300706,300707,600209]},""13:00"":{""zt"":[600110,600221,600237,600318,600365,600367,600416,600476,600499,600509,600555,600621,600774,603011,603033,603105,603189,603200,603290,603528,603533,603536,603838,603893,""000016"",""000585"",""000616"",""000723"",""000796"",""000936"",""000953"",""000993"",""002023"",""002028"",""002125"",""002160"",""002239"",""002255"",""002359"",""002369"",""002418"",""002455"",""002481"",""002516"",""002571"",""002639"",""002656"",""002805"",""002823"",""002824"",""002837"",""002865"",""002905"",300029,300032,300048,300057,300102,300160,300169,300248,300317,300321,300493,300558,300634,300641,300706,300707,300709,300769,300817,300820,600209],""dt"":[600083],""yzzt"":[600416,600555,603290,603893,""002125"",""002255"",""002455"",""002516"",300169,300769,300817,300820],""fyzzt"":[600110,600221,600237,600318,600365,600367,600476,600499,600509,600621,600774,603011,603033,603105,603189,603200,603528,603533,603536,603838,""000016"",""000585"",""000616"",""000723"",""000796"",""000936"",""000953"",""000993"",""002023"",""002028"",""002160"",""002239"",""002359"",""002369"",""002418"",""002481"",""002571"",""002639"",""002656"",""002805"",""002823"",""002824"",""002837"",""002865"",""002905"",300029,300032,300048,300057,300102,300160,300248,300317,300321,300493,300558,300634,300641,300706,300707,300709,600209]},""14:00"":{""zt"":[600088,600110,600221,600237,600318,600365,600367,600416,600476,600499,600509,600555,600621,600707,600774,603011,603033,603069,603093,603105,603158,603189,603200,603290,603528,603533,603536,603610,603838,603893,""000016"",""000100"",""000536"",""000585"",""000616"",""000936"",""000953"",""000993"",""002023"",""002028"",""002125"",""002160"",""002163"",""002239"",""002255"",""002359"",""002369"",""002418"",""002429"",""002455"",""002481"",""002516"",""002571"",""002639"",""002656"",""002740"",""002805"",""002823"",""002824"",""002837"",""002865"",""002905"",300029,300032,300033,300036,300048,300057,300059,300097,300102,300160,300162,300169,300220,300241,300248,300317,300321,300342,300427,300493,300613,300634,300706,300707,300769,300817,300820,600186,600209,600732],""dt"":[600083],""yzzt"":[600416,600555,603290,603893,""002125"",""002255"",""002455"",""002516"",300169,300769,300817,300820],""fyzzt"":[600088,600110,600221,600237,600318,600365,600367,600476,600499,600509,600621,600707,600774,603011,603033,603069,603093,603105,603158,603189,603200,603528,603533,603536,603610,603838,""000016"",""000100"",""000536"",""000585"",""000616"",""000936"",""000953"",""000993"",""002023"",""002028"",""002160"",""002163"",""002239"",""002359"",""002369"",""002418"",""002429"",""002481"",""002571"",""002639"",""002656"",""002740"",""002805"",""002823"",""002824"",""002837"",""002865"",""002905"",300029,300032,300033,300036,300048,300057,300059,300097,300102,300160,300162,300220,300241,300248,300317,300321,300342,300427,300493,300613,300634,300706,300707,600186,600209,600732]}}});";
                    if (string.IsNullOrEmpty(response) || response.IndexOf("fyzzt(") < 0)
                    {
                        return items;
                    }
                    try
                    {
                        var value = response.Replace("fyzzt(", "");
                        value = value.Replace("}}});", "}}}");
                        if (value != "{});")
                        {
                            JObject @object = JObject.Parse(value);
                            JToken jData = @object["data"];
                            foreach (var item in jData.Children())
                            {
                                if (item is JProperty jp)
                                {
                                    var s_item = new StrengthItem();
                                    var jName = jp.Name;
                                    var time = DateTime.ParseExact(jName, "HH:mm", CultureInfo.CurrentCulture); ;

                                    s_item.time = time;
                                    foreach (var child in item.Children().Children())
                                    {
                                        if (child is JProperty jp2)
                                        {
                                            var jName2 = jp2.Name;
                                            switch (jName2)
                                            {
                                                case "zt":
                                                    var str_zt = jp2.Value.ToString();
                                                    int zt = Convert.ToInt32(str_zt);
                                                    s_item.zt = zt;
                                                    break;
                                                case "dt":
                                                    var str_dt = jp2.Value.ToString();
                                                    int dt = Convert.ToInt32(str_dt);
                                                    s_item.dt = dt;
                                                    break;
                                                case "yzzt":
                                                    var str_yzzt = jp2.Value.ToString();
                                                    int yzzt = Convert.ToInt32(str_yzzt);
                                                    s_item.yzzt = yzzt;
                                                    break;
                                                case "fyzzt":
                                                    var str_fyzzt = jp2.Value.ToString();
                                                    int fyzzt = Convert.ToInt32(str_fyzzt);
                                                    s_item.yzzt = fyzzt;
                                                    break;
                                                default:
                                                    break;
                                            }
                                            items.Add(s_item);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    return items;
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
            AxisInteractionCanvas interaction = new AxisInteractionCanvas();
            public override InteractionCanvas InteractionCanvas => interaction;

        }
        public class ChartItem2 : RectDrawingCanvasContainer
        {
            ChartVisual chart = new ChartVisual();
            DiscreteAxis axisX = new DateTimeAxis(AxisPosition.Buttom) { Name = "时间", ValueFormat = "yyyyMMdd", SplitValueFormat = "yyyy/MM", IsInterregional = true, ShowGridLine = false, IsGridLineClose = true };

            StraightLineSeriesVisual lineSeries = new StraightLineSeriesVisual() { Name = "概念关注度", LinePen = new Pen(Brushes.SpringGreen, 1) };
            BarSeriesVisual barSeries = new BarSeriesVisual() { Name = "A股平均关注度" };

            ContinuousAxis axisY = new ContinuousAxis(AxisPosition.Left) { ValueFormat = "G4", SplitValueFormat = "G4", ShowGridLine = true, AxisPen = new Pen(Brushes.Green, 1), Unit = "万" };

            RectDrawingCanvas rectDrawingCanvas = new RectDrawingCanvas();

            TextBlock text1 = new TextBlock();
            TextBlock text2 = new TextBlock();

            AxisInteractionCanvas interaction = new AxisInteractionCanvas();

            public ChartItem2()
            {
                //axisX.MinUnit = 60000;
                //lineSeries2.RangeCalculator = data =>
                //{
                //    var range = data.YContextData.Range;
                //    range.Max.Data *= 1.2;
                //    return range;
                //};
                BlurryUserControl b = new BlurryUserControl() { Background = new SolidColorBrush(ColorHelper.StringToColor("#BE323337")).OfStrength(0.2d) };
                b.BlurContainer = DrawingCanvas;
                b.Magnification = 0.15;
                b.BlurRadius = 25;

                interaction.Tip.TextContainer.Margin = new Thickness(10);
                interaction.Tip.Layers.Children.Insert(0, b);
                interaction.Tip.FontSize = 11;
                interaction.Tip.Border.BorderThickness = new Thickness(1);
                interaction.Tip.Border.Padding = new Thickness(0);
                interaction.Tip.Border.BorderBrush = Brushes.Black;
                interaction.Tip.Foreground = Brushes.White;

                BorderThickness = new Thickness(1);
                BorderBrush = Brushes.Black;

                chart.IntersectChanged += Chart_IntersectChanged;

                chart.Offsets = new PaddingOffset(20);

                axisX.IsInterregional = false;

                chart.AddAsixY(axisY);
                chart.AddAsixX(axisX);

                chart.AddSeries(lineSeries);
                chart.AddSeries(barSeries);
                barSeries.BarWidth = new GridLength(1, GridUnitType.Star);

                rectDrawingCanvas.AddChild(chart);

                DrawingCanvas.DataSource = chart.DataSource;

                IsVisibleChanged += ChartItem_IsVisibleChanged;

                DockPanel dock = new DockPanel();
                DockPanel title = new DockPanel();
                title.VerticalAlignment = VerticalAlignment.Center;
                title.AddChild(new TextBlock() { Height = 20, Text = "MultiCharting" }, Dock.Left);
                title.AddChild(text1, Dock.Left);
                text2.Margin = new Thickness(10, 0, 0, 0);
                title.AddChild(text2, Dock.Right);
                dock.AddChild(title, Dock.Top);
                dock.AddChild(DrawingCanvasArea, Dock.Top);

                Content = dock;
            }

            private void Chart_IntersectChanged(Dictionary<string, SeriesData> data)
            {
                if (data.ContainsKey("概念关注度") && data["概念关注度"].YValue.ValueData("") is Value<double> currentValue
                    && !currentValue.IsBad)
                {
                    text1.Text = currentValue.Data.ToString("G4");
                }
                else
                {
                    text1.Text = "--";
                }

                if (data.ContainsKey("A股平均关注度") && data["A股平均关注度"].YValue.ValueData("") is Value<double> currentValue2
                    && !currentValue2.IsBad)
                {
                    text2.Text = currentValue2.Data.ToString("G4");
                }
                else
                {
                    text2.Text = "--";
                }
            }

            public override RectDrawingCanvas DrawingCanvas => rectDrawingCanvas;

            public override InteractionCanvas InteractionCanvas => interaction;

            private void ChartItem_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                if ((bool)e.NewValue)
                {
                    StartDataFeed();
                }
            }
            public async void StartDataFeed()
            {
                await Render("300843", "000001");
            }
            private async Task Render(string blockId, string marketId)
            {
                var dic = await Request(blockId);
                var dic2 = await Request(marketId, true);

                var group = dic.GroupBy(it => it.Key.ToString("yyyyMM")).Select(it => it.ElementAt(0).Key).ToList();
                group.AddRange(dic2.GroupBy(it => it.Key.ToString("yyyyMM")).Select(it => it.ElementAt(0).Key).ToList());
                group = group.Distinct().OrderBy(it => it).ToList();
                axisX.SplitValues = group.Select(it => it.ToFormatVisualData()).ToList();

                //dic = dic.Take(4).ToDictionary(it => it.Key, it => it.Value);
                barSeries.VisualData = dic.ToFormatVisualData();
                lineSeries.VisualData = dic.ToFormatVisualData();
                DrawingCanvas.Replot();
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
                        content = @"{""errorcode"":0,""errormsg"":"""",""result"":{""20191107"":{""market"":1220922,""300843"":""1559686""},""20191108"":{""market"":2095440,""300843"":3059686},""20191111"":{""market"":1095570,""300843"":973335},""20191112"":{""market"":1853666,""300843"":1615285},""20191113"":{""market"":1831204,""300843"":1893740},""20191114"":{""market"":1894441,""300843"":3001139},""20191115"":{""market"":1881099,""300843"":2168200},""20191118"":{""market"":2473021,""300843"":2857082},""20191119"":{""market"":2840414,""300843"":3371114},""20191120"":{""market"":2545418,""300843"":2956473},""20191121"":{""market"":2078763,""300843"":2394988},""20191122"":{""market"":2152260,""300843"":2972098},""20191125"":{""market"":2045025,""300843"":2212477},""20191126"":{""market"":725091,""300843"":810178},""20191127"":{""market"":2325209,""300843"":3306730},""20191128"":{""market"":2299273,""300843"":2303376},""20191129"":{""market"":2569900,""300843"":2676095},""20191202"":{""market"":1816521,""300843"":2136884},""20191203"":{""market"":2169489,""300843"":3055999},""20191204"":{""market"":1401311,""300843"":1502723},""20191205"":{""market"":2058801,""300843"":2784810},""20191206"":{""market"":1628176,""300843"":1942092},""20191209"":{""market"":2048284,""300843"":2281765},""20191210"":{""market"":2049793,""300843"":2153557},""20191211"":{""market"":2353469,""300843"":2868384},""20191212"":{""market"":2451059,""300843"":3622954},""20191213"":{""market"":1978221,""300843"":2381510},""20191216"":{""market"":2072249,""300843"":2680473},""20191217"":{""market"":2490681,""300843"":3240636},""20191218"":{""market"":1663759,""300843"":2000437},""20191219"":{""market"":2402602,""300843"":2943444},""20191220"":{""market"":2609730,""300843"":2479694},""20191223"":{""market"":2993228,""300843"":3421838},""20191224"":{""market"":2481527,""300843"":2555660},""20191225"":{""market"":2824943,""300843"":3015789},""20191226"":{""market"":3473285,""300843"":3698115},""20191227"":{""market"":2605087,""300843"":2375348},""20191230"":{""market"":2924813,""300843"":2779798},""20191231"":{""market"":1961475,""300843"":1956245},""20200102"":{""market"":2270549,""300843"":2545689},""20200103"":{""market"":2264138,""300843"":3373872},""20200106"":{""market"":1805448,""300843"":2462621},""20200107"":{""market"":2106266,""300843"":2390839},""20200108"":{""market"":2417253,""300843"":2527448},""20200109"":{""market"":1955393,""300843"":2249123},""20200110"":{""market"":2059266,""300843"":2438323},""20200113"":{""market"":1942617,""300843"":2764259},""20200114"":{""market"":2358335,""300843"":3176796},""20200115"":{""market"":2462698,""300843"":2618859},""20200116"":{""market"":1534428,""300843"":1755601},""20200117"":{""market"":2568594,""300843"":3525349},""20200120"":{""market"":1865553,""300843"":2510250},""20200121"":{""market"":1415054,""300843"":1797661},""20200122"":{""market"":2325162,""300843"":3168782},""20200123"":{""market"":2719654,""300843"":3632904},""20200203"":{""market"":1922227,""300843"":2210350},""20200204"":{""market"":2225673,""300843"":2252039},""20200205"":{""market"":1822513,""300843"":2045597},""20200206"":{""market"":1779576,""300843"":1903632},""20200207"":{""market"":1798294,""300843"":2086841}}}";
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
                        return dic.ToDictionary(it => it.Key, it => it.Value);
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
}
