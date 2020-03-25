using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HevoDrawing.Charting
{
    /// <summary>
    /// 分类 pipline
    /// 单轴共用，且为时间轴
    /// </summary>
    public class SeperatePipline : IPipline
    {
        public async Task PipAsync(ChartContext context, PiplineDelegate next)
        {
            var all = new List<DateTime>();
            var reply = context.Data as AggrateReplyData;

            var single_dic = new Dictionary<SeriesPackBase, ReplyData>();
            var multi_dic = new Dictionary<SeriesPackBase, ReplyData>();
            foreach (var item in reply.TotalData)
            {
                if (item.Value is SingleReplyData<DateTime> singleData)
                {
                    all.AddRange(singleData.Data.Keys);
                    single_dic.Add(item.Key, singleData);
                }
                if (item.Value is GenericReplyData<DateTime> multiData)
                {
                    all.AddRange(multiData.Data.Keys);
                    multi_dic.Add(item.Key, multiData);
                }
            }
            var datetime = new TimeLineGenericChartContext();
            datetime.Data = reply;
            datetime.SingleData = single_dic;
            datetime.MultiData = multi_dic;
            datetime.TimeLine = all.Distinct().OrderBy(it => it).ToList();
            foreach (var item in context.Items)
            {
                datetime.Items.Add(item.Key, item.Value);
            }
            await next(datetime);
        }
    }


}
