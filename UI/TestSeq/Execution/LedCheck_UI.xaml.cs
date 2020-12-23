﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace J_Project.UI.TestSeq.Execution
{
    /// <summary>
    /// DcOnCheck_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LedCheck_UI : Page
    {
        public LedCheck_UI(int caseNum)
        {
            InitializeComponent();

            DataContext = new ViewModel.TestItem.LedCheckVM(caseNum);
        }
    }
}
