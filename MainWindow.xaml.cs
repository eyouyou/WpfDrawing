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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFAnimation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Storyboard storyboard = new Storyboard();
        StackPanel stack = new StackPanel() { Orientation = Orientation.Horizontal };
        const double height = 500;
        const double width = 500;
        const double interval = 100;
        const double dotCount = 20;
        Canvas canvas = new Canvas() { };
        public MainWindow()
        {
            var dock = new DockPanel();

            SizeToContent = SizeToContent.WidthAndHeight;
            var length = (dotCount - 1) * interval;
            Padding = new Thickness(0);
            Grid grid = new Grid() { Height = height, Width = width };
            grid.Children.Add(stack);
            grid.Children.Add(canvas);
            InitializeComponent();
            //Width = 500;
            for (int i = 0; i < 5; i++)
            {
                var candle = new Candle();
                candle.Height = 120;
                candle.Width = 20;
                DoubleAnimation d1 = new DoubleAnimation();
                ScaleTransform Scale = new ScaleTransform();
                candle.RenderTransform = Scale;
                candle.RenderTransformOrigin = new Point(0, 0.5);
                candle.Margin = new Thickness(5, 0, 0, 0);
                d1.Duration = new Duration(TimeSpan.FromSeconds(1));

                if (i % 2 == 0)
                {
                    d1.From = Scale.ScaleX;
                    d1.To = Scale.ScaleX * 1.5;
                }
                else
                {
                    d1.From = Scale.ScaleX * 1.5;
                    d1.To = Scale.ScaleX;
                }
                d1.AutoReverse = true;
                d1.RepeatBehavior = RepeatBehavior.Forever;
                storyboard.Children.Add(d1);

                Storyboard.SetTarget(d1, candle);
                Storyboard.SetTargetProperty(d1, new PropertyPath("RenderTransform.ScaleY"));
                stack.Children.Add(candle);

            }
            //var candle = new Candle();

            //DoubleAnimation d2 = new DoubleAnimation();

            //this.Left = 5;

            CubicEase cubicEase = new CubicEase() { EasingMode = EasingMode.EaseOut };
            StreamGeometry geometry = new StreamGeometry();
            var random = new Random();
            var points = new List<Point>();
            var end = new Point();
            //var length = 0.0;
            for (int i = 0; i < dotCount; i++)
            {
                var start = end;
                end = new Point(i * interval, random.NextDouble() * height);

                //if (i != 0)
                //    length += Math.Sqrt(Math.Pow(start.X - end.X, 2) + Math.Pow(start.Y - end.Y, 2));

                points.Add(end);
            }

            Path myPath = new Path();
            //myPath.Stroke = Brushes.Red;
            myPath.StrokeThickness = 0;
            myPath.Fill = Brushes.Red;
            /////////////////////
            ///  这个方法是否可行？？？
            /////////////////////
            using (StreamGeometryContext ctx = geometry.Open())
            {
                ctx.BeginFigure(new Point(length, height), true, true);
                points.Insert(0, new Point(0, height));
                ctx.PolyLineTo(points, true, true);

                ctx.Close();
            }
            //length /= myPath.StrokeThickness;
            //length = 20000;
            geometry.FillRule = FillRule.EvenOdd;
            geometry.Freeze();
            myPath.Data = geometry;
            //myPath.StrokeDashOffset = length;
            //myPath.StrokeDashArray = new DoubleCollection(new[] { length, length });

            canvas.Children.Add(myPath);
            Canvas.SetBottom(myPath, 0);
            //DoubleAnimation animation = new DoubleAnimation();
            //animation.From = 2000;
            //animation.To = 0;
            //animation.EasingFunction = cubicEase;
            //animation.Duration = new Duration(new TimeSpan(0, 0, 5));
            Storyboard storyboard2 = new Storyboard();
            //storyboard2.Children.Add(animation);

            //Storyboard.SetTarget(animation, myPath);
            //Storyboard.SetTargetProperty(animation, new PropertyPath("StrokeDashOffset"));


            TranslateTransform translate = new TranslateTransform();

            DoubleAnimation animationX = new DoubleAnimation();
            animationX.From = 0;
            animationX.To = -length + width;
            animationX.Duration = new Duration(new TimeSpan(0, 0, 10));
            storyboard2.Children.Add(animationX);
            canvas.RenderTransform = translate;
            Storyboard.SetTarget(animationX, myPath);
            Storyboard.SetTargetProperty(animationX, new PropertyPath(Canvas.LeftProperty));

            //storyboard2.Children.Add(animation2);

            //Storyboard.SetTarget(animation2, myPath);
            //Storyboard.SetTargetProperty(animation2, new PropertyPath("Canvas.LeftProperty"));


            this.Loaded += (sender, e) => { storyboard.Begin(); storyboard2.Begin(); };

            this.Content = grid;
        }
    }
}
