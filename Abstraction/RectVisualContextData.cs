using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFAnimation
{
    /// <summary>
    /// 这样设计是否合理？
    /// </summary>
    public enum ContextDataItem : int
    {
        SortedSplitPoints,
        SortedSplitRatios,
        //值所在比例
        ValueRatios,
        ValueRatioCoordinate,
        SplitValues,
        //
        AxisYSeriesMappings,
        //
        MouseEvent,
        //
        Ratios,
        IsInterregional,
        //
        Pointer,
        HitPointer,
        SeriesData,
    }


    public abstract class RectVisualContextData
    {
        public int ComponentId { get; set; }
        /// <summary>
        /// 适配各种形式的调用
        /// </summary>
        public abstract bool IsEmpty { get; }
        /// <summary>
        /// 获取上层数据 作用域概念
        /// </summary>
        public RectVisualContextData Current { get; set; }
        /// <summary>
        /// plot上下文临时数据
        /// <see cref="RectDrawingVisual.Plot"/>之前需要重置 调用<see cref="RectDrawingVisual.Reset"/>
        /// 是否需要共享内存？
        /// </summary>
        public Dictionary<ContextDataItem, object> Items { get; set; } = new Dictionary<ContextDataItem, object>();
        public abstract RectVisualContextData Copy();
    }
}
