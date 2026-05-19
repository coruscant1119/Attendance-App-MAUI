using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Project;

public class PercentToWidthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        double percent = (double)value;
        return percent / 100.0 * 220;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
