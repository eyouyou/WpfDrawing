using HevoDrawing;
using HevoDrawing.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HevoDrawing
{
    public abstract class TwoDimensionalContextData : ContextData
    {
        public abstract bool TryGetValue(IVariable x, out Value<double> y);
        public abstract List<ChartCrood> ChartCroods { get; }
        public abstract ContinuousAxisContextData YContextData { get; }
        public abstract DiscreteAxisContextData XContextData { get; }
        public abstract TwoDimensionalContextData GeneraterNewData(List<ChartCrood> croods);
        public void CopyComponentIds(TwoDimensionalContextData data)
        {
            ComponentIds.Clear();
            XContextData.ComponentIds.Clear();
            YContextData.ComponentIds.Clear();

            ComponentIds.AddRange(data.ComponentIds);
            XContextData.ComponentIds.AddRange(data.XContextData.ComponentIds);
            YContextData.ComponentIds.AddRange(data.YContextData.ComponentIds);
        }
    }
    /// <summary>
    /// x不允许不同数据
    /// 查询速度快
    /// 调用请直接使用<see cref="TwoDimensionalContextData"/>
    /// </summary>
    internal class Chart2DContextData : TwoDimensionalContextData
    {
        public Chart2DContextData(Chart2DContextData axisVisualData)
            : this(axisVisualData.Data, axisVisualData.YData, axisVisualData.XData)
        {
        }

        public Chart2DContextData(Dictionary<IVariable, Value<double>> data)
        {
            Data = new Dictionary<IVariable, Value<double>>();
            List<Value<double>> yValues = new List<Value<double>>();
            List<IVariable> xValues = new List<IVariable>();
            _chartCroods = new List<ChartCrood>();

            foreach (var item in data)
            {
                if (double.IsNaN(item.Value.Data))
                {
                    continue;
                }
                Data[item.Key] = item.Value;
                yValues.Add(item.Value);
                xValues.Add(item.Key);
                _chartCroods.Add(new ChartCrood(item.Key, item.Value));
            }
            YData = new ContinuousAxisContextData(yValues);
            XData = new DiscreteAxisContextData(xValues);
            XData.Items = new Dictionary<ContextDataItem, object>(Items);
            YData.Items = new Dictionary<ContextDataItem, object>(Items);
        }
        private Chart2DContextData(Dictionary<IVariable, Value<double>> data, ContinuousAxisContextData ydata, DiscreteAxisContextData xdata)
        {
            Data = data.Where(it => !double.IsNaN(it.Value.Data)).ToDictionary(it => it.Key, it => it.Value);
            YData = ydata;
            XData = xdata;
            XData.Items = new Dictionary<ContextDataItem, object>(Items);
            YData.Items = new Dictionary<ContextDataItem, object>(Items);

            _chartCroods = Data.Select(it => new ChartCrood(it.Key, it.Value)).ToList();
        }
        private Chart2DContextData(Dictionary<IVariable, Value<double>> data, Range range)
            : this(data, new ContinuousAxisContextData(range), new DiscreteAxisContextData(data.Keys.ToList()))
        {
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
                if (double.IsNaN(yData[index]))
                {
                    continue;
                }
                Data.Add(item, new FormattableValue<double>(yData[index]));
                index++;
            }
            XData = new DiscreteAxisContextData(xData);
            YData = new ContinuousAxisContextData(yData);
            XData.Items = new Dictionary<ContextDataItem, object>(Items);
            YData.Items = new Dictionary<ContextDataItem, object>(Items);
            _chartCroods = Data.Select(it => new ChartCrood(it.Key, it.Value)).ToList();
        }
        private List<ChartCrood> _chartCroods = null;
        public Dictionary<IVariable, Value<double>> Data { get; private set; }
        private DiscreteAxisContextData XData { get; set; } = DiscreteAxisContextData.Empty;
        private ContinuousAxisContextData YData { get; set; } = ContinuousAxisContextData.Empty;

        public override bool IsEmpty => XData.Data.Count == 0 || YData.Range.IsEmpty;

        public static Chart2DContextData Empty => new Chart2DContextData(new Dictionary<IVariable, Value<double>>());

        public override List<ChartCrood> ChartCroods => _chartCroods;

        public override ContinuousAxisContextData YContextData => YData;

        public override DiscreteAxisContextData XContextData => XData;

        public override ContextData Copy()
        {
            var data = new Chart2DContextData(new Dictionary<IVariable, Value<double>>(Data), YData.Range.Copy());
            data.Items = new Dictionary<ContextDataItem, object>(Items);
            return data;
        }

        public override bool TryGetValue(IVariable x, out Value<double> y)
        {
            if (Data.TryGetValue(x, out var value))
            {
                y = value;
                return true;
            }
            else
            {
                y = Value<double>.BadT;
                return false;
            }
        }

        public override TwoDimensionalContextData GeneraterNewData(List<ChartCrood> croods)
        {
            return new Chart2DContextData(croods.ToDictionary(it => it.X, it => it.Y));
        }
    }

    /// <summary>
    /// 查询速度慢 list
    /// 但是可以重复key
    /// 调用请直接使用<see cref="TwoDimensionalContextData"/>
    /// </summary>
    internal class Chart2DContextData2 : TwoDimensionalContextData
    {
        public Chart2DContextData2(double max, double min, DiscreteAxisContextData xs)
            : this(new FormattableValue<double>(max), new FormattableValue<double>(min), xs)
        {
        }
        public Chart2DContextData2(Value<double> max, Value<double> min, List<IVariable> xs)
            : this(max, min, new DiscreteAxisContextData(xs))
        {
        }
        public Chart2DContextData2(Value<double> max, Value<double> min, DiscreteAxisContextData xs)
            : this(new Range(min, max), xs)
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
            Data = data.Where(it => !double.IsNaN(it.Y.Data)).ToList();
            XData = new DiscreteAxisContextData(Data.Select(it => it.X).ToList());
            var ydata = Data.Select(it => it.Y).ToList();
            YData = new ContinuousAxisContextData(ydata);
        }
        private Chart2DContextData2(List<ChartCrood> data, ContinuousAxisContextData ydata, DiscreteAxisContextData xdata)
        {
            Data = data.Where(it => !double.IsNaN(it.Y.Data)).ToList();
            YData = ydata;
            XData = xdata;
            XData.Items = new Dictionary<ContextDataItem, object>(Items);
            YData.Items = new Dictionary<ContextDataItem, object>(Items);
        }
        private Chart2DContextData2(List<IVariable> xData, List<double> yData)
        {
            if (xData.Count != yData.Count)
            {
                throw new ArgumentException($"{nameof(xData.Count)} != {nameof(yData.Count)}");
            }
            Data = new List<ChartCrood>();
            var index = 0;
            foreach (var item in xData)
            {
                if (double.IsNaN(yData[index]))
                {
                    continue;
                }
                Data.Add(new ChartCrood(item, new FormattableValue<double>(yData[index])));
                index++;
            }
            XData = new DiscreteAxisContextData(xData);
            YData = new ContinuousAxisContextData(yData);
            XData.Items = new Dictionary<ContextDataItem, object>(Items);
            YData.Items = new Dictionary<ContextDataItem, object>(Items);
        }

        public override bool TryGetValue(IVariable x, out Value<double> y)
        {
            var first = Data.FirstOrDefault(it => it.X.Equals(x));
            if (first.IsBad)
            {
                y = Value<double>.BadT;
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

        public override TwoDimensionalContextData GeneraterNewData(List<ChartCrood> croods)
        {
            return new Chart2DContextData2(croods);
        }
    }
    public struct ChartCrood
    {
        public ChartCrood(IVariable x, Value<double> y)
        {
            X = x;
            Y = y;
            Point = new Point();
        }
        public ChartCrood(IVariable x, Value<double> y, Point point)
        {
            X = x;
            Y = y;
            Point = point;
        }
        public bool IsBad => X == null || Y.IsBad;
        public IVariable X { get; set; }
        public Value<double> Y { get; set; }
        public Point Point { get; set; }
    }
}
