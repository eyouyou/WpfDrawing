using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HevoDrawing.Charting
{

    public class HttpRequestor : IRequstable<string, string>
    {
        ConcurrentDictionary<string, HttpClient> Clients = new ConcurrentDictionary<string, HttpClient>();
        public HttpClient MakeClient(string url)
        {
            var uri = new Uri(url);
            if (Clients.Count >= 5)
            {
                Clients.Clear();
            }
            return Clients.GetOrAdd(uri.AbsolutePath, (baseUri) =>
            {
                var httpclient = new HttpClient() { BaseAddress = new Uri(baseUri) };
                return httpclient;
            });
        }

        public async Task<string> Request(string url)
        {
            var httpclient = MakeClient(url);
            return await httpclient.GetStringAsync(url);
        }
    }
}
