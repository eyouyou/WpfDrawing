using HevoDrawing;
using HevoDrawing.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HevoDrawing
{
    public abstract class TwoDimensionalContextData : ContextData
    {
        public abstract bool ContainsX(IVariable x, out Value<double> y);
        public abstract List<ChartCrood> ChartCroods { get; }
        public abstract ContinuousAxisContextData YContextData { get; }
        public abstract DiscreteAxisContextData XContextData { get; }
    }
    /// <summary>
    /// x不允许不同数据
    /// 查询速度快
    /// </summary>
    public class Chart2DContextData : TwoDimensionalContextData
    {
        public Chart2DContextData(Chart2DContextData axisVisualData)
            : this(axisVisualData.Data, axisVisualData.YData, axisVisualData.XData)
        {
        }

        public Chart2DContextData(Dictionary<IVariable, Value<double>> data)
            : this(data, new ContinuousAxisContextData(data.Values.ToList()), new DiscreteAxisContextData(data.Keys.ToList()))
        {
        }

        private Chart2DContextData(Dictionary<IVariable, Value<double>> data, ContinuousAxisContextData ydata, DiscreteAxisContextData xdata)
        {
            Data = data;
            YData = ydata;
            XData = xdata;
            XData.Items = Items;
            YData.Items = Items;

            _chartCroods = Data.Select(it => new ChartCrood(it.Key, it.Value)).ToList();
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
            _chartCroods = Data.Select(it => new ChartCrood(it.Key, it.Value)).ToList();
        }
        private List<ChartCrood> _chartCroods = null;
        public Dictionary<IVariable, Value<double>> Data { get; private set; }
        public DiscreteAxisContextData XData { get; private set; }
        public ContinuousAxisContextData YData { get; private set; }

        public override bool IsEmpty => XData.Data.Count == 0 || YData.Range.IsEmpty;

        public static Chart2DContextData Empty => new Chart2DContextData(new Dictionary<IVariable, Value<double>>());

        public override List<ChartCrood> ChartCroods => _chartCroods;

        public override ContinuousAxisContextData YContextData => YData;

        public override DiscreteAxisContextData XContextData => XData;

        public override ContextData Copy()
        {
            var data = new Chart2DContextData(new Dictionary<IVariable, Value<double>>(Data));
            data.Data = Data;
            data.Items = new Dictionary<ContextDataItem, object>(Items);
            return data;
        }

        public override bool ContainsX(IVariable x, out Value<double> y)
        {
            if (Data.TryGetValue(x, out var value))
            {
                y = value;
                return true;
            }
            else
            {
                y = Value<double>.Bad;
                return false;
            }
        }
    }

    /// <summary>
    /// 查询速度慢 list
    /// 但是可以重复key
    /// </summary>
    public class Chart2DContextData2 : TwoDimensionalContextData
    {
        public Chart2DContextData2(double max, double min, DiscreteAxisContextData xs)
            : this(new Value<double>(max), new Value<double>(min), xs)
        {
        }
        public Chart2DContextData2(Value<double> max, Value<double> min, List<IVariable> xs)
            : this(max, min, new DiscreteAxisContextData(xs))
        {
        }
        public Chart2DContextData2(Value<double> max, Value<double> min, DiscreteAxisContextData xs)
            : this(new Range() { Max = max, Min = min }, xs)
        {
        }

        public Chart2DContextData2(Range range, DiscreteAxisContextData xs)
            : this(new List<ChartCrood>(), new ContinuousAxisContextData(range), xs)
        {
        }
        public Chart2DContextData2(Chart2DContextData2 axisVisualData)
            : this(axisVisualData.Data, axisVisualData.YData, axisVisualData.XData)
        {
        }
        public Chart2DContextData2(List<ChartCrood> data)
        {
            Data = data;
            XData = new DiscreteAxisContextData(Data.Select(it => it.X).ToList());
            var ydata = Data.Select(it => it.Y).ToList();
            YData = new ContinuousAxisContextData(ydata);
        }
        private Chart2DContextData2(List<ChartCrood> data, ContinuousAxisContextData ydata, DiscreteAxisContextData xdata)
        {
            Data = data;
            YData = ydata;
            XData = xdata;
            XData.Items = Items;
            YData.Items = Items;
        }
        public Chart2DContextData2(List<IVariable> xData, List<double> yData)
        {
            if (xData.Count != yData.Count)
            {
                throw new ArgumentException($"{nameof(xData.Count)} != {nameof(yData.Count)}");
            }
            Data = new List<ChartCrood>();
            var index = 0;
            foreach (var item in xData)
            {
                Data.Add(new ChartCrood(item, new Value<double>(yData[index])));
                index++;
            }
            XData = new DiscreteAxisContextData(xData);
            YData = new ContinuousAxisContextData(yData);
            XData.Items = Items;
            YData.Items = Items;
        }

        public override bool ContainsX(IVariable x, out Value<double> y)
        {
            var first = Data.FirstOrDefault(it => it.X == x);
            if (first == null)
            {
                y = Value<double>.Bad;
                return false;
            }
            y = first.Y;
            return true;
        }
        public List<ChartCrood> Data { get; set; }
        public DiscreteAxisContextData XData { get; set; }
        public ContinuousAxisContextData YData { get; set; }

        public override bool IsEmpty => XData.Data.Count == 0 || YData.Range.IsEmpty;

        public static Chart2DContextData Empty => new Chart2DContextData(new Dictionary<IVariable, Value<double>>());
        public override ContinuousAxisContextData YContextData => YData;

        public override DiscreteAxisContextData XContextData => XData;

        public override List<ChartCrood> ChartCroods => Data;

        public override ContextData Copy()
        {
            var data = new Chart2DContextData2(YData.Range.Max, YData.Range.Min, XData.Data.ToList());
            data.Data = Data;
            data.Items = new Dictionary<ContextDataItem, object>(Items);
            return data;
        }
    }
    public class ChartCrood
    {
        public ChartCrood(IVariable x, Value<double> y)
        {
            X = x;
            Y = y;
        }
        public IVariable X { get; set; }
        public Value<double> Y { get; set; }
    }
}
