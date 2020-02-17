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
    /// Window4.xaml 的交互逻辑
    /// </summary>
    public partial class Window4 : Window
    {
        Grid grid = new Grid();
        public Window4()
        {


            InitializeComponent();
            this.Resources = new ResourceDictionary() { Source = new Uri("pack://Application:,,,/WpfDrawing.Sample;component/ScrollViewRes.xaml") };
            ScrollViewer scrollViewer = new ScrollViewer() {  HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            scrollViewer.Content = new Label() { Content = "DAreDADAqqqqqweqweqweqweqweqweqweqweqwewwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwweeeeeqq", Background = Brushes.Red, Height = 500, Width = 500 };
            grid.Children.Add(scrollViewer);
            Content = grid;
        }
    }
}
