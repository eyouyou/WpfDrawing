using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HevoDrawing.Abstractions
{
    /// <summary>
    /// 部分隔離
    /// </summary>
    public interface IChartComponentizable
    {
        List<SeriesVisual> SeriesCollection { get; }
        List<AxisVisual> AxisYCollection { get; }
        List<AxisVisual> AxisXCollection { get; }
    }
}
