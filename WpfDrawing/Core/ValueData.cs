using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfDrawing
{
    public static class DataExtension
    {
        public static RectVisualContextData ToVisualData<X>(this Dictionary<X, double> dic)
            where X : IFormattable, IComparable
        {
            return new RectChartContextData(dic.ToDictionary(it => new Value<X>(it.Key) as IVariable, it => new Value<double>(it.Value)));
        }
        public static IVariable ToVisualData<T>(this T t)
            where T : IFormattable, IComparable
        {
            return new Value<T>(t);
        }
        public static bool IsBad(this IVariable value)
        {
            if (value == null)
            {
                return true;
            }
            return value.IsBad;
        }
        public static bool IsEmpty(this RectVisualContextData data)
        {
            if (data == null)
            {
                return true;
            }
            return data.IsEmpty;
        }

    }
    /// <summary>
    /// 值限制接口
    /// </summary>
    public interface IVariable : IFormattable, IComparable
    {
        bool IsBad { get; }
        Func<string, object> ValueData { get; set; }
    }

    /// <summary>
    /// 可以继承 获取更多数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Value<T> : IVariable
        where T : IFormattable, IComparable
    {
        public bool IsBad { get; private set; } = true;
        public Value(T value)
        {
            Data = value;
            IsBad = false;
            ValueData = (str) => this;
        }
        public T Data { get; set; }
        public Func<string, object> ValueData { get; set; }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return Data.ToString(format, formatProvider);
        }
        public int CompareTo(object obj)
        {
            if (obj is Value<T> value)
            {
                return Data.CompareTo(value.Data);
            }
            return 0;
        }

        public override int GetHashCode()
        {
            return Data.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj is Value<T> data)
            {
                return data.IsBad == IsBad && data.Data.Equals(Data);
            }
            return false;
        }


    }
    public class ListContextData<T> : RectVisualContextData
    {
        public ListContextData(List<T> list)
        {
            Value = list;
        }
        public List<T> Value { get; set; }

        public static RectVisualContextData Empty => throw new NotImplementedException();

        public override bool IsEmpty => throw new NotImplementedException();


        public override RectVisualContextData Copy()
        {
            return new ListContextData<T>(Value);
        }
    }
}
