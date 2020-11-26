using J_Project.ViewModel.CommandClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace J_Project.ViewModel.SubWindow
{
    class ReportProcessViewModel
    {
        public string WriteType { get; set; }

        public ICommand HowWriteCommand { get; set; }

        public ReportProcessViewModel()
        {
            HowWriteCommand = new BaseObjCommand(WriteTypeSelect);
        }

        public void WriteTypeSelect(object obj)
        {
            object[] itme = (object[])obj;

            Window window = (Window)itme[0];
            WriteType = (string)itme[1];

            window.Close();
        }
    }
}
