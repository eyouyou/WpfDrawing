using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HevoDrawing.Charting
{
    public delegate Task PiplineDelegate(ChartContext data); 
    
    public interface IPipline
    {
        Task PipAsync(ChartContext context, PiplineDelegate next);
    }
    public interface IAggratePipline : IPipline
    {
    }
    public interface ISpecificPipline : IPipline
    {
    }

}
