using HevoDrawing;
using HevoDrawing.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HevoDrawing
{
    public class Chart2DContextData : ContextData
    {
        public Chart2DContextData(double max, double min, DiscreteAxisContextData xs)
            : this(new Value<double>(max), new Value<double>(min), xs)
        {
        }
        public Chart2DContextData(Value<double> max, Value<double> min, List<IVariable> xs)
            : this(max, min, new DiscreteAxisContextData(xs))
        {
        }
        public Chart2DContextData(Value<double> max, Value<double> min, DiscreteAxisContextData xs)
            : this(new Range() { Max = max, Min = min }, xs)
        {
        }

        public Chart2DContextData(Range range, DiscreteAxisContextData xs)
            : this(new Dictionary<IVariable, Value<double>>(), new ContinuousAxisContextData(range), xs)
        {
        }
        public Chart2DContextData(Chart2DContextData axisVisualData)
            : this(axisVisualData.Data, axisVisualData.YData, axisVisualData.XData)
        {
        }
        public Chart2DContextData(Dictionary<IVariable, Value<double>> data)
        {
            Data = data;
            XData = new DiscreteAxisContextData(Data.Keys.ToList());
            var ydata = Data.Values.ToList();
            YData = new ContinuousAxisContextData(ydata);
        }
        private Chart2DContextData(Dictionary<IVariable, Value<double>> data, ContinuousAxisContextData ydata, DiscreteAxisContextData xdata)
        {
            Data = data;
            YData = ydata;
            XData = xdata;
            XData.Items = Items;
            YData.Items = Items;
        }
        public Chart2DContextData(List<IVariable> xData, List<double> yData)
        {
            if (xData.Count != yData.Count)
            {
                throw new ArgumentException($"{nameof(xData.Count)} != {nameof(yData.Count)}");
            }
            Data = new Dictionary<IVariable, Value<double>>();
            var index = 0;
            foreach (var item in xData)
            {
                Data.Add(item, new Value<double>(yData[index]));
                index++;
            }
            XData = new DiscreteAxisContextData(xData);
            YData = new ContinuousAxisContextData(yData);
            XData.Items = Items;
            YData.Items = Items;
        }
        public Dictionary<IVariable, Value<double>> Data { get; set; }
        public DiscreteAxisContextData XData { get; set; }
        public ContinuousAxisContextData YData { get; set; }

        public override bool IsEmpty => XData.Data.Count == 0 || YData.Range.IsEmpty;

        public static Chart2DContextData Empty => new Chart2DContextData(new Range(), DiscreteAxisContextData.Empty);

        public override ContextData Copy()
        {
            var data = new Chart2DContextData(YData.Range.Max, YData.Range.Min, XData.Data.ToList());
            data.Data = Data;
            data.Items = new Dictionary<ContextDataItem, object>(Items);
            return data;
        }
    }
}
