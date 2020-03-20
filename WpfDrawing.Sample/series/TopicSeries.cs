using HevoDrawing;
using HevoDrawing.Abstractions;
using HevoDrawing.Charting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WpfDrawing.Sample
{
    public interface IBlockSettable
    {
        string BlockId { get; set; }
    }
    public class TopicParam
    {
        public string BlockId { get; set; }
        public bool IsMarket { get; set; } = false;
    }
    public class TopicSeries : SeriesPackBase<TopicParam, Dictionary<DateTime, double>>
    {
        private T GetValue<T>(JToken it, string v)
        {
            if (it is JProperty value && value.Value[v] != null && !value.Value[v].HasValues && value.Value[v] is JValue trueValue && !string.IsNullOrEmpty(trueValue.Value.ToString()))
            {
                return value.Value[v].Value<T>();
            }
            return default;
        }
        private DateTime GetKeyTime(JToken it)
        {
            if (it is JProperty value && !string.IsNullOrEmpty(value.Name))
            {
                return DateTime.ParseExact(value.Name, "yyyyMMdd", CultureInfo.InvariantCulture);
            }
            return DateTime.MinValue;
        }

        public override Task<RequestResult<Dictionary<DateTime, double>>> GetData(TopicParam input, int request_id)
        {
            var str = $"http://zx.10jqka.com.cn/hotevent/api/getselfstocknum?blockid={input.BlockId}";
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(str);
                var content = await response.Content.ReadAsStringAsync();
                Dictionary<DateTime, double> dic;

                JObject @object = JObject.Parse(content);
                if (@object.TryGetValue("result", out var resultToken))
                {
                    if (input.IsMarket)
                    {
                        dic = resultToken.AsParallel().ToDictionary(it => GetKeyTime(it), it => GetValue<double>(it, "market") / 10000);
                    }
                    else
                    {
                        dic = resultToken.AsParallel().ToDictionary(it => GetKeyTime(it), it => GetValue<double>(it, input.BlockId) / 10000);
                    }

                    dic = dic.OrderBy(it => it.Key).ToDictionary(it => it.Key, it => it.Value);
                    return dic;
                }
                return new Dictionary<DateTime, double>();
            }

        }
    }
}
