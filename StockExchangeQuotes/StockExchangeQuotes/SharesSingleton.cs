using System;
using System.Collections.Generic;
using Windows.Data.Json;
using Windows.Web.Http;

namespace StockExchangeQuotes
{
    public class SharesSingleton
    {
        private string API_ADDRESS = "http://localhost:8080/api/";
        private List<Quotation> allItems = new List<Quotation>();

        public List<Quotation>  AllItems
        {
            get
            {
                return allItems;
            }
        } 

        private static readonly Lazy<SharesSingleton> lazy =
        new Lazy<SharesSingleton>(() => new SharesSingleton());

        public static SharesSingleton Instance { get { return lazy.Value; } }

        private SharesSingleton()
        {
            LoadAllItems();
        }

        private async void LoadAllItems()
        {
            Uri uri = new Uri(API_ADDRESS + "shares");

            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(uri);

            if (response.StatusCode != HttpStatusCode.Ok)
            {

            }
            else
            {
                string answer = await response.Content.ReadAsStringAsync();
                JsonArray json = JsonArray.Parse(answer);

                foreach (var share in json)
                {
                    JsonObject shareObj = share.GetObject();
                    string symbol = shareObj.GetNamedString("symbol");
                    string name = shareObj.GetNamedString("name");
                    Quotation q = new Quotation() { Name = name, Symbol = symbol };
                    allItems.Add(q);
                }

            }
        }
    }
}