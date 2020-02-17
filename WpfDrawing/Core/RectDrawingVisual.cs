using HevoDrawing.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace HevoDrawing
{

    public class TestVisual : RectDrawingVisual
    {
        public TestVisual()
        {

        }

        public override ContextData DefaultData => null;

        public override void Plot()
        {
            using (var dc = RenderOpen())
            {
                var text = new FormattedText("Click Me!",
                  CultureInfo.GetCultureInfo("en-us"),
                  FlowDirection.LeftToRight,
                  new Typeface("Verdana"),
                  36, System.Windows.Media.Brushes.Black);
                dc.DrawText(text, new System.Windows.Point(200, 116));
            }
        }
    }
}
