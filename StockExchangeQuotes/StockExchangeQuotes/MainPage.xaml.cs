using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Web.Http;
using StockExchangeQuotes.Annotations;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace StockExchangeQuotes
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    

    public sealed partial class MainPage : Page
    {
        //private List<Quotation> allItems = new List<Quotation>();
        //List<Quotation> items = new List<Quotation>();
        private MainPageViewModel pageModel;

        public MainPage()
        {
            this.InitializeComponent();

            pageModel = new MainPageViewModel();
            DataContext = pageModel;
            
            /*
            allItems.Add(new Quotation() { Name = "GOOG", Value = 1.323 });
            allItems.Add(new Quotation() { Name = "APPL", Value = 1.101 });
            allItems.Add(new Quotation() { Name = "IBM", Value = 0.922 });
            allItems.Add(new Quotation() { Name = "GAGL", Value = 1.323 });
            allItems.Add(new Quotation() { Name = "IAM", Value = 1.323 });
            */
        }


        void SelectShare(object sender, SelectionChangedEventArgs e)
        {
            Quotation SelectedQuotation = (Quotation)PortfolioListView.SelectedItem;
            string symbol = SelectedQuotation.Symbol;
            Frame.Navigate(typeof (QuotationDetails), symbol);
        }



        private async void AddToPortfolio_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            pageModel.AddToPortfolio_TextChanged(sender, args);
        }

        private Quotation[] GetSuggestions(string text)
        {
            return pageModel.GetSuggestions(text);
        }

        private void AddToPortfolio_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            pageModel.AddToPortfolio_QuerySubmitted(sender, args);
        }

        private void AddToPortfolio_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            pageModel.AddToPortfolio_SuggestionChosen(sender, args);
        }

        private void ToggleAddShare(object sender, RoutedEventArgs e)
        {
            AddShareBox.Visibility = AddShareBox.Visibility == Visibility.Visible ? (Visibility.Collapsed) : Visibility.Visible;
            ButtonToggle.Icon = AddShareBox.Visibility == Visibility.Visible ? new SymbolIcon(Symbol.Remove) : new SymbolIcon(Symbol.Add);
        }

        private void LogoutClick(object sender, RoutedEventArgs e)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            localSettings.Values["token"] = null;
            localSettings.Values["main_share"] = null;
            localSettings.Values["username"] = null;
            Frame.Navigate(typeof (Login));
        }
    }

    public class MainPageViewModel : INotifyPropertyChanged, OnApiRequestCompleted
    {
        SharesSingleton ss = SharesSingleton.Instance;

        private ObservableCollection<Quotation> _items;
        //private ObservableCollection<Quotation> _allItems;

        public ObservableCollection<Quotation> Items
        {
            get { return _items; }
            set
            {
                if (_items == value) return;
                _items = value;
                OnPropertyChanged();
            }
        }
        /*
        public ObservableCollection<Quotation> AllItems
        {
            get { return _allItems; }
            set
            {
                if (_allItems == value) return;
                _allItems = value;
                OnPropertyChanged();
            }
        }
        */
        public MainPageViewModel()
        {
            Items = new ObservableCollection<Quotation>();
            RefreshPortfolio();
            //LoadAllItems();
            /*
            Items.Add(new Quotation() { Symbol = "GOOG", Value = 1.323 });
            Items.Add(new Quotation() { Symbol = "APPL", Value = 1.101 });
            Items.Add(new Quotation() { Symbol = "IBM", Value = 0.922 });
            */
            /*
            AllItems = new ObservableCollection<Quotation>();
            AllItems.Add(new Quotation() { Name = "IAM", Value = 1.323 });
            AllItems.Add(new Quotation() { Name = "GOOG", Value = 1.323 });
            AllItems.Add(new Quotation() { Name = "APPL", Value = 1.101 });
            AllItems.Add(new Quotation() { Name = "IBM", Value = 0.922 });
            AllItems.Add(new Quotation() { Name = "GAGL", Value = 1.323 });
            */
            
        }
        /*
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
                JsonObject json = JsonObject.Parse(answer);
                JsonArray resultArray = json.GetNamedArray("result");

                foreach (var share in resultArray)
                {
                    JsonObject shareJson = JsonObject.Parse(share.Stringify());
                    string symbol = shareJson.GetNamedString("symbol");
                    string name = shareJson.GetNamedString("name");
                    Quotation q = new Quotation() {Name = name, Symbol = symbol};
                    AllItems.Add(q);
                }

            }
        }
        */

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        internal async void AddToPortfolio_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                string find = sender.Text.ToUpper();
                sender.ItemsSource = await Task.Run(
                    () => this.GetSuggestions(find));
            }
        }

        internal Quotation[] GetSuggestions(string text)
        {
            Quotation[] result = null;

            if (text.Length == 0)
                return null;

            result = ss.AllItems.Where(x => x.Symbol.Contains(text)).ToArray();

            return result;
        }

        internal void AddToPortfolio_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                AddPortfolioShare(sender.Text);
                // User selected an item from the suggestion list, take an action on it here.
                //TODO add only if it doesn't exist in the items yet. add in the db also
            }
            else
            {
                // Use args.QueryText to determine what to do.
                //TODO verify if the share in args.QueryText exists. If so, get from DB and add to Portfolio
                AddPortfolioShare(args.QueryText);
            }
        }

        private void AddPortfolioShare(string text)
        {
            APIRequest request = new APIRequest(APIRequest.POST, this, APIRequest.requestCodeType.PortfolioAdd, "portfolio/add");

            Dictionary<string, string> dict = new Dictionary<string, string>()
            {
                {"symbol", text}
            };
            var serializer = new DataContractJsonSerializer(dict.GetType(), new DataContractJsonSerializerSettings()
            {
                UseSimpleDictionaryFormat = true
            });
            MemoryStream stream = new MemoryStream();
            serializer.WriteObject(stream, dict);
            byte[] bytes = stream.ToArray();
            string content = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

            //TODO GET ACCESS TOKEN FROM SETTINGS (Windows.Storage.ApplicationData.Current.LocalSettings)
            request.Execute(
                "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJodWdvIiwiZXhwIjoxNDQ5NDIxMjgxOTEwfQ.n_MNFrjav_LPYCyTBx-u8ol0JUAJzUqlMtcoA1nufOo",
                content);
        }

        private void RefreshPortfolio()
        {
            Items.Clear();
            APIRequest request = new APIRequest(APIRequest.GET, this, APIRequest.requestCodeType.Portfolio, "portfolio");
            //TODO GET ACCESS TOKEN FROM SETTINGS (Windows.Storage.ApplicationData.Current.LocalSettings)
            request.Execute("eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJodWdvIiwiZXhwIjoxNDQ5NDIxMjgxOTEwfQ.n_MNFrjav_LPYCyTBx-u8ol0JUAJzUqlMtcoA1nufOo", null);
            
        }

        internal void AddToPortfolio_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            sender.Text = ((Quotation)args.SelectedItem).Symbol;
        }

        public void onTaskCompleted(string result, APIRequest.requestCodeType requestCode)
        {
            if (requestCode == APIRequest.requestCodeType.Portfolio)
            {
                if (result != null)
                {
                    JsonArray json = JsonArray.Parse(result);

                    foreach (var share in json)
                    {
                        JsonObject shareObj = share.GetObject();
                        string symbol = shareObj.GetNamedString("symbol");
                        string name = shareObj.GetNamedString("name");
                        double value = shareObj.GetNamedNumber("value");
                        Quotation q = new Quotation() { Name = name, Symbol = symbol, Value = value };
                        Items.Add(q);
                    }
                }
            }else if (requestCode == APIRequest.requestCodeType.PortfolioAdd)
            {
                RefreshPortfolio();
            }
        }
    }
}
