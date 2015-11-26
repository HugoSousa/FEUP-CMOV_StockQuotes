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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace StockExchangeQuotes
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            List<Quotation> items = new List<Quotation>();
            items.Add(new Quotation() { Name = "GOOG", Value = 1.323 });
            items.Add(new Quotation() { Name = "APPL", Value = 1.101 });
            items.Add(new Quotation() { Name = "IBM", Value = 0.922 });
            lvDataBinding.ItemsSource = items;

        }


        void TestMethod(object sender, SelectionChangedEventArgs e)
        {
            Quotation SelectedBook = (Quotation)lvDataBinding.SelectedItem;
            string name = SelectedBook.Name;
            Frame.Navigate(typeof (QuotationDetails));
            //var abc = SelectedBook.ID;
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }

    public class Quotation
    {
        public string Name { get; set; }

        public double Value { get; set; }
    }

}
