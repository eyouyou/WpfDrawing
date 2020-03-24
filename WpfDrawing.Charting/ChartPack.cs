using HevoDrawing.Abstractions;
using HevoDrawing.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace HevoDrawing.Charting
{
    public abstract class ChartPack : Chart
    {
        private ComponentId PackIdGernerator = new ComponentId();
        public ChartPack()
        {
            Assembly = ChartVisual.Assembly as ChartAssembly;
            DrawingCanvasArea.Children.Add(DrawingCanvas);
            DrawingCanvas.AddChild(ChartVisual);
            DrawingCanvas.Assembly = ChartVisual.Assembly;
            Content = DrawingCanvasArea;
        }
        public override AxisInteractionCanvas InteractionCanvas { get; } = new AxisInteractionCanvas();
        public IIntersectable Intersection => ChartVisual;
        public IChartComponentizable Components => Assembly;

        #region 交互独立情况下

        private bool _enable_interaction = true;
        public override bool EnableInteraction
        {
            get => _enable_interaction;
            set
            {
                if (_enable_interaction ^ value)
                {
                    if (value)
                    {
                        DrawingCanvasArea.Children.Add(InteractionCanvas);
                        Grid.SetColumn(InteractionCanvas, 0);
                        Grid.SetRow(InteractionCanvas, 0);
                    }
                    else
                    {
                        if (InteractionCanvas != null)
                        {
                            DrawingCanvasArea.Children.Remove(InteractionCanvas);
                        }
                    }
                }
                _enable_interaction = value;
            }
        }
        #endregion

        public void AddSeriesPack(SeriesPackBase pack)
        {
            foreach (var item in pack.SeriesVisuals)
            {
                ChartVisual.AddSeries(item);
            }
            pack.Id = PackIdGernerator.GenerateId();
            SeriesPacks.Add(pack);
        }
        public void AddXAxis(DiscreteAxis axis)
        {
            ChartVisual.AddAsixX(axis);
        }
        public void AddYAxis(ContinuousAxis axis)
        {
            ChartVisual.AddAsixY(axis);
        }

        public void AddResponsePipline(IPipline pipline)
        {
            _response_pipline.Add(pipline);
            ResponsePiplines = _response_pipline.Select(it => (Func<ChartContext, PiplineDelegate, Task>)it.PipAsync)
                .Select(it => (Func<PiplineDelegate, PiplineDelegate>)(next => c => it(c, next))).ToList();
        }
        public void AddResponsePipline(Func<ChartContext, PiplineDelegate, Task> func)
        {
            ResponsePiplines.Add(next => c => func(c, next));
        }
        public void AddSubscribePipline(IPipline pipline)
        {

        }
        public async Task RequestProcess(List<SeriesPackBase> packs)
        {
            var tasks = new List<Task<ReplyData>>();
            foreach (var item in packs)
            {
                if (item is IRequestable requestable)
                {
                    var data = requestable.DoRequest();
                    tasks.Add(data);
                }
            }
            var array = await Task.WhenAll(tasks);

            PiplineDelegate @delegate = new PiplineDelegate((data) => Task.FromResult(true));

            for (int i = ResponsePiplines.Count - 1; i >= 0; i--)
            {
                @delegate = ResponsePiplines[i](@delegate);
            }
            await @delegate.Invoke(new ChartContext() { Data = new AggrateReplyData(array.ToDictionary(it => FindById(it.Id), it => it)) });

            ChartVisual.RePlot();
        }

        private ChartTemplate _chart_template = null;
        public ChartTemplate ChartTemplate
        {
            get => _chart_template;
            set
            {
                if (_chart_template != null)
                {
                    _chart_template.Separate();
                }
                _chart_template = value;
            }
        }

        private ChartAssembly Assembly;

        protected List<SeriesPackBase> SeriesPacks { get; } = new List<SeriesPackBase>();
        /// <summary>
        /// 放置<see cref="InteractionCanvas"/> 的容器
        /// </summary>
        private Grid DrawingCanvasArea { get; } = new Grid();

        /// <summary>
        /// 已加入 <see cref="DrawingCanvasArea"/>
        /// </summary>
        public override RectDrawingCanvas DrawingCanvas { get; } = new RectDrawingCanvas();

        /// <summary>
        /// 主畫布
        /// </summary>
        private ChartVisual ChartVisual = new ChartVisual();

        private List<Func<PiplineDelegate, PiplineDelegate>> ResponsePiplines =
            new List<Func<PiplineDelegate, PiplineDelegate>>();

        private List<IPipline> _response_pipline = new List<IPipline>();

        private SeriesPackBase FindById(int pack_id)
        {
            return SeriesPacks.FirstOrDefault(it => it.Id == pack_id);
        }
    }
}
