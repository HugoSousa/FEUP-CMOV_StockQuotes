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
using System.Threading.Tasks;
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
            string name = SelectedQuotation.Name;
            Frame.Navigate(typeof (QuotationDetails));
            //var abc = SelectedBook.ID;
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

    }

    public class MainPageViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Quotation> _items;
        private ObservableCollection<Quotation> _allItems;

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

        public MainPageViewModel()
        {
            Items = new ObservableCollection<Quotation>();
            Items.Add(new Quotation() { Name = "GOOG", Value = 1.323 });
            Items.Add(new Quotation() { Name = "APPL", Value = 1.101 });
            Items.Add(new Quotation() { Name = "IBM", Value = 0.922 });

            AllItems = new ObservableCollection<Quotation>();
            AllItems.Add(new Quotation() { Name = "IAM", Value = 1.323 });
            AllItems.Add(new Quotation() { Name = "GOOG", Value = 1.323 });
            AllItems.Add(new Quotation() { Name = "APPL", Value = 1.101 });
            AllItems.Add(new Quotation() { Name = "IBM", Value = 0.922 });
            AllItems.Add(new Quotation() { Name = "GAGL", Value = 1.323 });
            
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

            result = AllItems.Where(x => x.Name.Contains(text)).ToArray();

            return result;
        }

        internal void AddToPortfolio_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                // User selected an item from the suggestion list, take an action on it here.
                //TODO add only if it doesn't exist in the items yet. add in the db also
                _items.Add(AllItems.First(x => x.Name.Equals(sender.Text)));

            }
            else
            {
                // Use args.QueryText to determine what to do.
                //TODO verify if the share in args.QueryText exists. If so, get from DB and add to Portfolio
                Items.Add(AllItems[0]);
            }
        }

        internal void AddToPortfolio_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            sender.Text = ((Quotation)args.SelectedItem).Name;
        }
    }
}
