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
        public Brush Fill { get; set; } = Brushes.Black;
        public GridLength BarWidth { get; set; } = new GridLength(1, GridUnitType.Star);
        public override Brush Color => Brushes.Black;

        public override void PlotToDc(DrawingContext dc)
        {
            var points = Points;
            if (points.Count == 0)
            {
                return;
            }
            var coms = DataSource as ChartDataSource;

            var x = coms.FindXById(Id) as DiscreteAxis;
            var plotArea = x.PlotArea;
            var startx = x.Start;
            var index = 0;
            //var valuecoords = new List<double>(x.ValueRatioCoordinates);

            //valuecoords.Insert(0, 0);
            //valuecoords.Add(1);

            //dc.PushClip(new RectangleGeometry() { Rect = plotArea });

            ///*
            // * 如果value
            // */
            //foreach (var item in points)
            //{
            //    var all_width = 0.0;
            //    if (index + 1 < valuecoords.Count)
            //    {
            //        all_width = (valuecoords[index + 1] - valuecoords[index]) * plotArea.Width;
            //    }
            //    var width = BarWidth.GetActualLength(all_width);


            //    var leftTopX = item.X - width / 2;
            //    dc.DrawRectangle(Fill, Pen, new Rect(new Point(leftTopX, item.Y), new Size(width, Math.Abs(item.Y - startx.Y))));
            //    index++;
            //}
            //dc.Pop();

        }
    }
}
