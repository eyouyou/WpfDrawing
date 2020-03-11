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

            var noDataVisuals = new List<RectDrawingVisual>();

            foreach (RectDrawingVisual visual in Visuals)
            {
                var item = list.FirstOrDefault(it => it.ComponentIds.Contains(visual.Id));
                if (item != null)
                {
                    visual.DeliverVisualData(item);
                }
                else
                {
                    noDataVisuals.Add(visual);
                }
            }
            if (list.Count > 0 && noDataVisuals.Count > 0 && NeedData)
            {
                foreach (var visual in noDataVisuals)
                {
                    visual.DeliverVisualData(list[0]);
                }
            }
            else
            {
                foreach (var visual in noDataVisuals)
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
                var datas = list.Where(it => it.ComponentIds.Contains(item.Id)).Select(it => it as DiscreteAxisContextData);

                if (!datas.Any())
                {
                    visualData.Data = new List<IVariable>();
                }
                else
                {
                    //针对DiscreteAxis轴 会聚多数据源
                    visualData.Data = datas.SelectMany(da => da.Data.Select(it => it.ValueData(item.Name) as IVariable)).Distinct().ToList();
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
                    if (item.ComponentIds.Contains(int.MinValue))
                    {
                        item.ComponentIds.RemoveAll(it => it == int.MinValue);
                        var ids = coms.AxisXCollection.Select(it => it.Id);
                        item.ComponentIds.AddRange(ids);
                    }
                }
            }
        }

        public override void Freeze()
        {
            CrossPen.Freeze();
            foreach (DiscreteAxis item in Visuals)
            {
                item.Freeze();
            }
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
                        var ranges = series.Where(it => !it.VisualData.IsEmpty()).Select(it => (it.VisualData as TwoDimensionalContextData).YContextData.Range).Where(it => !it.IsEmpty).ToList();
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
                    if (item.ComponentIds.Count == 0)
                    {
                        var ids = coms.AxisYCollection.Select(it => it.Id);
                        item.ComponentIds.AddRange(ids);
                    }
                }
            }
        }
        public override void Freeze()
        {
            CrossPen.Freeze();
            foreach (ContinuousAxis item in Visuals)
            {
                item.Freeze();
            }
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
        public override void Freeze()
        {
            foreach (SeriesVisual item in Visuals)
            {
                item.Freeze();
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

        public ChartGroupContextData InductiveData(ChartGroupContextData data)
        {
            if (DataSource is ChartDataSource coms)
            {
                var all_data = new List<TwoDimensionalContextData>();
                foreach (PointsSeriesVisual item in Visuals)
                {
                    var visualData = data.Data.FirstOrDefault(it => it.ComponentIds.Contains(item.Id));
                    all_data.Add(visualData as TwoDimensionalContextData);
                }
                return new ChartGroupContextData(all_data);
            }
            return ChartGroupContextData.Empty;
        }

        public ChartGroupContextData FilterAndCopyData()
        {
            if (DataSource is ChartDataSource coms)
            {
                var all_data = new List<TwoDimensionalContextData>();
                foreach (PointsSeriesVisual item in Visuals)
                {
                    var axisX = coms.FindXById(item.Id) as DiscreteAxis;
                    var data = item.VisualData as TwoDimensionalContextData;

                    if (axisX.SplitValues != null && axisX.SplitValues.Count > 0 && !axisX.IsDataFull)
                    {
                        List<Section> all_avaliable = Tools.ChangeToSections(axisX.SplitValues);
                        var avaliable_sections = new List<Section>();
                        if (axisX.ExceptSections != null)
                        {
                            foreach (var except in axisX.ExceptSections)
                            {
                                avaliable_sections.AddRange(except.ExceptFrom(all_avaliable));
                            }
                            all_avaliable = avaliable_sections.Distinct().ToList();
                        }

                        var data_temp = data;
                        data = data.GeneraterNewData(data.ChartCroods.Where(it => all_avaliable.Any(a => a.Contains(it.X))).ToList());
                        data.CopyComponentIds(data_temp);
                    }
                    else
                    {
                        data = data.Copy() as TwoDimensionalContextData;
                    }
                    all_data.Add(data);
                }
                return new ChartGroupContextData(all_data);
            }
            return ChartGroupContextData.Empty;
        }
    }
}
