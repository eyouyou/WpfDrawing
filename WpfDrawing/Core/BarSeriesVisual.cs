using HevoDrawing.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace HevoDrawing
{
    public class BarSeriesVisual : PointsSeriesVisual
    {
        public Pen Pen { get; set; } = new Pen();
        public Func<IVariable, Brush> Fill { get; set; } = data => Brushes.Black;
        public GridLength BarWidth { get; set; } = new GridLength(1, GridUnitType.Star);
        public bool ShowData { get; set; } = true;
        public GridLength DataVericalMargin { get; set; } = new GridLength(0);
        public double MinHeight { get; set; } = 10;
        public override Func<IVariable, Brush> Color
        {
            get
            {
                if (base.Color != null)
                {
                    return base.Color;
                }
                if (Fill != null)
                {
                    return data => Fill(data);
                }
                return data => Brushes.Black;
            }
            set => base.Color = value;
        }

        public override void PlotToDc(DrawingContext dc)
        {
            var points = Croods;
            if (points.Count == 0)
            {
                return;
            }
            if (!VisualData.TryTransformVisualData<TwoDimensionalContextData>(out var visual_data))
            {
                return;
            }
            var coms = DataSource as ChartDataSource;

            var x = coms.FindXById(Id) as DiscreteAxis;
            var y = coms.FindYById(Id) as ContinuousAxis;
            var plotArea = x.PlotArea;
            var startx = x.Start;
            var index = 0;
            var valuecoords = x.ValueRatioCoordinates;

            var @base = visual_data.YContextData.Range.Base;
            var base_line_position = y.GetPosition(@base).Y + y.Start.Y;

            dc.PushClip(new RectangleGeometry() { Rect = plotArea });

            foreach (var item in points)
            {
                if (index >= valuecoords.Count)
                {
                    break;
                }

                var crood = valuecoords[index];
                var width = BarWidth.GetActualLength((crood.Right - crood.Left) * plotArea.Width);
                var actual_right = crood.Right;
                if (crood.Right > 1)
                {
                    actual_right = 1;
                }
                else if (crood.Right < 0)
                {
                    actual_right = 0;
                }
                var actual_left = crood.Left;
                if (crood.Left > 1)
                {
                    actual_left = 1;
                }
                else if (crood.Left < 0)
                {
                    actual_left = 0;
                }
                var actual_width = BarWidth.GetActualLength((actual_right - actual_left) * plotArea.Width);
                var left_offset = BarWidth.GetActualLength((crood.Current - crood.Left) * plotArea.Width);

                var isUp = item.Y.IsBiggerThanOrEquals(@base);

                var offset = Math.Abs(item.Point.Y - base_line_position);
                var pointY = isUp ? item.Point.Y : base_line_position;

                if (offset < MinHeight)
                {
                    pointY -= MinHeight - offset;
                    offset = MinHeight;
                }

                var leftTopX = item.Point.X - left_offset;
                var centerX = leftTopX + actual_width / 2;
                dc.DrawRectangle(Fill(item.X), Pen, new Rect(new Point(leftTopX, pointY), new Size(width, offset)));

                if (ShowData)
                {
                    FormattedText formatted_text = new FormattedText(
                        $"{item.Y.ToString(y.SplitValueFormat, y.FormatProvider)}{y.SplitUnit}",
                        CultureInfo.InvariantCulture,
                        FlowDirection.LeftToRight,
                        new Typeface("Microsoft YaHei"),
                        y.ChartFontSize,
                        Brushes.Black);
                    var dataMargin = Tools.GetActualLength(DataVericalMargin, plotArea.Height);
                    var text_point = new Point(centerX - formatted_text.Width / 2, pointY - formatted_text.Height - dataMargin);
                    dc.DrawText(formatted_text, text_point);
                }
                index++;
            }
            dc.Pop();

        }

        public override void Freeze()
        {
            if (Pen.CanFreeze)
            {
                Pen.Freeze();
            }
        }
    }
}
