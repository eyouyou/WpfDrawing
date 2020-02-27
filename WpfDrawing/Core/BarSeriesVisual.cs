using HevoDrawing.Abstractions;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace HevoDrawing
{
    public class BarSeriesVisual : PointsSeriesVisual
    {
        public Pen Pen { get; set; } = new Pen();
        public Func<IVariable, Brush> Fill { get; set; } = data => Brushes.Black;
        public GridLength BarWidth { get; set; } = new GridLength(1, GridUnitType.Star);

        public double MinHeight { get; set; } = 0;
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
            var coms = DataSource as ChartDataSource;

            var x = coms.FindXById(Id) as DiscreteAxis;
            var plotArea = x.PlotArea;
            var startx = x.Start;
            var index = 0;
            var valuecoords = x.ValueRatioCoordinates;

            dc.PushClip(new RectangleGeometry() { Rect = plotArea });

            foreach (var item in points)
            {
                var all_width = 0.0;
                if (index < valuecoords.Count)
                {
                    var crood = valuecoords[index];
                    all_width = (crood.Right - crood.Left) * plotArea.Width;
                }
                var width = BarWidth.GetActualLength(all_width);

                var offset = Math.Abs(item.Point.Y - startx.Y);
                var pointY = item.Point.Y;

                if (offset < MinHeight)
                {
                    pointY -= MinHeight - offset;
                    offset = MinHeight;
                }

                var leftTopX = item.Point.X - width / 2;
                dc.DrawRectangle(Fill(item.X), Pen, new Rect(new Point(leftTopX, pointY), new Size(width, offset)));
                index++;
            }
            dc.Pop();

        }
    }
}
