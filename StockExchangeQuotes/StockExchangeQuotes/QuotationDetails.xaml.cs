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
            DataContext = pageModel;
            
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
    }

    public class QuotationDetailsViewModel : INotifyPropertyChanged, OnApiRequestCompleted
    {
        private ObservableCollection<Tuple<DateTime, double>> _valuesEvolution = new ObservableCollection<Tuple<DateTime, double>>();
        private ObservableCollection<Tuple<DateTime, double>> _values2Evolution = new ObservableCollection<Tuple<DateTime, double>>();

        private string _symbol;
        public string Symbol
        {
            get { return _symbol; }
            set
            {
                _symbol = value;
                APIRequest request = new APIRequest(APIRequest.GET, this, APIRequest.requestCodeType.Share, "share/" + _symbol);
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                var token = localSettings.Values["token"];
                request.Execute((string)token, null);

                UpdateGraph(null, null, null);
                OnPropertyChanged();
            }
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
            get
            {
                if (_limitUp.HasValue)
                    return _limitUp;
                else
                    return -999;
            }
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
            {
                if (_limitDown.HasValue)
                    return _limitDown;
                else
                    return -999;
            }
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
            if (requestCode == APIRequest.requestCodeType.Share)
            {
                if (result != null)
                {
                    JsonObject json = JsonObject.Parse(result);
                    Name = json.GetNamedString("name");
                    Value = json.GetNamedNumber("value");
                    if (json.GetNamedValue("limit_down").ValueType != JsonValueType.Null)
                        LimitDown = json.GetNamedNumber("limit_down");
                    else
                        LimitDown = null;
                    if (json.GetNamedValue("limit_up").ValueType != JsonValueType.Null)
                        LimitUp = json.GetNamedNumber("limit_up");
                    else
                        LimitUp = null;

                    //TODO GET IF ITS FAVORITE SHARE. SET FAVORITE
                    Favorite = true;

                }
            }
            else if(requestCode == APIRequest.requestCodeType.ShareEvolution)
            {
                if (result != null)
                {
                    ValuesEvolution.Clear();
                    Values2Evolution.Clear();

                    JsonArray json = JsonArray.Parse(result);
                    var i = 0;
                    foreach (var point in json)
                    {
                        i++;
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
            }else if (requestCode == APIRequest.requestCodeType.Favorite)
            {
                if (result != null)
                {
                    JsonObject json = JsonObject.Parse(result);
                    if (!json.ContainsKey("error"))
                    {
                        Favorite = true;
                    }
                }
            }
        }

        public void MainClick(object sender, RoutedEventArgs routedEventArgs)
        {
            
            APIRequest request = new APIRequest(APIRequest.POST, this, APIRequest.requestCodeType.Share, "portfolio/favorite");
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

            request.Execute((string)token, content);
            
        }
    }
    
    public class InvertBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool booleanValue = (bool)value;
            return !booleanValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            bool booleanValue = (bool)value;
            return !booleanValue;
        }
    }
}
