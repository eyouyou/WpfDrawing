using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfDrawing.Abstraction
{
    /// <summary>
    /// scatter\line
    /// </summary>
    public abstract class PointsSeriesVisual : SeriesVisual
    {
        /// <summary>
        /// TODO 所有点都需要固定
        /// </summary>
        public bool IsPointsFixed { get; set; }
        List<MarkLineVisual> MarkLineCollection { get; } = new List<MarkLineVisual>();
        public abstract List<Point> Points { get; }
    }

}
