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
    /// <summary>
    /// 限制X轴坐标类型
    /// </summary>
    /// <typeparam name="X"></typeparam>
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

        public void AddResponsePipline(IPipline pipline, int index = int.MinValue)
        {
            if (index == int.MinValue)
            {
                _response_pipline.Add(pipline);
            }
            else
            {
                _response_pipline.Insert(index, pipline);
            }
            ResponsePiplines = _response_pipline.Select(it => (Func<ChartContext, PiplineDelegate, Task>)((c, next) => c.IsCanceled ? Task.FromResult(false) : it.PipAsync(c, next)))
                .Select(it => (Func<PiplineDelegate, PiplineDelegate>)(next => c => it(c, next))).ToList();
        }
        public void AddResponsePipline(Func<ChartContext, PiplineDelegate, Task> func, int index = int.MinValue)
        {
            if (index == int.MinValue)
            {
                ResponsePiplines.Add(next => c => c.IsCanceled ? Task.FromResult(false) : func(c, next));
            }
            else
            {
                ResponsePiplines.Insert(index, next => c => c.IsCanceled ? Task.FromResult(false) : func(c, next));
            }
        }
        public void AddSubscribePipline(IPipline pipline)
        {

        }

        public async Task RequestProcess(List<SeriesPackBase> packs)
        {
            if (!packs.All(it => SeriesPacks.Contains(it)))
            {
                return;
            }
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
            var list = array.ToList();

            await Process(packs, list);
        }
        public void DoSubscribe(List<SeriesPackBase> packs)
        {
            foreach (var item in packs)
            {
                if (item is ISubscribeable subscribeable)
                {
                    subscribeable.DisposeSubscribe();
                    subscribeable.DoSubscribe();
                    subscribeable.OnPushed -= OnPushed;
                    subscribeable.OnPushed += OnPushed;
                }
            }
        }

        private async Task Process(List<SeriesPackBase> packs, List<ReplyData> list)
        {
            var excepts = SeriesPacks.Except(packs);
            list.AddRange(excepts.Select(it => it.CacheData));

            PiplineDelegate @delegate = new PiplineDelegate((data) => Task.WhenAll(packs.Select(it => it.OnReply((data.Data as AggrateReplyData).TotalData[it]))));

            for (int i = ResponsePiplines.Count - 1; i >= 0; i--)
            {
                @delegate = ResponsePiplines[i](@delegate);
            }
            var dic_data = list.ToDictionary(it => FindById(it.Id), it => it);
            await @delegate.Invoke(new ChartContext() { Data = new AggrateReplyData(dic_data) });

            foreach (var item in packs)
            {
                if (dic_data[item].IsBad)
                {
                    continue;
                }
                item.CacheData = dic_data[item];
            }

            ChartVisual.RePlot();
        }
        private async void OnPushed(SeriesPackBase seriesPack, SubscribeResponse response)
        {
            var data = await (seriesPack as ISubscribeable).TranformData(response);
            await Process(new List<SeriesPackBase> { seriesPack }, new List<ReplyData> { data });
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
        protected ChartVisual ChartVisual = new ChartVisual();

        private List<Func<PiplineDelegate, PiplineDelegate>> ResponsePiplines =
            new List<Func<PiplineDelegate, PiplineDelegate>>();

        private List<IPipline> _response_pipline = new List<IPipline>();

        private SeriesPackBase FindById(int pack_id)
        {
            return SeriesPacks.FirstOrDefault(it => it.Id == pack_id);
        }
    }
}
