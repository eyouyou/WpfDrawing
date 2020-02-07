using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfDrawing
{
    public delegate void VisualChanged(RectDrawingVisual visual, Operations op);
    public abstract class RectDrawingVisualDataSource
    {
        public abstract event VisualChanged VisualChangedHandler;
    }
    public enum Operations
    {
        Add, Remove
    }
    public class ChartVisualCollection : RectDrawingVisualDataSource
    {
        public XAxisVisualCollection AxisXVisuals = new XAxisVisualCollection();
        public YAxisVisualCollection AxisYVisuals = new YAxisVisualCollection();
        public SeriesVisualCollection SeriesVisuals = new SeriesVisualCollection();

        public ChartVisualCollection()
        {
            AxisXVisuals.DataSource = this;
            AxisYVisuals.DataSource = this;
            SeriesVisuals.DataSource = this;
        }
        /// <summary>
        /// Y <=> Series
        /// </summary>
        private Dictionary<int, List<SeriesVisual>> AxisMapping = new Dictionary<int, List<SeriesVisual>>();
        private Dictionary<int, List<AxisVisual>> SeriesMapping = new Dictionary<int, List<AxisVisual>>();

        List<SeriesVisual> Series = new List<SeriesVisual>();
        List<AxisVisual> AxisYs = new List<AxisVisual>();
        List<AxisVisual> AxisXs = new List<AxisVisual>();

        public override event VisualChanged VisualChangedHandler;
        public void AddSeries(SeriesVisual series)
        {
            SeriesVisuals.Add(series);
            Series.Add(series);

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
            AxisYVisuals.Add(axis);
            AxisYs.Add(axis);

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
            SeriesMapping[id].Add(axis);
            axis.DataSource = this;
        }
        public void AddAxisX(AxisVisual axis)
        {
            AxisXVisuals.Add(axis);
            AxisXs.Add(axis);
            axis.DataSource = this;
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
        public List<SeriesVisual> GetSeriesCollection()
        {
            if (Series.Count == 0)
            {
                return new List<SeriesVisual>();
            }
            return new List<SeriesVisual>(Series);
        }
        public List<AxisVisual> GetAxisYCollection()
        {
            if (AxisYs.Count == 0)
            {
                return new List<AxisVisual>();
            }
            return new List<AxisVisual>(AxisYs);
        }
        public List<AxisVisual> GetAxisXCollection()
        {
            if (AxisXs.Count == 0)
            {
                return new List<AxisVisual>();
            }
            return new List<AxisVisual>(AxisXs);
        }

    }
}
