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
            return new T() { Id = @base.SeriesVisual.Id };
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
        public SeriesVisual SeriesVisual { get; private set; }
        public SeriesPackBase(SeriesVisual seriesVisual)
        {
            SeriesVisual = seriesVisual;
        }
        public override int GetHashCode()
        {
            return SeriesVisual.Id;
        }
        public override bool Equals(object obj)
        {
            if (obj is SeriesPackBase pack)
            {
                return SeriesVisual.Id == pack.SeriesVisual.Id && SeriesVisual.Name == pack.SeriesVisual.Name;
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
