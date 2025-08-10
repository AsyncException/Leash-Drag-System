using Microsoft.UI.Xaml.Data;
using System;

namespace LDS.Converters;

public partial class ToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) => $"{value}";
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException("Only one way is supported");
}
