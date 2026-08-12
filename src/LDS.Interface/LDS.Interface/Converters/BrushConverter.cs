using Windows.UI;
using Microsoft.UI.Xaml.Data;

namespace LDS.Interface.Converters;

public class BrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not Color color)
        {
            throw new ArgumentException($"{nameof(value)} is not a {nameof(Color)}", nameof(value));
        }

        return new SolidColorBrush(color);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is not SolidColorBrush brush)
        {
            throw new ArgumentException($"{nameof(value)} is not a {nameof(SolidColorBrush)}", nameof(value));
        }

        return brush.Color;
    }
}
