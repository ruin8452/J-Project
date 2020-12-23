using System.Windows.Controls;

namespace J_Project.UI.TestSeq.TestSetting
{
    /// <summary>
    /// 역률.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Cal_AC_입력전압_Setting_UI : Page, ITestSettingList
    {
        public Cal_AC_입력전압_Setting_UI(int caseNum)
        {
            InitializeComponent();

            DataContext = new ViewModel.TestItem.CalAcVM(caseNum);
        }
    }
}