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

namespace J_Project.UI.MainWindow.Setting
{
    /// <summary>
    /// TestSettingPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TestSettingPage : Page
    {
        public TestSettingPage()
        {
            InitializeComponent();

            DataContext = new ViewModel.TestSettingPageVM();
        }

        private void SeqViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            SeqViewer.ScrollToHorizontalOffset(SeqViewer.HorizontalOffset - e.Delta);
        }
    }
}
