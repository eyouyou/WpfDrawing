using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using HevoDrawing.Abstractions;

namespace HevoDrawing
{
    public class ScatterSeriesVisual : SeriesVisual
    {
        public override Brush Color => Brushes.Red;

        public override ContextData DefaultData => throw new NotImplementedException();

        public override void PlotToDc(DrawingContext dc)
        {
        }
    }
}
