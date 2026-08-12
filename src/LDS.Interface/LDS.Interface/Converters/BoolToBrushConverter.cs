using Microsoft.UI;
using Microsoft.UI.Xaml.Data;

namespace LDS.Interface.Converters;

public class BoolToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not bool b)
        {
            throw new ArgumentException("Value is not a boolean",  nameof(value));
        }

        var color = b ? Colors.Green : Colors.Red;
        return new SolidColorBrush(color);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is not SolidColorBrush brush)
        {
            throw new ArgumentException("Value is not a SolidColorBrush", nameof(value));
        }

        var b = brush.Color == Colors.Green;
        return b;
    }
}
