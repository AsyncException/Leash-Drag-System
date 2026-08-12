using Microsoft.UI.Xaml.Data;

namespace LDS.Interface.Converters;

public class RoundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return $"{Math.Round((float)value, 2)}";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return float.Parse((string)value);
    }
}
