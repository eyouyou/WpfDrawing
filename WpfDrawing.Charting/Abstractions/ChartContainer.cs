using HevoDrawing.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace HevoDrawing.Charting
{
    public abstract class ChartContainer : UserControl
    {
        public abstract Chart Chart { get; }
    }
}
