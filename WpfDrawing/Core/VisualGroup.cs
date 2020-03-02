using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using HevoDrawing.Abstractions;

namespace HevoDrawing
{
    public abstract class RectVisualGroup : SubRectDrawingVisual
    {
        public override ContextData DefaultData => null;

        /// <summary>
        /// 必须要一个visualdata
        /// 如果需要就拿挡墙
        /// </summary>
        protected abstract bool NeedData { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="list">未处理数据</param>
        /// <returns></returns>
        public virtual void DataPush(ContextData data, IList<ContextData> list)
        {
            VisualDataSetupTidily(data);

            var visuals = new List<RectDrawingVisual>();

            foreach (RectDrawingVisual visual in Visuals)
            {
                var item = list.FirstOrDefault(it => it.ComponentIds.Contains(visual.Id));
                if (item != null)
                {
                    visual.DeliverVisualData(item.Copy());
                }
                else
                {
                    visuals.Add(visual);
                }
            }
            if (list.Count > 0 && visuals.Count > 0 && NeedData)
            {
                foreach (var visual in visuals)
                {
                    visual.DeliverVisualData(list[0].Copy());
                }
            }
            else
            {
                foreach (var visual in visuals)
                {
                    visual.DeliverVisualData(visual.DefaultData);
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
    public class XAxisVisualGroup : RectVisualGroup, IAxisVisualConfiguare
    {
        public Pen CrossPen { get; set; } = new Pen(Brushes.DarkGray, 1);

        protected override bool NeedData => true;

        /// <summary>
        /// <see cref="VisualData.set"/>:  从父亲<see cref="RectDrawingVisual.VisualData"/>继承<see cref="ContextData.Items"/>
        /// </summary>
        public override void DataPush(ContextData data, IList<ContextData> list)
        {
            base.DataPush(data, list);

            //存在一个轴对应多个data的情况

            foreach (DiscreteAxis item in Visuals)
            {
                item.IsDataComplete = false;
                // 这个visualdata 已分发好
                if (!(item.VisualData is DiscreteAxisContextData visualData))
                {
                    continue;
                }
                //TODO 性能
                var datas = list.Where(it => it is DiscreteAxisContextData && it.ComponentIds.Contains(item.Id)).Select(it => it as DiscreteAxisContextData);

                if (!datas.Any())
                {
                    item.Data = new List<IVariable>();
                }
                else
                {
                    //针对DiscreteAxis轴 会聚多数据源
                    item.Data = datas.SelectMany(da => da.Data.Select(it => it.ValueData(item.Name) as IVariable)).Distinct().ToList();
                }

                item.CalculateRequireData();
                item.IsDataComplete = true;
            }


        }
        public void InductiveData(ChartGroupContextData data)
        {
            if (DataSource is ChartDataSource coms)
            {
                foreach (var item in data.XData)
                {
                    if (item.ComponentIds.Contains(0))
                    {
                        item.ComponentIds.RemoveAll(it => it == 0);
                        var ids = coms.AxisXCollection.Select(it => it.Id);
                        item.ComponentIds.AddRange(ids);
                    }
                }
            }
        }

        public override void Freeze()
        {
            CrossPen.Freeze();
        }
    }

    public class YAxisVisualGroup : RectVisualGroup, IAxisVisualConfiguare
    {
        public Pen CrossPen { get; set; } = new Pen(Brushes.DarkGray, 1) { /*DashStyle = DashStyles.Dash, DashCap = PenLineCap.Flat*/ };

        protected override bool NeedData => true;

        public override void DataPush(ContextData data, IList<ContextData> list)
        {
            base.DataPush(data, list);

            foreach (ContinuousAxis item in Visuals)
            {
                item.IsDataComplete = false;
                if (!(item.VisualData is ContinuousAxisContextData visualData))
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
                        var ranges = series.Where(it => !it.VisualData.IsEmpty()).Select(it => (it.VisualData as TwoDimensionalContextData).YContextData.Range).ToList();
                        visualData.Range = new Range(ranges.Min(it => it.Min), ranges.Max(it => it.Max));
                    }
                }
                item.CalculateRequireData();
                item.IsDataComplete = true;
            }
        }
        public void InductiveData(ChartGroupContextData data)
        {
            if (DataSource is ChartDataSource coms)
            {
                foreach (var item in data.YData)
                {
                    if (item.ComponentIds.Contains(0))
                    {
                        item.ComponentIds.RemoveAll(it => it == 0);
                        var ids = coms.AxisYCollection.Select(it => it.Id);
                        item.ComponentIds.AddRange(ids);
                    }
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
    public class SeriesVisualGroup : RectVisualGroup
    {
        protected override bool NeedData => false;

        public override void PlotToDc(DrawingContext dc)
        {
            foreach (SeriesVisual item in Visuals)
            {
                item.PlotToDc(dc);
            }
        }
        public override void DataPush(ContextData data, IList<ContextData> list)
        {
            base.DataPush(data, list);
        }
        /// <summary>
        /// 归纳
        /// 临时方案 可以用分组或分治算法进行优化
        /// </summary>
        /// <returns></returns>
        public ChartGroupContextData InductiveData()
        {
            var index = -1;
            var list = new List<TwoDimensionalContextData>();
            foreach (PointsSeriesVisual item in Visuals)
            {
                index++;
                if (item.VisualData is TwoDimensionalContextData rectData && !rectData.IsEmpty)
                {
                    rectData.YContextData.Range = item.GetRange();
                    rectData.ComponentIds.Add(item.Id);
                    rectData.XContextData.ComponentIds.Add(item.XAxisId);
                    list.Add(rectData);
                }
            }
            if (list.Count == 0)
            {
                return ChartGroupContextData.Empty;
            }
            ChartGroupContextData data = new ChartGroupContextData(list);
            return data;
        }

    }
}
