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

        public override double IntervalPositioning(Section section, IVariable variable, int step)
        {
            var variable_data = (variable as Value<DateTime>).Data;
            variable = new Value<DateTime>(variable_data.AddMilliseconds(MinUnit * step));

            var data = (variable as Value<DateTime>).Data;

            var left = (section.Left as Value<DateTime>).Data;
            var right = (section.Right as Value<DateTime>).Data;

            if (data < left || data > right)
            {
                return double.NaN;
            }
            List<Section> total = GetSectionsExcept(section);
            var totalMilliseconds = 0.0;
            var index = 0;
            var data_index = -1;
            foreach (var item in total)
            {
                if (item.Contains(variable))
                {
                    data_index = index;
                }
                var left_except = (item.Left.ValueData("") as Value<DateTime>).Data;
                var right_except = (item.Right.ValueData("") as Value<DateTime>).Data;

                totalMilliseconds += (right_except - left_except).TotalMilliseconds;
                index++;
            }
            if (total.Count == 1 && total[0].Equals(section))
            {
                return (data - left).TotalMilliseconds / totalMilliseconds;
            }
            else if (total.Count == 0)
            {
                return double.NaN;
            }
            else
            {
                var sum = total.Take(data_index).Select(it => ((it.Right.ValueData("") as Value<DateTime>).Data - (it.Left.ValueData("") as Value<DateTime>).Data).TotalMilliseconds
                / totalMilliseconds).Sum();

                var current_left = (total[data_index].Left.ValueData("") as Value<DateTime>).Data;
                var current_right = (total[data_index].Right.ValueData("") as Value<DateTime>).Data;

                return (data - current_left).TotalMilliseconds / totalMilliseconds + sum;
            }
        }
    }
}
