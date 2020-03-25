using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HevoDrawing.Charting
{
    public abstract class ReplyData
    {
        public int Id { get; set; }
        public bool IsBad { get; set; } = false;
    }

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
}
