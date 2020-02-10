using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfDrawing
{
    public struct ComponentKey
    {
        public ComponentKey(int canvasId, int axisId)
        {
            CanvasId = canvasId;
            AxisId = axisId;
        }
        public int CanvasId { get; set; }
        public int AxisId { get; set; }
        public override bool Equals(object obj)
        {
            if (obj is ComponentKey key
                && key.CanvasId == CanvasId && key.AxisId == AxisId)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// canvas 16左右 axisid 10以内
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return AxisId << 5 ^ CanvasId;
        }
    }
}
