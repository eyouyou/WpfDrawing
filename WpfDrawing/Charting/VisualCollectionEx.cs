using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WpfDrawing
{
    public abstract class RectVisualCollection : SubRectDrawingVisual
    {
        public override RectVisualContextData DefaultData => null;

        public virtual void DataPush(RectVisualContextData data, IList<RectVisualContextData> list)
        {
            VisualDataSetupTidily(data);

            var visuals = new List<RectDrawingVisual>();
            foreach (RectDrawingVisual visual in Visuals)
            {
                var item = list.FirstOrDefault(it => it.ComponentId == visual.Id);
                if (item != null)
                {
                    if (!visual.IsShieldedParentData)
                    {
                        visual.VisualData = item.Copy();
                    }
                    visual.VisualData.Current = data;
                    visual.Reset();
                }
                else
                {
                    visuals.Add(visual);
                }
            }
            if (list.Count > 0)
            {
                foreach (var visual in visuals)
                {
                    if (!visual.IsShieldedParentData)
                    {
                        visual.VisualData = list[0].Copy();
                    }
                    visual.VisualData.Current = data;
                    visual.Reset();
                }
            }

        }
        public void Add(RectDrawingVisual item)
        {
            AddSubVisual(item);
        }

        public override void PlotToDc(DrawingContext dc)
        {
            foreach (SubRectDrawingVisual item in Visuals)
            {
                item.PlotToDc(dc);
            }
        }
    }

    /// <summary>
    /// 轴不多 不然直接从collection获取属性
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class XAxisVisualCollection : RectVisualCollection, IAxisVisualConfiguare
    {

        public Pen CrossPen { get; set; } = new Pen(Brushes.DarkGray, 1);


        /// <summary>
        /// <see cref="VisualData.set"/>:  从父亲<see cref="RectDrawingVisual.VisualData"/>继承<see cref="RectVisualContextData.Items"/>
        /// </summary>
        public override void DataPush(RectVisualContextData data, IList<RectVisualContextData> list)
        {
            base.DataPush(data, list);

            foreach (DiscreteAxis item in Visuals)
            {
                if (!(item.VisualData is DiscreteAxisVisualData visualData))
                {
                    continue;
                }

                //针对DiscreteAxis轴 
                if (item.Data == null || item.Data.Count == 0)
                {
                    item.Data = visualData.Data.Select(it => it.ValueData(item.Name) as IVariable).ToList();
                }

                item.CalculateRequireData();
            }
        }
        public void MakeData(RectChartVisualCollectionData data)
        {
            if (DataSource is ChartDataSource coms)
            {
                foreach (var item in data.XData)
                {
                    var component = coms.FindById(item.ComponentId);
                    item.ComponentId = component.Id;
                }
            }
        }

        public override void Freeze()
        {
            CrossPen.Freeze();
        }
    }

    public class YAxisVisualCollection : RectVisualCollection, IAxisVisualConfiguare
    {
        public Pen CrossPen { get; set; } = new Pen(Brushes.DarkGray, 1) { /*DashStyle = DashStyles.Dash, DashCap = PenLineCap.Flat*/ };

        public override void DataPush(RectVisualContextData data, IList<RectVisualContextData> list)
        {
            base.DataPush(data, list);

            foreach (ContinuousAxis item in Visuals)
            {
                if (!(item.VisualData is ContinuousAxisVisualData visualData))
                {
                    return;
                }
                if (item.Range != null)
                {
                    visualData.Range = item.Range;
                }
                else
                {
                    //一根轴对应多series的情况 
                    // 轴的range调整
                    if (DataSource is ChartDataSource coms)
                    {
                        var series = coms.GetMappingSeries(item.Id);
                        var ranges = series.Select(it => (it.VisualData as RectChartVisualData).YData.Range).ToList();
                        visualData.Range = new Range() { Max = ranges.Max(it => it.Max), Min = ranges.Min(it => it.Min) };
                    }
                }
                item.CalculateRequireData();

            }
        }
        public void MakeData(RectChartVisualCollectionData data)
        {
            if (DataSource is ChartDataSource coms)
            {
                foreach (var item in data.YData)
                {
                    var component = coms.FindById(item.ComponentId);
                    item.ComponentId = component.Id;
                }
            }
        }
        public override void Freeze()
        {
            CrossPen.Freeze();
        }

    }
    /// <summary>
    /// 和y轴有一一对应关系
    /// </summary>
    public class SeriesVisualCollection : RectVisualCollection
    {
        public override void PlotToDc(DrawingContext dc)
        {
            foreach (SeriesVisual item in Visuals)
            {
                item.PlotToDc(dc);
            }
        }
        public override void DataPush(RectVisualContextData data, IList<RectVisualContextData> list)
        {
            base.DataPush(data, list);
        }
        /// <summary>
        /// 临时方案 可以用分组或分治算法进行优化
        /// </summary>
        /// <returns></returns>
        public RectChartVisualCollectionData MakeData()
        {
            var index = -1;
            var list = new List<RectChartVisualData>();
            foreach (SeriesVisual item in Visuals)
            {
                index++;
                if (item.VisualData is RectChartVisualData rectData)
                {
                    rectData.YData.Range = item.GetRange();
                    rectData.ComponentId = item.Id;
                    rectData.XData.ComponentId = item.XAxisId;
                    list.Add(rectData);
                }
            }
            if (list.Count == 0)
            {
                return RectChartVisualCollectionData.Empty;
            }
            RectChartVisualCollectionData data = new RectChartVisualCollectionData(list);
            return data;
        }

    }
}
