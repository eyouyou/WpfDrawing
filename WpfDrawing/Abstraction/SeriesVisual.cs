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
        public override RectVisualContextData DefaultData => RectChartContextData.Empty;
        public int XAxisId { get; set; } = 0;
        public SeriesVisual()
        {
        }
        /// <summary>
        /// tip 使用
        /// </summary>
        public abstract Brush Color { get; }
        
        /// <summary>
        /// 对外接口 计算range
        /// </summary>
        public Func<RectChartContextData, Range> RangeCalculator { get; set; }

        /// <summary>
        /// 独立获取range
        /// </summary>
        /// <returns></returns>
        public Range GetRange()
        {
            if (!(VisualData is RectChartContextData data))
            {
                return default;
            }
            if (RangeCalculator != null)
            {
                return RangeCalculator(data);
            }
            return data.YData.Range;
        }


    }

}
