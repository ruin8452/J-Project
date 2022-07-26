﻿using System.Windows.Controls;

namespace J_Project.UI.TestSeq.Execution
{
    /// <summary>
    /// 효율.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class 누설전류_UI : Page, ITestExeList
    {
        public 누설전류_UI(int caseNum)
        {
            InitializeComponent();

            DataContext = new ViewModel.TestItem.LeakageVM(caseNum);
        }
    }
}