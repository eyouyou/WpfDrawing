using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HevoDrawing.Charting
{
    public enum DefaultContextItem
    {
        TimeLine,

    }

    public class GenericChartContext : ChartContext
    {
        public Dictionary<SeriesPackBase, ReplyData> SingleData { get; set; }
        public Dictionary<SeriesPackBase, ReplyData> MultiData { get; set; }
    }

    public class TimeLineGenericChartContext : GenericChartContext
    {
        public List<DateTime> TimeLine { get; set; }
    }
}
