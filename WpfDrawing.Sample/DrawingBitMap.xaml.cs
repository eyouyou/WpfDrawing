using System;
using System.Collections.Generic;
using System.Drawing;
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
    /// DrawingBitMap.xaml 的交互逻辑
    /// </summary>
    public partial class DrawingBitMap : Window
    {
        public DrawingBitMap()
        {
            Bitmap bitmap = new Bitmap(100, 200);
            bitmap.Save(@"");
            InitializeComponent();
        }
    }
}
