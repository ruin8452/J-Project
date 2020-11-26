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
    [ImplementPropertyChanged]
    public class BasicInfoPageVM
    {
        public TestOption Option { get; set; }

        public string InfoSaveText { get; set; }
        public SolidColorBrush InfoSaveColor { get; set; }

        public BasicInfo Info { get; set; }

        public Rectifier Rect { get; set; }

        public string SettingStateText { get; set; }

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