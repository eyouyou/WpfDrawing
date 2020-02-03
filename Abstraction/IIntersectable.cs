using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFAnimation
{
    public delegate void IntersectChangedHandler(List<SeriesData> data);

    public interface IIntersectable
    {
        event IntersectChangedHandler IntersectChanged;
    }
}
