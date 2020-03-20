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
    public class ChartPackBase<Input, Output> : UserControl, IDataAvailable
    {
        public ChartPackBase()
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

        public void AddSeriesPack(SeriesPackBase<Input, Output> pack)
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
            var tasks = new List<Task<RequestResult<Output>>>();
            foreach (var item in SeriesPacks)
            {
                var data = item.GetData(item.SeriesData);
                tasks.Add(data);
            }
            var array = await Task.WhenAll(tasks);
            foreach (var item in SeriesPacks)
            {
                item.SeriesVisual.VisualData = item.SeriesDataAnalysizer.Analysis();
            }
            PipToAxis(array.ToDictionary(it => FindById(it.Id), it => it.Data));
            ChartVisual.RePlot();
        }

        public void StopDataFeed()
        {
        }

        public DrawingGrid Templete { get; set; }



        private ChartAssembly Assembly;

        private List<SeriesPackBase<Input, Output>> SeriesPacks = new List<SeriesPackBase<Input, Output>>();
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
        private SeriesPackBase<Input, Output> FindById(int id)
        {
            return SeriesPacks.FirstOrDefault(it => it.SeriesVisual.Id == id);
        }

    }
}
