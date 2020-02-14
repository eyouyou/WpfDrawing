using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace WpfDrawing
{
    /// <summary>
    /// 在此基础上可以做多个图表的操作
    /// 组合visual和canvas
    /// </summary>
    public class RectInteractionGroup : Grid
    {
        public Grid DrawingGrid = new Grid();
        Dictionary<int, RectDrawingCanvas> Canvas;
        ComponentId IdGenerater = new ComponentId();
        double ColPercentage = 0.0;
        double RowPercentage = 0.0;
        public RectInteractionGroup(InteractionCanvas interaction, int col = 1, int row = 1, params RectDrawingCanvasContainer[] canvas)
        {
            InteractionVisuals = interaction;
            InteractionVisuals.ParentElement = this;

            ColPercentage = 1.0 / col;
            for (int i = 0; i < col; i++)
            {
                DrawingGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(ColPercentage, GridUnitType.Star) });
            }

            RowPercentage = 1.0 / row;
            for (int i = 0; i < row; i++)
            {
                DrawingGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(RowPercentage, GridUnitType.Star) });
            }
            var index = 0;
            foreach (var item in canvas)
            {
                var c = item.Canvas;
                DrawingGrid.Children.Add(item);
                c.Col = index % col;
                c.Row = index / col;
                SetColumn(item, c.Col);
                SetRow(item, c.Row);
                if (c.Id == -1)
                {
                    c.Id = IdGenerater.GenerateId();
                }

                interaction?.DataSources.Add(c.Id, c.DataSource);
                c.InteractionCanvas = interaction;
                index++;
            }
            Canvas = canvas.ToDictionary(it => it.Canvas.Id, it => it.Canvas);

            Children.Add(DrawingGrid);

            if (interaction != null)
            {
                SetColumn(interaction, 0);
                SetRow(interaction, 0);
                SetColumnSpan(interaction, col);
                SetRowSpan(interaction, row);

                EnableInteraction = true;
            }

            SizeChanged += RectInteractionGroup_SizeChanged;
        }

        private void RectInteractionGroup_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (IsVisible)
            {
                var index = 0;
                foreach (var item in Canvas)
                {
                    var canvas = item.Value;
                    canvas.InteractionCanvas.Hide();
                    var point = canvas.TranslatePoint(new Point(0, 0), this);
                    canvas.CurrentLocation = point;
                    canvas.Offset = new Vector(point.X, point.Y);
                    index++;
                }
            }
        }

        private bool _enableInteraction = false;
        public bool EnableInteraction
        {
            get => _enableInteraction;
            set
            {
                if (InteractionVisuals != null)
                {
                    if (value && !_enableInteraction)
                    {
                        Children.Add(InteractionVisuals);
                    }
                    else if (!value && _enableInteraction)
                    {
                        Children.Remove(InteractionVisuals);
                    }
                }
                _enableInteraction = value;
            }
        }


        public void Replot()
        {
            foreach (var item in Canvas)
            {
                item.Value.Replot();
            }
        }
        /// <summary>
        /// 可空 调用时需要判断
        /// </summary>
        public InteractionCanvas InteractionVisuals { get; } = null;
    }
}
