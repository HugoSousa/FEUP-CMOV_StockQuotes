using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization.Json;
using System.Text;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using StockExchangeQuotes.Annotations;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace StockExchangeQuotes
{
    public sealed partial class SetLimitDialog : Page
    {
        private SetLimitDialogViewModel pageModel;
        public SetLimitDialog()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var quotation = e.Parameter as Quotation;
            pageModel = new SetLimitDialogViewModel() {Symbol = quotation.Symbol, Name = quotation.Name, Value = quotation.Value, LimitType = quotation.LimitType};
            pageModel.NavigateBack += NavigateQuotationDetails;
            DataContext = pageModel;
            
        }

        private void NavigateQuotationDetails()
        {
            Frame.GoBack();
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void OkClick(object sender, RoutedEventArgs e)
        {
            pageModel.OkClick(sender, e);
        }
    }

    public class SetLimitDialogViewModel : INotifyPropertyChanged, OnApiRequestCompleted
    {
        public delegate void NavigateBackAction();

        public event NavigateBackAction NavigateBack;
        
        private const string LimitUpString = "Upper Limit";
        private const string LimitDownString = "Lower Limit";

        private string _symbol;
        public string Symbol
        {
            get { return _symbol; }
            set
            {
                if (_symbol == value) return;
                _symbol = value;
                OnPropertyChanged();
            }
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

        private string _limitType;

        public string LimitType
        {
            get { return _limitType; }
            set
            {
                if (_limitType == value) return;
                _limitType = value;
                OnPropertyChanged();
            }
        }

        private double _limitValue;

        public double LimitValue
        {
            get { return _limitValue; }
            set
            {
                if (_limitValue == value) return;
                _limitValue = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public SetLimitDialogViewModel()
        {
        }

        public void OkClick(object sender, RoutedEventArgs routedEventArgs)
        {
            string path = null;
            if (LimitType == LimitUpString)
            {
                path = "portfolio/setlimitup";
            }else if (LimitType == LimitDownString)
            {
                path = "portfolio/setlimitdown";
            }
            APIRequest request = new APIRequest(APIRequest.POST, this, APIRequest.requestCodeType.SetLimit, path);

            Dictionary<string, string> dict = new Dictionary<string, string>()
                {
                    {"symbol", Symbol},
                    {"limit", LimitValue.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture)}
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

        public void onTaskCompleted(string result, APIRequest.requestCodeType requestCode)
        {
            if (requestCode == APIRequest.requestCodeType.SetLimit)
            {
                if (result != null)
                {
                    JsonObject json = new JsonObject();
                    JsonObject.TryParse(result, out json);
                    if (!json.ContainsKey("error"))
                    {
                        if (NavigateBack != null)
                            NavigateBack();
                    }
                }
            }
        }
    }
}
