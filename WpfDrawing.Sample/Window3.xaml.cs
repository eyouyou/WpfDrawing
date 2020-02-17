using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HevoDrawing.Sample
{
    public class Poly : DrawingVisual
    {

    }
    /// <summary>
    /// Interaction logic for Window3.xaml
    /// </summary>
    public partial class Window3 : Window
    {
        const double width = 404;
        const double height = 366;

        Canvas canvas = new Canvas() { Width = width, Height = height };
        Storyboard trendStoryboard = new Storyboard() { };
        Storyboard mainStoryborad = new Storyboard() { };
        int pageSize = 10;
        int totalCount = 20;
        const double during = 20;
        const double pause = 2;
        public Window3()
        {
            var brush = (Path)FindResource("WQ");
            brush.Stroke = Brushes.LightGray;//new SolidColorBrush(Color.FromRgb(255, 255, 255));
            brush.Opacity = 0.1;
            //brush.StrokeThickness = 1;
            canvas.Children.Add(brush);

            //var brush = (Brush)FindResource("MyGridBrushResource");
            //canvas.Children.Add(brush);
            SizeToContent = SizeToContent.WidthAndHeight;
            this.Content = canvas;
            Button btn_test = new Button() { Content = "test" };
            InitializeComponent();
            var random = new Random();
            var points = new List<Point>();
            var heights = new List<double>();
            var length = 0;
            var interval = width / (pageSize - 1);
            var totalLength = interval * totalCount;
            var alpha = height / totalLength;
            totalCount = ys.Count;
            pageSize = totalCount;
            var min = ys.Min();
            var max = ys.Max();
            for (int i = 0; i < ys.Count; i++)
            {
                var y = height - (ys[i] - min) / (max - min)*height;
                points.Add(new Point(width / ys.Count*i, y));
            }
            //heights = heights.OrderByDescending(it => it).ToList();
            //for (int i = 0; i < heights.Count; i++)
            //{
            //    points.Add(new Point(interval * i, heights[i]));
            //}
            //for (int i = 1; i < points.Count - 1; i++)
            //{
            //    var item = points[i];
            //    var t1 = (points[i].X - points[i - 1].X) / (points[i].Y - points[i - 1].Y);
            //    var t2 = (points[i + 1].X - points[i].X) / (points[i + 1].Y - points[i].Y);
            //    if (Math.Abs(t1 - t2) <= 10)
            //    {
            //        item.Y = item.Y - 50;
            //    }
            //}
            //Polyline
            //Polyline polyline = new Polyline() { };

            //StreamGeometry stream = new StreamGeometry();
            //using (var ctx = stream.Open())
            //{
            //    ctx.BeginFigure(points[0], false, false);
            //    ctx.PolyLineTo(points, true, true);
            //    ctx.Close();
            //}
            //Path myPath = new Path();
            //myPath.StrokeThickness = 1;
            //myPath.Stroke = Brushes.DarkRed;
            //canvas.Children.Add(myPath);
            //myPath.Fill = Brushes.Red;
            DrawingGroup drawingGroup = new DrawingGroup();
            //GeometryGroup geometryGroup = new GeometryGroup() { FillRule = FillRule.Nonzero};
            //Polyline polyline = new Polyline() { Stroke = Brushes.Red };
            PathGeometry pathGeometry = new PathGeometry() { FillRule = FillRule.Nonzero };
            pathGeometry.Figures.Add(pathFigure);
            PathGeometry pathGeometry2 = new PathGeometry() { FillRule = FillRule.Nonzero };
            pathGeometry2.Figures.Add(pathFigure2);
            //geometryGroup.Children.Add(pathGeometry);
            //geometryGroup.Children.Add(pathGeometry2);
            LinearGradientBrush trendGradientBrush = new LinearGradientBrush() { StartPoint = new Point(0, 1), EndPoint = new Point(0, 0) };
            trendGradientBrush.GradientStops.Add(new GradientStop() { Offset = 0, Color = Color.FromRgb(248, 50, 47) });
            trendGradientBrush.GradientStops.Add(new GradientStop() { Offset = 0.5, Color = Color.FromRgb(248, 101, 68) });
            trendGradientBrush.GradientStops.Add(new GradientStop() { Offset = 1, Color = Color.FromRgb(248, 101, 68) });


            LinearGradientBrush backGradientBrush = new LinearGradientBrush() { StartPoint = new Point(0, 0), EndPoint = new Point(1, 1) };
            backGradientBrush.GradientStops.Add(new GradientStop() { Offset = 0, Color = Color.FromRgb(249, 111, 56) });
            var stop = new GradientStop() { Offset = 0.5, Color = Color.FromRgb(248, 50, 47) };
            backGradientBrush.GradientStops.Add(stop);
            this.RegisterName("backGradientStop", stop);
            backGradientBrush.GradientStops.Add(new GradientStop() { Offset = 1, Color = Color.FromRgb(248, 50, 47) });

            //DoubleAnimation offsetAnimation = new DoubleAnimation();
            //offsetAnimation.From = 0.5;
            //offsetAnimation.To = 0.8;
            //offsetAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(during * totalCount));
            //Storyboard.SetTargetName(offsetAnimation, "backGradientStop");
            //Storyboard.SetTargetProperty(offsetAnimation, new PropertyPath(GradientStop.OffsetProperty));
            //mainStoryborad.Children.Add(offsetAnimation);

            //GeometryDrawing line = new GeometryDrawing() { Geometry = pathGeometry, Pen = new Pen(Brushes.DarkRed, 0) };

            var highlight = new SolidColorBrush(Color.FromRgb(249, 111, 56));
            Path path = new Path() { Stroke = new SolidColorBrush(Color.FromArgb(30, 239, 215, 0)/*Color.FromArgb(255, 239, 215, 0)*/), StrokeThickness = 3 };
            path.Effect = new BlurEffect() { Radius = 2, RenderingBias = RenderingBias.Performance, KernelType = KernelType.Gaussian };//new BlurEffect() { Radius = 20, KernelType = KernelType.Gaussian }; 
            GeometryDrawing polygon = new GeometryDrawing() { Geometry = pathGeometry2, Brush = trendGradientBrush, Pen = new Pen(highlight, 1) };
            path.Data = pathGeometry;
            //path.;
            //geometryDrawing.Geometry = pathGeometry;
            //drawingGroup.Children.Add(line);
            drawingGroup.Children.Add(polygon);
            DrawingImage drawingImage = new DrawingImage();
            drawingImage.Drawing = drawingGroup;
            Image image = new Image() { Source = drawingImage };
            //btn_test.Effect = new BlurEffect() {  Radius=5, KernelType= KernelType.Box };
            //image.LayoutTransform = new RotateTransform(180, 0.5, 0.5);
            canvas.Children.Add(image);
            canvas.Children.Add(path);

            canvas.Background = backGradientBrush;
            Canvas.SetBottom(image, 0);
            //pathFigure.Segments.Add(new LineSegment(points[0], false));
            //pathFigure.Segments.Add(new LineSegment(points[1], false));
            //pathFigure.Segments.Add(new LineSegment(points[2], false));

            //myPath.Data = drawingImage;
            //pathFigure.StartPoint = new Point(0,0);

            //var lineSegment = new LineSegment(points[0], true);

            //var startSegment = new LineSegment(points[0], true);
            //pathFigure.Segments.Add(startSegment);
            //RegisterName("lineSegment" + i, startSegment);



            //for (int i = 0; i < 200 - 1;  ++)
            //{

            //    PointAnimation pointAnimation = new PointAnimation(points[i], points[i + 1], new Duration(TimeSpan.FromMilliseconds(1000)));
            //    pointAnimation.BeginTime = TimeSpan.FromMilliseconds(i * 1010);
            //    pointAnimation.Completed += (sender, e) =>
            //    {
            //        startSegment = new LineSegment(points[i + 1], true);
            //        pathFigure.Segments.Add(startSegment);
            //    };
            //    Storyboard.SetTargetName(pointAnimation, "lineSegment" + i);
            //    Storyboard.SetTargetProperty(pointAnimation, new PropertyPath(LineSegment.PointProperty));
            //    storyboard.Children.Add(pointAnimation);
            //    //storyboard.FillBehavior = FillBehavior.HoldEnd;
            //storyboard.Begin(canvas);
            //}

            //canvas.Children.Add(polyline);
            //polyline.Points.Add(points[0]);
            //polyline.Points.Add(points[0]);
            //RegisterName("hevoPolyline", polyline);
            //var dx = new DoubleAnimation();
            //dx.From = points[0].X;
            //dx.To = points[0 + 1].X;
            //dx.Duration = TimeSpan.FromMilliseconds(10000);
            //dx.BeginTime = TimeSpan.FromMilliseconds(10000 * 0);
            ////dx.Completed += (sender, e) => { polyline.Points.Add(points[0 + 1]); };
            //Storyboard.SetTarget(dx, polyline);
            //GeometryGroup geometryGroup = new GeometryGroup();
            //Path path = new Path();
            //path.Data = geometryGroup;
            //Storyboard.SetTargetProperty(dx, new PropertyPath("Points[" + (1) + "].X"));
            //storyboard.Children.Add(dx);

            //var dx = new PointAnimation();
            //dx.From = points[0];
            //dx.To = points[0 + 1];
            //dx.Duration = TimeSpan.FromMilliseconds(10000);
            //dx.BeginTime = TimeSpan.FromMilliseconds(10000 * 0);
            ////dx.Completed += (sender, e) => { polyline.Points.Add(points[0 + 1]); };
            //Storyboard.SetTarget(dx, polyline);
            //Storyboard.SetTargetProperty(dx, new PropertyPath("Points[" + (1) + "]"));
            //storyboard.Children.Add(dx);

            //for (int i = 0; i < 200 - 1; i++)
            //{
            //    var dx = new DoubleAnimation();
            //    dx.From = points[i].X;
            //    dx.To = points[i + 1].X;
            //    dx.Duration = TimeSpan.FromMilliseconds(10000);
            //    dx.BeginTime = TimeSpan.FromMilliseconds(10000 * i);
            //    dx.Completed += (sender, e) => { polyline.Points.Add(points[i + 1]); };
            //    Storyboard.SetTargetName(dx, "hevoPolyline");
            //    Storyboard.SetTargetProperty(dx, new PropertyPath("Points[" + (i) + "].X"));
            //    storyboard.Children.Add(dx);

            //    var dy = new DoubleAnimation();
            //    dy.From = points[i].Y;
            //    dy.To = points[i + 1].Y;
            //    dy.Duration = TimeSpan.FromMilliseconds(10000);
            //    dy.BeginTime = TimeSpan.FromMilliseconds(10000 * i);
            //    dy.Completed += (sender, e) => { polyline.Points.Add(points[i + 1]); };
            //    Storyboard.SetTargetName(dy, "hevoPolyline");
            //    Storyboard.SetTargetProperty(dy, new PropertyPath("Points[" + (i) + "].Y"));
            //    storyboard.Children.Add(dy);
            //}

            DoubleAnimation leftAnimation1 = new DoubleAnimation();
            leftAnimation1.From = 0;
            leftAnimation1.To = -totalLength + interval + width;
            leftAnimation1.Duration = new Duration(TimeSpan.FromMilliseconds(during * totalCount));
            mainStoryborad.Children.Add(leftAnimation1);
            Storyboard.SetTarget(leftAnimation1, image);
            Storyboard.SetTargetProperty(leftAnimation1, new PropertyPath(Canvas.LeftProperty));

            DoubleAnimation leftAnimation2 = new DoubleAnimation();
            leftAnimation2.From = 0;
            leftAnimation2.To = -totalLength + interval + width;
            leftAnimation2.Duration = new Duration(TimeSpan.FromMilliseconds(during * totalCount));
            mainStoryborad.Children.Add(leftAnimation2);
            Storyboard.SetTarget(leftAnimation2, path);
            Storyboard.SetTargetProperty(leftAnimation2, new PropertyPath(Canvas.LeftProperty));

            //AddPoint(points[0]);
            //Fill
            //Polygon polygon = new Polygon();
            ////polygon.se
            //polygon.Margin = new Thickness(0, 50, 0, 0);
            //canvas.Children.Add(polygon);
            //polygon.Points.Add(new Point(0, 0));
            //polygon.Points.Add(new Point(0, 0));
            //polygon.Fill = Brushes.Black;
            pathFigure2.StartPoint = new Point(0, height);
            var first_seg = new LineSegment(new Point(0, height), false) { };
            RegisterName("first_seg", first_seg);
            RegisterName("pathFigure2", pathFigure2);

            pathFigure2.Segments.Add(first_seg);
            pathFigure2.Segments.Add(new LineSegment(points[0], false) { });
            //pathFigure2.Segments.Add(new LineSegment(points[1], false));

            //pathFigure2.Segments.Add(new LineSegment(points[2], false));
            trendStoryboard.Completed += (sender, e) =>
            {
                if (CurrentPoints.Count == pageSize)
                {

                }
                if (CurrentPoints.Any())
                {
                    //pathFigure2.Segments.Add(new LineSegment(CurrentPoints.Last(), false)); 
                }
            };
            var j = 0;
            Timer timer = new Timer();
            timer.Interval = during + pause;
            timer.Elapsed += (sender, e) =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (j == pageSize)
                    {
                        //mainStoryborad.Begin(canvas);
                    }
                    if (j == points.Count)
                    {
                        timer.Stop();
                        return;
                    }
                    AddPoint(points[j]);
                    j++;
                }));

            };
            canvas.Children.Add(btn_test);

            timer.Start();
            btn_test.Click += (sender, e) =>
            {
                //polygon.Points.Add(points[j]);
                AddPoint(points[j]);

                j++;
            };
        }
        public List<Point> CurrentPoints = new List<Point>();
        List<LineSegment> lineSegments = new List<LineSegment>();
        PathFigure pathFigure = new PathFigure() { IsClosed = false };
        PathFigure pathFigure2 = new PathFigure() { IsClosed = true };
        object locker = new object();
        public Storyboard AddPoint(Point point)
        {
            var count = CurrentPoints.Count();
            var last = CurrentPoints.LastOrDefault();
            if (count == 0)
            {
                pathFigure.StartPoint = point;
                last = point;
            }
            CurrentPoints.Add(point);
            //if (lineSegments.Count >= 1)
            //    pathFigure2.Segments.Add(lineSegments.LastOrDefault());

            var lineSegment = new LineSegment(last, true);
            lineSegments.Add(lineSegment);
            RegisterName("lineSegment" + count, lineSegment);
            pathFigure.Segments.Add(lineSegment);
            //var lineSegment2 = new LineSegment(last, false);
            //RegisterName("rectLineSegment" + count, lineSegment2);

            if (count != 0)
            {
                var lineSegment2 = new LineSegment(last, false) { };
                pathFigure2.Segments.Add(lineSegment2);
                RegisterName("LastlineSegment" + count, lineSegment2);
                lock (locker)
                {
                    PointAnimation pointAnimation = new PointAnimation(last, point, new Duration(TimeSpan.FromMilliseconds(during)));
                    //pointAnimation.BeginTime = TimeSpan.FromMilliseconds(i * 1010);
                    //pointAnimation.Completed += (sender, e) =>
                    //{
                    //    startSegment = new LineSegment(points[i + 1], true);
                    //    pathFigure.Segments.Add(startSegment);
                    //};
                    Storyboard.SetTargetName(pointAnimation, "lineSegment" + count);
                    Storyboard.SetTargetProperty(pointAnimation, new PropertyPath(LineSegment.PointProperty));

                    PointAnimation LastPointAnimation = new PointAnimation(last, point, new Duration(TimeSpan.FromMilliseconds(during)));

                    Storyboard.SetTargetName(LastPointAnimation, "LastlineSegment" + count);
                    Storyboard.SetTargetProperty(LastPointAnimation, new PropertyPath(LineSegment.PointProperty));

                    PointAnimation pointAnimation2 = new PointAnimation(new Point(last.X, height), new Point(point.X, height), new Duration(TimeSpan.FromMilliseconds(during)));
                    Storyboard.SetTargetName(pointAnimation2, "pathFigure2");
                    Storyboard.SetTargetProperty(pointAnimation2, new PropertyPath(PathFigure.StartPointProperty));


                    trendStoryboard.Children.Clear();
                    trendStoryboard.Children.Add(pointAnimation);
                    trendStoryboard.Children.Add(LastPointAnimation);
                    trendStoryboard.Children.Add(pointAnimation2);

                    //storyboard.FillBehavior = FillBehavior.HoldEnd;
                    trendStoryboard.Begin(canvas);

                }
                return trendStoryboard;
            }
            return trendStoryboard;
        }
        List<double> ys = new List<double>()
        {
2013,
2005,
2054,
2033,
2033,
2116,
2114,
2056,
2058,
2004,
2048,
2042,
2059,
2131,
2098,
2037,
2026,
2011,
2027,
2035,
2039,
2030,
2071,
2027,
2037,
2059,
2047,
2059,
2127,
2185,
2194,
2227,
2241,
2217,
2326,
2332,
2329,
2348,
2364,
2375,
2341,
2302,
2420,
2418,
2479,
2487,
2683,
2938,
2938,
3109,
3158,
3235,
3285,
3377,
3352,
3210,
3076,
3204,
3247,
3310,
3241,
3373,
3617,
3691,
3864,
4034,
4287,
4394,
4442,
4206,
4309,
4658,
4612,
5023,
5166,
4478,
4193,
3687,
3878,
3957,
4071,
3664,
3744,
3965,
3508,
3232,
3160,
3200,
3098,
3092,
3053,
3183,
3391,
3412,
3383,
3590,
3581,
3631,
3436,
3525,
3435,
3579,
3628,
3539,
3186,
2901,
2917,
2738,
2763,
2860,
2767,
2874,
2810,
2955,
2979,
3010,
2985,
3078,
2959,
2938,
2913,
2827,
2854,
2932,
2988,
3054,
3013,
2979,
2977,
3051,
3108,
3070,
3067,
3079,
3003,
3034,
3005,
3064,
3091,
3104,
3125,
3196,
3193,
3262,
3244,
3233,
3123,
3110,
3104,
3154,
3113,
3123,
3159,
3159,
3197,
3202,
3253,
3218,
3213,
3237,
3269,
3222,
3287,
3246,
3173,
3155,
3103,
3084,
3091,
3110,
3106,
3158,
3123,
3158,
3192,
3218,
3222,
3238,
3253,
3262,
3209,
3269,
3332,
3367,
3365,
3354,
3353,
3349,
3391,
3379,
3417,
3372,
3433,
3383,
3354,
3318,
3290,
3266,
3297,
3307,
3392,
3429,
3488,
3558,
3462,
3130,
3199,
3289,
3255,
3307,
3270,
3153,
3169,
3131,
3159,
3072,
3082,
3091,
3163,
3193,
3141,
3075,
3067,
3022,
2890,
2847,
2747,
2831,
2829,
2874,
2740,
2795,
2669,
2729,
2725,
2702,
2682,
2797,
2821,
2607,
        };
    }
}
