using J_Project.Communication.CommFlags;
using J_Project.Data;
using J_Project.Equipment;
using J_Project.FileSystem;
using J_Project.Manager;
using J_Project.Manager.EventArgsClass;
using J_Project.UI.SubWindow;
using J_Project.UI.TestSeq.Execution;
using J_Project.ViewModel.SubWindow;
using J_Project.ViewModel.TestItem;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GalaSoft.MvvmLight.Command;
using Timer = System.Timers.Timer;

namespace J_Project.ViewModel
{
    /**
     *  @brief 테스트 화면 UI VM 클래스
     *  @details 테스트 화면 UI에서 사용하는 변수 및 메소드를 포함하고 있는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    [ImplementPropertyChanged]
    public class TestingPageVM
    {
        readonly SolidColorBrush Gray = new SolidColorBrush(Color.FromRgb(0x80, 0x80, 0x80));
        readonly SolidColorBrush DodgerBlue = new SolidColorBrush(Color.FromRgb(0x1E, 0x90, 0xFF));

        public string Log { get; set; }

        CsvReport csvReport = CsvReport.GetObj();
        BasicInfo basicInfo = BasicInfo.GetObj();

        TestExecution testExe = new TestExecution();
        string csvSavePath;
        string filePath;    // 테스트 타입에 따른 저장경로
        string TestType;

        Timer TestTimeView = new Timer();   // 일정주기마다 스톱워치의 시간을 UI로 갱신하는 타이머
        Stopwatch TotalTestTimer = new Stopwatch(); // 총 테스트 시간 측정

        public ObservableCollection<TestItemUnit> TreeTestItems { get; set; }
        public Page TestUi { get; set; }
        ScrollViewer Viewer;
        
        public TestOption Option { get; set; }

        public PowerMeter Pm { get; set; }
        public Dmm1 Dmm1 { get; set; }
        public Dmm2 Dmm2 { get; set; }
        public Rectifier Rect { get; set; }

        // 동작 플래그
        public bool TestRunFlag   { get; set; } = false;
        public bool TestPauseFlag { get; set; } = false;
        public bool IsTestEnd     { get; set; } = true;

        // 동작 색상
        public SolidColorBrush StartBtnBrush { get; set; }
        public SolidColorBrush PauseBtnBrush { get; set; }
        public SolidColorBrush StopBtnBrush  { get; set; }

        // 세부 사항 텍스트
        public string PassOrFailStr { get; set; }
        public string DetailStr { get; set; }

        // 테스트 진행 상황 텍스트
        public string TestRunningText { get; set; } = "대기";

        // 테스트 진행 시간
        public string TotalTestTime { get; set; } = "00:00";

        // 스크롤뷰어
        public double ScrollExtendWidth { get; set; }
        public double ScrollViewportWidth { get; set; }
        public double ScrollHorizontalOffset { get; set; }

        public RelayCommand<object> TreeItemSelectedCommand { get; set; }
        public RelayCommand<object> ScrollView { get; set; }

        public RelayCommand AutoTestStartCommand { get; set; }
        public RelayCommand AutoTestPauseCommand { get; set; }
        public RelayCommand AutoTestStopCommand { get; set; }

        public RelayCommand AllCheckCommand { get; set; }
        public RelayCommand AllUncheckCommand { get; set; }

        public RelayCommand ResetCommand { get; set; }
        public RelayCommand CalResetCommand { get; set; }
        public RelayCommand LogRenewalCommand { get; set; }

        public RelayCommand<object> NodeUpCommand { get; set; }
        public RelayCommand<object> NodeDownCommand { get; set; }

        public RelayCommand FirstSendCommand { get; set; }
        public RelayCommand SecondSendCommand { get; set; }
        public RelayCommand CsvConverterCommand { get; set; }

        public TestingPageVM(string testType)
        {
            TestType = testType;

            Pm = PowerMeter.GetObj();
            Dmm1 = Dmm1.GetObj();
            Dmm2 = Dmm2.GetObj();
            Rect = Rectifier.GetObj();

            testExe.BeforeAutoTestStart += BeforeTestStart;
            testExe.AutoTestStart       += TestStart;
            testExe.UnitTestStart       += TestUnitItemStart;
            testExe.UnitTestEnd         += TestUnitItemStop;
            testExe.AutoTestEnd         += TestEndCheck;
            testExe.AfterAutoTestEnd    += AfterTestEnd;

            testExe.AutoTestPause       += TestPause;
            testExe.AutoTestStop        += TestStop;

            Option = TestOption.GetObj();
            TreeTestItems = MakeTree();

            TestTimeView.Interval = 500;    // ms
            TestTimeView.Elapsed += new ElapsedEventHandler((object sender, ElapsedEventArgs e) =>
            {
                TotalTestTime = TotalTestTimer.Elapsed.ToString(@"mm\:ss");
            });

            StartBtnBrush = DodgerBlue;
            PauseBtnBrush = Gray;
            StopBtnBrush  = Gray;

            TreeItemSelectedCommand = new RelayCommand<object>(TreeViewSelected);
            ScrollView = new RelayCommand<object>(AutoScrolling);

            AutoTestStartCommand = new RelayCommand(StartClick);
            AutoTestPauseCommand = new RelayCommand(PauseClick);
            AutoTestStopCommand = new RelayCommand(StopClick);

            AllCheckCommand = new RelayCommand(TreeItemAllCheck);
            AllUncheckCommand = new RelayCommand(TreeItemAllUnCheck);

            ResetCommand = new RelayCommand(RectResetCheck);
            CalResetCommand = new RelayCommand(CalResetCheck);
            LogRenewalCommand = new RelayCommand(LogRenewal);

            NodeUpCommand = new RelayCommand<object>(NodeUp);
            NodeDownCommand = new RelayCommand<object>(NodeDown);

            FirstSendCommand = new RelayCommand(FirstServerSend);
            SecondSendCommand = new RelayCommand(SecondServerSend);
            CsvConverterCommand = new RelayCommand(CsvConverter);
        }

        /**
         *  @brief 테스트 항목 선택 시 UI 변경
         *  @details 테스트 리스트에서 항목을 선택할 경우, 메인 화면의 테스트 스퀀스 뷰를 변경시키는 역할
         *  
         *  @param object selectedItem - 선택한 테스트 항목
         *  
         *  @return
         */
        private void TreeViewSelected(object selectedItem)
        {
            TestItemUnit Item = selectedItem as TestItemUnit;
            try
            {
                TestUi = Item.TestExeUi;
                Log = ((AllTestVM)TestUi.DataContext).TestLog.ToString();
            }
            catch (Exception) { }
        }

        /**
         *  @brief 스크롤 뷰어 가져오기
         *  @details 테스트 스퀀스 뷰의 스코롤 뷰어를 가져온다
         *  
         *  @param object scroll - 가져올 스크롤 뷰어
         *  
         *  @return
         */
        private void AutoScrolling(object scroll)
        {
            Viewer = scroll as ScrollViewer;
        }

        /**
         *  @brief 테스트 항목 전체 선택
         *  @details 테스트 항목의 모든 테스트를 체크 상태로 한다
         *  
         *  @param
         *  
         *  @return
         */
        private void TreeItemAllCheck()
        {
            foreach (var temp in MakeList(TreeTestItems))
                temp.Checked = true;
        }

        /**
         *  @brief 테스트 항목 전체 선택 해제
         *  @details 테스트 항목의 모든 테스트를 체크 해제 상태로 한다
         *  
         *  @param
         *  
         *  @return
         */
        private void TreeItemAllUnCheck()
        {
            foreach (var temp in MakeList(TreeTestItems))
                temp.Checked = false;
        }

        /**
         *  @brief CSV 보고서 변환기
         *  @details 작성된 CSV 보고서를 엑셀 보고서로 변환시킨다
         *  
         *  @param
         *  
         *  @return
         */
        private void CsvConverter()
        {
            // CSV 파일 다듬기 /////////////////////////////////////////////////////////////
            List<string[]> csvList = csvReport.CsvReader(csvSavePath);
            List<string> failList = csvReport.FailTest(csvSavePath);

            basicInfo.SwVersion = Rect.FwVersion.ToString();
            basicInfo.CheckDate = DateTime.Today.ToShortDateString();
            basicInfo.TestResult = failList.Any() ? "NG" : "OK";

            string[] str = new string[] { "0", basicInfo.Checker, basicInfo.ModelName, basicInfo.ProductCode, basicInfo.SerialNumber,
                                          basicInfo.DcdcSerial, basicInfo.PfcSerial, basicInfo.McuSerial, basicInfo.CheckDate, basicInfo.HwVersion, basicInfo.SwVersion,
                                          basicInfo.TestResult, basicInfo.DcdcNumber, basicInfo.PfcNumber, basicInfo.McuNumber };

            csvList[0] = str;
            csvReport.ReportSave(csvSavePath, csvList);
            /////////////////////////////////////////////////////////////

            if (TestType == "FirstTest") csvReport.ReportConverter(csvSavePath, basicInfo.FirstReportOpenPath);
            else                         csvReport.ReportConverter(csvSavePath, basicInfo.SecondReportOpenPath);
        }

        /**
         *  @brief 테스트 목록 순서 UP
         *  @details 테스트 목록 순서를 한단계 올린다
         *  
         *  @param object selectedItem - 선택된 테스트 항목
         *  
         *  @return
         */
        public void NodeUp(object selectedItem)
        {
            if (selectedItem == null) return;

            //TreeTestItems;
            TestItemUnit node = selectedItem as TestItemUnit;

            // 부모노드가 존재할 경우
            if (node.Parents != null)
            {
                int index = node.Parents.Child.IndexOf(node);
                if (index > 0) // 첫번째 노드일 경우 변동 없음
                {
                    node.Parents.Child.RemoveAt(index);
                    node.Parents.Child.Insert(index - 1, node);
                }
            }
            // 루트 노드일 경우
            else
            {
                int index = TreeTestItems.IndexOf(node);
                if (index > 0) // 첫번째 노드일 경우 변동 없음
                {
                    TreeTestItems.RemoveAt(index);
                    TreeTestItems.Insert(index - 1, node);
                }
            }
        }
        /**
         *  @brief 테스트 목록 순서 DOWN
         *  @details 테스트 목록 순서를 한단계 내린다
         *  
         *  @param object selectedItem - 선택된 테스트 항목
         *  
         *  @return
         */
        public void NodeDown(object selectedItem)
        {
            if (selectedItem == null) return;

            //TreeTestItems;
            TestItemUnit node = selectedItem as TestItemUnit;

            // 부모노드가 존재할 경우
            if (node.Parents != null)
            {
                int index = node.Parents.Child.IndexOf(node);
                if (index < node.Parents.Child.Count) // 마지막 노드일 경우 변동 없음
                {
                    node.Parents.Child.RemoveAt(index);
                    node.Parents.Child.Insert(index + 1, node);
                }
            }
            // 루트 노드일 경우
            else
            {
                int index = TreeTestItems.IndexOf(node);
                if (index < TreeTestItems.Count) // 마지막 노드일 경우 변동 없음
                {
                    TreeTestItems.RemoveAt(index);
                    TreeTestItems.Insert(index + 1, node);
                }
            }
        }

        /**
         *  @brief 정류기 리셋
         *  @details 정류기를 리셋시킨다
         *  
         *  @param
         *  
         *  @return
         */
        private void RectResetCheck()
        {
            if(Rectifier.GetObj().RectCommand(CommandList.SW_RESET, 1))
                MessageBox.Show("정류기 리셋 OK");
        }
        /**
         *  @brief 정류기 CAL 리셋
         *  @details 정류기의 CAL 데이터를 리셋시킨다
         *  
         *  @param
         *  
         *  @return
         */
        private void CalResetCheck()
        {
            if(Rectifier.GetObj().RectCommand(CommandList.CAL_RESET, 1))
                MessageBox.Show("정류기 CAL 리셋 OK");
        }
        /**
         *  @brief 로그 갱신
         *  @details 로그창을 갱신시킨다
         *  
         *  @param
         *  
         *  @return
         */
        private void LogRenewal()
        {
            if (TestUi == null) return;

            Log = ((AllTestVM)TestUi.DataContext).TestLog.ToString();
        }

        /**
         *  @brief 자동테스트 시작 버튼 클릭
         *  @details 자동테스트를 시작한다
         *  
         *  @param
         *  
         *  @return
         */
        private void StartClick()
        {
            // 일시중지 상태일 경우
            if (TestPauseFlag == true)
            {
                StartBtnBrush = Gray;
                PauseBtnBrush = DodgerBlue;
                StopBtnBrush = DodgerBlue;

                TestRunFlag = true;
                TestPauseFlag = false;

                TotalTestTimer.Start();
                TestTimeView.Start();

                testExe.TestContinue();

                return;
            }


            List<TestItemUnit> source = MakeList(TreeTestItems);

            var selectedItems = from tempItem in source
                                where tempItem.Checked != false
                                select tempItem;

            List<TestItemUnit> runableTestList = selectedItems.ToList();


            // 양식 경로 유효성 검사
            if (TestType == "FirstTest")
            {
                if (!File.Exists(basicInfo.FirstReportOpenPath))
                {
                    MessageBox.Show("경로에 양산 보고서 양식 파일이 존재하지 않습니다.\n보고서 양식 경로를 확인해주세요");
                    return;
                }
            }
            else
            {
                if (!File.Exists(basicInfo.SecondReportOpenPath))
                {
                    MessageBox.Show("경로에 출하 보고서 양식 파일이 존재하지 않습니다.\n보고서 양식 경로를 확인해주세요");
                    return;
                }
            }
            
            // 기본 정보 검사 및 폴더트리 생성
            if(!DirectoryManager.CreateReportFolderTree(basicInfo.ModelName, basicInfo.SerialNumber, basicInfo.ReportSavePath))
            {
                MessageBox.Show("폴더 생성에 실패했습니다.\n보고서 저장 경로 또는 기본 정보를 확인해주세요");
                return;
            }

            // 선택 테스트 검사
            if (runableTestList.Any() == false)
            {
                MessageBox.Show("1개 이상의 테스트를 선택하세요");
                return;
            }

            // 장비 연결 검사
            //string result = Test.EquiConnectCheck(true, TestType);
            //if (result.Length > 0)
            //{
            //    MessageBox.Show($"다음의 장비의 연결이 원할하지 않습니다.\n\n{result}", "장비 연결");
            //    return;
            //}

            if (TestType == "FirstTest")
                filePath = DirectoryManager.FirstCsvPath;
            else
                filePath = DirectoryManager.SecondCsvPath;

            // 이전 작성된 성적서 처리
            if (Directory.GetFiles(filePath).Length > 0)
            {
                ReportProcessWindow reportWindow = new ReportProcessWindow
                {
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                reportWindow.ShowDialog();

                string writeTypeStr = ((ReportProcessViewModel)reportWindow.DataContext).WriteType;

                if (writeTypeStr == "덮어쓰기")
                {
                    csvSavePath = csvReport.SetFilePath(basicInfo.Checker, filePath, FileWriteType.OVER_WRITE);

                    if (TestType == "FirstTest")
                    {
                        AllTestVM.FirstOrderInit();
                        csvReport.ReportSave(csvSavePath, new List<string[]>(AllTestVM.FirstOrder));
                    }
                    else
                    {
                        AllTestVM.FirstOrderInit();
                        csvReport.ReportSave(csvSavePath, new List<string[]>(AllTestVM.FirstOrder));
                        //AllTestVM.SecondOrderInit();
                        //csvReport.ReportSave(csvSavePath, new List<string[]>(AllTestVM.SecondOrder));
                    }
                }
                else if (writeTypeStr == "새로쓰기")
                {
                    csvSavePath = csvReport.SetFilePath(basicInfo.Checker, filePath, FileWriteType.NEW_WRITE);

                    if (TestType == "FirstTest")
                    {
                        AllTestVM.FirstOrderInit();
                        csvReport.ReportSave(csvSavePath, new List<string[]>(AllTestVM.FirstOrder));
                    }
                    else
                    {
                        AllTestVM.FirstOrderInit();
                        csvReport.ReportSave(csvSavePath, new List<string[]>(AllTestVM.FirstOrder));
                        //AllTestVM.SecondOrderInit();
                        //csvReport.ReportSave(csvSavePath, new List<string[]>(AllTestVM.SecondOrder));
                    }
                }
                else if (writeTypeStr == "이어쓰기")
                    csvSavePath = csvReport.SetFilePath(basicInfo.Checker, filePath, FileWriteType.CONTINUE_WRITE);
                else if (writeTypeStr == "취소")
                    return;
            }
            else
            {
                csvSavePath = csvReport.SetFilePath(basicInfo.Checker, filePath, FileWriteType.NEW_WRITE);

                if (TestType == "FirstTest")
                {
                    AllTestVM.FirstOrderInit();
                    csvReport.ReportSave(csvSavePath, new List<string[]>(AllTestVM.FirstOrder));
                }
                else
                {
                    AllTestVM.FirstOrderInit();
                    csvReport.ReportSave(csvSavePath, new List<string[]>(AllTestVM.FirstOrder));
                    //AllTestVM.SecondOrderInit();
                    //csvReport.ReportSave(csvSavePath, new List<string[]>(AllTestVM.SecondOrder));
                }
            }

            AllTestVM.ReportSavePath = csvSavePath;
            testExe.TestStart(runableTestList);
        }

        /**
         *  @brief 자동테스트 일시정지 버튼 클릭
         *  @details 자동테스트를 일시정지 한다
         *  
         *  @param
         *  
         *  @return
         */
        private void PauseClick()
        {
            testExe.TestPause();
        }

        /**
         *  @brief 자동테스트 정지 버튼 클릭
         *  @details 자동테스트를 정지 한다
         *  
         *  @param
         *  
         *  @return
         */
        private void StopClick()
        {
            testExe.TestStop();
        }

        /**
         *  @brief 테스트 시작 전 이벤트 핸들러
         *  @details 테스트 시작 전에 발생한 이벤트를 감지하여 수행한다
         *  
         *  @param object sender - 이벤트 발생 객체
         *  @param TestRunCheckEventArgs e - 이벤트 변수
         *  
         *  @return
         */
        private void BeforeTestStart(object sender, TestRunCheckEventArgs e)
        {
            List<TestItemUnit> source = MakeList(TreeTestItems);

            foreach (var item in source)
            {
                AllTestVM testItem = item.TestExeUi.DataContext as AllTestVM;
                testItem.UiReset();
                ((AllTestVM)item.TestExeUi.DataContext).TestLog.Clear();
            }

            TotalTestTimer.Reset();
            TotalTestTimer.Start();
            TestTimeView.Start();

            TestRunFlag = true;
            TestPauseFlag = false;
            IsTestEnd = false;

            StartBtnBrush = Gray;
            PauseBtnBrush = DodgerBlue;
            StopBtnBrush = DodgerBlue;

            TestRunningText = "테스트 진행중";
        }

        /**
         *  @brief 테스트 시작 이벤트 핸들러
         *  @details 테스트 시작 시 발생한 이벤트를 감지하여 수행한다
         *  
         *  @param object sender - 이벤트 발생 객체
         *  @param TestStartEventArgs e - 이벤트 변수
         *  
         *  @return
         */
        private void TestStart(object sender, TestStartEventArgs e)
        {
            List<TestItemUnit> source = MakeList(TreeTestItems);

            var itemUi = from temp in source
                       where temp.TestIndex == e.TestIndex && temp.CaseIndex == e.CaseIndex
                       select temp.TestExeUi;

            TestUi = itemUi.First();
        }

        /**
         *  @brief 테스트 세부 항목 시작 이벤트 핸들러
         *  @details 테스트 세부 항목 시작 시 발생한 이벤트를 감지하여 수행한다
         *  
         *  @param object sender - 이벤트 발생 객체
         *  @param UnitTestStartEventArgs e - 이벤트 변수
         *  
         *  @return
         */
        private void TestUnitItemStart(object sender, UnitTestStartEventArgs e)
        {
            //string result = Test.EquiConnectCheck(false, TestType);
            //if (!string.IsNullOrEmpty(result))
            //{
            //    PauseClick();
            //    MessageBox.Show($"다음의 장비의 연결이 원할하지 않습니다.\n\n{result}", "장비 연결");
            //}

            // 테스트 UI의 뷰포트 자동 이동
            if (Viewer != null)
            {
                double uintTestUiAvgWidth = Viewer.ExtentWidth / e.TotalSeqNumer; // 테스트의 한 항목 UI의 평균 가로길이
                double unitTestCenter = uintTestUiAvgWidth * e.CurrentSeqNumer + uintTestUiAvgWidth / 2;

                ScrollHorizontalOffset = unitTestCenter - Viewer.ViewportWidth / 2;
                Viewer.ScrollToHorizontalOffset(ScrollHorizontalOffset);
            }

            // 테스트 시작 시 글씨를 노란색으로 표기
            AllTestVM testItem = e.TestItem as AllTestVM;
            testItem.TextColorChange(e.CurrentSeqNumer, StateFlag.WAIT);
        }

        /**
         *  @brief 테스트 세부 항목 종료 이벤트 핸들러
         *  @details 테스트 세부 항목 종료 시 발생한 이벤트를 감지하여 수행한다
         *  
         *  @param object sender - 이벤트 발생 객체
         *  @param UnitTestEndEventArgs e - 이벤트 변수
         *  
         *  @return
         */
        private void TestUnitItemStop(object sender, UnitTestEndEventArgs e)
        {
            List<TestItemUnit> source = MakeList(TreeTestItems);

            var testItem = from item in source
                           where item.TestIndex == e.TestIndex && item.CaseIndex == e.CaseIndex
                           select item;

            // 유닛테스트 항목의 결과에 따른 색 변경
            TestItemUnit testInfo = testItem.First();
            AllTestVM unitItem = testInfo.TestExeUi.DataContext as AllTestVM;

            unitItem.TextColorChange(e.CurrentSeqNumer, e.Result);
        }

        /**
         *  @brief 테스트 종료 이벤트 핸들러
         *  @details 테스트 종료 시 발생한 이벤트를 감지하여 수행한다
         *  
         *  @param object sender - 이벤트 발생 객체
         *  @param TestEndEventArgs e - 이벤트 변수
         *  
         *  @return
         */
        private void TestEndCheck(object sender, TestEndEventArgs e)
        {
            List<TestItemUnit> source = MakeList(TreeTestItems);

            var testItem = from item in source
                           where item.TestIndex == e.TestIndex && item.CaseIndex == e.CaseIndex
                           select item;

            if (e.Result == "테스트 강제 종료")
            {
                testExe.TestStop();
                MessageBox.Show("필수 테스트 실패로 인한 테스트 중지");
            }
        }

        /**
         *  @brief 테스트 종료 후 이벤트 핸들러
         *  @details 테스트 완전 종료 후 발생한 이벤트를 감지하여 수행한다
         *  
         *  @param object sender - 이벤트 발생 객체
         *  @param EventArgs e - 이벤트 변수
         *  
         *  @return
         */
        private void AfterTestEnd(object sender, EventArgs e)
        {
            StartBtnBrush = DodgerBlue;
            PauseBtnBrush = Gray;
            StopBtnBrush  = Gray;

            TestRunFlag = false;
            TestPauseFlag = false;
            IsTestEnd = true;

            TotalTestTimer.Stop();
            TestTimeView.Stop();

            TestRunningText = "테스트 종료";

            // CSV 파일 다듬기 /////////////////////////////////////////////////////////////
            List<string[]> csvList = csvReport.CsvReader(csvSavePath);
            List<string> failList = csvReport.FailTest(csvSavePath);

            basicInfo.SwVersion = Rect.FwVersion.ToString();
            basicInfo.CheckDate = DateTime.Today.ToShortDateString();
            basicInfo.TestResult = failList.Any() ? "NG" : "OK";

            // 기본정보 삽입
            string[] str = new string[] { "0", basicInfo.Checker, basicInfo.ModelName, basicInfo.ProductCode, basicInfo.SerialNumber,
                                 basicInfo.DcdcSerial, basicInfo.PfcSerial, basicInfo.McuSerial, basicInfo.CheckDate,
                                 basicInfo.HwVersion, basicInfo.SwVersion, basicInfo.TestResult, basicInfo.DcdcNumber, basicInfo.PfcNumber, basicInfo.McuNumber };

            csvList[0] = str;
            csvReport.ReportSave(csvSavePath, csvList);
            /////////////////////////////////////////////////////////////
            
            if (TestType == "FirstTest") csvReport.ReportConverter(csvSavePath, basicInfo.FirstReportOpenPath);
            else                         csvReport.ReportConverter(csvSavePath, basicInfo.SecondReportOpenPath);


            // 테스트 종료 후 장비 OFF
            if (DcSource.GetObj().IsConnected) DcSource.GetObj().DcPowerCtrl(CtrlFlag.OFF);
            if(Option.IsFullAuto)
            {
                AcSource.GetObj().AcPowerCtrl(CtrlFlag.OFF);
                DcLoad.GetObj().LoadPowerCtrl(CtrlFlag.OFF);
            }

            StringBuilder endText = new StringBuilder();

            endText.AppendLine("테스트 완료\n");
            endText.AppendLine("[ 불합격 테스트 목록 ]");

            foreach (var item in failList)
                endText.AppendLine(item);

            MessageBox.Show(endText.ToString());
        }

        /**
         *  @brief 양산 테스트 보고서 서버 전송
         *  @details 양산 테스트 후 생성된 보고서를 서버로 전송시킨다
         *  
         *  @param
         *  
         *  @return
         */
        private void FirstServerSend()
        {
            if (!File.Exists(csvSavePath))
            {
                MessageBox.Show($"접근할 수 없는 경로입니다.\n{csvSavePath}");
                return;
            }

            ReportSender reportSender = new ReportSender();
            string result = reportSender.Reporsend(ReportSender.FIRST_TEST, csvSavePath);

            //reportSender.SetHttp();
            //byte[] a = reportSender.ConvertCvsToJson(ReportSend.FIRST_TEST, csvSavePath);
            //string result = reportSender.DataSend(a);

            if (!string.IsNullOrEmpty(result))
            {
                MessageBox.Show(result);
                return;
            }

            result = reportSender.AnswerReceive();
            //result = reportSender.DataReceive();
            //reportSender.Deserialize(result, out string str1);

            MessageBox.Show($"시리얼 번호 : {basicInfo.SerialNumber}\n전송 결과 : {result}");
        }

        /**
         *  @brief 출하 테스트 보고서 서버 전송
         *  @details 출하 테스트 후 생성된 보고서를 서버로 전송시킨다
         *  
         *  @param
         *  
         *  @return
         */
        private void SecondServerSend()
        {
            if (!File.Exists(csvSavePath))
            {
                MessageBox.Show($"접근할 수 없는 경로입니다.\n{csvSavePath}");
                return;
            }

            ReportSender reportSender = new ReportSender();
            string result = reportSender.Reporsend(ReportSender.SECOND_TEST, csvSavePath);

            //reportSender.SetHttp();
            //byte[] a = reportSender.ConvertCvsToJson(ReportSender.SECOND_TEST, csvSavePath);
            //string result = reportSender.DataSend(a);

            if (!string.IsNullOrEmpty(result))
            {
                MessageBox.Show(result);
                return;
            }

            result = reportSender.AnswerReceive();
            //result = reportSender.DataReceive();
            //reportSender.Deserialize(result, out string str1);

            MessageBox.Show($"시리얼 번호 : {basicInfo.SerialNumber}\n전송 결과 : {result}");
        }

        /**
         *  @brief 테스트 일시 정지 이벤트 핸들러
         *  @details 테스트 일시 정지 시 발생한 이벤트를 감지하여 수행한다
         *  
         *  @param object sender - 이벤트 발생 객체
         *  @param EventArgs e - 이벤트 변수
         *  
         *  @return
         */
        private void TestPause(object sender, EventArgs e)
        {
            StartBtnBrush = DodgerBlue;
            PauseBtnBrush = Gray;
            StopBtnBrush = DodgerBlue;

            TestRunFlag = false;
            TestPauseFlag = true;

            TotalTestTimer.Stop();
            TestTimeView.Stop();

            TestRunningText = "일시정지";

            if (DcSource.GetObj().IsConnected) DcSource.GetObj().DcPowerCtrl(CtrlFlag.OFF);
            if (Option.IsFullAuto) DcLoad.GetObj().LoadPowerCtrl(CtrlFlag.OFF);
        }

        /**
         *  @brief 테스트 일정지 이벤트 핸들러
         *  @details 테스트 정지 시 발생한 이벤트를 감지하여 수행한다
         *  
         *  @param object sender - 이벤트 발생 객체
         *  @param EventArgs e - 이벤트 변수
         *  
         *  @return
         */
        private void TestStop(object sender, EventArgs e)
        {
            StartBtnBrush = DodgerBlue;
            PauseBtnBrush = Gray;
            StopBtnBrush = Gray;

            TestRunFlag = false;
            TestPauseFlag = false;
            IsTestEnd = true;

            TotalTestTimer.Stop();
            TestTimeView.Stop();

            List<string[]> csvList = csvReport.CsvReader(csvSavePath);
            List<string> failList = csvReport.FailTest(csvSavePath);

            basicInfo.SwVersion = Rect.FwVersion.ToString();
            basicInfo.CheckDate = DateTime.Today.ToShortDateString();
            basicInfo.TestResult = failList.Any() ? "NG" : "OK";

            string[] str = new string[] { "0", basicInfo.Checker, basicInfo.ModelName, basicInfo.ProductCode, basicInfo.SerialNumber,
                                          basicInfo.DcdcSerial, basicInfo.PfcSerial, basicInfo.McuSerial, basicInfo.CheckDate,
                                          basicInfo.HwVersion, basicInfo.SwVersion, basicInfo.TestResult, basicInfo.DcdcNumber, basicInfo.PfcNumber, basicInfo.McuNumber };

            csvList[0] = str;
            csvReport.ReportSave(csvSavePath, csvList);

            if (TestType == "FirstTest") csvReport.ReportConverter(csvSavePath, basicInfo.FirstReportOpenPath);
            else                         csvReport.ReportConverter(csvSavePath, basicInfo.SecondReportOpenPath);

            TestRunningText = "테스트 정지";

            MessageBox.Show("테스트 정지");
        }

        /**
         *  @brief 트리뷰 소스 제작
         *  @details 테스트 목록에 들어가는 테스트의 트리뷰에 대한 소스를 만든다.
         *  
         *  @param
         *  
         *  @return ObservableCollection<TestItemUint> - 테스트 트리뷰 소스
         */
        private ObservableCollection<TestItemUnit> MakeTree()
        {
            int index = 0;
            ObservableCollection<TestItemUnit> TestTree = new ObservableCollection<TestItemUnit>();

            if(TestType == "FirstTest")
            {
                TestItemUnit cal  = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = null, TestName = CalReadyVM.TestName,  TestExeUi = new CalReady_UI(0) };
                TestItemUnit M200 = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = null, TestName = M200ReadyVM.TestName, TestExeUi = new M200Ready_UI(0) };
                TestItemUnit M100 = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = null, TestName = M100ReadyVM.TestName, TestExeUi = new M100Ready_UI(0) };

                TestItemUnit init = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = null, TestName = InitVM.TestName, TestExeUi = new 초기세팅_UI(0) };

                TestItemUnit acCal = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = cal, TestName = CalAcVM.TestName, TestExeUi = new Cal_AC_입력전압_UI(0) };
                TestItemUnit voltCal = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = cal, TestName = CalDcVoltVM.TestName, TestExeUi = new Cal_DC_출력전압_UI(0) };
                TestItemUnit currCal = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = cal, TestName = CalDcCurrVM.TestName, TestExeUi = new Cal_DC_출력전류_UI(0) };

                TestItemUnit isoReg = new TestItemUnit() { TestIndex = 0, CaseIndex = 0, Checked = false, TestName = IsolResVM.TestName, Parents = null, remark = new IsolResVM(0) };
                TestItemUnit isoPress = new TestItemUnit() { TestIndex = 0, CaseIndex = 0, Checked = false, TestName = IsolPressVM.TestName, Parents = null, remark = new IsolPressVM(0) };
                TestItemUnit PowerSup = new TestItemUnit() { TestIndex = 0, CaseIndex = 0, Checked = false, TestName = PowerSupplyVM.TestName, Parents = null, remark = new PowerSupplyVM(0) };
                TestItemUnit inrush = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, TestName = InrushVM.TestName, Parents = null, TestExeUi = new 돌입전류_UI(0) };
                TestItemUnit id = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = null, TestName = IdChangeVM.TestName, TestExeUi = new IdChange_UI(0) };
                TestItemUnit temp = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = null, TestName = TempVM.TestName, TestExeUi = new 온도센서_점검_UI(0) };
                TestItemUnit leakage = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, TestName = LeakageVM.TestName, Parents = null, TestExeUi = new 누설전류_UI(0) };
                TestItemUnit local = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = null, TestName = LocalSwitchVM.TestName, TestExeUi = new LocalSwitch_UI(0) };
                TestItemUnit remote = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = null, TestName = RemoteCommVM.TestName, TestExeUi = new RemoteComm_UI(0) };
                TestItemUnit bat = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = null, TestName = BatteryCommVM.TestName, TestExeUi = new BatteryComm_UI(0) };
                TestItemUnit led = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = null, TestName = LedCheckVM.TestName, TestExeUi = new LedCheck_UI(0) };

                //TestItemUnit noload = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M200, TestName = NoLoadVM.TestName, TestExeUi = new 무부하_전원_ON_UI(0) };

                TestItemUnit Reg200_1 = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M200, TestName = RegulM200VM.TestName + " 1", TestExeUi = new 레귤레이션_200V_UI(0) };
                TestItemUnit Reg200_2 = new TestItemUnit() { TestIndex = index++, CaseIndex = 1, Checked = false, Parents = M200, TestName = RegulM200VM.TestName + " 2", TestExeUi = new 레귤레이션_200V_UI(1) };
                TestItemUnit Reg200_3 = new TestItemUnit() { TestIndex = index++, CaseIndex = 2, Checked = false, Parents = M200, TestName = RegulM200VM.TestName + " 3", TestExeUi = new 레귤레이션_200V_UI(2) };
                TestItemUnit Reg200_4 = new TestItemUnit() { TestIndex = index++, CaseIndex = 3, Checked = false, Parents = M200, TestName = RegulM200VM.TestName + " 4", TestExeUi = new 레귤레이션_200V_UI(3) };
                TestItemUnit Reg200_5 = new TestItemUnit() { TestIndex = index++, CaseIndex = 4, Checked = false, Parents = M200, TestName = RegulM200VM.TestName + " 5", TestExeUi = new 레귤레이션_200V_UI(4) };
                TestItemUnit Reg200_6 = new TestItemUnit() { TestIndex = index++, CaseIndex = 5, Checked = false, Parents = M200, TestName = RegulM200VM.TestName + " 6", TestExeUi = new 레귤레이션_200V_UI(5) };
                TestItemUnit Reg200_7 = new TestItemUnit() { TestIndex = index++, CaseIndex = 6, Checked = false, Parents = M200, TestName = RegulM200VM.TestName + " 7", TestExeUi = new 레귤레이션_200V_UI(6) };
                TestItemUnit Reg200_8 = new TestItemUnit() { TestIndex = index++, CaseIndex = 7, Checked = false, Parents = M200, TestName = RegulM200VM.TestName + " 8", TestExeUi = new 레귤레이션_200V_UI(7) };
                TestItemUnit Reg200_9 = new TestItemUnit() { TestIndex = index++, CaseIndex = 8, Checked = false, Parents = M200, TestName = RegulM200VM.TestName + " 9", TestExeUi = new 레귤레이션_200V_UI(8) };

                TestItemUnit noise = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M200, TestName = NoiseVM.TestName, TestExeUi = new 리플_노이즈_UI(0) };
                TestItemUnit pf = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M200, TestName = PowerFactorVM.TestName, TestExeUi = new 역률_UI(0) };
                TestItemUnit effic = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M200, TestName = EfficiencyVM.TestName, TestExeUi = new 효율_UI(0) };
                TestItemUnit outLow = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M200, TestName = OutputLowVM.TestName, TestExeUi = new 출력_저전압_보호_UI(0) };
                TestItemUnit outHigh = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M200, TestName = OutputHighVM.TestName, TestExeUi = new 출력_고전압_보호_UI(0) };
                TestItemUnit acLow1 = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M100, TestName = AcLowVM.TestName + " 1", TestExeUi = new AC_저전압_알람_UI(0) };
                TestItemUnit acHigh = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M200, TestName = AcHighVM.TestName, TestExeUi = new AC_고전압_알람_UI(0) };
                TestItemUnit outOver1 = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M200, TestName = OutputOverVM.TestName + " 1", TestExeUi = new 출력_과부하_보호_UI(0) };

                TestItemUnit Reg100_1 = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M100, TestName = RegulM100VM.TestName + " 1", TestExeUi = new 레귤레이션_100V_UI(0) };
                TestItemUnit Reg100_2 = new TestItemUnit() { TestIndex = index++, CaseIndex = 1, Checked = false, Parents = M100, TestName = RegulM100VM.TestName + " 2", TestExeUi = new 레귤레이션_100V_UI(1) };
                TestItemUnit Reg100_3 = new TestItemUnit() { TestIndex = index++, CaseIndex = 2, Checked = false, Parents = M100, TestName = RegulM100VM.TestName + " 3", TestExeUi = new 레귤레이션_100V_UI(2) };
                TestItemUnit Reg100_4 = new TestItemUnit() { TestIndex = index++, CaseIndex = 3, Checked = false, Parents = M100, TestName = RegulM100VM.TestName + " 4", TestExeUi = new 레귤레이션_100V_UI(3) };
                TestItemUnit Reg100_5 = new TestItemUnit() { TestIndex = index++, CaseIndex = 4, Checked = false, Parents = M100, TestName = RegulM100VM.TestName + " 5", TestExeUi = new 레귤레이션_100V_UI(4) };
                TestItemUnit Reg100_6 = new TestItemUnit() { TestIndex = index++, CaseIndex = 5, Checked = false, Parents = M100, TestName = RegulM100VM.TestName + " 6", TestExeUi = new 레귤레이션_100V_UI(5) };
                TestItemUnit Reg100_7 = new TestItemUnit() { TestIndex = index++, CaseIndex = 6, Checked = false, Parents = M100, TestName = RegulM100VM.TestName + " 7", TestExeUi = new 레귤레이션_100V_UI(6) };
                TestItemUnit Reg100_8 = new TestItemUnit() { TestIndex = index++, CaseIndex = 7, Checked = false, Parents = M100, TestName = RegulM100VM.TestName + " 8", TestExeUi = new 레귤레이션_100V_UI(7) };
                TestItemUnit Reg100_9 = new TestItemUnit() { TestIndex = index++, CaseIndex = 8, Checked = false, Parents = M100, TestName = RegulM100VM.TestName + " 9", TestExeUi = new 레귤레이션_100V_UI(8) };

                TestItemUnit acLow2 = new TestItemUnit() { TestIndex = index++, CaseIndex = 1, Checked = false, Parents = M200, TestName = AcLowVM.TestName + " 2", TestExeUi = new AC_저전압_알람_UI(1) };
                TestItemUnit acOut = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M100, TestName = AcBlackOutVM.TestName, TestExeUi = new AC_정전전압_인식_UI(0) };
                TestItemUnit outOver2 = new TestItemUnit() { TestIndex = index++, CaseIndex = 1, Checked = false, Parents = M100, TestName = OutputOverVM.TestName + " 2", TestExeUi = new 출력_과부하_보호_UI(1) };
                TestItemUnit rtc = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M200, TestName = RtcCheckVM.TestName, TestExeUi = new RTC_TIME_체크_UI(0) };


                cal.Child.Add(acCal);    // 입력 CAL
                cal.Child.Add(voltCal);    // 출력 전압 CAL
                cal.Child.Add(currCal);    // 출력 전류 CAL

                //M200.Child.Add(temp);
                M200.Child.Add(Reg200_1);
                M200.Child.Add(Reg200_2);
                M200.Child.Add(Reg200_3);
                M200.Child.Add(Reg200_4);
                M200.Child.Add(Reg200_5);
                M200.Child.Add(Reg200_6);
                M200.Child.Add(Reg200_7);
                M200.Child.Add(Reg200_8);
                M200.Child.Add(Reg200_9);
                M200.Child.Add(noise);
                M200.Child.Add(pf);
                M200.Child.Add(effic);
                M200.Child.Add(outLow);
                M200.Child.Add(outHigh);
                M200.Child.Add(acLow1);
                M200.Child.Add(acHigh);
                M200.Child.Add(outOver1);

                M100.Child.Add(Reg100_1);
                M100.Child.Add(Reg100_2);
                M100.Child.Add(Reg100_3);
                M100.Child.Add(Reg100_4);
                M100.Child.Add(Reg100_5);
                M100.Child.Add(Reg100_6);
                M100.Child.Add(Reg100_7);
                M100.Child.Add(Reg100_8);
                M100.Child.Add(Reg100_9);
                M100.Child.Add(acLow2);
                M100.Child.Add(acOut);
                M100.Child.Add(outOver2);
                M100.Child.Add(rtc);

                TestTree.Add(init); // 초기세팅
                TestTree.Add(inrush); // 돌입전류
                TestTree.Add(id); // ID 변경
                TestTree.Add(temp); // 온도
                TestTree.Add(leakage); // 누설전류
                TestTree.Add(local); // Local Switch
                TestTree.Add(remote); // 리모트 통신
                TestTree.Add(bat); // 배터리 통신
                TestTree.Add(led); // LED
                TestTree.Add(cal);   // CAL
                TestTree.Add(M200);  // 200V Mode
                TestTree.Add(M100);  // 100V Mode
            }
            else
            {
                TestItemUnit M200 = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = null, TestName = M200ReadyVM.TestName, TestExeUi = new M200Ready_UI(0) };
                TestItemUnit M100 = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = null, TestName = M100ReadyVM.TestName, TestExeUi = new M100Ready_UI(0) };

                TestItemUnit init = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = null, TestName = InitVM.TestName, TestExeUi = new 초기세팅_UI(0) };

                TestItemUnit isoReg = new TestItemUnit() { TestIndex = 0, CaseIndex = 0, Checked = false, TestName = IsolResVM.TestName, Parents = null, remark = new IsolResVM(0) };
                TestItemUnit isoPress = new TestItemUnit() { TestIndex = 0, CaseIndex = 0, Checked = false, TestName = IsolPressVM.TestName, Parents = null, remark = new IsolPressVM(0) };
                TestItemUnit PowerSup = new TestItemUnit() { TestIndex = 0, CaseIndex = 0, Checked = false, TestName = PowerSupplyVM.TestName, Parents = null, remark = new PowerSupplyVM(0) };
                TestItemUnit inrush = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, TestName = InrushVM.TestName, Parents = null, TestExeUi = new 돌입전류_UI(0) };
                TestItemUnit id = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = null, TestName = IdChangeVM.TestName, TestExeUi = new IdChange_UI(0) };
                TestItemUnit temp = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = null, TestName = TempVM.TestName, TestExeUi = new 온도센서_점검_UI(0) };
                TestItemUnit leakage = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, TestName = LeakageVM.TestName, Parents = null, TestExeUi = new 누설전류_UI(0) };
                TestItemUnit local = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = null, TestName = LocalSwitchVM.TestName, TestExeUi = new LocalSwitch_UI(0) };
                TestItemUnit remote = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = null, TestName = RemoteCommVM.TestName, TestExeUi = new RemoteComm_UI(0) };
                TestItemUnit bat = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = null, TestName = BatteryCommVM.TestName, TestExeUi = new BatteryComm_UI(0) };
                TestItemUnit led = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = null, TestName = LedCheckVM.TestName, TestExeUi = new LedCheck_UI(0) };

                //TestItemUnit noload = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M200, TestName = NoLoadVM.TestName, TestExeUi = new 무부하_전원_ON_UI(0) };

                TestItemUnit Reg200_1 = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M200, TestName = RegulM200VM.TestName + " 1", TestExeUi = new 레귤레이션_200V_UI(0) };
                TestItemUnit Reg200_2 = new TestItemUnit() { TestIndex = index++, CaseIndex = 1, Checked = false, Parents = M200, TestName = RegulM200VM.TestName + " 2", TestExeUi = new 레귤레이션_200V_UI(1) };
                TestItemUnit Reg200_3 = new TestItemUnit() { TestIndex = index++, CaseIndex = 2, Checked = false, Parents = M200, TestName = RegulM200VM.TestName + " 3", TestExeUi = new 레귤레이션_200V_UI(2) };
                TestItemUnit Reg200_4 = new TestItemUnit() { TestIndex = index++, CaseIndex = 3, Checked = false, Parents = M200, TestName = RegulM200VM.TestName + " 4", TestExeUi = new 레귤레이션_200V_UI(3) };
                TestItemUnit Reg200_5 = new TestItemUnit() { TestIndex = index++, CaseIndex = 4, Checked = false, Parents = M200, TestName = RegulM200VM.TestName + " 5", TestExeUi = new 레귤레이션_200V_UI(4) };
                TestItemUnit Reg200_6 = new TestItemUnit() { TestIndex = index++, CaseIndex = 5, Checked = false, Parents = M200, TestName = RegulM200VM.TestName + " 6", TestExeUi = new 레귤레이션_200V_UI(5) };
                TestItemUnit Reg200_7 = new TestItemUnit() { TestIndex = index++, CaseIndex = 6, Checked = false, Parents = M200, TestName = RegulM200VM.TestName + " 7", TestExeUi = new 레귤레이션_200V_UI(6) };
                TestItemUnit Reg200_8 = new TestItemUnit() { TestIndex = index++, CaseIndex = 7, Checked = false, Parents = M200, TestName = RegulM200VM.TestName + " 8", TestExeUi = new 레귤레이션_200V_UI(7) };
                TestItemUnit Reg200_9 = new TestItemUnit() { TestIndex = index++, CaseIndex = 8, Checked = false, Parents = M200, TestName = RegulM200VM.TestName + " 9", TestExeUi = new 레귤레이션_200V_UI(8) };

                TestItemUnit noise = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M200, TestName = NoiseVM.TestName, TestExeUi = new 리플_노이즈_UI(0) };
                TestItemUnit pf = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M200, TestName = PowerFactorVM.TestName, TestExeUi = new 역률_UI(0) };
                TestItemUnit effic = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M200, TestName = EfficiencyVM.TestName, TestExeUi = new 효율_UI(0) };
                TestItemUnit outLow = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M200, TestName = OutputLowVM.TestName, TestExeUi = new 출력_저전압_보호_UI(0) };
                TestItemUnit outHigh = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M200, TestName = OutputHighVM.TestName, TestExeUi = new 출력_고전압_보호_UI(0) };
                TestItemUnit acLow1 = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M100, TestName = AcLowVM.TestName + " 1", TestExeUi = new AC_저전압_알람_UI(0) };
                TestItemUnit acHigh = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M200, TestName = AcHighVM.TestName, TestExeUi = new AC_고전압_알람_UI(0) };
                TestItemUnit outOver1 = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M200, TestName = OutputOverVM.TestName + " 1", TestExeUi = new 출력_과부하_보호_UI(0) };

                TestItemUnit Reg100_1 = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M100, TestName = RegulM100VM.TestName + " 1", TestExeUi = new 레귤레이션_100V_UI(0) };
                TestItemUnit Reg100_2 = new TestItemUnit() { TestIndex = index++, CaseIndex = 1, Checked = false, Parents = M100, TestName = RegulM100VM.TestName + " 2", TestExeUi = new 레귤레이션_100V_UI(1) };
                TestItemUnit Reg100_3 = new TestItemUnit() { TestIndex = index++, CaseIndex = 2, Checked = false, Parents = M100, TestName = RegulM100VM.TestName + " 3", TestExeUi = new 레귤레이션_100V_UI(2) };
                TestItemUnit Reg100_4 = new TestItemUnit() { TestIndex = index++, CaseIndex = 3, Checked = false, Parents = M100, TestName = RegulM100VM.TestName + " 4", TestExeUi = new 레귤레이션_100V_UI(3) };
                TestItemUnit Reg100_5 = new TestItemUnit() { TestIndex = index++, CaseIndex = 4, Checked = false, Parents = M100, TestName = RegulM100VM.TestName + " 5", TestExeUi = new 레귤레이션_100V_UI(4) };
                TestItemUnit Reg100_6 = new TestItemUnit() { TestIndex = index++, CaseIndex = 5, Checked = false, Parents = M100, TestName = RegulM100VM.TestName + " 6", TestExeUi = new 레귤레이션_100V_UI(5) };
                TestItemUnit Reg100_7 = new TestItemUnit() { TestIndex = index++, CaseIndex = 6, Checked = false, Parents = M100, TestName = RegulM100VM.TestName + " 7", TestExeUi = new 레귤레이션_100V_UI(6) };
                TestItemUnit Reg100_8 = new TestItemUnit() { TestIndex = index++, CaseIndex = 7, Checked = false, Parents = M100, TestName = RegulM100VM.TestName + " 8", TestExeUi = new 레귤레이션_100V_UI(7) };
                TestItemUnit Reg100_9 = new TestItemUnit() { TestIndex = index++, CaseIndex = 8, Checked = false, Parents = M100, TestName = RegulM100VM.TestName + " 9", TestExeUi = new 레귤레이션_100V_UI(8) };

                TestItemUnit acLow2 = new TestItemUnit() { TestIndex = index++, CaseIndex = 1, Checked = false, Parents = M200, TestName = AcLowVM.TestName + " 2", TestExeUi = new AC_저전압_알람_UI(1) };
                TestItemUnit acOut = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M100, TestName = AcBlackOutVM.TestName, TestExeUi = new AC_정전전압_인식_UI(0) };
                TestItemUnit outOver2 = new TestItemUnit() { TestIndex = index++, CaseIndex = 1, Checked = false, Parents = M100, TestName = OutputOverVM.TestName + " 2", TestExeUi = new 출력_과부하_보호_UI(1) };
                TestItemUnit rtc = new TestItemUnit() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M200, TestName = RtcCheckVM.TestName, TestExeUi = new RTC_TIME_체크_UI(0) };

                //M200.Child.Add(temp);
                M200.Child.Add(Reg200_1);
                M200.Child.Add(Reg200_2);
                M200.Child.Add(Reg200_3);
                M200.Child.Add(Reg200_4);
                M200.Child.Add(Reg200_5);
                M200.Child.Add(Reg200_6);
                M200.Child.Add(Reg200_7);
                M200.Child.Add(Reg200_8);
                M200.Child.Add(Reg200_9);
                M200.Child.Add(noise);
                M200.Child.Add(pf);
                M200.Child.Add(effic);
                M200.Child.Add(outLow);
                M200.Child.Add(outHigh);
                M200.Child.Add(acLow1);
                M200.Child.Add(acHigh);
                M200.Child.Add(outOver1);

                M100.Child.Add(Reg100_1);
                M100.Child.Add(Reg100_2);
                M100.Child.Add(Reg100_3);
                M100.Child.Add(Reg100_4);
                M100.Child.Add(Reg100_5);
                M100.Child.Add(Reg100_6);
                M100.Child.Add(Reg100_7);
                M100.Child.Add(Reg100_8);
                M100.Child.Add(Reg100_9);
                M100.Child.Add(acLow2);
                M100.Child.Add(acOut);
                M100.Child.Add(outOver2);
                M100.Child.Add(rtc);

                TestTree.Add(init); // 초기세팅
                TestTree.Add(inrush); // 돌입전류
                TestTree.Add(id); // ID 변경
                TestTree.Add(temp); // 온도
                TestTree.Add(leakage); // 누설전류
                TestTree.Add(local); // Local Switch
                TestTree.Add(remote); // 리모트 통신
                TestTree.Add(bat); // 배터리 통신
                TestTree.Add(led); // LED
                TestTree.Add(M200);  // 200V Mode
                TestTree.Add(M100);  // 100V Mode
            }

            return TestTree;
        }

        /**
         *  @brief 테스트 트리뷰 리스트 변환기
         *  @details 테스트 트리뷰를 일차원의 리스트로 변환한다
         *  
         *  @param
         *  
         *  @return ObservableCollection<TestItemUint> - 테스트 트리뷰 소스
         */
        private List<TestItemUnit> MakeList(ObservableCollection<TestItemUnit> source)
        {
            List<TestItemUnit> tempList = new List<TestItemUnit>();

            foreach (var item in source)
            {
                if (item.Child.Count == 0)
                    tempList.Add(item);
                else
                {
                    tempList.Add(item);
                    foreach (var childItem in item.Child)
                        tempList.Add(childItem);
                }
            }
            return tempList;
        }
    }
}