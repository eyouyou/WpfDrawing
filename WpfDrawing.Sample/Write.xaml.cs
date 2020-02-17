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

namespace HevoDrawing.Sample
{
    /// <summary>
    /// Interaction logic for Write.xaml
    /// </summary>
    public partial class Write : Window
    {
        Canvas Canvas = new Canvas();
        public Write()
        {
            this.Content = Canvas;
            Canvas.Background = Brushes.Red;
            Path path = (Path)FindResource("p14");
            //path.Fill = Brushes.White;
            //path.StrokeThickness = 1;
            Canvas.Children.Add(path);
            NameScope.SetNameScope(this, new NameScope());
            Storyboard storyboard = new Storyboard();
            DoubleAnimation doubleAnimation = new DoubleAnimation() { To = 0 , Duration = TimeSpan.FromMilliseconds(10000) };
            storyboard.Children.Add(doubleAnimation);
            Storyboard.SetTarget(doubleAnimation, path);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("StrokeDashOffset"));
            InitializePathAndItsAnimation(path, doubleAnimation);
            storyboard.Begin();
        }

        private void InitializePathAndItsAnimation(System.Windows.Shapes.Path path, DoubleAnimation animation)
        {
            var length = path.Data.GetProximateLength() / path.StrokeThickness;
            path.StrokeDashOffset = length;
            path.StrokeDashArray = new DoubleCollection(new[] { length, length });
            animation.From = length;
        }

    }
    public static class AA
    {
        public static double GetProximateLength(this Geometry geometry)
        {
            var path = geometry.GetFlattenedPathGeometry();
            var length = 0.0;
            foreach (var figure in path.Figures)
            {
                var start = figure.StartPoint;
                foreach (var segment in figure.Segments)
                {
                    if (segment is PolyLineSegment polyLine)
                    {
                        // 一般的路径会转换成折线。
                        foreach (var point in polyLine.Points)
                        {
                            length += ProximateDistance(start, point);
                            start = point;
                        }
                    }
                    else if (segment is LineSegment line)
                    {
                        // 少部分真的是线段的路径会转换成线段。
                        length += ProximateDistance(start, line.Point);
                        start = line.Point;
                    }
                }
            }
            return length;

            double ProximateDistance(Point p1, Point p2)
            {
                return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
            }
        }

    }
}
