using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace HevoDrawing.Abstractions
{
    public interface ILocatable
    {
        Rect PlotArea { get; }
        Vector Offset { set; get; }
    }
    public interface IVisualControllable
    {
        bool IsVisualEnable { get; set; }
        void Clear();
    }
    public abstract class VisualModule : DrawingVisual, ILocatable, IVisualControllable, IEquatable<VisualModule>
    {
        public abstract ContextData DefaultData { get; }
        /// <summary>
        /// 屏蔽父visual的<see cref="VisualData"/>
        /// </summary>
        public bool IsolateData { get; set; } = false;

        public VisualModule(VisualAssembly dataSource) : this()
        {
            Assembly = dataSource;
        }
        public VisualModule()
        {
            Visuals = new System.Windows.Media.VisualCollection(this);
        }
        private VisualAssembly _dataSource;
        public virtual VisualAssembly Assembly
        {
            get
            {
                if (_dataSource == null)
                {
                    throw new ArgumentNullException();
                }
                return _dataSource;
            }
            internal set
            {
                _dataSource = value;
            }
        }
        public int Id { get; internal set; } = int.MinValue;
        public string Name { get; set; }

        /// <summary>
        /// 使用<see cref="RectInteractionGroup"/>
        /// </summary>
        public InteractionCanvas InteractionVisuals { get; set; }
        protected VisualCollection Visuals { get; set; }

        /// <summary>
        /// 不做缓存 在各自plot节约调用
        /// </summary>
        public Rect PlotArea
        {
            get
            {
                var area = ParentLocateble.PlotArea;
                return new Rect(new Point(area.Location.X + Offsets.LeftOffset, area.Location.Y + Offsets.TopOffset),
                    Tools.GetSize(area.Size.Width - Offsets.LeftOffset - Offsets.RightOffset, area.Size.Height - Offsets.TopOffset - Offsets.ButtomOffset));
            }
        }

        public Rect InteractionPlotArea
        {
            get
            {
                var area = ParentLocateble.PlotArea;
                var offset = ParentCanvas.Offset;
                return new Rect(new Point(area.Location.X + Offsets.LeftOffset + offset.X, area.Location.Y + Offsets.TopOffset + offset.Y),
                    Tools.GetSize(area.Size.Width - Offsets.LeftOffset - Offsets.RightOffset, area.Size.Height - Offsets.TopOffset - Offsets.ButtomOffset));
            }
        }

        private PaddingOffset _paddingOffset = PaddingOffset.Default;
        public virtual PaddingOffset Offsets
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
        public virtual void HitTest(Point point, EventMessage message)
        {
            VisualTreeHelper.HitTest(this, null, MyCallback, new PointHitTestParameters(point));
        }
        private HitTestResultBehavior MyCallback(HitTestResult result)
        {
            //Console.WriteLine(result.VisualHit.GetType());
            foreach (VisualModule item in Visuals)
            {
                if (result.VisualHit == item)
                {
                    item.HitTest(new Point(), EventMessage.MouseOn);
                }
            }

            return HitTestResultBehavior.Stop;
        }
        private ContextData visual_data = null;
        public virtual ContextData VisualData
        {
            get
            {
                if (visual_data == null)
                    visual_data = DefaultData;
                return visual_data;
            }
            set
            {
                visual_data = value;
                RePlot();
            }
        }
        public virtual void VisualDataSetupTidily(ContextData data)
        {
            visual_data = data;
        }
        /// <summary>
        /// tree
        /// </summary>
        /// <param name="subRectVisual"></param>
        protected void AddSubVisual(VisualModule subRectVisual)
        {
            if (subRectVisual == null)
            {
                return;
            }
            Visuals.Add(subRectVisual);
            subRectVisual.ParentCanvas = ParentCanvas;
        }

        public virtual ILocatable ParentLocateble => Parent as ILocatable;

        private RectDrawingCanvas _parentCanvas;
        public virtual RectDrawingCanvas ParentCanvas
        {
            get { return _parentCanvas; }
            set
            {
                _parentCanvas = value;

                lock (Visuals.SyncRoot)
                {
                    foreach (VisualModule item in Visuals)
                    {
                        item.ParentCanvas = value;
                    }
                }
            }
        }
        private bool _isVisualEnable = true;
        public bool IsVisualEnable
        {
            get => _isVisualEnable;
            set
            {
                _isVisualEnable = value;
                if (!value)
                {
                    Clear();
                }
            }
        }

        public virtual void Freeze()
        {

        }
        public void Reset()
        {
            VisualData.Items.Clear();
        }

        public virtual void RePlot()
        {
            Reset();
            Plot();
        }
        public abstract void Plot();

        public virtual void Clear()
        {
            using (var dc = RenderOpen())
            {

            }
        }

        public bool Equals(VisualModule other)
        {
            if (other.Id == int.MinValue && Id == int.MinValue)
            {
                return other.Equals(this);
            }
            return other.Id == Id;
        }
    }
}
