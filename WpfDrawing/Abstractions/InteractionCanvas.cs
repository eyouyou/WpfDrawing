using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HevoDrawing.Abstractions
{
    /// <summary>
    /// 交互、控件展示层
    /// 多图的情况下 <see cref="InteractionCanvas"/> 依赖于 <see cref="RectDrawingCanvas"/> 或 <see cref="DrawingGrid"/>
    /// [<see cref="RectDrawingCanvas"/>: <see cref="InteractionCanvas"/>存在于<see cref="RectInteractionGroup"/>的顶层]
    /// [<see cref="DrawingGrid"/>：<see cref="InteractionCanvas"/>存在于 <see cref="DrawingGrid"/>的顶层]
    /// </summary>
    public abstract class InteractionCanvas : Canvas
    {
        public abstract ContextData DefaultData { get; }
        public InteractionCanvas()
        {
            Background = Brushes.Transparent;
        }
        public void AddElement(UIElement element)
        {
            Children.Add(element);
        }
        public void RemoveElement(UIElement element)
        {
            Children.Remove(element);
        }

        /// <summary>
        /// 多个数据源
        /// </summary>
        public Dictionary<int, VisualAssembly> DataSources { get; } = new Dictionary<int, VisualAssembly>();
        /// <summary>
        /// 依赖的container 独立的话1个
        /// 复用的话多个
        /// </summary>
        public Dictionary<int, DrawingGrid> DependencyContainers { get; } = new Dictionary<int, DrawingGrid>();
        public DrawingGrid UniqueDependencyContainer => DependencyContainers.Count == 1 ? DependencyContainers.ElementAt(0).Value : null;

        private ContextData _visualData = null;
        public ContextData VisualData
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
            }
        }
        public bool Standalone => DependencyContainers.Count == 1 && DependencyContainers.ElementAt(0).Value.InteractionCanvas != null;
        public abstract void Hide();
        public UIElement ParentElement { get; set; }
        public abstract void Plot(Point point, EventMessage @event);
        public abstract void PlotStandalone(Point point, EventMessage @event);
    }
    public abstract class InteractionLayer : IVisualControllable
    {
        protected Dictionary<int, VisualAssembly> DataSources { get; set; }

        public InteractionLayer(InteractionCanvas canvas)
        {
            ParentCanvas = canvas;
            DataSources = canvas.DataSources;
        }
        public InteractionCanvas ParentCanvas { get; set; }

        private ContextData _visualData;
        public ContextData VisualData
        {
            get => _visualData;
            set
            {
                _visualData = value.Copy();
                _visualData.Current = value;
                _visualData.Items.Clear();
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

        public abstract void Clear();
        public abstract void Hide();
        public abstract void PlotToParent(Point point, EventMessage @event);
        public abstract void PlotToParentStandalone(Point point, EventMessage @event);

    }

}
