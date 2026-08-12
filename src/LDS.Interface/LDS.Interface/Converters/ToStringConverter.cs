using Microsoft.UI.Xaml.Data;

namespace LDS.Interface.Converters;

public class ToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value?.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException("Only one way is supported");
    }
}
