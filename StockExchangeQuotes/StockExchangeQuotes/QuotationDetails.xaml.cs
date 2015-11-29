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
        public QuotationDetails()
        {
            this.InitializeComponent();

            var pageModel = new QuotationDetailsViewModel();
            DataContext = pageModel;
        }
    }

    public class QuotationDetailsViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Tuple<string, int>> _valuesEvolution;
        private ObservableCollection<Tuple<string, int>> _values2Evolution;

        public ObservableCollection<Tuple<string, int>> ValuesEvolution
        {
            get { return _valuesEvolution; }
            set
            {
                if (_valuesEvolution == value) return;
                _valuesEvolution = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Tuple<string, int>> Values2Evolution
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

        }
    }
}
