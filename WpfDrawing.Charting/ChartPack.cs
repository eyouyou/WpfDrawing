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
    public class ChartPack<Input, Output> : UserControl, IDataAvailable
    {
        public ChartPack()
        {
            Assembly = ChartVisual.Assembly as ChartAssembly;
            DrawingCanvasArea.Children.Add(DrawingCanvas);
            Content = DrawingCanvasArea;
        }
        public AxisInteractionCanvas InteractionCanvas { get; } = new AxisInteractionCanvas();
        public IIntersectable Intersection => ChartVisual;
        public IChartComponentizable Components => Assembly;

        #region 交互独立情况下

        private bool _enable_interaction = true;
        public bool EnableInteraction
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
        public async void StartDataFeed()
        {
            await Process(SeriesPacks);
        }

        public void AddResponsePipline(IPipline<Dictionary<IRequestable<Input, Output>, Output>> pipline)
        {
            ResponsePiplines1.Add(pipline);
            ResponsePiplines2 = ResponsePiplines1.Select(it => (Func<ChartContext<Dictionary<IRequestable<Input, Output>, Output>>, PiplineDelegate<Dictionary<IRequestable<Input, Output>, Output>>, Task>)it.PipAsync)
                .Select(it => (Func<PiplineDelegate<Dictionary<IRequestable<Input, Output>, Output>>, PiplineDelegate<Dictionary<IRequestable<Input, Output>, Output>>>)(next => c => it(c, next))).ToList();
        }
        public void AddResponsePipline(Func<ChartContext<Dictionary<IRequestable<Input, Output>, Output>>, PiplineDelegate<Dictionary<IRequestable<Input, Output>, Output>>, Task> func)
        {
            ResponsePiplines2.Add(new Func<PiplineDelegate<Dictionary<IRequestable<Input, Output>, Output>>, PiplineDelegate<Dictionary<IRequestable<Input, Output>, Output>>>(() =>)) = ResponsePiplines1.Select(it => (Func<ChartContext<Dictionary<IRequestable<Input, Output>, Output>>, PiplineDelegate<Dictionary<IRequestable<Input, Output>, Output>>, Task>)it.PipAsync)
                .Select(it => (Func<PiplineDelegate<Dictionary<IRequestable<Input, Output>, Output>>, PiplineDelegate<Dictionary<IRequestable<Input, Output>, Output>>>)(next => c => it(c, next))).ToList();
        }
        public void AddSubscribePipline(IPipline<Dictionary<IRequestable<Input, Output>, Output>> pipline)
        {

        }
        private List<Func<PiplineDelegate<Dictionary<IRequestable<Input, Output>, Output>>, PiplineDelegate<Dictionary<IRequestable<Input, Output>, Output>>>> ResponsePiplines2 =
            new List<Func<PiplineDelegate<Dictionary<IRequestable<Input, Output>, Output>>, PiplineDelegate<Dictionary<IRequestable<Input, Output>, Output>>>>();
        private List<IPipline<Dictionary<IRequestable<Input, Output>, Output>>> ResponsePiplines1 = new List<IPipline<Dictionary<IRequestable<Input, Output>, Output>>>();
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

            PiplineDelegate<Dictionary<IRequestable<Input, Output>, Output>> @delegate = new PiplineDelegate<Dictionary<IRequestable<Input, Output>, Output>>((data) => Task.FromResult(true));

            for (int i = ResponsePiplines2.Count - 1; i >= 0; i--)
            {
                @delegate = ResponsePiplines2[i](@delegate);
            }
            await @delegate.Invoke(new ChartContext<Dictionary<IRequestable<Input, Output>, Output>>() { Data = array.ToDictionary(it => FindById(it.Id) as IRequestable<Input, Output>, it => it.Data) });

            ChartVisual.RePlot();
        }
        public void StopDataFeed()
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
        private RectDrawingCanvas DrawingCanvas { get; } = new RectDrawingCanvas();

        /// <summary>
        /// 主畫布
        /// </summary>
        private ChartVisual ChartVisual = new ChartVisual();
        private SeriesPackBase FindById(int id)
        {
            return SeriesPacks.FirstOrDefault(it => it.SeriesVisual.Id == id);
        }

    }
}
