using GalaSoft.MvvmLight.Command;
using System.Windows;

namespace J_Project.ViewModel.SubWindow
{
    class ReportProcessViewModel
    {
        public string WriteType { get; set; }

        public RelayCommand<object> HowWriteCommand { get; set; }

        public ReportProcessViewModel()
        {
            HowWriteCommand = new RelayCommand<object>(WriteTypeSelect);
        }

        /**
         *  @brief 파일 쓰기 방식 선택
         *  @details 보고서 파일을 작성 시 기존 데이터가 남아있을 때, 덮어쓰기/이어쓰기/새로쓰기 등의 방식을 선택
         *  
         *  @param object obj - 해당 윈도우, 사용자가 선택한 쓰기 방식
         *  
         *  @return
         */
        public void WriteTypeSelect(object obj)
        {
            object[] itme = (object[])obj;

            Window window = (Window)itme[0];
            WriteType = (string)itme[1];

            window.Close();
        }
    }
}
