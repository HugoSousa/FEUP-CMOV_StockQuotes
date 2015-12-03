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
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Json;
using System.Text;
using Windows.Data.Json;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using StockExchangeQuotes.Annotations;
using WinRTXamlToolkit;
using WinRTXamlToolkit.Controls;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;

namespace StockExchangeQuotes
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class QuotationDetails : Page
    {
        private QuotationDetailsViewModel pageModel;
        public QuotationDetails()
        {
            this.InitializeComponent();
            pageModel = new QuotationDetailsViewModel();
            pageModel.NavigatePortfolio += NavigatePortfolio;
            DataContext = pageModel;
            
        }

        private void NavigatePortfolio()
        {
            Frame.Navigate(typeof (MainPage));
        }

        protected override void OnNavigatedTo(NavigationEventArgs arg)
        {
            pageModel.Symbol = (string) arg.Parameter;
            base.OnNavigatedTo(arg);
        }

        private void MainClick(object sender, RoutedEventArgs e)
        {
            pageModel.MainClick(sender, e);
        }

        private void SetLimit(object sender, RoutedEventArgs e)
        {
            Quotation q = null;

            if ((string)((Button)sender).Tag == "SetLimitUp")
                q = new Quotation() {Symbol = pageModel.Symbol, Name = pageModel.Name, Value = pageModel.Value, LimitType = "Upper Limit"};
            else if((string)((Button)sender).Tag == "SetLimitDown")
                q = new Quotation() { Symbol = pageModel.Symbol, Name = pageModel.Name, Value = pageModel.Value, LimitType = "Lower Limit" };

            Frame.Navigate(typeof (SetLimitDialog), q);
        }
        

        private void ClearLimit(object sender, RoutedEventArgs e)
        {
            pageModel.ClearLimit(sender, e);
        }


        private void Refresh(object sender, RoutedEventArgs e)
        {
            pageModel.RefreshShare();
        }

        private async void Delete(object sender, RoutedEventArgs e)
        {
            // Create the message dialog and set its content
            var messageDialog = new MessageDialog("Are you sure you want to delete this share from portfolio?");

            // Add commands and set their callbacks; both buttons use the same callback function instead of inline event handlers
            messageDialog.Commands.Add(new UICommand(
                "Cancel",
                new UICommandInvokedHandler(this.CommandInvokedHandler),
                0));
            messageDialog.Commands.Add(new UICommand(
                "Yes",
                new UICommandInvokedHandler(this.CommandInvokedHandler),
                1));

            // Set the command that will be invoked by default
            messageDialog.DefaultCommandIndex = 0;

            // Set the command to be invoked when escape is pressed
            messageDialog.CancelCommandIndex = 0;

            // Show the message dialog
            await messageDialog.ShowAsync();
        }

        private void CommandInvokedHandler(IUICommand command)
        {
            // Display message showing the label of the command that was invoked
            //get clicked button
            int id = (int)command.Id;
            if (id == 1)
            {
                //delete share from portfolio and redirect to portfolio
                pageModel.DeleteShare();
            }

        }
        
    }

    public class QuotationDetailsViewModel : INotifyPropertyChanged, OnApiRequestCompleted
    {
        public delegate void NavigatePortfolioAction();
        public event NavigatePortfolioAction NavigatePortfolio;

        private ObservableCollection<Tuple<DateTime, double>> _valuesEvolution = new ObservableCollection<Tuple<DateTime, double>>();
        private ObservableCollection<Tuple<DateTime, double>> _values2Evolution = new ObservableCollection<Tuple<DateTime, double>>();

        private string _symbol;
        public string Symbol
        {
            get { return _symbol; }
            set
            {
                _symbol = value;
                RefreshShare();
                OnPropertyChanged();
            }
        }

        public void RefreshShare()
        {
            APIRequest request = new APIRequest(APIRequest.GET, this, APIRequest.requestCodeType.Share, "share/" + _symbol);
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var token = localSettings.Values["token"];
            request.Execute((string) token, null);

            UpdateGraph(null, null, null);
        }

        private void UpdateGraph(string start, string end, string periodicity)
        {
            APIRequest request = null;
            if (start == null && end == null)
                request = new APIRequest(APIRequest.GET, this, APIRequest.requestCodeType.ShareEvolution, "share/evolution/" + Symbol);
            
            request.Execute(null, null);
        }

        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        private double _value;

        public double Value
        {
            get { return _value; }
            set
            {
                if (_value == value) return;
                _value = value;
                OnPropertyChanged();
            }
        }

        private double? _limitUp;

        public double? LimitUp
        {
            get{ return _limitUp; }
            set
            {
                if (_limitUp == value) return;
                _limitUp = value;
                OnPropertyChanged();
            }
        }

        private double? _limitDown;

        public double? LimitDown
        {
            get
            { return _limitDown; }
            set
            {
                if (_limitDown == value) return;
                _limitDown = value;
                OnPropertyChanged();
            }
        }

        private bool _favorite;

        public bool Favorite
        {
            get { return _favorite; }
            set
            {
                if (_favorite == value) return;
                _favorite = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Tuple<DateTime, double>> ValuesEvolution
        {
            get { return _valuesEvolution; }
            set
            {
                if (_valuesEvolution == value) return;
                _valuesEvolution = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Tuple<DateTime, double>> Values2Evolution
        {
            get { return _values2Evolution; }
            set
            {
                if (_values2Evolution == value) return;
                _values2Evolution = value;
                OnPropertyChanged();
            }
        }

        private string _time;

        public string Time
        {
            get { return _time; }
            set
            {
                if (_time == value) return;
                _time = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public QuotationDetailsViewModel()
        {
        }
       

        public void onTaskCompleted(string result, APIRequest.requestCodeType requestCode)
        {
            if (result != null)
            {
                if (requestCode == APIRequest.requestCodeType.Share)
                {
                    JsonObject json = JsonObject.Parse(result);
                    Name = json.GetNamedString("name");
                    Value = json.GetNamedNumber("value");
                    Time = json.GetNamedString("time");
                    if (json.GetNamedValue("limit_down").ValueType != JsonValueType.Null)
                        LimitDown = json.GetNamedNumber("limit_down");
                    else
                        LimitDown = null;
                    if (json.GetNamedValue("limit_up").ValueType != JsonValueType.Null)
                        LimitUp = json.GetNamedNumber("limit_up");
                    else
                        LimitUp = null;
                    Favorite = Convert.ToBoolean(json.GetNamedNumber("is_main"));

                }
                else if (requestCode == APIRequest.requestCodeType.ShareEvolution)
                {
                    ValuesEvolution.Clear();
                    Values2Evolution.Clear();

                    JsonArray json = JsonArray.Parse(result);
                    //var i = 0;
                    foreach (var point in json)
                    {
                        //i++;
                        JsonObject jsonPoint = JsonObject.Parse(point.Stringify());
                        string date = jsonPoint.GetNamedString("date");
                        date = date.Substring(5, date.Length - 5);
                        double high = jsonPoint.GetNamedNumber("high");
                        double low = jsonPoint.GetNamedNumber("low");

                        var split = date.Split('-');
                        DateTime date2 = new DateTime(2015, Int32.Parse(split[0]), Int32.Parse(split[1]));
                        ValuesEvolution.Add(new Tuple<DateTime, double>(date2, low));
                        Values2Evolution.Add(new Tuple<DateTime, double>(date2, high));
                    }

                }
                else if (requestCode == APIRequest.requestCodeType.Favorite)
                {
                    JsonObject json = new JsonObject();
                    JsonObject.TryParse(result, out json);
                    if (!json.ContainsKey("error"))
                    {
                        Favorite = true;
                    }
                }
                else if (requestCode == APIRequest.requestCodeType.Unfavorite)
                {
                    JsonObject json = new JsonObject();
                    JsonObject.TryParse(result, out json);
                    if (!json.ContainsKey("error"))
                    {
                        Favorite = false;
                    }
                }
                else if (requestCode == APIRequest.requestCodeType.ClearLimitUp)
                {
                    JsonObject json = new JsonObject();
                    JsonObject.TryParse(result, out json);
                    if (!json.ContainsKey("error"))
                    {
                        LimitUp = null;
                    }
                }
                else if (requestCode == APIRequest.requestCodeType.ClearLimitDown)
                {
                    JsonObject json = new JsonObject();
                    JsonObject.TryParse(result, out json);
                    if (!json.ContainsKey("error"))
                    {
                        LimitDown = null;
                    }
                }else if (requestCode == APIRequest.requestCodeType.PortfolioRemove)
                {
                    JsonObject json = new JsonObject();
                    JsonObject.TryParse(result, out json);
                    if (!json.ContainsKey("error"))
                    {
                        if (NavigatePortfolio != null)
                            NavigatePortfolio();
                    }
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

        public void MainClick(object sender, RoutedEventArgs routedEventArgs)
        {
            if (Favorite == false)
            {
                APIRequest request = new APIRequest(APIRequest.POST, this, APIRequest.requestCodeType.Favorite, "portfolio/favorite");
                Dictionary<string, string> dict = new Dictionary<string, string>()
                {
                    {"symbol", Symbol}
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
            else
            {
                APIRequest request = new APIRequest(APIRequest.POST, this, APIRequest.requestCodeType.Unfavorite, "portfolio/unfavorite");

                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                var token = localSettings.Values["token"];

                request.Execute((string)token, null);
            }

        }

        public void ClearLimit(object sender, RoutedEventArgs routedEventArgs)
        {
            APIRequest request = null;

            if((string)((Button)sender).Tag == "ClearUp") 
                request = new APIRequest(APIRequest.POST, this, APIRequest.requestCodeType.ClearLimitUp, "portfolio/setlimitup");
            else if((string)((Button)sender).Tag == "ClearDown")
                request = new APIRequest(APIRequest.POST, this, APIRequest.requestCodeType.ClearLimitDown, "portfolio/setlimitdown");

            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var token = localSettings.Values["token"];


            Dictionary<string, string> dict = new Dictionary<string, string>()
            {
                {"symbol", Symbol},
                {"limit", null}
            };
            var serializer = new DataContractJsonSerializer(dict.GetType(), new DataContractJsonSerializerSettings()
            {
                UseSimpleDictionaryFormat = true
            });
            MemoryStream stream = new MemoryStream();
            serializer.WriteObject(stream, dict);
            byte[] bytes = stream.ToArray();
            string content = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

            request.Execute((string)token, content);
        }

        public void DeleteShare()
        {
            APIRequest request = request = new APIRequest(APIRequest.POST, this, APIRequest.requestCodeType.PortfolioRemove, "portfolio/remove");

            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var token = localSettings.Values["token"];
            
            Dictionary<string, string> dict = new Dictionary<string, string>()
            {
                {"symbol", Symbol}
            };
            var serializer = new DataContractJsonSerializer(dict.GetType(), new DataContractJsonSerializerSettings()
            {
                UseSimpleDictionaryFormat = true
            });
            MemoryStream stream = new MemoryStream();
            serializer.WriteObject(stream, dict);
            byte[] bytes = stream.ToArray();
            string content = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

            request.Execute((string)token, content);
        }
    }
}
