using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HevoDrawing
{
    /// <summary>
    /// 这样设计是否合理？
    /// </summary>
    public enum ContextDataItem : int
    {
        //值所在比例
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
        IsHintData,
        SeriesData,
    }


    public abstract class ContextData
    {
        public List<int> ComponentIds { get; } = new List<int>();
        /// <summary>
        /// 适配各种形式的调用
        /// </summary>
        public abstract bool IsEmpty { get; }
        /// <summary>
        /// 获取上层数据 作用域概念
        /// </summary>
        public ContextData Current { get; set; }
        /// <summary>
        /// plot上下文临时数据
        /// <see cref="RectDrawingVisual.Plot"/>之前需要重置 调用<see cref="RectDrawingVisual.Reset"/>
        /// 是否需要共享内存？
        /// </summary>
        public Dictionary<ContextDataItem, object> Items { get; set; } = new Dictionary<ContextDataItem, object>();
        public abstract ContextData Copy();
    }
}
