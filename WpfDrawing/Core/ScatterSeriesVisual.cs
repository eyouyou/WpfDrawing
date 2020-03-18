using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using HevoDrawing.Abstractions;

namespace HevoDrawing
{
    public class ScatterSeriesVisual : PointsSeriesVisual
    {
        public override ContextData DefaultData => throw new NotImplementedException();

        public override Func<IVariable, Value<double>, Brush> Color
        {
            get
            {
                return base.Color ?? ((xdata, ydata) => Brushes.Red);
            }
            set => base.Color = value;
        }
        public override void PlotToDc(DrawingContext dc)
        {
        }
    }
}
