using Microsoft.UI.Xaml.Data;
using System;

namespace LDS.Converters;

public partial class RoundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) => $"{Math.Round((float)value, 2)}";
    public object ConvertBack(object value, Type targetType, object parameter, string language) => value;
}
