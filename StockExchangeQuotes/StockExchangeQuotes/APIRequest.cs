using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Web.Http.Headers;

namespace StockExchangeQuotes
{
    public class APIRequest
    {
        private OnApiRequestCompleted listener;
        private const string API_ADDRESS = "http://localhost:8080/api/";

        public static int GET = 1;
        public static int POST = 2;
        private int requestType;

        public enum requestCodeType
        {
            Login,
            Register,
            Portfolio,
            PortfolioAdd,
            AllShares,
            Share,
            ShareEvolution,
            Favorite,
            Unfavorite,
            SetLimit,
            ClearLimitUp,
            ClearLimitDown,
            UpdateUri
        }

        private requestCodeType requestCode;
        private string path;

        public APIRequest(int requestType, OnApiRequestCompleted listener, requestCodeType requestCode, string path)
        {
            this.requestType = requestType;
            this.listener = listener;
            this.requestCode = requestCode;
            this.path = path;
        }

        public async void Execute(string token, string content)
        {
            Uri uri = new Uri(API_ADDRESS + path);

            var rootFilter = new HttpBaseProtocolFilter();

            rootFilter.CacheControl.ReadBehavior = Windows.Web.Http.Filters.HttpCacheReadBehavior.MostRecent;
            rootFilter.CacheControl.WriteBehavior = Windows.Web.Http.Filters.HttpCacheWriteBehavior.NoCache;

            HttpClient client = new HttpClient(rootFilter);
            //client.DefaultRequestHeaders.Add("timestamp", DateTime.Now.ToString());
            if(token != null)
                client.DefaultRequestHeaders.Add("x-access-token", token);

            System.Threading.CancellationTokenSource source = new System.Threading.CancellationTokenSource(2000);

            HttpResponseMessage response = null;
            if (requestType == GET)
            {
                try
                {
                    response = await client.GetAsync(uri).AsTask(source.Token);
                }
                catch (TaskCanceledException)
                {
                    response = null;
                }

            }else if (requestType == POST)
            {
                HttpRequestMessage msg = new HttpRequestMessage(new HttpMethod("POST"), uri);
                if (content != null)
                {
                    msg.Content = new HttpStringContent(content);
                    msg.Content.Headers.ContentType = new HttpMediaTypeHeaderValue("application/json");
                }

                try
                {
                    response = await client.SendRequestAsync(msg).AsTask(source.Token);
                }
                catch (TaskCanceledException)
                {
                    response = null;
                }
            }

            if (response == null)
            {
                if (listener != null)
                    listener.onTaskCompleted(null, requestCode);
            }
            else
            {
                string answer = await response.Content.ReadAsStringAsync();

                if(listener != null)
                    listener.onTaskCompleted(answer, requestCode);
            }
        }
    }

    public interface OnApiRequestCompleted
    {
        void onTaskCompleted(string result, APIRequest.requestCodeType requestCode);
    }
}
