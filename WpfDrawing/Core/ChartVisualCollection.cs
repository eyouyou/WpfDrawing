using HevoDrawing.Abstractions;
using System.Collections.Generic;

namespace HevoDrawing
{
    public class ChartDataSource : RectDrawingVisualDataSource
    {
        public ComponentId IdGenerater = new ComponentId();

        ChartVisual ConnectChart;
        public ChartDataSource(ChartVisual chart) : base(chart)
        {
            ConnectChart = chart;
        }
        /// <summary>
        /// Y <=> Series
        /// </summary>
        private Dictionary<int, List<SeriesVisual>> AxisMapping = new Dictionary<int, List<SeriesVisual>>();
        private Dictionary<int, List<AxisVisual>> SeriesMapping = new Dictionary<int, List<AxisVisual>>();

        List<SeriesVisual> Series = new List<SeriesVisual>();
        List<AxisVisual> AxisYs = new List<AxisVisual>();
        List<AxisVisual> AxisXs = new List<AxisVisual>();

        protected Dictionary<int, RectDrawingVisual> XMappings = new Dictionary<int, RectDrawingVisual>();
        protected Dictionary<int, RectDrawingVisual> YMappings = new Dictionary<int, RectDrawingVisual>();
        List<RectDrawingVisual> All = new List<RectDrawingVisual>();

        public override event VisualChanged VisualChangedHandler;
        public void AddSeries(SeriesVisual series)
        {
            GenerateId(series);

            Series.Add(series);
            All.Add(series);

            var id = -1;
            foreach (var item in AxisYs)
            {
                if (item.Name == series.Name)
                {
                    id = item.Id;
                }
            }
            if (!AxisMapping.ContainsKey(id))
            {
                AxisMapping[id] = new List<SeriesVisual>();
            }
            AxisMapping[id].Add(series);
            series.DataSource = this;
            VisualChangedHandler?.Invoke(series, Operations.Add);
        }

        public void AddAxisY(AxisVisual axis)
        {
            GenerateId(axis);

            AxisYs.Add(axis);
            All.Add(axis);

            var id = -1;
            foreach (var item in Series)
            {
                if (item.Name == axis.Name)
                {
                    id = item.Id;
                }
            }
            if (!SeriesMapping.ContainsKey(id))
            {
                SeriesMapping[id] = new List<AxisVisual>();
            }
            YMappings.Add(axis.Id, axis);
            SeriesMapping[id].Add(axis);
            axis.DataSource = this;
        }
        public void AddAxisX(AxisVisual axis)
        {
            GenerateId(axis);

            AxisXs.Add(axis);
            All.Add(axis);

            XMappings.Add(axis.Id, axis);
            axis.DataSource = this;
        }
        private void GenerateId(RectDrawingVisual visual)
        {
            visual.Id = IdGenerater.GenerateId();
        }
        public List<SeriesVisual> GetMappingSeries(int id)
        {
            if (AxisMapping.TryGetValue(id, out var y))
            {
                return y;
            }
            if (y == null && Series.Count >= 1)
            {
                return Series;
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
            if (SeriesMapping.TryGetValue(id, out var y))
            {
                return y[0];
            }
            if (y == null && AxisYs.Count >= 1)
            {
                return AxisYs[0];
            }
            return null;
        }
        public List<SeriesVisual> SeriesCollection => Series.Count == 0 ? new List<SeriesVisual>() : new List<SeriesVisual>(Series);
        public List<AxisVisual> AxisYCollection => AxisYs.Count == 0 ? new List<AxisVisual>() : new List<AxisVisual>(AxisYs);
        public List<AxisVisual> AxisXCollection => AxisXs.Count == 0 ? new List<AxisVisual>() : new List<AxisVisual>(AxisXs);

        public RectDrawingVisual FindXById(int id)
        {
            if (XMappings.ContainsKey(id))
            {
                return XMappings[id];
            }
            if (AxisXs.Count == 0)
            {
                return null;
            }
            return AxisXs[0] as RectDrawingVisual;
        }
        public RectDrawingVisual FindYById(int id)
        {
            if (YMappings.ContainsKey(id))
            {
                return YMappings[id];
            }
            if (AxisYs.Count == 0)
            {
                return null;
            }
            return AxisYs[0] as RectDrawingVisual;
        }
    }
}
