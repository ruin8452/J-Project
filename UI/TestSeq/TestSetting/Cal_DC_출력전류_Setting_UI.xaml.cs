using System.Windows.Controls;

namespace J_Project.UI.TestSeq.TestSetting
{
    /// <summary>
    /// 역률.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Cal_DC_출력전류_Setting_UI : Page, ITestSettingList
    {
        public Cal_DC_출력전류_Setting_UI()
        {
            InitializeComponent();

            DataContext = new ViewModel.TestItem.CalDcCurrVM();
        }
    }
}