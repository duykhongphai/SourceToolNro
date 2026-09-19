using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace CreateSkillNro.Options;

public class SubtractConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double doubleValue && parameter != null)
        {
            if (double.TryParse(parameter.ToString(), out double subtractValue))
            {
                return Math.Max(0, doubleValue - subtractValue);
            }
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}