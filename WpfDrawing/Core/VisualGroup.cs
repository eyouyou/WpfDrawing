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
                var item = list.FirstOrDefault(it => it.ComponentId == visual.Id);
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
                // 这个visualdata 已分发好
                if (!(item.VisualData is DiscreteAxisContextData visualData))
                {
                    continue;
                }
                //TODO 性能
                var datas = list.Where(it => it is DiscreteAxisContextData && it.ComponentId == item.Id).Select(it => it as DiscreteAxisContextData);

                //针对DiscreteAxis轴 会聚多数据源
                item.Data = datas.SelectMany(da => da.Data.Select(it => it.ValueData(item.Name) as IVariable)).Distinct().OrderBy(it => it).ToList();

                item.CalculateRequireData();
            }


        }
        public void InductiveData(ChartGroupContextData data)
        {
            if (DataSource is ChartDataSource coms)
            {
                foreach (var item in data.XData)
                {
                    var component = coms.FindXById(item.ComponentId);
                    item.ComponentId = component.Id;
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
                        visualData.Range = new Range() { Max = ranges.Max(it => it.Max), Min = ranges.Min(it => it.Min) };
                    }
                }
                item.CalculateRequireData();

            }
        }
        public void InductiveData(ChartGroupContextData data)
        {
            if (DataSource is ChartDataSource coms)
            {
                foreach (var item in data.YData)
                {
                    var component = coms.FindYById(item.ComponentId);
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
                    rectData.ComponentId = item.Id;
                    rectData.XContextData.ComponentId = item.XAxisId;
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
