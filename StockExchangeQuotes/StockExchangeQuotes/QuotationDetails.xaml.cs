using System;
using System.Collections.Generic;
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
using Windows.UI.Core;
using WinRTXamlToolkit;
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

            this.Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            List<Tuple<string, int>> myList = new List<Tuple<string, int>>()
            {
                new Tuple<string, int>("Item 1", 20),
                new Tuple<string, int>("Item 2", 30),
                new Tuple<string, int>("Item 3", 50)
            };

            (MyChart.Series[0] as PieSeries).ItemsSource = myList;
        }
    }
}
