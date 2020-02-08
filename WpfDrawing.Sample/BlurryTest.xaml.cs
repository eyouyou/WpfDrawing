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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfDrawing.Sample
{
    /// <summary>
    /// BlurryTest.xaml 的交互逻辑
    /// </summary>
    public partial class BlurryTest : Window
    {
        public BlurryTest()
        {
            InitializeComponent();
            Grid grid = new Grid();
            this.Content = grid;
            var uri = $"/wallpaper_mikael_gustafsson.png";
            Canvas grid2 = new Canvas();
            Canvas grid3 = new Canvas();
            grid.Children.Add(grid2);
            grid.Children.Add(grid3);
            grid2.Children.Add(new Image() { Source = new BitmapImage(new Uri(uri, UriKind.Relative)) });
            BlurryUserControl b = new BlurryUserControl() { };
            b.BorderBrush = Brushes.White;
            b.BorderThickness = new Thickness(2);
            b.Background = Brushes.Transparent;
            b.BlurContainer = grid2;
            b.Width = 300;
            b.Height = 300;
            b.Magnification = 0.25;
            b.BlurRadius = 45;
            grid3.Children.Add(b);
            Panel.SetZIndex(b, 100);

        }
    }
}
