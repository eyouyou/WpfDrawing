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
    public class ChartPack<Input, Output> : Chart
    {
        public ChartPack()
        {
            Assembly = ChartVisual.Assembly as ChartAssembly;
            DrawingCanvasArea.Children.Add(DrawingCanvas);
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
            ChartVisual.AddSeries(pack.SeriesVisual);
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
        public async override void StartDataFeed()
        {
            await Process(SeriesPacks);
        }

        public void AddResponsePipline(IPipline<Dictionary<SeriesPackBase, Output>> pipline)
        {
            _response_pipline.Add(pipline);
            ResponsePiplines = _response_pipline.Select(it => (Func<ChartContext<Dictionary<SeriesPackBase, Output>>, PiplineDelegate<Dictionary<SeriesPackBase, Output>>, Task>)it.PipAsync)
                .Select(it => (Func<PiplineDelegate<Dictionary<SeriesPackBase, Output>>, PiplineDelegate<Dictionary<SeriesPackBase, Output>>>)(next => c => it(c, next))).ToList();
        }
        public void AddResponsePipline(Func<ChartContext<Dictionary<SeriesPackBase, Output>>, PiplineDelegate<Dictionary<SeriesPackBase, Output>>, Task> func)
        {
            ResponsePiplines.Add(next => c => func(c, next));
        }
        public void AddSubscribePipline(IPipline<Dictionary<SeriesPackBase, Output>> pipline)
        {

        }
        public async Task Process(List<SeriesPackBase> packs)
        {
            var tasks = new List<Task<Result<Output>>>();
            foreach (var item in packs)
            {
                if (item is IRequestable<Input, Output> requestable)
                {
                    var data = requestable.DoRequest(requestable.Data);
                    tasks.Add(data);
                }
            }
            var array = await Task.WhenAll(tasks);

            PiplineDelegate<Dictionary<SeriesPackBase, Output>> @delegate = new PiplineDelegate<Dictionary<SeriesPackBase, Output>>((data) => Task.FromResult(true));

            for (int i = ResponsePiplines.Count - 1; i >= 0; i--)
            {
                @delegate = ResponsePiplines[i](@delegate);
            }
            await @delegate.Invoke(new ChartContext<Dictionary<SeriesPackBase, Output>>() { Data = array.ToDictionary(it => FindById(it.Id), it => it.Data) });

            ChartVisual.RePlot();
        }
        public override void StopDataFeed()
        {
        }

        public DrawingGrid Templete { get; set; }

        private ChartAssembly Assembly;

        private List<SeriesPackBase> SeriesPacks = new List<SeriesPackBase>();
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

        private List<Func<PiplineDelegate<Dictionary<SeriesPackBase, Output>>, PiplineDelegate<Dictionary<SeriesPackBase, Output>>>> ResponsePiplines =
            new List<Func<PiplineDelegate<Dictionary<SeriesPackBase, Output>>, PiplineDelegate<Dictionary<SeriesPackBase, Output>>>>();

        private List<IPipline<Dictionary<SeriesPackBase, Output>>> _response_pipline = new List<IPipline<Dictionary<SeriesPackBase, Output>>>();

        private SeriesPackBase FindById(int id)
        {
            return SeriesPacks.FirstOrDefault(it => it.SeriesVisual.Id == id);
        }

    }
}
