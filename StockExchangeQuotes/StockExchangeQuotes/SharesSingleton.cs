using System;
using System.Collections.Generic;
using Windows.Data.Json;
using Windows.Web.Http;

namespace StockExchangeQuotes
{
    public class SharesSingleton: OnApiRequestCompleted
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

        private void LoadAllItems()
        {
            APIRequest request = new APIRequest(APIRequest.GET, this, APIRequest.requestCodeType.AllShares, "shares");
            request.Execute(
                "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJodWdvIiwiZXhwIjoxNDQ5NDIxMjgxOTEwfQ.n_MNFrjav_LPYCyTBx-u8ol0JUAJzUqlMtcoA1nufOo",
                null);
        }

        public void onTaskCompleted(string result, APIRequest.requestCodeType requestCode)
        {
            if (requestCode == APIRequest.requestCodeType.AllShares)
            {
                if (result != null)
                {
                    JsonArray json = JsonArray.Parse(result);

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
}