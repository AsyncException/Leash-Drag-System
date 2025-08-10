using Microsoft.UI.Xaml.Data;
using System;

namespace LDS.Converters;

public partial class PercentageTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) => $"{Math.Round((float)value * 100, 2)} %";

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException("Only one way is supported");
}
