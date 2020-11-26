using System.Windows.Controls;

namespace J_Project.UI.TestSeq.TestSetting
{
    /// <summary>
    /// 효율.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RTC_TIME_체크_Setting_UI : Page, ITestSettingList
    {
        public RTC_TIME_체크_Setting_UI()
        {
            InitializeComponent();

            DataContext = new ViewModel.TestItem.RtcCheckVM();
        }
    }
}