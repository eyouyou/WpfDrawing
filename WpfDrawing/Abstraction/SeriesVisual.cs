using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace HevoDrawing.Abstractions
{
    public abstract class HitElement
    {
        /// <summary>
        /// TODO 需要调整 配置remove和enable
        /// </summary>
        public bool IsAdded { get; set; } = false;
        public SeriesVisual ParentSeries { get; set; }
        public double Width { get; set; } = 10;
        public double Height { get; set; } = 10;
        public Brush Color { get; set; }
        public abstract FrameworkElement Content { get; }
        public int ZIndex { get; set; } = 0;

    }

    /// <summary>
    /// Line\Scatter\Bar
    /// </summary>
    public abstract class SeriesVisual : SubRectDrawingVisual
    {
        public int XAxisId { get; set; } = 0;
        /// <summary>
        /// tip 使用
        /// </summary>
        public abstract Func<IVariable, Brush> Color { get; set; }
    }

}
