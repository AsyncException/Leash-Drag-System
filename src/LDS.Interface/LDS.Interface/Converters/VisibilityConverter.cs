using Microsoft.UI.Xaml.Data;

namespace LDS.Interface.Converters;

public class VisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not bool isVisible)
        {
            throw new ArgumentException($"{nameof(value)} is not a bool", nameof(value));
        }
        
        return isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is not Visibility visibility)
        {
            throw new ArgumentException($"{nameof(value)} is not a visibility", nameof(value));
        }

        return visibility == Visibility.Visible;
    }
}

public class InvertedVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not bool isVisible)
        {
            throw new ArgumentException($"{nameof(value)} is not a bool", nameof(value));
        }
        
        return isVisible ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is not Visibility visibility)
        {
            throw new ArgumentException($"{nameof(value)} is not a visibility", nameof(value));
        }

        return visibility == Visibility.Collapsed;
    }
}
