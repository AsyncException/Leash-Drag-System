using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;

namespace LDS.Converters;
public partial class BoolToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) {
        bool isConnected = (bool)value;
        return new SolidColorBrush(isConnected ? Colors.Green : Colors.Red);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) {
        SolidColorBrush brush = (SolidColorBrush)value;
        return brush.Color == Colors.Green;
    }
}