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
using System.Windows.Shapes;

namespace WPFAnimation
{
    /// <summary>
    /// Interaction logic for _3DRotate.xaml
    /// </summary>
    public partial class _3DRotate : Window
    {
        //Canvas canvas = new Canvas();
        Grid grid = new Grid() { Width = 250, Height = 250 };
        public _3DRotate()
        {
            InitializeComponent();
            this.Content = grid;
            var brush = (Path)FindResource("WE");
            var brush2 = (Path)FindResource("ER");
            Viewport3D viewport3D = new Viewport3D();
            viewport3D.Camera = new PerspectiveCamera() { Position = new Point3D(0, 0, 72), LookDirection = new Vector3D(0, 0, -1) };
            var material = new DiffuseMaterial() { };
            var v1 = new Viewport2DVisual3D() { Visual = brush, Material = material, };
            Viewport2DVisual3D.SetIsVisualHostMaterial(material, true);
            v1.Geometry = new MeshGeometry3D()
            {
                Positions = new Point3DCollection(
                new List<Point3D>() { new Point3D(-30, 30, 0), new Point3D(-30, -30, 0), new Point3D(30, -30, 0), new Point3D(30, 30, 0) }),
                TriangleIndices = new Int32Collection(new List<int>() { 0, 1, 2, 0, 2, 3 }),
                TextureCoordinates = new PointCollection(new List<Point>() { new Point(0, 0), new Point(0, 1), new Point(1, 1), new Point(1, 0) })
            };
            var material2 = new DiffuseMaterial() { };
            var v2 = new Viewport2DVisual3D() { Visual = brush2, Material = material2 };
            Viewport2DVisual3D.SetIsVisualHostMaterial(material2, true);
            v2.Geometry = new MeshGeometry3D()
            {
                Positions = new Point3DCollection(
                new List<Point3D>() { new Point3D(30, 30, 0), new Point3D(30, -30, 0), new Point3D(-30, -30, 0), new Point3D(-30, 30, 0) }),
                TriangleIndices = new Int32Collection(new List<int>() { 0, 1, 2, 0, 2, 3 }),
                TextureCoordinates = new PointCollection(new List<Point>() { new Point(0, 0), new Point(0, 1), new Point(1, 1), new Point(1, 0) })
            };
            ContainerUIElement3D containerUIElement3D = new ContainerUIElement3D();

            viewport3D.Children.Add(containerUIElement3D);
            containerUIElement3D.Children.Add(v1);
            containerUIElement3D.Children.Add(v2);

            AxisAngleRotation3D aaa = new AxisAngleRotation3D() { Angle = 0, Axis = new Vector3D(0, 1, 0) };
            RegisterName("aar", aaa);

            RotateTransform3D rotate = new RotateTransform3D();
            rotate.Rotation = aaa;
            containerUIElement3D.Transform = rotate;

            ModelVisual3D modelVisual3D = new ModelVisual3D();
            var light = new AmbientLight() { Color = Color.FromArgb(255, 255, 255, 255),  };
            modelVisual3D.Content = light;
            viewport3D.Children.Add(modelVisual3D);

            grid.Children.Add(viewport3D);
            grid.Background = Brushes.Yellow;
            var i = 0;
            Button btn_test = new Button() { Content = "test" };
            //grid.Children.Add(btn_test);

            grid.MouseLeftButtonDown += (sender, e) =>
            {
                DoubleAnimation da = new DoubleAnimation();
                da.Duration = new Duration(TimeSpan.FromSeconds(1));
                if (i % 2 == 1)
                    da.To = 0d;
                else
                    da.To = 180d;
                AxisAngleRotation3D aar = Application.Current.MainWindow.FindName("aar") as AxisAngleRotation3D;
                aar.BeginAnimation(AxisAngleRotation3D.AngleProperty, da);

                i++;
            };

        }
    }
}
