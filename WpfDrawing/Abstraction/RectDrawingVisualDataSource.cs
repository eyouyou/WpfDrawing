using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HevoDrawing.Abstractions
{
    public enum Operations
    {
        Add, Remove
    }

    public delegate void VisualChanged(RectDrawingVisual visual, Operations op);
    public abstract class RectDrawingVisualDataSource
    {
        public RectDrawingVisual ConnectVisual;
        public RectDrawingVisualDataSource(RectDrawingVisual visual)
        {
            ConnectVisual = visual ?? throw new ArgumentNullException("please");
        }
        public abstract event VisualChanged VisualChangedHandler;
    }
}
