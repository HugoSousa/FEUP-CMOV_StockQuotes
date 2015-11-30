using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace StockExchangeQuotes
{
    public class InvertedBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool booleanValue = (bool)value;
            booleanValue = !booleanValue;
            if (booleanValue)
                return Visibility.Collapsed;
            else
                return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            bool booleanValue = (bool)value;
            booleanValue = !booleanValue;
            if (booleanValue)
                return Visibility.Collapsed;
            else
                return Visibility.Visible;
        }
    }

    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool booleanValue = (bool)value;
            booleanValue = !booleanValue;
            if (booleanValue)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            bool booleanValue = (bool)value;
            booleanValue = !booleanValue;
            if (booleanValue)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }
    }
}