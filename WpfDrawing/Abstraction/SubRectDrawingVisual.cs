using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace HevoDrawing.Abstractions
{
    public abstract class SubRectDrawingVisual : RectDrawingVisual
    {

        private PaddingOffset _paddingOffset = PaddingOffset.Default;
        public override PaddingOffset Offsets
        {
            get
            {
                if (ParentCanvas != null)
                    _paddingOffset.Parent = ParentCanvas.PlotArea.Size;
                return _paddingOffset;
            }
            set
            {
                _paddingOffset = value;
            }
        }
        private ContextData _visualData = null;
        public override void VisualDataSetupTidily(ContextData data)
        {
            _visualData = data;
        }
        /// <summary>
        /// 需要从上层<see cref="RectDrawingVisual"/>赋值
        /// <see cref="SubRectDrawingVisual.set"/>:  从父亲<see cref="SubRectDrawingVisual.VisualData"/>继承<see cref="ContextData.Items"/>
        /// </summary>
        public override ContextData VisualData
        {
            get
            {
                if (_visualData == null)
                {
                    return DefaultData;
                }
                return _visualData;
            }
            set
            {
                _visualData = value;
                foreach (RectDrawingVisual item in Visuals)
                {
                    item.DeliverVisualData(VisualData.Copy());
                }
            }
        }
        public override void Plot()
        {
            using (var dc = RenderOpen())
            {
                PlotToDc(dc);
            }
        }
        public abstract void PlotToDc(DrawingContext dc);

    }
}
