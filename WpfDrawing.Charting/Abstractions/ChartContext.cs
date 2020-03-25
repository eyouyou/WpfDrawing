using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HevoDrawing.Charting
{
    /// <summary>
    /// 即使在pipline中替换了 Cotnext <see cref="Data"/> 该属性不能变 必须是<see cref="AggrateReplyData"/>
    /// </summary>
    public class ChartContext
    {
        public bool IsCanceled { get; set; } = false;
        public ReplyData Data { get; set; }
        public IDictionary<object, object> Items { get; } = new Dictionary<object, object>();
    }
}
