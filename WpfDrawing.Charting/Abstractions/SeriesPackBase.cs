using HevoDrawing.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HevoDrawing.Charting
{
    public interface IRequestAnalyzable<DataT>
    {
        IAnalysizer<DataT> RequestAnalysizer { get; }
    }
    public class RequestResult<Result>
    {
        public Result Data { get; set; }
        public int Id { get; set; }
    }

    public abstract class SeriesPackBase<Input, Output>
    {
        public Input SeriesData { get; set; }
        public SeriesVisual SeriesVisual { get; private set; }
        public SeriesPackBase(SeriesVisual seriesVisual)
        {
            SeriesVisual = seriesVisual;
        }
        public abstract Task<RequestResult<Output>> GetData(Input input);
        public override int GetHashCode()
        {
            return SeriesVisual.Id;
        }

        public override bool Equals(object obj)
        {
            if (obj is SeriesPackBase<Input, Output> pack)
            {
                return true;
            }
            return false;
        }
    }
}
