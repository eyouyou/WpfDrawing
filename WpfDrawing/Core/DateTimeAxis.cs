using HevoDrawing.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HevoDrawing
{
    public class DateTimeAxis : DiscreteAxis
    {
        public DateTimeAxis(AxisPosition position) : base(position)
        {

        }
        public override IFormatProvider FormatProvider => null;

        public override double IntervalPositioning(Section section, IVariable variable)
        {
            var data = (DateTime)variable.ValueData("");

            var left = (DateTime)section.Left.ValueData("");
            var right = (DateTime)section.Right.ValueData("");

            if (data < left || data > right)
            {
                return double.NaN;
            }
            return (data - left).TotalMilliseconds / (right - left).TotalMilliseconds;
        }
    }
}
