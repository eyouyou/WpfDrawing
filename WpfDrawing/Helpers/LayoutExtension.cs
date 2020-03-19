using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HevoDrawing
{
    public static class LayoutExtension
    {
        public static void AddChild(this DockPanel panel, UIElement element, Dock dock)
        {
            DockPanel.SetDock(element, dock);
            panel.Children.Add(element);
        }
        public static void AddElement(this Grid grid, UIElement element, int col, int row, int col_span = 1, int row_span = 1)
        {
            grid.Children.Add(element);
            Grid.SetColumn(element, col);
            Grid.SetRow(element, row);
            Grid.SetColumnSpan(element, col_span);
            Grid.SetRowSpan(element, row_span);
        }
    }
}
