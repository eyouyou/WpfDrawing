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
        public RectInteractionGroup(AxisInteractionCanvas interaction, int col = 1, int row = 1, params RectDrawingCanvas[] canvas)
        {
            InteractionVisuals = interaction;
            InteractionVisuals.ParentElement = this;

            var colPercent = 1.0 / col;
            for (int i = 0; i < col; i++)
            {
                ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(colPercent, GridUnitType.Star) });
            }

            var rowPercent = 1.0 / row;
            for (int i = 0; i < row; i++)
            {
                RowDefinitions.Add(new RowDefinition() { Height = new GridLength(rowPercent, GridUnitType.Star) });
            }
            for (int i = 0; i < canvas.Length; i++)
            {
                var item = canvas[i];
                Children.Add(item);
                SetColumn(item, i % col);
                SetRow(item, i / col);
                interaction.DataSource.Add(item.Id, item.DataSource);
            }

            SetColumn(interaction, 0);
            SetRow(interaction, 0);
            SetColumnSpan(interaction, col);
            SetRowSpan(interaction, row);

            EnableInteraction = true;
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
        public InteractionCanvas InteractionVisuals { get; } = null;
    }
}
