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

        public virtual List<Point> Points
        {
            get
            {
                var list = new List<Point>();
                var vData = VisualData.TransformVisualData<RectChartContextData>();
                if (vData.IsBad)
                {
                    return list;
                }
                var coms = DataSource as ChartDataSource;

                var axisxs = coms.AxisXCollection;

                Point lasted = new Point(double.MinValue, double.MinValue);

                var y = coms.GetMappingAxisY(Id) as ContinuousAxis;
                if (y == null)
                {
                    return list;
                }
                var x = coms.FindXById(Id) as DiscreteAxis;
                if (x == null)
                {
                    return list;
                }

                var offsetx = x.Start.X;
                var offsety = x.Start.Y;
                var index = 0;
                foreach (var item in vData.Value.Data)
                {
                    var current = new Point(offsetx + x.GetPosition(item.Key.ValueData(Name) as IVariable).X, offsety + y.GetPosition(item.Value).Y);
                    list.Add(current);
                    index++;
                }
                return list;
            }
        }
    }

}
