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

    public interface IRequstable<T>
    {
        /// <summary>
        /// Key=>字段
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        Dictionary<string, double> Request(T param);
    }

    public interface IHttpRequstable : IRequstable<string>
    {

    }
    public interface IQuoteRequstable : IRequstable<QuoteParams>
    {

    }

}
