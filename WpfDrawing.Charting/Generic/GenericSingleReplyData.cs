using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HevoDrawing.Charting
{
    public class GenericSingleReplyData : ReplyData
    {
        public GenericSingleReplyData(Dictionary<DateTime, double> data)
        {
            Data = data;
        }
        public GenericSingleReplyData()
        {

        }
        public Dictionary<DateTime, double> Data { get; set; } = new Dictionary<DateTime, double>();
    }
    public class GenericReplyData : ReplyData
    {
        public GenericReplyData(Dictionary<DateTime, Dictionary<ChartField, double>> data)
        {
            Data = data;
        }
        public GenericReplyData()
        {

        }
        public Dictionary<DateTime, Dictionary<ChartField, double>> Data { get; set; } = new Dictionary<DateTime, Dictionary<ChartField, double>>();
    }
}
