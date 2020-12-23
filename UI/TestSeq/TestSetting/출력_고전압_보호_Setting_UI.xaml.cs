using System.Windows.Controls;

namespace J_Project.UI.TestSeq.TestSetting
{
    /// <summary>
    /// 출력_고전압_보호.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class 출력_고전압_보호_Setting_UI : Page, ITestSettingList
    {
        public 출력_고전압_보호_Setting_UI(int caseNum)
        {
            InitializeComponent();

            DataContext = new ViewModel.TestItem.OutputHighVM(caseNum);
        }
    }
}