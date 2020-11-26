using System.Windows.Controls;

namespace J_Project.UI.TestSeq.Execution
{
    /// <summary>
    /// 출력_과부하_보호_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class 출력_과부하_보호1_UI : Page, ITestExeList
    {
        public 출력_과부하_보호1_UI()
        {
            InitializeComponent();

            DataContext = new ViewModel.TestItem.OutputOverVM();
        }
    }
}