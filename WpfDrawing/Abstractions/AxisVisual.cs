using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HevoDrawing.Abstractions
{
    public struct ElementPosition
    {
        public ElementPosition(UIElement element, bool isVisible = false, double left = double.NaN, double top = double.NaN, int zIndex = -1)
        {
            Element = element;
            Left = left;
            Top = top;
            IsVisible = isVisible;
            ZIndex = zIndex;
        }
        public UIElement Element { get; set; }
        public double Top { get; set; }
        public double Left { get; set; }
        public bool IsVisible { get; set; }
        public int ZIndex { get; set; }

        public void Render(bool forceHidden = false)
        {
            if (Element == null)
            {
                return;
            }

            if (forceHidden)
            {
                Element.Visibility = Visibility.Collapsed;
                return;
            }

            if (IsVisible)
            {
                Canvas.SetLeft(Element, Math.Round(Left, 1));
                Canvas.SetTop(Element, Math.Round(Top, 1));
                Canvas.SetZIndex(Element, ZIndex);
                Element.Visibility = Visibility.Visible;
            }
            else
            {
                Element.Visibility = Visibility.Collapsed;
            }

        }
    }

    public class PointVector
    {
        public Vector Start { get; set; }
        public Vector End { get; set; }
    }
    /// <summary>
    /// 默认y向上，x向右 如果有需要 在基类调整
    /// </summary
    public abstract class AxisVisual : SubRectDrawingVisual, IAxisConfiguare
    {
        public Vector Start
        {
            get
            {
                var plotArea = PlotArea;
                Vector start = new Vector();
                switch (Position)
                {
                    case AxisPosition.Left:
                    case AxisPosition.Buttom:
                        {
                            start.Y = plotArea.Bottom;
                            start.X = plotArea.Location.X;
                        }
                        break;
                    case AxisPosition.Right:
                        {
                            start.X = plotArea.Right;
                            start.Y = plotArea.Bottom;
                        }
                        break;
                    case AxisPosition.Top:
                        {
                            start.Y = plotArea.Top;
                            start.X = plotArea.Left;
                        }
                        break;
                }
                return start;
            }
        }
        public Vector End
        {
            get
            {
                var plotArea = PlotArea;
                Vector end = new Vector();
                switch (Position)
                {
                    case AxisPosition.Left:
                        {
                            end.X = plotArea.Left;
                            end.Y = plotArea.Top;
                        }
                        break;
                    case AxisPosition.Right:
                    case AxisPosition.Top:
                        {
                            end.X = plotArea.Right;
                            end.Y = plotArea.Top;
                        }
                        break;
                    case AxisPosition.Buttom:
                        {
                            end.X = plotArea.Right;
                            end.Y = plotArea.Bottom;
                        }
                        break;
                }
                return end;
            }
        }

        public double OffsetPostion(double position)
        {
            var offset = 0.0;
            switch (Position)
            {
                case AxisPosition.Left:
                case AxisPosition.Right:
                    {
                        var start = Start.Y;
                        var dir = End.Y - start > 0;
                        offset = dir ? position - start : start - position;
                    }
                    break;
                case AxisPosition.Buttom:
                case AxisPosition.Top:
                    {
                        var start = Start.X;
                        var dir = End.X - start > 0;
                        offset = dir ? position - start : start - position;
                    }
                    break;
            }
            return offset;
        }

        /// <summary>
        /// 放在value之后
        /// </summary>
        public string Unit { get; set; } = string.Empty;

        /// <summary>
        /// 放在value之后
        /// </summary>
        public string SplitUnit { get; set; } = string.Empty;
        public string ValueFormat { get; set; }
        public string SplitValueFormat { get; set; }
        public AxisPosition Position { get; set; }
        public GridLength ShowValueMargin { get; set; } = new GridLength(5);
        public Pen AxisPen { get; set; } = new Pen(Brushes.Black, 1);

        public double ChartFontSize { get; set; } = 12;
        /// <summary>
        /// 不输入则平均
        /// </summary>
        public List<double> Ratios { get; set; } = null;
        /// <summary>
        /// 不输入按照ratios和data来
        /// </summary>
        public List<IVariable> SplitValues { get; set; }
        /// <summary>
        /// 显示比例 
        /// </summary>
        public double ShowRatio { get; set; } = double.NaN;
        public Brush SplitForeColor { get; set; } = Brushes.Black;
        public int SplitFontSize { get; set; } = 12;
        public bool ShowGridLine { get; set; } = false;

        /// <summary>
        /// 封闭 ？
        /// </summary>
        public bool IsGridLineClose { get; set; } = true;
        public Pen GridLinePen { get; set; } = new Pen(Brushes.Gray, 1);
        public bool IsAxisLabelShow { get; set; } = false;
        public GridLength AxisLabelOffset { get; set; } = GridLength.Auto;
        public AxisLabel AxisLabel { get; set; } = new AxisLabel() { Padding = new Thickness(4), Background = Brushes.DarkBlue, Foreground = Brushes.White, Opacity = 1 };
        public abstract IFormatProvider FormatProvider { get; }


        public abstract void CalculateRequireData();
        public volatile bool IsDataComplete = false;
        public override void Freeze()
        {
            if (AxisPen.CanFreeze)
            {
                AxisPen.Freeze();
            }
            if (GridLinePen.CanFreeze)
            {
                GridLinePen.Freeze();
            }
        }

    }
    public abstract class AxisVisual<T> : AxisVisual
    {
        public abstract string GetStringValue(T value);
        public abstract Vector GetPosition(T value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="offsetPosition"></param>
        /// <param name="withOutOfBoundData">超出边界按照最近的来</param>
        /// <returns></returns>
        public abstract T GetValue(double offsetPosition, bool withOutOfBoundData = false);
        public AxisLabelData GetAxisLabelData(T value)
        {
            var position = GetPosition(value);
            if (position.IsBad())
            {
                return AxisLabelData.Bad;
            }
            var data = new AxisLabelData();

            var plotArea = PlotArea;
            var margin = AxisLabelOffset.GetActualLength(plotArea.Height);
            AxisLabel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            switch (Position)
            {
                case AxisPosition.Left:
                    {
                        data.Top = Start.Y - Math.Abs(position.Y) - AxisLabel.DesiredSize.Height / 2;
                        data.Left = Start.X - AxisLabel.DesiredSize.Width - margin;
                    }
                    break;
                case AxisPosition.Buttom:
                    {
                        data.Top = Start.Y + margin;
                        data.Left = position.X - AxisLabel.DesiredSize.Width / 2 + Start.X;
                    }
                    break;
                case AxisPosition.Right:
                    {
                        data.Top = Start.Y - Math.Abs(position.Y) - AxisLabel.DesiredSize.Height / 2;
                        data.Left = Start.X + margin;
                    }
                    break;
                case AxisPosition.Top:
                    {
                        data.Top = Start.Y - AxisLabel.DesiredSize.Height - margin;
                        data.Left = Start.X - AxisLabel.DesiredSize.Width / 2 + position.X;
                    }
                    break;
                default:
                    break;
            }
            return data;
        }

    }
    public class AxisLabelData
    {
        public static AxisLabelData Bad => new AxisLabelData() { IsBad = true };

        public double Top { get; set; }
        public double Left { get; set; }
        public bool IsBad { get; private set; } = false;
    }
    public class AxisLabel : TextBlock
    {
        public int ZIndex { get; set; } = 1;
    }
}
