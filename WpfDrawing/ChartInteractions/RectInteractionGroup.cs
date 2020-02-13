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
        Dictionary<int, RectDrawingCanvas> Canvas;
        ComponentId IdGenerater = new ComponentId();
        double ColPercentage = 0.0;
        double RowPercentage = 0.0;
        public RectInteractionGroup(InteractionCanvas interaction, int col = 1, int row = 1, params RectDrawingCanvas[] canvas)
        {
            InteractionVisuals = interaction;
            InteractionVisuals.ParentElement = this;

            ColPercentage = 1.0 / col;
            for (int i = 0; i < col; i++)
            {
                ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(ColPercentage, GridUnitType.Star) });
            }

            RowPercentage = 1.0 / row;
            for (int i = 0; i < row; i++)
            {
                RowDefinitions.Add(new RowDefinition() { Height = new GridLength(RowPercentage, GridUnitType.Star) });
            }
            var index = 0;
            foreach (var item in canvas)
            {
                var value = item;
                Children.Add(value);
                item.Col = index % col;
                item.Row = index / col;
                SetColumn(value, item.Col);
                SetRow(value, item.Row);
                if (value.Id == int.MinValue)
                {
                    value.Id = IdGenerater.GenerateId();
                }

                interaction?.DataSources.Add(value.Id, value.DataSource);
                item.InteractionCanvas = interaction;
                index++;
            }
            Canvas = canvas.ToDictionary(it => it.Id, it => it);

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
                    canvas.Offset = new Vector(canvas.Col * ColPercentage * ActualWidth, canvas.Row * RowPercentage * ActualHeight);
                    canvas.CurrentLocation = Point.Add(new Point(0, 0), canvas.Offset);

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
