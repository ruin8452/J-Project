﻿using System.Windows.Controls;

namespace J_Project.UI.TestSeq.TestSetting
{
    /// <summary>
    /// 온도센서_점검.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SerialSave_Setting_UI : Page, ITestSettingList
    {
        public SerialSave_Setting_UI(int caseNum)
        {
            InitializeComponent();

            DataContext = new ViewModel.TestItem.SerialSaveVM(caseNum);
        }
    }
}