﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfDrawing
{
    /// <summary>
    /// 交互、控件展示层
    /// </summary>
    public abstract class InteractionCanvas : Canvas
    {
        public Dictionary<int, RectDrawingVisualDataSource> DataSource { get; } = new Dictionary<int, RectDrawingVisualDataSource>();
        public abstract RectVisualContextData DefaultData { get; }
        public InteractionCanvas(RectDrawingCanvas canvas)
        {
            DependencyCanvas = canvas;
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

        public RectDrawingCanvas DependencyCanvas { get; }

        private RectVisualContextData _visualData = null;
        public RectVisualContextData VisualData
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
        public abstract void Hide();
        public UIElement ParentElement { get; set; }
        public abstract void Plot(Point point, EventMessage @event);
    }
    public abstract class InteractionLayer : IVisualControllable
    {
        protected Dictionary<int, RectDrawingVisualDataSource> DataSource { get; set; }

        public InteractionLayer(InteractionCanvas canvas)
        {
            ParentCanvas = canvas;
            DataSource = canvas.DataSource;
        }
        public InteractionCanvas ParentCanvas { get; set; }

        private RectVisualContextData _visualData;
        public RectVisualContextData VisualData
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
    }

}
