using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HevoDrawing.Charting
{
    public class GenericSeriesPipline : ISpecificPipline
    {
        public async Task PipAsync(ChartContext context, PiplineDelegate next)
        {
            await next(context);
        }
    }
    public class SeriesWithTimeLinePipline : IAggratePipline
    {
        public async Task PipAsync(ChartContext context, PiplineDelegate next)
        {
            //var all = new List<DateTime>();
            //var reply = context.Data as AggrateReplyData;
            //foreach (var item in reply.TotalData)
            //{
            //    var data = item.Value as DatetimeReplyData;
            //    all.AddRange(data.Data.Keys);
            //    item.Key.SeriesVisual.VisualData = data.Data.ToFormatVisualData();
            //}
            //context.Items[DefaultContextItem.TimeLine] = all.Distinct().OrderBy(it => it).ToList();
            //await next(context);
        }
    }
}
