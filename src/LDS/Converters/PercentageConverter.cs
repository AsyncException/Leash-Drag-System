using Microsoft.UI.Xaml.Data;
using System;

namespace LDS.Converters;

public partial class PercentageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) => (double)(Math.Round((float)value * 100, 2));

    public object ConvertBack(object value, Type targetType, object parameter, string language) => (float)Math.Round(((double)value) * 0.01, 2);
}