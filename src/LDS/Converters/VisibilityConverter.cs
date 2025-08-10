using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace LDS.Converters;

public partial class VisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) => ((bool)value) ? Visibility.Visible : Visibility.Collapsed;
    public object ConvertBack(object value, Type targetType, object parameter, string language) => ((Visibility)value) == Visibility.Visible;
}