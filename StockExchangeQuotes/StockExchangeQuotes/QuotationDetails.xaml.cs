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
    }

    public class QuotationDetailsViewModel : INotifyPropertyChanged, OnApiRequestCompleted
    {
        private ObservableCollection<Tuple<string, double>> _valuesEvolution = new ObservableCollection<Tuple<string, double>>();
        private ObservableCollection<Tuple<string, double>> _values2Evolution = new ObservableCollection<Tuple<string, double>>();

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

        private double _limitUp;

        public double LimitUp
        {
            get { return _limitUp; }
            set
            {
                if (_limitUp == value) return;
                _limitUp = value;
                OnPropertyChanged();
            }
        }

        private double _limitDown;

        public double LimitDown
        {
            get { return _limitDown; }
            set
            {
                if (_limitDown == value) return;
                _limitDown = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Tuple<string, double>> ValuesEvolution
        {
            get { return _valuesEvolution; }
            set
            {
                if (_valuesEvolution == value) return;
                _valuesEvolution = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Tuple<string, double>> Values2Evolution
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
            /*
            ValuesEvolution = new ObservableCollection<Tuple<string, int>>()
            {
                new Tuple<string, int>("Item 1", 20),
                new Tuple<string, int>("Item 2", 30),
                new Tuple<string, int>("Item 3", 50),
                new Tuple<string, int>("Item 4", 40),
                new Tuple<string, int>("Item 5", 60),
                new Tuple<string, int>("Item 6", 65)
            };

            Values2Evolution = new ObservableCollection<Tuple<string, int>>()
            {
                new Tuple<string, int>("Item 1", 10),
                new Tuple<string, int>("Item 2", 20),
                new Tuple<string, int>("Item 3", 30),
                new Tuple<string, int>("Item 4", 20),
                new Tuple<string, int>("Item 5", 10),
                new Tuple<string, int>("Item 6", 5)
            };
            */
            

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
                    if (json.GetNamedValue("limit_up").ValueType != JsonValueType.Null)
                        LimitUp = json.GetNamedNumber("limit_up");
                }
            }
            else if(requestCode == APIRequest.requestCodeType.ShareEvolution)
            {
                if (result != null)
                {
                    ValuesEvolution.Clear();
                    Values2Evolution.Clear();

                    JsonArray json = JsonArray.Parse(result);
                    foreach (var point in json)
                    {
                        JsonObject jsonPoint = JsonObject.Parse(point.Stringify());
                        string date = jsonPoint.GetNamedString("date");
                        date = date.Substring(5, date.Length - 5);
                        double high = jsonPoint.GetNamedNumber("high");
                        double low = jsonPoint.GetNamedNumber("low");

                        ValuesEvolution.Add(new Tuple<string, double>(date, low));
                        Values2Evolution.Add(new Tuple<string, double>(date, high));
                    }
                    
                }
            }
        }
    }
}
