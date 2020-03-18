using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using HevoDrawing.Abstractions;

namespace HevoDrawing
{
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
                    _ellipse.Fill = ParentSeries.Color(Value<double>.BadT, Value<double>.BadT);
                }
                return _ellipse;
            }
        }
    }
}
