using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HevoDrawing.Charting
{
    public class ChartField
    {
        public int Id { get; }
        public ChartField(int id)
        {
            Id = id;
        }
        public override int GetHashCode()
        {
            return Id;
        }
        public override bool Equals(object obj)
        {
            if (obj is ChartField cf)
            {
                return cf.Id == Id;
            }
            return false;
        }
    }

    public static class ChartFields
    {
        public static ChartField ZT = new ChartField(1);
        public static ChartField DT = new ChartField(2);
        public static ChartField yzzt = new ChartField(3);
        public static ChartField fyzzt = new ChartField(4);
    }
}
