using Microsoft.UI.Xaml.Data;

namespace LDS.Interface.Converters;

public class PercentageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (double)Math.Round((float)value * 100, 2);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return (float)Math.Round((double)value * 0.01, 2);
    }
}
