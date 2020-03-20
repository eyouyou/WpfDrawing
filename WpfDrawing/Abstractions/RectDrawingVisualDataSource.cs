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

    public delegate void VisualChanged(VisualModule visual, Operations op);
    public abstract class VisualAssembly
    {
        public VisualModule ConnectVisual;
        public VisualAssembly(VisualModule visual)
        {
            ConnectVisual = visual ?? throw new ArgumentNullException("please");
        }
        public abstract event VisualChanged VisualChangedHandler;
        public abstract bool IsDataComplete { get; }
    }
}
