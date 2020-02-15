using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using WpfDrawing.Abstraction;

namespace WpfDrawing.Core
{
    public class ScatterSeriesVisual : SeriesVisual
    {
        public override Brush Color => Brushes.Red;

        public override void PlotToDc(DrawingContext dc)
        {
        }
    }
}
