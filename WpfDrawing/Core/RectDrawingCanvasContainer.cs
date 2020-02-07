using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WpfDrawing
{
    /// <summary>
    /// 在此基础上可以做多个图表的操作
    /// 组合visual和canvas
    /// </summary>
    public class RectInteractionContainer : Grid
    {
        RectDrawingVisual DrawingVisual;
        RectDrawingCanvas Canvas;
        public RectInteractionContainer(RectDrawingVisual visual, RectDrawingCanvas canvas)
        {
            DrawingVisual = visual ?? throw new ArgumentNullException($"{nameof(visual)}");
            Canvas = canvas ?? throw new ArgumentNullException($"{nameof(canvas)}");

            InteractionVisuals = DrawingVisual.InteractionVisuals;
            InteractionVisuals.ParentElement = this;
            Children.Add(Canvas);
            Canvas.AddChild(visual);
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
