using HevoDrawing.Abstractions;
using System.Collections.Generic;
using System.Linq;

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
        private Dictionary<int, List<SeriesVisual>> AxisMappings = new Dictionary<int, List<SeriesVisual>>();
        private Dictionary<int, List<AxisVisual>> SeriesMappings = new Dictionary<int, List<AxisVisual>>();

        List<SeriesVisual> Series = new List<SeriesVisual>();
        List<AxisVisual> YAxes = new List<AxisVisual>();
        List<AxisVisual> XAxes = new List<AxisVisual>();

        protected Dictionary<int, RectDrawingVisual> XMappings = new Dictionary<int, RectDrawingVisual>();
        protected Dictionary<int, RectDrawingVisual> YMappings = new Dictionary<int, RectDrawingVisual>();
        List<RectDrawingVisual> All = new List<RectDrawingVisual>();

        public override event VisualChanged VisualChangedHandler;
        public void AddSeries(SeriesVisual series)
        {
            GenerateId(series);

            Series.Add(series);
            All.Add(series);
            InitMappings();
            series.DataSource = this;
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
                foreach (var yAxis in YAxes)
                {
                    if (yAxis.Name == item.Name)
                    {
                        axis_id = yAxis.Id;
                        series_id = item.Id;
                        axis = yAxis;
                    }
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
                SeriesMappings[series_id].Add(axis);
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
            axis.DataSource = this;
        }
        public void AddAxisX(AxisVisual axis)
        {
            GenerateId(axis);

            XAxes.Add(axis);
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
            if (AxisMappings.TryGetValue(id, out var y))
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

        public RectDrawingVisual FindXById(int id)
        {
            if (XMappings.ContainsKey(id))
            {
                return XMappings[id];
            }
            if (XAxes.Count == 0)
            {
                return null;
            }
            return XAxes[0] as RectDrawingVisual;
        }
        public RectDrawingVisual FindYById(int id)
        {
            if (YMappings.ContainsKey(id))
            {
                return YMappings[id];
            }
            if (YAxes.Count == 0)
            {
                return null;
            }
            return YAxes[0] as RectDrawingVisual;
        }
    }
}
