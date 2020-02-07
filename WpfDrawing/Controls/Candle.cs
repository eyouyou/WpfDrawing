using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfDrawing
{
    public class Candle : UserControl
    {
        public Candle()
        {
            // Create a path to draw a geometry with.
            Path myPath = new Path();
            myPath.Stroke = Brushes.Red;
            myPath.StrokeThickness = 1;
            myPath.Fill = Brushes.Red; 

            // Create a StreamGeometry to use to specify myPath.
            //StreamGeometry theGeometry = BuildRegularPolygon(new Point(200, 200), 200, 3, 0);
            //theGeometry.FillRule = FillRule.EvenOdd;
            StreamGeometry theGeometry = BuildCandle();
            theGeometry.FillRule = FillRule.EvenOdd;
            // Freeze the geometry (make it unmodifiable)
            // for additional performance benefits.
            theGeometry.Freeze();

            // Use the StreamGeometry returned by the BuildRegularPolygon to 
            // specify the shape of the path.
            myPath.Data = theGeometry;

            // Add path shape to the UI.

            StackPanel mainPanel = new StackPanel();
            mainPanel.Children.Add(myPath);
            this.Content = mainPanel;

        }
        public class CandleParam
        {
            //柱
            public double Width { get; set; }

            public double Start { get; set; }
            public double End { get; set; }
            //芯
            public double Heightest { get; set; }
            public double Lowest { get; set; }

        }
        StreamGeometry BuildCandle()
        {
            StreamGeometry geometry = new StreamGeometry();

            using (StreamGeometryContext ctx = geometry.Open())
            {
                ctx.BeginFigure(new Point(0,10), true, true);

                ctx.PolyLineTo(new List<Point> { new Point(20, 10), new Point(20, 110), new Point(0, 110) }, true, true);

                ctx.BeginFigure(new Point(10,0), false, false);

                ctx.LineTo(new Point(10, 120), true, false);
                ctx.Close();
            }

            return geometry;

            ////BuildLine(ctx, new LineParam() { Start = new Point(pith_x, 0), Angle = 90, LineLength = 100, Pen = new Pen(Brushes.Red, 12) });
            //return new RectangleBuilder() { Start = new Point(0, 100), Angle = 30, Height = 100, Width = 200 }.BuildRectangle();
        }

        //GeometryGroup BuildK()
        //{
        //    GeometryGroup group = new GeometryGroup();
        //    RectangleGeometry 
        //    group.Children.Add()
        //        group.
        //}

        StreamGeometry BuildRect()
        {
            // c is the center, r is the radius,
            // numSides the number of sides, offsetDegree the offset in Degrees.
            // Do not add the last point.

            StreamGeometry geometry = new StreamGeometry();

            using (StreamGeometryContext ctx = geometry.Open())
            {
                ctx.BeginFigure(new Point(), true, true);

                ctx.PolyLineTo(new List<Point> {  new Point(200, 0), new Point(200, 300), new Point(0, 300) }, true, true);
            }

            return geometry;
        }

    }
    public class LineArgs
    {
        public double Angle { get; set; }
        //public double LineWidth { get; set; }
        public double LineLength { get; set; }

    }


}
