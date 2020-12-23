using System.Windows.Controls;

namespace J_Project.UI.TestSeq.TestSetting
{
    /// <summary>
    /// AC_저전압_알람.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AC_저전압_알람_Setting_UI : Page, ITestSettingList
    {
        public AC_저전압_알람_Setting_UI(int caseNum)
        {
            InitializeComponent();

            DataContext = new ViewModel.TestItem.AcLowVM(caseNum);
        }
    }
}