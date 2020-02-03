using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFAnimation
{
    public delegate void IntersectChanged(List<SeriesData> data);

    public interface IIntersectable
    {
        event IntersectChanged IntersectChangedHandler;
    }
}
