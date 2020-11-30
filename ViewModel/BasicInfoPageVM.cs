using J_Project.Data;
using J_Project.Equipment;
using J_Project.Manager;
using J_Project.ViewModel.CommandClass;
using Microsoft.WindowsAPICodePack.Dialogs;
using PropertyChanged;
using System;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;

namespace J_Project.ViewModel
{
    /**
     *  @brief 기본 정보 UI VM 클래스
     *  @details 기본 정보 UI에서 사용하는 변수 및 메소드를 포함하고 있는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    [ImplementPropertyChanged]
    public class BasicInfoPageVM
    {
        public TestOption Option { get; set; }

        public string InfoSaveText { get; set; }
        public SolidColorBrush InfoSaveColor { get; set; }

        public BasicInfo Info { get; set; }

        public Rectifier Rect { get; set; }

        public ICommand FirstReportOpenClickCommand { get; set; }
        public ICommand SecondReportOpenClickCommand { get; set; }
        public ICommand ReportSaveClickCommand { get; set; }

        public ICommand BasicInfoSaveCommand { get; set; }

        public BasicInfoPageVM()
        {
            InfoSaveColor = Brushes.White;

            Info = BasicInfo.GetObj();

            Rect = Rectifier.GetObj();

            BasicInfoSaveCommand = new BaseCommand(SaveBasicInfo);

            FirstReportOpenClickCommand = new BaseCommand(FirstReportOpenDialog);
            SecondReportOpenClickCommand = new BaseCommand(SecondReportOpenDialog);
            ReportSaveClickCommand = new BaseCommand(ReportSaveDialog);
        }

        /**
         *  @brief 기본 정보 저장
         *  @details 기본 정보를 저장할 때 필수 항목을 적었는지 검사한다
         *  
         *  @param
         *  
         *  @return
         */
        private void SaveBasicInfo()
        {
            if (string.IsNullOrEmpty(Info.Checker) || string.IsNullOrEmpty(Info.ModelName) || string.IsNullOrEmpty(Info.SerialNumber) ||
                string.IsNullOrEmpty(Info.FirstReportOpenPath) || string.IsNullOrEmpty(Info.SecondReportOpenPath) ||
                string.IsNullOrEmpty(Info.ReportSavePath))
            {
                InfoSaveColor = System.Windows.Application.Current.Resources["LedRed"] as SolidColorBrush;
                InfoSaveText = "필수 항목이 비어있습니다.";
            }
            else
            {
                InfoSaveColor = Brushes.White;
                InfoSaveText = "저장을 완료했습니다";
                BasicInfo.Save();
            }
        }

        /**
         *  @brief 양산 테스트 보고서 양식 경로 설정 다이얼로그 열기
         *  @details 양산 테스트 보고서 양식의 위치를 저장하기 위한 다이얼로그를 연다
         *  
         *  @param
         *  
         *  @return
         */
        private void FirstReportOpenDialog()
        {
            using OpenFileDialog openReport = new OpenFileDialog
            {
                Title = "양산 성적서 양식 파일 선택",
                Filter = "Excel파일(*.xlsx)|*.xlsx|Excel파일 (*.xls)|*.xls|csv (*.csv)|*.csv|All files (*.*)|*.*"
            };

            if (Info.FirstReportOpenPath.Length > 0)
            {
                string folderPath = Info.FirstReportOpenPath.Substring(0, Info.FirstReportOpenPath.LastIndexOf('\\'));
                openReport.InitialDirectory = folderPath;
            }
            else
                openReport.InitialDirectory = Environment.CurrentDirectory;

            if (openReport.ShowDialog() == DialogResult.OK)   // 다이얼 로그에서 OK버튼을 눌렀을 경우
                Info.FirstReportOpenPath = openReport.FileName;
        }

        /**
         *  @brief 출하 테스트 보고서 양식 경로 설정 다이얼로그 열기
         *  @details 출하 테스트 보고서 양식의 위치를 저장하기 위한 다이얼로그를 연다
         *  
         *  @param
         *  
         *  @return
         */
        private void SecondReportOpenDialog()
        {
            using OpenFileDialog openReport = new OpenFileDialog
            {
                Title = "출하 성적서 양식 파일 선택",
                Filter = "Excel파일(*.xlsx)|*.xlsx|Excel파일 (*.xls)|*.xls|csv (*.csv)|*.csv|All files (*.*)|*.*"
            };

            if (Info.SecondReportOpenPath.Length > 0)
            {
                string folderPath = Info.SecondReportOpenPath.Substring(0, Info.SecondReportOpenPath.LastIndexOf('\\'));
                openReport.InitialDirectory = folderPath;
            }
            else
                openReport.InitialDirectory = Environment.CurrentDirectory;

            if (openReport.ShowDialog() == DialogResult.OK)   // 다이얼 로그에서 OK버튼을 눌렀을 경우
                Info.SecondReportOpenPath = openReport.FileName;
        }

        // 성적서 저장 폴더 선택 버튼 클릭
        /**
         *  @brief 보고서 저장 경로 설정 다이얼로그 열기
         *  @details 테스트를 마치고 생성된 보고서를 저장할 루트 폴더를 설정할 다이얼로그를 연다
         *  
         *  @param
         *  
         *  @return
         */
        private void ReportSaveDialog()
        {
            using CommonOpenFileDialog saveRepoPath = new CommonOpenFileDialog
            {
                Title = "성적서 저장 경로 선택",
                IsFolderPicker = true // 폴더 선택 가능 여부 설정
            };

            if (Info.ReportSavePath.Length > 0)
                saveRepoPath.InitialDirectory = Info.ReportSavePath;
            else
                saveRepoPath.InitialDirectory = Environment.CurrentDirectory;

            if (saveRepoPath.ShowDialog() == CommonFileDialogResult.Ok)
                Info.ReportSavePath = saveRepoPath.FileName;
        }
    }
}