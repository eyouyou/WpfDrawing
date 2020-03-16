using HevoDrawing.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace HevoDrawing
{
    public class EllipsePointStyle : DataPointStyle
    {
        public double RadiusX { get; set; } = 2;
        public double RadiusY { get; set; } = 2;
        public Brush Brush { get; set; }
        public Pen Pen { get; set; }
    }
}
