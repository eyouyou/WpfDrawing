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
    /// Interaction logic for Test3D.xaml
    /// </summary>
    public partial class Test3D : Window
    {
        public Test3D()
        {
            InitializeComponent();
        }
        public void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DoubleAnimation da = new DoubleAnimation();
            da.Duration = new Duration(TimeSpan.FromSeconds(1));
            if (e.ClickCount == 2)
                da.To = 0d;
            else
                da.To = 180d;
            System.Windows.Media.Media3D.AxisAngleRotation3D aar = Application.Current.MainWindow.FindName("aar") as AxisAngleRotation3D;
            aar.BeginAnimation(AxisAngleRotation3D.AngleProperty, da);

        }
    }
}
