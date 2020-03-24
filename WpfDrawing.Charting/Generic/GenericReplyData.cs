using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HevoDrawing.Charting
{
    public class ReplyData<X, Y> : ReplyData
    {
        public ReplyData(Dictionary<X, Y> data)
        {
            Data = data;
        }
        public ReplyData()
        {

        }
        public Dictionary<X, Y> Data { get; set; } = new Dictionary<X, Y>();
    }
    public class SingleReplyData<X> : ReplyData<X, double>
    {
        public SingleReplyData(Dictionary<X, double> data) : base(data)
        {
            Data = data;
        }
        public SingleReplyData()
        {

        }
    }
    public class GenericReplyData<X> : ReplyData<X, Dictionary<ChartField, double>>
    {
        public GenericReplyData(Dictionary<X, Dictionary<ChartField, double>> data) : base(data)
        {
            Data = data;
        }
        public GenericReplyData()
        {

        }
    }

    public class DatetimeReplyData<Y> : ReplyData<DateTime, Y>
    {
        public DatetimeReplyData(Dictionary<DateTime, Y> data) : base(data)
        {

        }
    }
}
