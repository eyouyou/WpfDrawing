using HevoDrawing.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace HevoDrawing
{
    public class RectDrawingCanvas : Canvas, ILocatable
    {
        public volatile bool IsPloted = false;
        //在container中所在的位置 
        public int Col { get; set; } = -1;
        public int Row { get; set; } = -1;

        private VisualAssembly _dataSource;
        public VisualAssembly DataSource
        {
            get
            {
                if (_dataSource == null)
                {
                    throw new ArgumentNullException("please init datasource!");
                }
                return _dataSource;
            }
            set
            {
                _dataSource = value;
            }
        }
        /// <summary>
        /// int.MinValue 无效
        /// </summary>
        public int Id { get; set; } = -1;
        /// <summary>
        /// drawingvisual交互
        /// </summary>
        public bool EnableInteraction { get; set; }

        private InteractionCanvas _InteractionCanvas = null;
        /// <summary>
        /// 其他交互 目前有十字线、tip
        /// </summary>
        public InteractionCanvas InteractionCanvas
        {
            get => _InteractionCanvas;
            set
            {
                foreach (Visual item in Visuals)
                {
                    if (item is VisualModule rect)
                    {
                        rect.InteractionVisuals = value;
                    }
                }
                _InteractionCanvas = value;
            }
        }

        /// <summary>
        /// 和所有visual的交互
        /// </summary>
        CancellationTokenSource cts = new CancellationTokenSource();
        public RectDrawingCanvas(bool isEnableInteraction = false)
        {
            EnableInteraction = isEnableInteraction;

            Visuals = new System.Windows.Media.VisualCollection(this);
            SizeChanged += RectCanvas_SizeChanged;
            MouseMove += Value_MouseMove;

            Loaded += RectDrawingCanvas_Loaded;
        }

        private void RectDrawingCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            Replot();
        }

        private void Value_MouseMove(object sender, MouseEventArgs e)
        {
            VisualTreeHelper.HitTest(this, null, HitTestCallback, new PointHitTestParameters(Mouse.GetPosition(this)));
        }
        public HitTestResultBehavior HitTestCallback(HitTestResult result)
        {
            if (result.VisualHit is InteractionCanvas)
            {
                return HitTestResultBehavior.Continue;
            }

            if (!(result is PointHitTestResult pointResult))
            {
                return HitTestResultBehavior.Continue;
            }
            if (ReferenceEquals(result.VisualHit, this) && EnableInteraction)
            {
                foreach (VisualModule item in Visuals)
                {
                    item.HitTest(pointResult.PointHit, EventMessage.MouseOn);
                }
            }
            //else
            //{
            //    (result.VisualHit as RectVisual).HitTest(pointResult.PointHit, EventMessage.MouseOn);
            //}
            //Console.WriteLine(result.VisualHit.GetType());
            return HitTestResultBehavior.Stop;
        }

        private void RectCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //cts.Cancel();
            //cts = new CancellationTokenSource();
            if (!IsVisible)
            {
                return;
            }
            //try
            //{
            //    await Task.Delay(600, cts.Token);
            //}
            //catch
            //{
            //    return;
            //}
        }

        /// <summary>
        /// 给 <see cref="InteractionCanvas"/> 定位用
        /// </summary>
        public Vector Offset { get; set; } = new Vector(0, 0);

        /// <summary>
        /// 当前整个 <see cref="RectInteractionGroup"/> 内的定位
        /// </summary>
        public Point CurrentLocation { get; set; } = new Point(0, 0);

        /// <summary>
        /// 在 <see cref="InteractionCanvas"/> 中的画板位置
        /// </summary>
        public Rect InteractionCanvasPlotArea => new Rect(CurrentLocation, PlotArea.Size);

        public void Reset()
        {
            string stackInfo = new StackTrace().ToString();
            Console.WriteLine(stackInfo);
            Console.WriteLine($"{ActualWidth}, {ActualHeight}");
            PlotArea = new Rect(PlotArea.Location, new Size(ActualWidth, ActualHeight));

            foreach (Visual item in Visuals)
            {
                if (item is VisualModule rect)
                {
                    rect.Reset();
                }
            }
        }

        /// <summary>
        /// 清理根数据 通过sub重新逆推根数据
        /// </summary>
        public void Replot()
        {
            IsPloted = false;
            Reset();
            foreach (Visual item in Visuals)
            {
                if (item is VisualModule rect)
                {
                    rect.VisualDataSetupTidily(null);
                    rect.InteractionVisuals?.Hide();
                    rect.Plot();
                }
            }
            IsPloted = true;
        }

        /// <summary>
        /// 不清理根数据
        /// </summary>
        public void Plot()
        {
            IsPloted = false;
            Reset();
            foreach (Visual item in Visuals)
            {
                if (item is VisualModule rect)
                {
                    rect.InteractionVisuals?.Hide();
                    rect.Plot();
                }
            }
            IsPloted = true;
        }
        public VisualCollection Visuals { get; set; }
        public Rect PlotArea { get; set; }
        public void AddChild(VisualModule visual)
        {
            visual.InteractionVisuals = _InteractionCanvas;
            Visuals.Add(visual);
            visual.ParentCanvas = this;
        }
        protected override int VisualChildrenCount => Visuals.Count;
        protected override Visual GetVisualChild(int index)
        {
            return Visuals[index];
        }

    }
}
