using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace CoffeeMachine.UI.Converters;

public class StatusToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string status)
        {
            if (status.Contains("Error", StringComparison.OrdinalIgnoreCase) ||
                status.Contains("Failed", StringComparison.OrdinalIgnoreCase))
                return Application.Current.Resources["ErrorRed"];

            if (status.Contains("Success", StringComparison.OrdinalIgnoreCase) ||
                status.Contains("Complete", StringComparison.OrdinalIgnoreCase) ||
                status.Contains("Ready", StringComparison.OrdinalIgnoreCase))
                return Application.Current.Resources["SuccessGreen"];

            if (status.Contains("Preparing", StringComparison.OrdinalIgnoreCase) ||
                status.Contains("Processing", StringComparison.OrdinalIgnoreCase))
                return Application.Current.Resources["CoffeeBrown"];
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is bool boolValue && boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value is Visibility visibility && visibility == Visibility.Visible;
    }
}
