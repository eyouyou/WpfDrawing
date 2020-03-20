using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HevoDrawing.Charting
{
    public class QuoteParams
    {
        public string Code { get; set; }
        public List<string> Fields { get; set; }
    }

    public interface IRequstable<InputT, OutPutT>
    {
        Task<OutPutT> Request(InputT param);
    }

    public interface IAnalysizer<Key, InputT, OutputT>
    {
        OutputT Analysis(InputT value);
    }
    /// <summary>
    /// 目前都是這個格式
    /// </summary>
    /// <typeparam name="Key"></typeparam>
    /// <typeparam name="InputT"></typeparam>
    public interface ITupleResultAnalysizer<Key, InputT> : IAnalysizer<Key, InputT, Dictionary<Key, double>>
    {
    }
    public interface ITableResultAnalysizer<Key, InputT, Result> : IAnalysizer<Key, InputT, Dictionary<Key, Result>>
    {
    }

}
