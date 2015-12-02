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
using Windows.UI.Notifications;
using Windows.Web.Http;
using StockExchangeQuotes.Annotations;
using Windows.Networking.PushNotifications;

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
        private readonly MainPageViewModel pageModel;

         public MainPage()
        {
            this.InitializeComponent();

            pageModel = new MainPageViewModel();
            DataContext = pageModel;

            pageModel.updateChannelUri();

        }



        private void SelectShare(object sender, SelectionChangedEventArgs e)
        {
            Quotation SelectedQuotation = (Quotation) PortfolioListView.SelectedItem;
            string symbol = SelectedQuotation.Symbol;
            Frame.Navigate(typeof (QuotationDetails), symbol);
        }



        private void AddToPortfolio_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
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
            AddShareBox.Visibility = AddShareBox.Visibility == Visibility.Visible
                ? (Visibility.Collapsed)
                : Visibility.Visible;
            ButtonToggle.Icon = AddShareBox.Visibility == Visibility.Visible
                ? new SymbolIcon(Symbol.Remove)
                : new SymbolIcon(Symbol.Add);
        }

        private void LogoutClick(object sender, RoutedEventArgs e)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            localSettings.Values["token"] = null;
            localSettings.Values["main_share"] = null;
            localSettings.Values["username"] = null;
            Frame.Navigate(typeof (Login));
        }

        private void RefreshPortfolio(object sender, RoutedEventArgs e)
        {
            pageModel.RefreshPortfolio();
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


        }

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
            }
            else
            {
                // Use args.QueryText to determine what to do.
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

            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var token = localSettings.Values["token"];

            request.Execute((string) token, content);
        }

        public void RefreshPortfolio()
        {
            APIRequest request = new APIRequest(APIRequest.GET, this, APIRequest.requestCodeType.Portfolio, "portfolio");

            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var token = localSettings.Values["token"];

            request.Execute((string) token, null);

        }

        async public void updateChannelUri()
        {
            PushNotificationChannel channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
            string channelUri = channel.Uri;
            APIRequest request = new APIRequest(APIRequest.POST, this, APIRequest.requestCodeType.UpdateUri, "user/updatechannelUri");

            Dictionary<string, string> dict = new Dictionary<string, string>()
            {
                {"channelUri", channelUri}
            };
            var serializer = new DataContractJsonSerializer(dict.GetType(), new DataContractJsonSerializerSettings()
            {
                UseSimpleDictionaryFormat = true
            });
            MemoryStream stream = new MemoryStream();
            serializer.WriteObject(stream, dict);
            byte[] bytes = stream.ToArray();
            string content = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var token = localSettings.Values["token"];

            request.Execute((string)token, content);
        }

        internal void AddToPortfolio_SuggestionChosen(AutoSuggestBox sender,
            AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            sender.Text = ((Quotation) args.SelectedItem).Symbol;
        }

        public void onTaskCompleted(string result, APIRequest.requestCodeType requestCode)
        {
            if (result != null)
            {
                if (requestCode == APIRequest.requestCodeType.Portfolio)
                {
                    Items.Clear();

                    JsonArray json = JsonArray.Parse(result);

                    foreach (var share in json)
                    {
                        JsonObject shareObj = share.GetObject();
                        string symbol = shareObj.GetNamedString("symbol");
                        string name = shareObj.GetNamedString("name");
                        double value = shareObj.GetNamedNumber("value");
                        bool isMain = shareObj.GetNamedBoolean("is_main");
                        Quotation q = new Quotation() {Name = name, Symbol = symbol, Value = value, IsMain = isMain};
                        Items.Add(q);
                    }
                }
                else if (requestCode == APIRequest.requestCodeType.PortfolioAdd)
                {
                    RefreshPortfolio();
                }
                else if (requestCode == APIRequest.requestCodeType.UpdateUri)
                {
                    // update sucessfull
                }
            }
            else
            {
                var toastXmlContent = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);

                var txtNodes = toastXmlContent.GetElementsByTagName("text");
                txtNodes[0].AppendChild(toastXmlContent.CreateTextNode("Server request failed."));
                txtNodes[1].AppendChild(toastXmlContent.CreateTextNode("Server is down or you lost internet connection."));

                var toast = new ToastNotification(toastXmlContent);
                var toastNotifier = ToastNotificationManager.CreateToastNotifier();
                toastNotifier.Show(toast);
            }
        }
    }
}
