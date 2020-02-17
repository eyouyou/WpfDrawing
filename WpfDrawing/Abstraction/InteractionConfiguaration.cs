using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;

namespace HevoDrawing.Abstractions
{
    public interface ICrossConfiguaration
    {
        bool IsYShow { get; set; }
        bool IsXShow { get; set; }
        bool IsCrossShow { get; set; }
        bool IsLabelShow { get; set; }
        Line X { get; }
        Line Y { get; }
    }

    public delegate void IntersectChangedHandler(Dictionary<string, SeriesData> data);

    public interface IIntersectable
    {
        event IntersectChangedHandler IntersectChanged;
    }

}
