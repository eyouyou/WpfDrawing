using HevoDrawing.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HevoDrawing
{
    public class CroodData<X>
        where X : IFormattable, IComparable
    {
        public CroodData(X xdata, double ydata)
        {
            XData = xdata;
            YData = ydata;
        }
        public X XData { get; set; }
        public double YData { get; set; }
    }
    /// <summary>
    /// 值限制接口
    /// </summary>
    public interface IVariable : IFormattable, IComparable
    {
        bool IsBad { get; }
        Func<string, object> ValueData { get; set; }
    }

    public class Value : IVariable
    {
        public bool IsBad { get; protected set; } = true;

        public virtual Func<string, object> ValueData { get; set; }

        public virtual int CompareTo(object obj)
        {
            return 0;
        }

        public virtual string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Empty;
        }
        public override int GetHashCode()
        {
            return 0;
        }
    }

    /// <summary>
    /// 可以继承 获取更多数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Value<T> : Value, IVariable, IEquatable<Value<T>>
        where T : IComparable
    {
        public static Value<T> Bad = new Value<T>() { };
        protected Value()
        {

        }
        public Value(T value)
        {
            Data = value;
            IsBad = false;
            ValueData = (str) => this;
        }
        public T Data { get; set; }
        public override Func<string, object> ValueData { get; set; }

        public override string ToString(string format, IFormatProvider formatProvider)
        {
            return Data.ToString();
        }
        public override int CompareTo(object obj)
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

        public bool Equals(Value<T> other)
        {
            return other.IsBad == IsBad && other.Data.Equals(Data);
        }
    }

    public class FormattableValue<T> : Value<T>
        where T : IFormattable, IComparable
    {
        public override string ToString(string format, IFormatProvider formatProvider)
        {
            return Data.ToString(format, formatProvider);
        }
    }
    public class ListContextData<T> : ContextData
    {
        public ListContextData(List<T> list)
        {
            Value = list;
        }
        public List<T> Value { get; set; }

        public static ContextData Empty => throw new NotImplementedException();

        public override bool IsEmpty => throw new NotImplementedException();


        public override ContextData Copy()
        {
            return new ListContextData<T>(Value);
        }
    }
}
