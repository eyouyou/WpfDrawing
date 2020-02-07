using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WpfDrawing
{
    public class ComponentId
    {
        public static ComponentId Current = new ComponentId();
        private int _id = 0;
        public int GenerateId()
        {
            return Interlocked.Increment(ref _id);
        }
        public void Reset()
        {
            Interlocked.Exchange(ref _id, 0);
        }
    }
}
