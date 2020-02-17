using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HevoDrawing.Sample
{
    public class MyVisualHost : FrameworkElement
    {
        // Create a collection of child visual objects.
        private readonly System.Windows.Media.VisualCollection _children;

        public MyVisualHost()
        {
            _children = new System.Windows.Media.VisualCollection(this)
            {
                CreateDrawingVisualAA(),
                //CreateDrawingImage(),
                //CreateDrawingVisualRectangle(),
                //CreateDrawingVisualText(),
                //CreateDrawingVisualEllipses()
            };


            // Add the event handler for MouseLeftButtonUp.
            MouseLeftButtonUp += MyVisualHost_MouseLeftButtonUp;
        }


        // Capture the mouse event and hit test the coordinate point value against
        // the child visual objects.
        private void MyVisualHost_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Retreive the coordinates of the mouse button event.
            Point pt = e.GetPosition((UIElement)sender);

            // Initiate the hit test by setting up a hit test result callback method.
            VisualTreeHelper.HitTest(this, null, MyCallback, new PointHitTestParameters(pt));
        }

        // If a child visual object is hit, toggle its opacity to visually indicate a hit.
        public HitTestResultBehavior MyCallback(HitTestResult result)
        {
            if (result.VisualHit.GetType() == typeof(System.Windows.Media.DrawingVisual))
            {
                ((System.Windows.Media.DrawingVisual)result.VisualHit).Opacity =
                    ((System.Windows.Media.DrawingVisual)result.VisualHit).Opacity == 1.0 ? 0.4 : 1.0;
            }

            // Stop the hit test enumeration of objects in the visual tree.
            return HitTestResultBehavior.Stop;
        }

        // Create a DrawingVisual that contains a rectangle.
        private System.Windows.Media.DrawingVisual CreateDrawingVisualRectangle()
        {
            System.Windows.Media.DrawingVisual drawingVisual = new System.Windows.Media.DrawingVisual();

            // Retrieve the DrawingContext in order to create new drawing content.
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            // Create a rectangle and draw it in the DrawingContext.
            Rect rect = new Rect(new Point(160, 100), new Size(320, 80));
            drawingContext.DrawRectangle(Brushes.LightBlue, null, rect);

            // Persist the drawing content.
            drawingContext.Close();

            return drawingVisual;
        }

        // Create a DrawingVisual that contains text.
        private System.Windows.Media.DrawingVisual CreateDrawingVisualText()
        {
            // Create an instance of a DrawingVisual.
            System.Windows.Media.DrawingVisual drawingVisual = new System.Windows.Media.DrawingVisual();

            // Retrieve the DrawingContext from the DrawingVisual.
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            // Draw a formatted text string into the DrawingContext.
            drawingContext.DrawText(
                new FormattedText("Click Me!",
                    CultureInfo.GetCultureInfo("en-us"),
                    FlowDirection.LeftToRight,
                    new Typeface("Verdana"),
                    36, Brushes.Black),
                new Point(200, 116));

            // Close the DrawingContext to persist changes to the DrawingVisual.
            drawingContext.Close();

            return drawingVisual;
        }

        // Create a DrawingVisual that contains an ellipse.
        private System.Windows.Media.DrawingVisual CreateDrawingVisualEllipses()
        {
            System.Windows.Media.DrawingVisual drawingVisual = new System.Windows.Media.DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            drawingContext.DrawEllipse(Brushes.Maroon, null, new Point(430, 136), 20, 20);
            drawingContext.Close();

            return drawingVisual;
        }
        private DrawingVisual CreateDrawingVisualAA()
        {
            System.Windows.Media.DrawingVisual drawingVisual = new System.Windows.Media.DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            drawingVisual.XSnappingGuidelines = new DoubleCollection(new List<double>() { 1 });
            drawingVisual.YSnappingGuidelines = new DoubleCollection(new List<double>() { 1 });
            drawingContext.DrawLine(new Pen(Brushes.Maroon, 1), new Point(430, 136), new Point(136, 430));
            drawingContext.Close();

            return drawingVisual;

        }
        private DrawingVisual CreateDrawingImage()
        {
            var uri = $"wallpaper_mikael_gustafsson.png";

            System.Windows.Media.DrawingVisual drawingVisual = new System.Windows.Media.DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            drawingVisual.XSnappingGuidelines = new DoubleCollection(new List<double>() { 1 });
            drawingVisual.YSnappingGuidelines = new DoubleCollection(new List<double>() { 1 });
            //Image image = new Image() { Source =  };
            var source = new BitmapImage(new Uri(uri, UriKind.Relative));
            drawingContext.DrawImage(source, new Rect(new Size(800, 450)));
            drawingContext.Close();

            return drawingVisual;

        }


        // Provide a required override for the VisualChildrenCount property.
        protected override int VisualChildrenCount => _children.Count;

        // Provide a required override for the GetVisualChild method.
        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _children.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return _children[index];
        }
    }    /// <summary>
         /// WindowCharting2.xaml 的交互逻辑
         /// </summary>
    public partial class WindowCharting2 : Window
    {
        public WindowCharting2()
        {
            InitializeComponent();
            Grid grid = new Grid();
            var host = new MyVisualHost();
            grid.Children.Add(host);
            this.Content = grid;
            BlurryUserControl b = new BlurryUserControl() { };
            b.BorderBrush = Brushes.White;
            b.BorderThickness = new Thickness(2);
            b.Background = Brushes.Transparent;
            b.BlurContainer = host;
            b.Width = 300;
            b.Height = 300;
            b.Magnification = 0.25;
            b.BlurRadius = 45;
            Panel.SetZIndex(b, 100);
            grid.Children.Add(b);

        }
    }
}
