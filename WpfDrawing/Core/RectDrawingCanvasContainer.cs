using HevoDrawing.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HevoDrawing
{
    public abstract class DrawingGrid : UserControl
    {
        /// <summary>
        /// 放置<see cref="InteractionCanvas"/> 的容器
        /// </summary>
        protected Grid DrawingCanvasArea { get; } = new Grid();

        /// <summary>
        /// 已加入 <see cref="DrawingCanvasArea"/>
        /// </summary>
        public RectDrawingCanvas DrawingCanvas { get; } = new RectDrawingCanvas();

        public DrawingGrid()
        {
            DrawingCanvasArea.Children.Add(DrawingCanvas);
        }
        /// <summary>
        /// 如果该值为空，
        /// </summary>
        public virtual InteractionCanvas InteractionCanvas { get; } = null;

        #region 交互独立情况下

        public void EnableInteraction()
        {
            if (InteractionCanvas != null)
            {
                DrawingCanvasArea.Children.Insert(1, InteractionCanvas);
                Grid.SetColumn(InteractionCanvas, 0);
                Grid.SetRow(InteractionCanvas, 0);
            }
        }
        public void DisableInteraction()
        {
            if (InteractionCanvas != null)
            {
                DrawingCanvasArea.Children.Remove(InteractionCanvas);
            }
        }

        #endregion
    }

    /// <summary>
    /// 自带canvas
    /// </summary>
    public class GenericCanvasContainer : DrawingGrid
    {
        RectDrawingCanvas canvas;
        public GenericCanvasContainer(bool isEnableInteraction = false)
        {
            canvas = new RectDrawingCanvas(isEnableInteraction);
            DrawingCanvasArea.Children.Add(canvas);
            Content = DrawingCanvasArea;
        }
        public override InteractionCanvas InteractionCanvas => null;

        public void Replot()
        {
            canvas.Replot();
        }

    }
}
