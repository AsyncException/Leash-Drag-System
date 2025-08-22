using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;

namespace LDS.Converters;
public partial class LogLevelToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) {
        string? level = value?.ToString();

        return level switch {
            "INF" => new SolidColorBrush(Microsoft.UI.Colors.DodgerBlue),
            "WRN" => new SolidColorBrush(Microsoft.UI.Colors.Orange),
            "ERR" => new SolidColorBrush(Microsoft.UI.Colors.Red),
            "DBG" => new SolidColorBrush(Microsoft.UI.Colors.MediumPurple),
            _ => new SolidColorBrush(Microsoft.UI.Colors.Gray)
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}