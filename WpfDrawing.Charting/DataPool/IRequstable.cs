using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HevoDrawing.Charting
{
    public enum DefaultContextItem
    {
        TimeLine
    }
    public class ChartContext
    {
        public ReplyData Data { get; set; }
        public IDictionary<object, object> Items { get; } = new Dictionary<object, object>();
    }
    public delegate Task PiplineDelegate(ChartContext data);
    public class QuoteParams
    {
        public string Code { get; set; }
        public List<string> Fields { get; set; }
    }
    public abstract class ReplyData
    {
        public int Id { get; set; }
    }

    public class AggrateReplyData : ReplyData
    {
        public AggrateReplyData(Dictionary<SeriesPackBase, ReplyData> data)
        {
            TotalData = data;
        }
        public Dictionary<SeriesPackBase, ReplyData> TotalData { get; set; }
    }
    public abstract class RequestParams
    {

    }
    public interface IRequstable<InputT, OutPutT>
    {
        Task<OutPutT> Request(InputT param);
    }
    public interface IPipline
    {
        Task PipAsync(ChartContext context, PiplineDelegate next);
    }
    public interface IAggratePipline : IPipline
    {
    }
    public interface ISpecificPipline : IPipline
    {
    }

    public interface IKeyAnalysizer<Key, InputT, OutputT>
    {
    }
    /// <summary>
    /// 目前都是這個格式
    /// </summary>
    /// <typeparam name="Key"></typeparam>
    /// <typeparam name="InputT"></typeparam>
    public interface ITupleResultAnalysizer<Key, InputT> : IKeyAnalysizer<Key, InputT, Dictionary<Key, double>>
    {
    }
    public interface ITableResultAnalysizer<Key, InputT, Result> : IKeyAnalysizer<Key, InputT, Dictionary<Key, Result>>
    {
    }

}
