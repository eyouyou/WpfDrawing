using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using HevoDrawing.Abstractions;

namespace HevoDrawing.Charting
{
    /// <summary>
    /// 在此基础上可以做多个图表的操作
    /// 组合visual和canvas
    /// </summary>
    public class RectInteractionGroup : Grid
    {
        public GridLength CellMargin { get; set; } = new GridLength(0);
        public Grid ContainerGrid = new Grid();
        Dictionary<int, RectDrawingCanvas> Canvas;
        public List<ChartPack> Containers;
        List<Chart> ContainersUsed;
        List<Chart> ContainersUnused;
        ComponentId IdGenerater = new ComponentId();
        int Col = -1;
        int Row = -1;
        InteractionCanvas GlobalInteraction = null;
        public RectInteractionGroup(InteractionCanvas interaction, int col = 1, int row = 1, params ChartPack[] canvas)
        {
            InteractionVisuals = new List<InteractionCanvas>();
            Containers = canvas.ToList();
            ContainersUsed = new List<Chart>(Containers);
            ContainersUnused = new List<Chart>();
            Containers.ForEach(it => ContainerGrid.Children.Add(it.ChartTemplate));
            GlobalInteraction = interaction;
            if (interaction != null)
            {
                SetColumn(interaction, 0);
                SetRow(interaction, 0);
                SetColumnSpan(interaction, col);
                SetRowSpan(interaction, row);
                SizeChanged += RectInteractionGroup_SizeChanged;
            }
            else
            {
                SizeChanged += RectInteractionGroup_SizeChanged2;
            }
            Resize(col, row);

            Canvas = canvas.ToDictionary(it => it.DrawingCanvas.Id, it => it.DrawingCanvas);

            Children.Add(ContainerGrid);

            EnableInteraction = true;
        }
        public void Resize(int col, int row)
        {
            Tools.GetActualLength(CellMargin, ActualWidth);

            if (Col == col && Row == row)
            {
                return;
            }
            else
            {
                Col = col;
                Row = row;
            }
            var col_percent = 1.0 / col;
            ContainerGrid.ColumnDefinitions.Clear();
            var total = col * row;
            for (int i = 0; i < col; i++)
            {
                ContainerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(col_percent, GridUnitType.Star) });
            }

            var row_percent = 1.0 / row;
            ContainerGrid.RowDefinitions.Clear();
            for (int i = 0; i < row; i++)
            {
                ContainerGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(row_percent, GridUnitType.Star) });
            }
            ContainerGrid.UpdateLayout();
            InteractionVisuals.Clear();
            if (GlobalInteraction != null)
            {
                InteractionVisuals.Add(GlobalInteraction);
                GlobalInteraction.ParentElement = this;
            }

            var index = 0;
            ContainersUsed.Clear();
            ContainersUnused.Clear();
            foreach (var chart in Containers)
            {
                var item = chart.ChartTemplate;
                if (index >= total)
                {
                    ContainersUnused.Add(chart);
                    continue;
                }
                var c = chart.DrawingCanvas;
                ContainersUsed.Add(chart);
                c.Col = index % col;
                c.Row = index / col;
                SetColumn(item, c.Col);
                SetRow(item, c.Row);
                SetColumnSpan(item, 1);
                SetRowSpan(item, 1);
                if (c.Id == -1)
                {
                    c.Id = IdGenerater.GenerateId();
                }
                if (GlobalInteraction != null)
                {
                    GlobalInteraction.Assemblies.Clear();
                    GlobalInteraction.Assemblies.Add(c.Id, c.Assembly);
                    GlobalInteraction.AssociatedCharts.Clear();
                    GlobalInteraction.AssociatedCharts.Add(c.Id, chart);
                    c.InteractionCanvas = GlobalInteraction;
                }
                else
                {
                    // 送默认AxisInteractionCanvas
                    var interaction2 = chart.InteractionCanvas;
                    if (interaction2 != null)
                    {
                        interaction2.AssociatedCharts.Clear();
                        interaction2.AssociatedCharts.Add(c.Id, chart);
                        interaction2.Assemblies.Clear();
                        interaction2.Assemblies.Add(c.Id, c.Assembly);
                        interaction2.ParentElement = item;
                        c.InteractionCanvas = interaction2;
                        chart.EnableInteraction = false;
                        chart.EnableInteraction = true;
                        InteractionVisuals.Add(interaction2);
                    }
                }
                index++;
            }

        }
        public RectInteractionGroup(int col = 1, int row = 1, params ChartPack[] canvas) : this(null, col, row, canvas)
        {

        }
        private void RectInteractionGroup_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (IsVisible)
            {
                var index = 0;
                foreach (var item in ContainersUsed)
                {
                    var canvas = item.DrawingCanvas;
                    canvas.InteractionCanvas.Hide();
                    var point = canvas.TranslatePoint(new Point(0, 0), this);
                    canvas.CurrentLocation = point;
                    canvas.Offset = new Vector(point.X, point.Y);
                    index++;
                }
            }
        }
        private void RectInteractionGroup_SizeChanged2(object sender, SizeChangedEventArgs e)
        {
            if (IsVisible)
            {
                var index = 0;
                foreach (var item in ContainersUsed)
                {
                    var canvas = item.DrawingCanvas;
                    canvas.InteractionCanvas?.Hide();
                    var point = canvas.TranslatePoint(new Point(0, 0), item);

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
                        foreach (var item in InteractionVisuals)
                        {
                            if (!item.Standalone)
                            {
                                Children.Add(item);
                            }
                        }
                    }
                    else if (!value && _enableInteraction)
                    {
                        foreach (var item in InteractionVisuals)
                        {
                            if (!item.Standalone)
                            {
                                Children.Remove(item);
                            }
                        }
                    }
                }
                _enableInteraction = value;
            }
        }


        public void Replot()
        {
            foreach (var item in ContainersUnused)
            {
                if (item.IsVisible)
                {
                    item.Visibility = Visibility.Collapsed;
                }
            }

            foreach (var item in ContainersUsed)
            {
                if (!item.IsVisible)
                {
                    item.Visibility = Visibility.Visible;
                }
                else
                {
                    item.DrawingCanvas.Replot();
                }
            }
        }
        /// <summary>
        /// 可空 调用时需要判断
        /// </summary>
        public List<InteractionCanvas> InteractionVisuals { get; } = null;
    }
}
