using HevoDrawing.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HevoDrawing
{
    public static class Tools
    {
        public static Vector BadVector => new Vector(double.NaN, double.NaN);
        public static bool IsBad(this Vector v)
        {
            return double.IsNaN(v.X) || double.IsNaN(v.Y);
        }
        public static List<double> GetAverageRatiosWithZero(int segements)
        {
            var ratio = 1.00 / segements;
            List<double> list = new List<double>();
            for (int i = 0; i < segements; i++)
            {
                if (i == 0)
                {
                    list.Add(0);
                }
                list.Add(ratio);
            }
            return list;
        }
        public static List<Section> ChangeToSections(List<IVariable> splitValues, List<double> splitRatio)
        {
            var sections = new List<Section>();
            if (splitRatio.IndexOf(0) == 0)
            {
                splitRatio = new List<double>(splitRatio);
                splitRatio.RemoveAt(0);
            }
            for (int i = 0; i < splitValues.Count; i++)
            {
                if (i < splitValues.Count - 1)
                {
                    if (i > splitRatio.Count)
                    {
                        return new List<Section>();
                    }
                    sections.Add(new Section() { Left = splitValues[i], Right = splitValues[i + 1], SectionRatio = splitRatio[i] });
                }
            }
            return sections;
        }
        public static List<double> GetAverageRatios(int segements)
        {
            var ratio = 1.00 / segements;
            List<double> list = new List<double>();
            for (int i = 0; i < segements; i++)
            {
                list.Add(ratio);
            }
            return list;
        }
        /// <summary>
        /// 按照当前的比例进行均分 实在不行就拿最后一个去调整
        /// </summary>
        /// <param name="ratios"></param>
        /// <returns></returns>
        public static List<double> GetAverageRatios(List<double> ratios, int retry_time = 1)
        {
            var isStartWithZero = false;
            if (ratios.IndexOf(0) == 0)
            {
                ratios = new List<double>(ratios);
                isStartWithZero = true;
            }

            for (double sum = ratios.Sum(); retry_time > 0 && (sum > 1 || sum < 0.9999); retry_time--)
            {
                for (int i = 0; i < ratios.Count; i++)
                {
                    if (isStartWithZero && i == 0)
                    {
                        continue;
                    }
                    ratios[i] = ratios[i] / sum;
                }
            }
            double sum2 = ratios.Sum();
            if (sum2 > 1)
            {
                var offset = sum2 - 1.0;
                ratios[ratios.Count - 1] = ratios[ratios.Count - 1] - offset;
            }
            return ratios;
        }
        public static double GetActualLength(this GridLength length, double all)
        {
            switch (length.GridUnitType)
            {
                case GridUnitType.Auto:
                    return 0;
                case GridUnitType.Pixel:
                    return length.Value;
                case GridUnitType.Star:
                    return all * length.Value;
            }
            return 0;
        }
        public static Size GetSize(double width, double height)
        {
            Size size = new Size();
            if (width >= 0)
            {
                size.Width = width;
            }
            if (height >= 0)
            {
                size.Height = height;
            }
            return size;
        }
        public static IEnumerable<T> find_visual_children<T>(DependencyObject root_object) where T : class
        {
            if (root_object != null)
            {
                int children_count = VisualTreeHelper.GetChildrenCount(root_object);
                for (int i = 0; i < children_count; ++i)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(root_object, i);
                    if (child != null && child is T)
                        yield return child as T;
                    foreach (T visualChild in Tools.find_visual_children<T>(child))
                        yield return visualChild;
                }
            }
        }

        public static Point GetPointByRatio(Vector diff, Vector start, double ratio)
        {
            var scal = Vector.Multiply(diff, ratio);
            return new Point(scal.X + start.X, scal.Y + start.Y);
        }


        /// <summary>
        /// 二分比较
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="low"></param>
        /// <param name="high"></param>
        /// <param name="key"></param>
        /// <param name="isCloseToBigger">往右侧靠近</param>
        /// <returns></returns>
        public static int BinaryCompare(List<double> arr, int low, int high, double key, bool isCloseToBigger)
        {
            int mid = (low + high) / 2;
            if (high < 0 || arr.Count <= mid)
            {
                return mid;
            }
            else if (low > high)
                return mid + 1;
            else
            {
                if (arr[mid] >= key)
                    return BinaryCompare(arr, low, mid - 1, key, isCloseToBigger);
                else
                    return BinaryCompare(arr, mid + 1, high, key, isCloseToBigger);
            }
        }

        public static T GetVisualDataItem<T>(this ContextData data, ContextDataItem tag)
            where T : class
        {
            if (!data.Items.ContainsKey(tag)
                || !(data.Items[tag] is T item))
            {
                return default;
            }
            return item;
        }
        public static T GetMainVisualDataItem<T>(this ContextData data, ContextDataItem tag)
            where T : class
        {
            if (!data.Current.Items.ContainsKey(tag)
                || !(data.Current.Items[tag] is T item))
            {
                return default;
            }
            return item;
        }

        public static T GetMainVisualDataItemThrow<T>(this ContextData data, ContextDataItem tag)
        {
            if (!data.Current.Items.ContainsKey(tag)
                || !(data.Current.Items[tag] is T item))
            {
                throw new ArgumentNullException($"plot need {tag} argument");
            }
            return item;
        }
        public static T GetVisualDataItemThrow<T>(this ContextData data, ContextDataItem tag)
        {
            if (!data.Items.ContainsKey(tag)
                || !(data.Items[tag] is T item))
            {
                throw new ArgumentNullException($"plot need {tag} argument");
            }
            return item;
        }

        public static VisualData<T> TransformVisualData<T>(this ContextData data)
        {
            VisualData<T> vData = new VisualData<T>();
            if (data is T t && !data.IsEmpty)
            {
                vData.Value = t;
            }
            else
            {
                vData.IsBad = true;
            }
            return vData;
        }

        public static void AddChild(this DockPanel panel, UIElement element, Dock dock)
        {
            DockPanel.SetDock(element, dock);
            panel.Children.Add(element);
        }

        /// <summary>
        /// 传递visualdata
        /// </summary>
        /// <param name="visual"></param>
        /// <param name="data"></param>
        public static void DeliverVisualData(this RectDrawingVisual visual, ContextData data)
        {
            if (!visual.IsolateData)
            {
                visual.VisualData = data;
            }
            visual.VisualData.Current = data;
            visual.Reset();
        }

    }
    public class VisualData<T>
    {
        public bool IsBad { get; set; } = false;
        public T Value { get; set; }
    }
    public static class DataExtension
    {
        public static ContextData ToFormatVisualData<X>(this Dictionary<X, double> dic)
            where X : IFormattable, IComparable
        {
            return new Chart2DContextData(dic.ToDictionary(it => new FormattableValue<X>(it.Key) as IVariable, it => new FormattableValue<double>(it.Value) as Value<double>));
        }
        public static ContextData ToVisualData<X>(this Dictionary<X, double> dic)
            where X : IComparable
        {
            return new Chart2DContextData(dic.ToDictionary(it => new Value<X>(it.Key) as IVariable, it => new Value<double>(it.Value)));
        }

        public static ContextData ToFormatVisualData<X>(this List<CroodData<X>> croods)
            where X : IFormattable, IComparable
        {
            return new Chart2DContextData2(croods.Select(it => new ChartCrood(new FormattableValue<X>(it.XData), new FormattableValue<double>(it.YData))).ToList());
        }
        public static ContextData ToFormatVisualData<X>(this IEnumerable<CroodData<X>> croods)
            where X : IFormattable, IComparable
        {
            return new Chart2DContextData2(croods.Select(it => new ChartCrood(new FormattableValue<X>(it.XData), new FormattableValue<double>(it.YData))).ToList());
        }

        public static IVariable ToFormatVisualData<T>(this T t)
            where T : IFormattable, IComparable
        {
            return new FormattableValue<T>(t);
        }
        public static bool IsBad(this IVariable value)
        {
            if (value == null)
            {
                return true;
            }
            return value.IsBad;
        }
        public static bool IsEmpty(this ContextData data)
        {
            if (data == null)
            {
                return true;
            }
            return data.IsEmpty;
        }

    }

}
