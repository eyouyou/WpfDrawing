using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfDrawing.Sample
{
    /// <summary>
    /// Interaction logic for Window2.xaml
    /// </summary>
    public partial class Window2 : Window
    {
        Canvas canvas = new Canvas() { Width = 500, Height = 500 };
        Storyboard storyboard = new Storyboard() { };

        public Window2()
        {
            var brush = (Path)FindResource("WQ");
            canvas.Children.Add(brush);
            SizeToContent = SizeToContent.WidthAndHeight;
            InitializeComponent();
            this.Content = canvas;
            //canvas.Children.Add(new Button() { Content = "123" });
            CubicEase cubicEase = new CubicEase() { EasingMode = EasingMode.EaseOut };
            //StreamGeometry geometry = new StreamGeometry();
            var random = new Random();
            var points = new List<Point>();
            for (int i = 0; i < 200; i++)
            {
                var start = new Point(i * 100, random.NextDouble() * 500);

                points.Add(start);
            }
            //var lineGeometry2 = new LineGeometry(new Point(0,0), new Point(100,0));
            //var path2 = new Path() { Stroke = Brushes.Red, StrokeThickness = 2, Data = lineGeometry2 };

            //canvas.Children.Add(path2);
            //var animation2 = new PointAnimation(new Point(100,0), new Point(100,100), new Duration(TimeSpan.FromMilliseconds(10000))) { /*EasingFunction = cubicEase*/ };
            //storyboard.Children.Add(animation2);
            //RegisterName("lineGeometry2", lineGeometry2);
            //Storyboard.SetTargetName(animation2, "lineGeometry2");
            //Storyboard.SetTargetProperty(animation2, new PropertyPath("EndPoint"));

            //StreamGeometry streamGeometry = new StreamGeometry();
            //var first_lineGeometry = new CombinedGeometry();
            //var path = new Path() { Stroke = Brushes.Red, StrokeThickness = 2, Data = first_lineGeometry, Fill = Brushes.Black };
            //canvas.Children.Add(path);
            //var right_lineGeometry = new LineGeometry(points[0], new Point(points[0].X, 0));
            //var buttom_lineGeometry = new LineGeometry(new Point(0,0), new Point(points[0].X, 0));
            //var left_lineGeometry = new LineGeometry(new Point(0, 0), new Point(0, points[0].Y));
            //var sunKen_combine0 = new CombinedGeometry(left_lineGeometry, buttom_lineGeometry);
            //first_lineGeometry.Geometry1 = sunKen_combine0;
            //first_lineGeometry.Geometry2 = right_lineGeometry;

            //var animation2 = new PointAnimation(points[0], points[1], new Duration(TimeSpan.FromMilliseconds(10000)));
            //storyboard.Children.Add(animation2);
            //RegisterName("lineGeometry", right_lineGeometry);
            //Storyboard.SetTargetName(animation2, "lineGeometry");
            //Storyboard.SetTargetProperty(animation2, new PropertyPath("EndPoint"));

            var path = new Path() { Stroke = Brushes.Red, StrokeThickness = 2, Fill = Brushes.Black };
            canvas.Children.Add(path);
            var right_lineGeometry = new LineGeometry(new Point(100, 100), new Point(100, 0));
            var buttom_lineGeometry = new LineGeometry(new Point(0, 0), new Point(100, 0));
            var buttom_lineGeometry2 = new LineGeometry(new Point(0, 0), new Point(0, 100));
            var buttom_lineGeometry3 = new LineGeometry(new Point(0, 100), new Point(100, 100));
            var a = new EllipseGeometry(new Point(100,100), 100, 100);
            var b = new EllipseGeometry(new Point(100, 200), 100, 100);
            //var left_lineGeometry = new LineGeometry(new Point(0, 0), new Point(0, 100));
            //var sunKen_combine0 = new CombinedGeometry(left_lineGeometry, buttom_lineGeometry);
            //GeometryGroup myGeometryGroup = new GeometryGroup();
            //myGeometryGroup.Children.Add(right_lineGeometry);
            //myGeometryGroup.Children.Add(buttom_lineGeometry);
            //myGeometryGroup.Children.Add(buttom_lineGeometry2);
            //myGeometryGroup.Children.Add(buttom_lineGeometry3);
            //myGeometryGroup.FillRule = FillRule.EvenOdd;
            var first_lineGeometry = new CombinedGeometry(GeometryCombineMode.Union, a, b);
            path.Data = first_lineGeometry;
            //var animation2 = new PointAnimation(points[0], points[1], new Duration(TimeSpan.FromMilliseconds(10000)));
            //storyboard.Children.Add(animation2);
            //RegisterName("lineGeometry", right_lineGeometry);
            //Storyboard.SetTargetName(animation2, "lineGeometry");
            //Storyboard.SetTargetProperty(animation2, new PropertyPath("EndPoint"));


            /////////////////////////////////////////
            //for (int i = 0; i < points.Count - 1; i++)
            //{
            //    var lineGeometry = new LineGeometry(points[i], points[i + 1]);
            //    first_lineGeometry = new CombinedGeometry(first_lineGeometry, lineGeometry);
            //    path.Data = first_lineGeometry;
            //    using (var sgc = streamGeometry.Open())
            //    {
            //        sgc.BeginFigure(points[i], true, true);
            //        sgc.LineTo(points[i + 1], true, true);
            //    }
            //    var animation = new PointAnimation(points[i], points[i + 1], Duration.Automatic) { EasingFunction = cubicEase };
            //    animation.BeginTime = TimeSpan.FromMilliseconds(i * 1010);
            //    animation.Completed += (sender, e) =>
            //    {
            //        var combine = new CombinedGeometry(lineGeometry, new LineGeometry(new Point(points[i].X, 0), new Point(points[i + 1].X, 0)));
            //    };
            //    RegisterName("lineGeometry" + i, lineGeometry);

            //    storyboard.Children.Add(animation);
            //    Storyboard.SetTargetName(animation, "lineGeometry" + i);
            //    Storyboard.SetTargetProperty(animation, new PropertyPath("EndPoint"));

            //}
            storyboard.Begin(canvas);

        }
    }
}
