using HevoDrawing.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace HevoDrawing
{
    public class ChartAssembly : VisualAssembly, IChartComponentizable
    {
        public ComponentId IdGenerater = new ComponentId();

        ChartVisual ConnectChart;
        public ChartAssembly(ChartVisual chart) : base(chart)
        {
            ConnectChart = chart;
        }
        /// <summary>
        /// Y <=> Series
        /// </summary>
        private Dictionary<int, List<SeriesVisual>> AxisMappings = new Dictionary<int, List<SeriesVisual>>();
        private Dictionary<int, List<AxisVisual>> SeriesMappings = new Dictionary<int, List<AxisVisual>>();

        List<SeriesVisual> Series = new List<SeriesVisual>();
        List<AxisVisual> YAxes = new List<AxisVisual>();
        List<AxisVisual> XAxes = new List<AxisVisual>();

        protected Dictionary<int, VisualModule> XMappings = new Dictionary<int, VisualModule>();
        protected Dictionary<int, VisualModule> YMappings = new Dictionary<int, VisualModule>();
        List<VisualModule> All = new List<VisualModule>();

        public override event VisualChanged VisualChangedHandler;
        public void AddSeries(SeriesVisual series)
        {
            GenerateId(series);

            Series.Add(series);
            All.Add(series);
            InitMappings();
            series.Assembly = this;
            VisualChangedHandler?.Invoke(series, Operations.Add);
        }
        public void InitMappings()
        {
            AxisMappings.Clear();
            SeriesMappings.Clear();
            //Y轴和series的映射
            //通过Name一一对应
            foreach (var item in Series)
            {
                var axis_id = int.MinValue;
                var series_id = int.MinValue;
                AxisVisual axis = null;
                var index = 0;
                foreach (var yAxis in YAxes)
                {
                    if (index == 0 || (item.YAxisIds != null && item.YAxisIds.Contains(yAxis.Id)))
                    {
                        axis_id = yAxis.Id;
                        series_id = item.Id;
                        axis = yAxis;
                    }
                    index++;
                }
                if (!AxisMappings.ContainsKey(axis_id))
                {
                    AxisMappings[axis_id] = new List<SeriesVisual>();
                }
                if (!SeriesMappings.ContainsKey(series_id))
                {
                    SeriesMappings[series_id] = new List<AxisVisual>();
                }
                AxisMappings[axis_id].Add(item);
                if (axis != null)
                {
                    SeriesMappings[series_id].Add(axis);
                }
                else
                {
                    SeriesMappings[series_id].AddRange(YAxes);
                }
            }
            if (Series.Count == 0)
            {
                if (!SeriesMappings.ContainsKey(int.MinValue))
                {
                    SeriesMappings[int.MinValue] = new List<AxisVisual>();
                }
                SeriesMappings[int.MinValue].AddRange(YAxes);
            }
        }
        public void AddAxisY(AxisVisual axis)
        {
            GenerateId(axis);

            YAxes.Add(axis);
            All.Add(axis);

            InitMappings();

            YMappings.Add(axis.Id, axis);
            axis.Assembly = this;
        }
        public void AddAxisX(AxisVisual axis)
        {
            GenerateId(axis);

            XAxes.Add(axis);
            All.Add(axis);

            XMappings.Add(axis.Id, axis);
            axis.Assembly = this;
        }
        private void GenerateId(VisualModule visual)
        {
            visual.Id = IdGenerater.GenerateId();
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<SeriesVisual> GetMappingSeries(int id)
        {
            if (AxisMappings.TryGetValue(id, out var y))
            {
                return y;
            }
            if (y == null && AxisMappings.TryGetValue(int.MinValue, out var y2))
            {
                return y2;
            }
            return null;
        }
        /// <summary>
        /// 返回多个y轴没有意义
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public AxisVisual GetMappingAxisY(int id)
        {
            if (SeriesMappings.TryGetValue(id, out var y))
            {
                return y[0];
            }
            if (y == null && YAxes.Count >= 1)
            {
                return YAxes[0];
            }
            return null;
        }
        public List<SeriesVisual> SeriesCollection => Series.Count == 0 ? new List<SeriesVisual>() : new List<SeriesVisual>(Series);
        public List<AxisVisual> AxisYCollection => YAxes.Count == 0 ? new List<AxisVisual>() : new List<AxisVisual>(YAxes);
        public List<AxisVisual> AxisXCollection => XAxes.Count == 0 ? new List<AxisVisual>() : new List<AxisVisual>(XAxes);

        public override bool IsDataComplete => AxisYCollection.All(it => it.IsDataComplete) && AxisXCollection.All(it => it.IsDataComplete);

        public VisualModule FindXById(int id)
        {
            if (XMappings.ContainsKey(id))
            {
                return XMappings[id];
            }
            if (XAxes.Count == 0)
            {
                return null;
            }
            return XAxes[0] as VisualModule;
        }
        public VisualModule FindYById(int id)
        {
            if (YMappings.ContainsKey(id))
            {
                return YMappings[id];
            }
            if (YAxes.Count == 0)
            {
                return null;
            }
            return YAxes[0] as VisualModule;
        }
    }
}
