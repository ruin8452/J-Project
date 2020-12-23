using J_Project.Equipment;
using J_Project.Manager;
using J_Project.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;

namespace J_Project.UI.MainWindow.Testing
{
    /// <summary>
    /// MainTestingPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainTestingPage : Page
    {
        public MainTestingPage()
        {
            InitializeComponent();

            DataContext = new TestingPageVM("FirstTest");
        }

        private void ClickTreeCheckBox(object sender, RoutedEventArgs e)
        {
            TestItemUnit checkItem = ((CheckBox)sender).DataContext as TestItemUnit;

            if(checkItem.Parents == null) //부모 아이템
            {
                // 부모 체크박스 상태에 따른 자식 체크박스의 상태 결정
                if (checkItem.Child.Count > 0)
                {
                    foreach (var tempItem in checkItem.Child)
                    {
                        tempItem.Checked = checkItem.Checked;
                    }
                }
            }
            else //자식 아이템
            {
                bool? checkState = checkItem.Parents.Child[0].Checked;
                foreach (var tempItem in checkItem.Parents.Child)
                {
                    if (checkState != tempItem.Checked)
                    {
                        checkState = null;
                        break;
                    }
                }

                checkItem.Parents.Checked = checkState;
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Rectifier.GetObj().RectCommand(CommandList.CAL_RESET, 1);
        }

        private void ListBorder_LayoutUpdated(object sender, EventArgs e)
        {
            double height = ListBorder.ActualHeight - 25 - 29;

            if (height > 0)
                TestTreeView.Height = height;
            else
                TestTreeView.Height = 0;
        }

        private void LogBorder_LayoutUpdated(object sender, EventArgs e)
        {
            double height = LogBorder.ActualHeight - 29;

            if (height > 0)
                LogViewer.Height = height;
            else
                LogViewer.Height = 0;
        }

        private void SequenceViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            SequenceViewer.ScrollToHorizontalOffset(SequenceViewer.HorizontalOffset - e.Delta);
        }
    }
}
