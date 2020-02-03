using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WPFAnimation
{
    public enum AxisPosition
    {
        Left = 1,
        Buttom = 2,
        Right = 4,
        Top = 8,
    }

    public class PaddingOffset
    {
        public static PaddingOffset Default => new PaddingOffset();

        public GridLength Top { get; set; } = GridLength.Auto;
        public GridLength Left { get; set; } = GridLength.Auto;
        public GridLength Right { get; set; } = GridLength.Auto;
        public GridLength Buttom { get; set; } = GridLength.Auto;
        public Size Parent { get; set; } = new Size();

        public double TopOffset
        {
            get
            {
                return Top.GetActualLength(Parent.Width);
            }
        }
        public double LeftOffset
        {
            get
            {
                return Left.GetActualLength(Parent.Width);
            }
        }

        public double RightOffset
        {
            get
            {
                return Right.GetActualLength(Parent.Width);
            }
        }

        public double ButtomOffset
        {
            get
            {
                return Buttom.GetActualLength(Parent.Height);
            }
        }

    }
    /// <summary>
    /// Asix 公用组件属性 框架内部不可修改值
    /// </summary>
    public interface IAxisVisualConfiguare
    {
        Pen CrossPen { get; set; }
    }

    /// <summary>
    /// 独立属性 框架内部不可修改值
    /// </summary>
    public interface IAxisConfiguare
    {
        AxisPosition Position { get; set; }
        string ValueFormat { get; set; }
        Pen AxisPen { get; set; }

        PaddingOffset Offsets { get; set; }
        List<double> Ratios { get; set; }

        //GridLine
        bool ShowGridLine { get; set; }
        Pen GridLinePen { get; set; }

        //AxisLabel
        bool IsAxisLabelShow { get; set; }
        GridLength AxisLabelOffset { get; set; }
        AxisLabel AxisLabel { get; set; }

    }
}
