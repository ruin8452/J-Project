using System;
using System.Globalization;
using System.Windows.Data;

namespace J_Project.UI.BindConverterClass
{
    class DisplayIdConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (byte)value + 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
