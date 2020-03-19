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

namespace HevoDrawing.Sample
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
            Grid grid2 = new Grid();
            grid2.ColumnDefinitions.Add(new ColumnDefinition());
            grid2.ColumnDefinitions.Add(new ColumnDefinition());

            grid2.RowDefinitions.Add(new RowDefinition());
            grid2.RowDefinitions.Add(new RowDefinition());

            Canvas grid3 = new Canvas();
            grid.Children.Add(grid2);
            grid.Children.Add(grid3);

            var image = new Image() { Source = new BitmapImage(new Uri(uri, UriKind.Relative)), Margin = new Thickness(5) };
            var image2 = new Image() { Source = new BitmapImage(new Uri(uri, UriKind.Relative)), Margin = new Thickness(5) };
            var image3 = new Image() { Source = new BitmapImage(new Uri(uri, UriKind.Relative)), Margin = new Thickness(5) };
            var image4 = new Image() { Source = new BitmapImage(new Uri(uri, UriKind.Relative)), Margin = new Thickness(5) };

            grid2.AddElement(image, 0, 0);
            grid2.AddElement(image2, 1, 0);
            grid2.AddElement(image3, 0, 1);
            grid2.AddElement(image4, 1, 1);

            BlurryUserControl b = new BlurryUserControl() { };
            b.BorderBrush = Brushes.White;
            b.BorderThickness = new Thickness(2);
            b.Background = Brushes.Transparent;
            b.BlurContainer = this.Content as UIElement;
            b.Width = 300;
            b.Height = 300;
            b.Magnification = 0.25;
            b.BlurRadius = 45;
            ToolTip = b;
            Panel.SetZIndex(b, 100);

        }
    }
}
