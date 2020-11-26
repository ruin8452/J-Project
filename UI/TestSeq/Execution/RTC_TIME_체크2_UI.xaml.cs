using System.Windows.Controls;

namespace J_Project.UI.TestSeq.Execution
{
    /// <summary>
    /// 효율.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RTC_TIME_체크2_UI : Page, ITestExeList
    {
        public RTC_TIME_체크2_UI()
        {
            InitializeComponent();

            DataContext = new ViewModel.TestItem.RtcCheck2VM();
        }
    }
}