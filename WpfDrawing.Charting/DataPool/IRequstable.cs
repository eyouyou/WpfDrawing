using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HevoDrawing.Charting
{
    public class ChartContext<DataT>
    {
        public DataT Data { get; set; }
        public IDictionary<object, object> Items { get; } = new Dictionary<object, object>();
    }
    public delegate Task PiplineDelegate<DataT>(ChartContext<DataT> data);
    public class QuoteParams
    {
        public string Code { get; set; }
        public List<string> Fields { get; set; }
    }

    public interface IRequstable<InputT, OutPutT>
    {
        Task<OutPutT> Request(InputT param);
    }
    public interface IPipline<Data>
    {
        Task PipAsync(ChartContext<Data> context, PiplineDelegate<Data> next);
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
