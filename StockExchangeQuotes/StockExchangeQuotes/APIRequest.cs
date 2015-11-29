using System;
using System.Net.Http.Headers;
using Windows.Data.Json;
using Windows.Web.Http;
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
            AllShares
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

            HttpClient client = new HttpClient();
            if(token != null)
                client.DefaultRequestHeaders.Add("x-access-token", token);

            HttpResponseMessage response = null;
            if (requestType == GET)
            {
                response = await client.GetAsync(uri);
            }else if (requestType == POST)
            {
                HttpRequestMessage msg = new HttpRequestMessage(new HttpMethod("POST"), uri);
                msg.Content = new HttpStringContent(content);
                msg.Content.Headers.ContentType = new HttpMediaTypeHeaderValue("application/json");
                response = await client.SendRequestAsync(msg).AsTask();
            }

            if (response.StatusCode == HttpStatusCode.Ok)
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
