using HevoDrawing.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace HevoDrawing
{
    /// <summary>
    /// 依附series的数据 二维
    /// </summary>
    public class MarkLineVisual : SeriesVisual
    {
        /// <summary>
        /// 一元方程
        /// </summary>
        public Func<double, double> Equation { get; set; }
        /// <summary>
        /// 坐标
        /// </summary>
        public List<Point> Croods { get; set; } = new List<Point>();
        public Pen LinePen { get; set; } = new Pen(Brushes.Black, 1);
        public override Brush Color => LinePen.Brush;

        public override ContextData DefaultData => Chart2DContextData.Empty;

        public override void PlotToDc(DrawingContext dc)
        {
            //按照方程来画线 点数按照plotArea大小来设定
            if (Equation != null)
            {

            }
            //之间按照坐标定位
            else if (Croods != null && Croods.Count > 0)
            {
                StreamGeometry streamGeo = new StreamGeometry();
                using (var sgc = streamGeo.Open())
                {

                }
            }
            //按照数据定位
            else if (VisualData is TwoDimensionalContextData data && !data.IsEmpty)
            {

            }
        }
    }
}
