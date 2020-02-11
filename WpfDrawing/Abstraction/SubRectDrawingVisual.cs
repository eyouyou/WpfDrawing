using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WpfDrawing
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
        private RectVisualContextData _visualData = null;
        public override void VisualDataSetupTidily(RectVisualContextData data)
        {
            _visualData = data;
        }
        /// <summary>
        /// 需要从上层<see cref="RectDrawingVisual"/>赋值
        /// <see cref="SubRectDrawingVisual.set"/>:  从父亲<see cref="SubRectDrawingVisual.VisualData"/>继承<see cref="RectVisualContextData.Items"/>
        /// </summary>
        public override RectVisualContextData VisualData
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
