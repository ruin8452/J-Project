using System;
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
    /// BasicInfoPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BasicInfoPage : Page
    {
        public BasicInfoPage()
        {
            InitializeComponent();

            DataContext = new ViewModel.BasicInfoPageVM();
        }

        private void TextBoxSelectAll_Tab(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        private void TextBoxSelectAll_Mouse(object sender, MouseEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }
    }
}
