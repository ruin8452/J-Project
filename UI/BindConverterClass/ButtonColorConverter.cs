using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace J_Project.UI.BindConverterClass
{
    class ButtonColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool passiveFlag = (bool)values[0];

            if (passiveFlag)
                return new SolidColorBrush(Color.FromRgb(0x80, 0x80, 0x80));
            else
            {
                if (passiveFlag)
                    return new SolidColorBrush(Color.FromRgb(0x80, 0x80, 0x80));
                else
                    return (SolidColorBrush)values[1];
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
