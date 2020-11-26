using System;
using System.Globalization;
using System.Windows.Data;

namespace J_Project.UI.BindConverterClass
{
    public class ButtonActiveConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null) return false;

            if(parameter.ToString() == "시작")
            {
                bool passiveFlag = (bool)values[0];
                bool runFlag = (bool)values[1];

                if (runFlag == true)
                    return !runFlag;
                else
                    return !passiveFlag;
            }
            else if(parameter.ToString() == "일시정지")
            {
                bool runFlag = (bool)values[0];

                if (runFlag)
                    return true;
                else
                    return false;
            }
            else
            {
                bool runFlag = (bool)values[0];
                bool pauseFlag = (bool)values[1];

                if (runFlag || pauseFlag)  // 시작 또는 일시정지 상태일 때 정지 버튼 활성화
                    return true;
                else                    // 그 외의 상태는 시작상태의 반대
                    return false;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
