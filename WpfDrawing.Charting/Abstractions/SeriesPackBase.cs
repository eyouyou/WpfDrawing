using HevoDrawing.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HevoDrawing.Charting
{
    public static class ResultExtension
    {
        public static T MakeReplyData<T>(this SeriesPackBase @base)
            where T : ReplyData, new()
        {
            return new T() { Id = @base.Id };
        }
    }

    public interface IRequestable
    {
        Task<ReplyData> DoRequest();
    }
    public interface ISubscribeable<Parameters, Response>
    {
        Parameters Data { get; set; }
        Task<Response> DoRequest(Parameters input);
    }
    public abstract class SeriesPackBase
    {
        public int Id { get; set; }
        public List<SeriesVisual> SeriesVisuals { get; private set; }
        public SeriesPackBase(params SeriesVisual[] seriesVisual)
        {
            SeriesVisuals = seriesVisual.Length == 0 ? throw new ArgumentNullException() : seriesVisual.ToList();
        }
        public override int GetHashCode()
        {
            return Id;
        }
        public override bool Equals(object obj)
        {
            if (obj is SeriesPackBase pack)
            {
                return Id == pack.Id;
            }
            return false;
        }
    }


    //public abstract class RequestableSeriesPackBase<Parameters, Response> : SeriesPackBase, IRequestable<Parameters, Response>
    //{
    //    public RequestableSeriesPackBase(SeriesVisual seriesVisual) : base(seriesVisual)
    //    {

    //    }
    //    public Parameters Data { get; set; }
    //    protected RequestResult<Response> MakeResult() { return new RequestResult<Response>(SeriesVisual.Id); }
    //    public abstract Task<RequestResult<Response>> DoRequest(Parameters input);
    //}
}
