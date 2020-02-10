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

    public abstract class SeriesVisual : SubRectDrawingVisual
    {
        public override RectVisualContextData DefaultData => RectChartVisualData.Empty;
        public int XAxisId { get; set; } = 0;
        public SeriesVisual()
        {
            HitElement = new EllipseSolidHitElement();
        }
        public abstract Brush Color { get; }
        private HitElement _hitElement = null;
        public HitElement HitElement
        {
            get => _hitElement;
            set
            {
                _hitElement = value;
                _hitElement.ParentSeries = this;
            }
        }

        /// <summary>
        /// 对外接口 计算range
        /// </summary>
        public Func<RectChartVisualData, Range> RangeCalculator { get; set; }

        /// <summary>
        /// 独立获取range
        /// </summary>
        /// <returns></returns>
        public Range GetRange()
        {
            if (!(VisualData is RectChartVisualData data))
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
    public class EllipseSolidHitElement : HitElement
    {
        private Ellipse _ellipse = new Ellipse() { };
        public override FrameworkElement Content
        {
            get
            {
                _ellipse.Width = Width;
                _ellipse.Height = Height;
                ZIndex = 2;
                if (Color != null)
                {
                    _ellipse.Fill = Color;
                }
                else if (ParentSeries != null)
                {
                    _ellipse.Fill = ParentSeries.Color;
                }
                return _ellipse;
            }
        }
    }

}
