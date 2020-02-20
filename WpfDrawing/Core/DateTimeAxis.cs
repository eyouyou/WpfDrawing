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
            var data = (variable.ValueData("") as Value<DateTime>).Data;

            var left = (section.Left.ValueData("") as Value<DateTime>).Data;
            var right = (section.Right.ValueData("") as Value<DateTime>).Data;

            if (data < left || data > right)
            {
                return double.NaN;
            }
            var total = GetExceptSections(section);
            var totalExceptMilliseconds = 0.0;
            foreach (var item in total)
            {
                if (item.Contains(variable))
                {
                    return double.NaN;
                }
                var left_except = (item.Left.ValueData("") as Value<DateTime>).Data;
                var right_except = (item.Right.ValueData("") as Value<DateTime>).Data;

                totalExceptMilliseconds += (right_except - left_except).TotalMilliseconds;
            }

            return (data - left).TotalMilliseconds / ((right - left).TotalMilliseconds - totalExceptMilliseconds);
        }
    }
}
