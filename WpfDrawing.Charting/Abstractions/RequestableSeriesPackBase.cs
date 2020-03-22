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
        public static Result<T> MakeResult<T>(this SeriesPackBase @base)
        {
            return new Result<T>(@base.SeriesVisual.Id);
        }
    }
    public class Result<T>
    {
        public Result(int id)
        {
            Id = id;
        }
        public T Data { get; set; }
        public int Id { get; set; }
    }
    public interface IRequestable<Parameters, Response>
    {
        Parameters Data { get; set; }
        Task<Result<Response>> DoRequest(Parameters input);
    }
    public interface ISubscribeable<Parameters, Response>
    {
        Parameters Data { get; set; }
        Task<Result<Response>> DoRequest(Parameters input);
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
