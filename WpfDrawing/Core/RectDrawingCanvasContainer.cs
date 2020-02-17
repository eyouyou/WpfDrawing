using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace HevoDrawing
{
    public abstract class RectDrawingCanvasContainer : UserControl
    {
        public abstract RectDrawingCanvas Canvas { get; }
    }

    /// <summary>
    /// 自带canvas
    /// </summary>
    public class GenericCanvasContainer : RectDrawingCanvasContainer
    {
        RectDrawingCanvas canvas;
        public GenericCanvasContainer(bool isEnableInteraction = false)
        {
            canvas = new RectDrawingCanvas(isEnableInteraction);
            Content = canvas;
        }
        public override RectDrawingCanvas Canvas => canvas;

        public void Replot()
        {
            canvas.Replot();
        }
    }
}
