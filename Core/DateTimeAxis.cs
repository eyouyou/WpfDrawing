﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFAnimation
{
    public class DateTimeAxis : DiscreteAxis
    {
        public DateTimeAxis(AxisPosition position) : base(position)
        {

        }
        public override IFormatProvider FormatProvider => null;
    }
}
