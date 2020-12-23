using System.Windows.Controls;

namespace J_Project.UI.TestSeq.TestSetting
{
    /// <summary>
    /// 효율.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class 효율_Setting_UI : Page, ITestSettingList
    {
        public 효율_Setting_UI(int caseNum)
        {
            InitializeComponent();

            DataContext = new ViewModel.TestItem.EfficiencyVM(caseNum);
        }
    }
}