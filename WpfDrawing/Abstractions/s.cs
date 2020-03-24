using HevoDrawing.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace HevoDrawing.Abstractions
{
    public abstract class Chart : UserControl, IDataAvailable
    {
        public abstract RectDrawingCanvas DrawingCanvas { get; }
        public abstract AxisInteractionCanvas InteractionCanvas { get; }
        public abstract bool EnableInteraction { get; set; }
        public abstract void StartDataFeed();
        public abstract void StopDataFeed();

    }
}
