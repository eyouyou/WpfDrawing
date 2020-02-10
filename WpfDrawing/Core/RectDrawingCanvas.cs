using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfDrawing
{
    public class RectDrawingCanvas : Canvas, ILocatable
    {
        private RectDrawingVisualDataSource _dataSource;
        public RectDrawingVisualDataSource DataSource
        {
            get
            {
                if (_dataSource == null)
                {
                    throw new ArgumentNullException();
                }
                return _dataSource;
            }
            set
            {
                _dataSource = value;
            }
        }

        public int Id { get; set; }
        /// <summary>
        /// drawingvisual交互
        /// </summary>
        public bool EnableInteraction { get; set; }

        /// <summary>
        /// 其他交互 目前有十字线、tip
        /// </summary>
        public InteractionCanvas InteractionCanvas { get; set; }

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
                foreach (RectDrawingVisual item in Visuals)
                {
                    item.HitTest(pointResult.PointHit, EventMessage.MouseOn);
                }
            }
            //else
            //{
            //    (result.VisualHit as RectVisual).HitTest(pointResult.PointHit, EventMessage.MouseOn);
            //}
            Console.WriteLine(result.VisualHit.GetType());
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
        public void Reset()
        {
            PlotArea = new Rect(PlotArea.Location, new Size(this.ActualWidth, this.ActualHeight));
            foreach (Visual item in Visuals)
            {
                if (item is RectDrawingVisual rect)
                {
                    rect.Reset();
                }
            }
        }

        /// <summary>
        /// 清理数据
        /// </summary>
        public void Replot()
        {
            Reset();
            foreach (Visual item in Visuals)
            {
                if (item is RectDrawingVisual rect)
                {
                    rect.VisualDataSetupTidily(null);
                    rect.InteractionVisuals?.Hide();
                    rect.Plot();
                }
            }
        }

        /// <summary>
        /// 沿用之前的数据
        /// </summary>
        public void Plot()
        {
            Reset();
            foreach (Visual item in Visuals)
            {
                if (item is RectDrawingVisual rect)
                {
                    rect.Plot();
                }
            }
        }
        public System.Windows.Media.VisualCollection Visuals { get; set; }
        public Rect PlotArea { get; set; }
        public void AddChild(RectDrawingVisual visual)
        {
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
