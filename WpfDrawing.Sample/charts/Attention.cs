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
    public class Attention : ChartPack
    {
        StraightLineSeriesVisual lineSeries = new StraightLineSeriesVisual() { Name = "概念关注度", LinePen = new Pen(Brushes.SpringGreen, 1) };
        StraightLineSeriesVisual lineSeries2 = new StraightLineSeriesVisual() { Name = "A股平均关注度", LinePen = new Pen(Brushes.Red, 1) };

        DiscreteAxis axisX = new DateTimeAxis(AxisPosition.Buttom) { Name = "时间", ValueFormat = "yyyyMMdd", SplitValueFormat = "yyyy/MM", IsInterregional = true, ShowGridLine = false, IsGridLineClose = true };
        ContinuousAxis axisY = new ContinuousAxis(AxisPosition.Left) { ValueFormat = "G4", SplitValueFormat = "G4", ShowGridLine = true, AxisPen = new Pen(Brushes.Green, 1), Unit = "万" };

        public Attention()
        {
            TopicSeries TopicSeries = new TopicSeries(lineSeries);
            TopicSeries TopicSeries2 = new TopicSeries(lineSeries2);

            axisX.IsInterregional = false;

            AddYAxis(axisY);
            AddXAxis(axisX);

            AddSeriesPack(TopicSeries);
            AddSeriesPack(TopicSeries2);

            AddResponsePipline(new GenericSeriesWithTimeLinePipline());
            AddResponsePipline(async (context, next) =>
            {
                var all = context.Items[DefaultContextItem.TimeLine] as List<DateTime>;
                var a = all.GroupBy(it => it.ToString("yyyyMM")).Select(it => it.ElementAt(0)).Distinct().OrderBy(it => it).ToList();
                axisX.SplitValues = a.Select(it => it.ToFormatVisualData()).ToList();
                await next(context);
            });

            ChartTemplate = new GenericChartTemplate(this);
        }

        public async override void StartDataFeed()
        {
            await RequestProcess(SeriesPacks);
        }

        public override void StopDataFeed()
        {
        }
    }

}
