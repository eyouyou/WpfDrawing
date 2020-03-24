using HevoDrawing.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace HevoDrawing.Charting
{
    public abstract class ChartTemplate : UserControl
    {
        public abstract string TemplateName { get; }
        public ChartTemplate(Chart chart)
        {
            Chart = chart ?? throw new ArgumentNullException();
        }

        public Chart Chart { get; private set; }
        public abstract void Separate();
    }
}
