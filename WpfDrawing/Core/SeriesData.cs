using HevoDrawing;
using HevoDrawing.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace HevoDrawing
{
    /// <summary>
    /// TODO 这部分可能需要调整
    /// </summary>
    public class SeriesData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Brush Color { get; set; }
        public IVariable XValue { get; set; }
        public AxisVisual<IVariable> AxisX { get; set; }
        public IVariable YValue { get; set; }
        public AxisVisual<IVariable> AxisY { get; set; }
    }
}
