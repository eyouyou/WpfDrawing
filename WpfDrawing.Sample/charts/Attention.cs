using HevoDrawing;
using HevoDrawing.Abstractions;
using HevoDrawing.Charting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace WpfDrawing.Sample
{
    public class AttentionInput : IBlockSettable
    {
        public string BlockId { get; set; }

    }
    public class Attention : ChartPackBase<AttentionInput>
    {
        TopicSeries TopicSeries = new TopicSeries();

        DiscreteAxis axisX = new DateTimeAxis(AxisPosition.Buttom) { Name = "时间", ValueFormat = "yyyyMMdd", SplitValueFormat = "yyyy/MM", IsInterregional = true, ShowGridLine = false, IsGridLineClose = true };

        StraightLineSeriesVisual lineSeries = new StraightLineSeriesVisual() { Name = "概念关注度", LinePen = new Pen(Brushes.SpringGreen, 1) };
        StraightLineSeriesVisual lineSeries2 = new StraightLineSeriesVisual() { Name = "A股平均关注度", LinePen = new Pen(Brushes.Red, 1) };

        ContinuousAxis axisY = new ContinuousAxis(AxisPosition.Left) { ValueFormat = "G4", SplitValueFormat = "G4", ShowGridLine = true, AxisPen = new Pen(Brushes.Green, 1), Unit = "万" };

        public Attention()
        {
            axisX.IsInterregional = false;

            ChartVisual.AddAsixY(axisY);
            ChartVisual.AddAsixX(axisX);

            ChartVisual.AddSeries(lineSeries);
            ChartVisual.AddSeries(lineSeries2);

            IsVisibleChanged += ChartItem_IsVisibleChanged;
        }

        private void ChartItem_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                StartDataFeed();
            }
        }
        private async Task Render(string blockId, string marketId)
        {
            var dic = await TopicSeries.Request(blockId);
            var dic2 = await TopicSeries.Request(marketId, true);

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

        public async override void StartDataFeed()
        {
            await Render("300843", "000001");
        }

        public override void StopDataFeed()
        {
        }


        public string BlockId { get; set; }

    }

}
