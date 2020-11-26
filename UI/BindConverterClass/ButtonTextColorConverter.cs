using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace J_Project.UI.BindConverterClass
{
    class ButtonTextColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length != 2)
                return null;

            ObservableCollection<SolidColorBrush> color = values[0] as ObservableCollection<SolidColorBrush>;
            short? index = values[1] as short?;

            if (!index.HasValue || color == null)
                return null;

            return color[index.Value];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
