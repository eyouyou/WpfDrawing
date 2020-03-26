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
    public delegate void PushHandler(SeriesPackBase seriesPack, SubscribeResponse response);
    public interface ISubscribeable
    {
        void DoSubscribe();
        void DisposeSubscribe();
        ReplyData TranformSubscribeData(SubscribeResponse response);
        event PushHandler OnPushed;
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
                return ReferenceEquals(pack, this);
            }
            return false;
        }

        public ReplyData CacheData { get; set; }
        public void ClearCache()
        {
            CacheData = null;
        }

        /// <summary>
        /// 处理所有回包 包括推送和请求
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public abstract Task OnReply(ReplyData result);

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
