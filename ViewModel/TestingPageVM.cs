﻿using J_Project.Communication.CommFlags;
using J_Project.Data;
using J_Project.Equipment;
using J_Project.FileSystem;
using J_Project.Manager;
using J_Project.Manager.EventArgsClass;
using J_Project.UI.SubWindow;
using J_Project.UI.TestSeq.Execution;
using J_Project.ViewModel.CommandClass;
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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Timer = System.Timers.Timer;

namespace J_Project.ViewModel
{
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

        Timer TestTimeView = new Timer();
        Stopwatch TotalTestTimer = new Stopwatch();

        public ObservableCollection<TestItemUint> TreeTestItems { get; set; }
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

        public ICommand TreeItemSelectedCommand { get; set; }
        public ICommand ScrollView { get; set; }

        public ICommand AutoTestStartCommand { get; set; }
        public ICommand AutoTestPauseCommand { get; set; }
        public ICommand AutoTestStopCommand { get; set; }

        public ICommand AllCheckCommand { get; set; }
        public ICommand AllUncheckCommand { get; set; }

        public ICommand ResetCommand { get; set; }
        public ICommand CalResetCommand { get; set; }
        public ICommand LogRenewalCommand { get; set; }

        public ICommand NodeUpCommand { get; set; }
        public ICommand NodeDownCommand { get; set; }

        public ICommand FirstSendCommand { get; set; }
        public ICommand SecondSendCommand { get; set; }
        public ICommand CsvConverterCommand { get; set; }

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

            TreeItemSelectedCommand = new BaseObjCommand(TreeViewSelected);
            ScrollView = new BaseObjCommand(AutoScrolling);

            AutoTestStartCommand = new BaseCommand(StartClick);
            AutoTestPauseCommand = new BaseCommand(PauseClick);
            AutoTestStopCommand = new BaseCommand(StopClick);

            AllCheckCommand = new BaseCommand(TreeItemAllCheck);
            AllUncheckCommand = new BaseCommand(TreeItemAllUnCheck);

            ResetCommand = new BaseCommand(RectResetCheck);
            CalResetCommand = new BaseCommand(CalResetCheck);
            LogRenewalCommand = new BaseCommand(LogRenewal);

            NodeUpCommand = new BaseObjCommand(NodeUp);
            NodeDownCommand = new BaseObjCommand(NodeDown);

            FirstSendCommand = new BaseCommand(FirstServerSend);
            SecondSendCommand = new BaseCommand(SecondServerSend);
            CsvConverterCommand = new BaseCommand(CsvConverter);
        }

        // 테스트 항목 선택 시 UI 변경
        private void TreeViewSelected(object selectedItem)
        {
            TestItemUint Item = selectedItem as TestItemUint;
            try
            {
                TestUi = Item.TestExeUi;
                Log = ((AllTestVM)TestUi.DataContext).TestLog.ToString();
            }
            catch (Exception) { }
        }

        // 스크롤 뷰어 가져오기
        private void AutoScrolling(object scroll)
        {
            Viewer = scroll as ScrollViewer;
        }

        // 테스트 항목 전체 선택
        private void TreeItemAllCheck()
        {
            foreach (var temp in MakeList(TreeTestItems))
                temp.Checked = true;
        }

        // 테스트 항목 전체 선택 해제
        private void TreeItemAllUnCheck()
        {
            foreach (var temp in MakeList(TreeTestItems))
                temp.Checked = false;
        }

        // 보고서 변환기
        private void CsvConverter()
        {
            // CSV 파일 다듬기 /////////////////////////////////////////////////////////////
            List<string[]> sortList = csvReport.DataSort(csvReport.CsvReader(csvSavePath), TestType);

            basicInfo.SwVersion = Rect.FwVersion.ToString();
            basicInfo.CheckDate = DateTime.Today.ToShortDateString();
            basicInfo.TestResult = "합격";
            for (int i = 1; i < sortList.Count; i++)
            {
                if (sortList[i].Contains("불합격"))
                {
                    basicInfo.TestResult = "불합격";
                    break;
                }
            }

            string[] str = new string[] { "0", basicInfo.Checker, basicInfo.ModelName, basicInfo.ProductCode, basicInfo.SerialNumber,
                                          basicInfo.DcdcSerial, basicInfo.PfcSerial, basicInfo.McuSerial, basicInfo.CheckDate, basicInfo.HwVersion, basicInfo.SwVersion,
                                          basicInfo.TestResult, basicInfo.DcdcNumber, basicInfo.PfcNumber, basicInfo.McuNumber };

            sortList[0] = str;
            csvReport.ReportSave(csvSavePath, sortList);
            /////////////////////////////////////////////////////////////

            if (TestType == "FirstTest")
                csvReport.ReportConverter(csvSavePath, basicInfo.FirstReportOpenPath);
            else
                csvReport.ReportConverter(csvSavePath, basicInfo.SecondReportOpenPath);
        }

        public void NodeUp(object selectedItem)
        {
            if (selectedItem == null) return;

            //TreeTestItems;
            TestItemUint node = selectedItem as TestItemUint;

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
        public void NodeDown(object selectedItem)
        {
            if (selectedItem == null) return;

            //TreeTestItems;
            TestItemUint node = selectedItem as TestItemUint;

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

        // 정류기 리셋
        private void RectResetCheck()
        {
            if(Rectifier.GetObj().RectCommand(CommandList.SW_RESET, 1))
                MessageBox.Show("정류기 리셋 OK");
        }
        // 정류기 CAL 리셋
        private void CalResetCheck()
        {
            if(Rectifier.GetObj().RectCommand(CommandList.CAL_RESET, 1))
                MessageBox.Show("정류기 CAL 리셋 OK");
        }
        // 로그 갱신
        private void LogRenewal()
        {
            if (TestUi == null) return;

            Log = ((AllTestVM)TestUi.DataContext).TestLog.ToString();
        }

        // 자동테스트 시작 버튼 클릭
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


            List<TestItemUint> source = MakeList(TreeTestItems);

            var selectedItems = from tempItem in source
                                where tempItem.Checked != false
                                select tempItem;

            List<TestItemUint> runableTestList = selectedItems.ToList();


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
                    csvSavePath = csvReport.SetFilePath(basicInfo.Checker, filePath, FileWriteType.OVER_WRITE);
                else if (writeTypeStr == "이어쓰기")
                    csvSavePath = csvReport.SetFilePath(basicInfo.Checker, filePath, FileWriteType.CONTINUE_WRITE);
                else if (writeTypeStr == "새로쓰기")
                    csvSavePath = csvReport.SetFilePath(basicInfo.Checker, filePath, FileWriteType.NEW_WRITE);
                else if (writeTypeStr == "취소")
                    return;
            }
            else
                csvSavePath = csvReport.SetFilePath(basicInfo.Checker, filePath, FileWriteType.NEW_WRITE);


            if (TestType == "FirstTest")
            {
                csvReport.ReportSave(csvSavePath, ((int)FirstTestOrder.IsolRes).ToString(), "절연저항(TNR 제거)", "OK", "합격");
                csvReport.ReportSave(csvSavePath, ((int)FirstTestOrder.IsolPress).ToString(), "절연내압(TNR 제거)", "OK", "합격");
                csvReport.ReportSave(csvSavePath, ((int)FirstTestOrder.PowerSupply).ToString(), "배터리 전원 공급 확인", "OK", "합격");
            }

            AllTestVM.ReportSavePath = csvSavePath;
            testExe.TestStart(runableTestList);
        }

        private void PauseClick()
        {
            testExe.TestPause();
        }

        private void StopClick()
        {
            testExe.TestStop();
        }

        // 테스트 시작 전 이벤트 핸들러
        private void BeforeTestStart(object sender, TestRunCheckEventArgs e)
        {
            List<TestItemUint> source = MakeList(TreeTestItems);

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

        // 테스트 시작 이벤트 핸들러
        private void TestStart(object sender, TestStartEventArgs e)
        {
            List<TestItemUint> source = MakeList(TreeTestItems);

            var itemUi = from temp in source
                       where temp.TestIndex == e.TestIndex && temp.CaseIndex == e.CaseIndex
                       select temp.TestExeUi;

            TestUi = itemUi.First();
        }

        // 테스트 단일 요소 시작 이벤트 핸들러
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

        // 테스트 단일 요소 종료 이벤트 핸들러
        private void TestUnitItemStop(object sender, UnitTestEndEventArgs e)
        {
            List<TestItemUint> source = MakeList(TreeTestItems);

            var testItem = from item in source
                           where item.TestIndex == e.TestIndex && item.CaseIndex == e.CaseIndex
                           select item;

            // 유닛테스트 항목의 결과에 따른 색 변경
            TestItemUint testInfo = testItem.First();
            AllTestVM unitItem = testInfo.TestExeUi.DataContext as AllTestVM;

            unitItem.TextColorChange(e.CurrentSeqNumer, e.Result);
        }

        // 테스트 종료 이벤트 핸들러
        private void TestEndCheck(object sender, TestEndEventArgs e)
        {
            List<TestItemUint> source = MakeList(TreeTestItems);

            var testItem = from item in source
                           where item.TestIndex == e.TestIndex && item.CaseIndex == e.CaseIndex
                           select item;

            if (e.Result == "테스트 강제 종료")
            {
                testExe.TestStop();
                MessageBox.Show("필수 테스트 실패로 인한 테스트 중지");
            }
        }

        // 테스트 종료 후 이벤트 핸들러
        private void AfterTestEnd(object sender, TestRunCheckEventArgs e)
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
            List<string[]> sortList = csvReport.DataSort(csvReport.CsvReader(csvSavePath), TestType);

            basicInfo.SwVersion = Rect.FwVersion.ToString();
            basicInfo.CheckDate = DateTime.Today.ToShortDateString();
            basicInfo.TestResult = "합격";
            for (int i = 1; i < sortList.Count; i++)
            {
                if (sortList[i].Contains("불합격"))
                {
                    basicInfo.TestResult = "불합격";
                    break;
                }
            }
            
            string[] str = new string[] { "0", basicInfo.Checker, basicInfo.ModelName, basicInfo.ProductCode, basicInfo.SerialNumber, basicInfo.CheckDate,
                                 basicInfo.HwVersion, basicInfo.SwVersion, basicInfo.TestResult, basicInfo.DcdcNumber, basicInfo.PfcNumber, basicInfo.McuNumber };

            sortList[0] = str;
            csvReport.ReportSave(csvSavePath, sortList);
            /////////////////////////////////////////////////////////////

            if (TestType == "FirstTest")
                csvReport.ReportConverter(csvSavePath, basicInfo.FirstReportOpenPath);
            else
                csvReport.ReportConverter(csvSavePath, basicInfo.SecondReportOpenPath);


            // 테스트 종료 후 장비 OFF
            if (DcSource.GetObj().IsConnected) DcSource.GetObj().DcPowerCtrl(CtrlFlag.OFF);
            if(Option.IsFullAuto)
            {
                AcSource.GetObj().AcPowerCtrl(CtrlFlag.OFF);
                DcLoad.GetObj().LoadPowerCtrl(CtrlFlag.OFF);
            }

            List<string> failTest = csvReport.FailTest(csvSavePath);
            StringBuilder endText = new StringBuilder();

            endText.AppendLine("테스트 완료\n");
            endText.AppendLine("불합격 테스트 목록");

            foreach (var item in failTest)
                endText.AppendLine(item);

            MessageBox.Show(endText.ToString());
        }

        // 양산 테스트 보고서 서버 전송
        private void FirstServerSend()
        {
            if (!File.Exists(csvSavePath))
            {
                MessageBox.Show($"접근할 수 없는 경로입니다.\n{csvSavePath}");
                return;
            }

            ReportSend reportSender = new ReportSend();

            reportSender.SetHttp();
            byte[] a = reportSender.ConvertCvsToJson(ReportSend.FIRST_TEST, csvSavePath);
            string result = reportSender.DataSend(a);

            if (!string.IsNullOrEmpty(result))
            {
                MessageBox.Show(result);
                return;
            }

            result = reportSender.DataReceive();
            reportSender.Deserialize(result, out string str1);

            MessageBox.Show($"시리얼 번호 : {basicInfo.SerialNumber}\n전송 결과 : {str1}");
        }

        // 출하 테스트 보고서 서버 전송
        private void SecondServerSend()
        {
            if (!File.Exists(csvSavePath))
            {
                MessageBox.Show($"접근할 수 없는 경로입니다.\n{csvSavePath}");
                return;
            }

            ReportSend reportSender = new ReportSend();

            reportSender.SetHttp();
            byte[] a = reportSender.ConvertCvsToJson(ReportSend.SECOND_TEST, csvSavePath);
            string result = reportSender.DataSend(a);

            if (!string.IsNullOrEmpty(result))
            {
                MessageBox.Show(result);
                return;
            }

            result = reportSender.DataReceive();
            reportSender.Deserialize(result, out string str1);

            MessageBox.Show($"시리얼 번호 : {basicInfo.SerialNumber}\n전송 결과 : {str1}");
        }

        // 테스트 일시 정지 이벤트 핸들러
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

        // 테스트 정지 이벤트 핸들러
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

            List<string[]> sortList = csvReport.DataSort(csvReport.CsvReader(csvSavePath), TestType);

            basicInfo.SwVersion = Rect.FwVersion.ToString();
            basicInfo.CheckDate = DateTime.Today.ToShortDateString();
            basicInfo.TestResult = "합격";
            for (int i = 1; i < sortList.Count; i++)
            {
                if (sortList[i].Contains("불합격"))
                {
                    basicInfo.TestResult = "불합격";
                    break;
                }
            }
            //csvReport.ReportSave(csvSavePath, "0", basicInfo.Checker, basicInfo.ModelName, basicInfo.ProductCode, basicInfo.SerialNumber, basicInfo.CheckDate,
            //                     basicInfo.HwVersion, basicInfo.SwVersion, basicInfo.TestResult, basicInfo.DcdcNumber, basicInfo.PfcNumber, basicInfo.McuNumber);

            string[] str = new string[] { "0", basicInfo.Checker, basicInfo.ModelName, basicInfo.ProductCode, basicInfo.SerialNumber,
                                          basicInfo.DcdcSerial, basicInfo.PfcSerial, basicInfo.McuSerial, basicInfo.CheckDate, basicInfo.HwVersion, basicInfo.SwVersion,
                                          basicInfo.TestResult, basicInfo.DcdcNumber, basicInfo.PfcNumber, basicInfo.McuNumber };

            sortList[0] = str;
            csvReport.ReportSave(csvSavePath, sortList);

            if (TestType == "FirstTest")
            {
                csvReport.ReportConverter(csvSavePath, basicInfo.FirstReportOpenPath);
            }
            else
            {
                csvReport.ReportConverter(csvSavePath, basicInfo.SecondReportOpenPath);
            }

            TestRunningText = "테스트 정지";

            MessageBox.Show("테스트 정지");
        }

        // 트리뷰 소스 제작 함수
        private ObservableCollection<TestItemUint> MakeTree()
        {
            int index = 0;
            ObservableCollection<TestItemUint> TestTree = new ObservableCollection<TestItemUint>();

            if(TestType == "FirstTest")
            {
                TestItemUint cal  = new TestItemUint() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = null, TestName = CalReadyVM.TestName,  TestExeUi = new CalReady_UI() };
                TestItemUint M200 = new TestItemUint() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = null, TestName = M200ReadyVM.TestName, TestExeUi = new M200Ready_UI() };
                TestItemUint M100 = new TestItemUint() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = null, TestName = M100ReadyVM.TestName, TestExeUi = new M100Ready_UI() };

                TestItemUint init = new TestItemUint() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = null, TestName = InitVM.TestName, TestExeUi = new 초기세팅_UI() };
                //TestItemUint test1 = new TestItemUint() { TestIndex = index++, CaseIndex = 0, Checked = false, TestName = "정류기 전원 확인", Parents = null, TestExeUi = new RectOnCheck_UI() };

                TestItemUint acCal = new TestItemUint() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = cal, TestName = CalAcVM.TestName, TestExeUi = new Cal_AC_입력전압_UI() };
                TestItemUint voltCal = new TestItemUint() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = cal, TestName = CalDcVoltVM.TestName, TestExeUi = new Cal_DC_출력전압_UI() };
                TestItemUint currCal = new TestItemUint() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = cal, TestName = CalDcCurrVM.TestName, TestExeUi = new Cal_DC_출력전류_UI() };

                TestItemUint led = new TestItemUint() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = null, TestName = LedCheckVM.TestName, TestExeUi = new LedCheck_UI() };
                TestItemUint id = new TestItemUint() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = null, TestName = IdChangeVM.TestName, TestExeUi = new IdChange_UI() };
                TestItemUint local = new TestItemUint() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = null, TestName = LocalSwitchVM.TestName, TestExeUi = new LocalSwitch_UI() };
                TestItemUint remote = new TestItemUint() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = null, TestName = RemoteCommVM.TestName, TestExeUi = new RemoteComm_UI() };
                TestItemUint bat = new TestItemUint() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = null, TestName = BatteryCommVM.TestName, TestExeUi = new BatteryComm_UI() };

                TestItemUint temp = new TestItemUint() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M200, TestName = TempVM.TestName, TestExeUi = new 온도센서_점검_UI() };
                TestItemUint noload = new TestItemUint() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M200, TestName = NoLoadVM.TestName, TestExeUi = new 무부하_전원_ON_UI() };

                TestItemUint outLow = new TestItemUint() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M200, TestName = OutputLowVM.TestName, TestExeUi = new 출력_저전압_보호_UI() };
                TestItemUint outHigh = new TestItemUint() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M200, TestName = OutputHighVM.TestName, TestExeUi = new 출력_고전압_보호_UI() };

                TestItemUint loadReg1 = new TestItemUint() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M200, TestName = LoadRegVM.TestName + " 1", TestExeUi = new 로드_레귤레이션1_UI() };
                TestItemUint loadReg2 = new TestItemUint() { TestIndex = index++, CaseIndex = 1, Checked = false, Parents = M200, TestName = LoadRegVM.TestName + " 2", TestExeUi = new 로드_레귤레이션2_UI() };
                TestItemUint loadReg3 = new TestItemUint() { TestIndex = index++, CaseIndex = 2, Checked = false, Parents = M200, TestName = LoadRegVM.TestName + " 3", TestExeUi = new 로드_레귤레이션3_UI() };
                TestItemUint lineReg1 = new TestItemUint() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M100, TestName = LineRegVM.TestName + " 1", TestExeUi = new 라인_레귤레이션1_UI() };
                TestItemUint lineReg2 = new TestItemUint() { TestIndex = index++, CaseIndex = 1, Checked = false, Parents = M200, TestName = LineRegVM.TestName + " 2", TestExeUi = new 라인_레귤레이션2_UI() };
                TestItemUint lineReg3 = new TestItemUint() { TestIndex = index++, CaseIndex = 2, Checked = false, Parents = M200, TestName = LineRegVM.TestName + " 3", TestExeUi = new 라인_레귤레이션3_UI() };
                //TestItemUint test18 = new TestItemUint() { TestIndex = index++, CaseIndex = 3, Checked = false, TestName = "라인 레귤레이션 4", Parents = M200, TestExeUi = new 라인_레귤레이션4_UI() };

                TestItemUint noise = new TestItemUint() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M200, TestName = NoiseVM.TestName, TestExeUi = new 리플_노이즈_UI() };
                TestItemUint pf = new TestItemUint() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M200, TestName = PowerFactorVM.TestName, TestExeUi = new 역률_UI() };
                TestItemUint effic = new TestItemUint() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M200, TestName = EfficiencyVM.TestName,  TestExeUi = new 효율_UI() };

                TestItemUint acLow1 = new TestItemUint() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M100, TestName = AcLowVM.TestName + " 1", TestExeUi = new AC_저전압_알람1_UI() };
                TestItemUint acLow2 = new TestItemUint() { TestIndex = index++, CaseIndex = 1, Checked = false, Parents = M200, TestName = AcLowVM.TestName + " 2", TestExeUi = new AC_저전압_알람2_UI() };

                TestItemUint acHigh = new TestItemUint() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M200, TestName = AcHighVM.TestName, TestExeUi = new AC_고전압_알람_UI() };
                TestItemUint acOut = new TestItemUint() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M100, TestName = AcBlackOutVM.TestName, TestExeUi = new AC_정전전압_인식_UI() };

                TestItemUint outOver1 = new TestItemUint() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M100, TestName = OutputOverVM.TestName + " 1", TestExeUi = new 출력_과부하_보호1_UI() };
                TestItemUint outOver2 = new TestItemUint() { TestIndex = index++, CaseIndex = 1, Checked = false, Parents = M200, TestName = OutputOverVM.TestName + " 2", TestExeUi = new 출력_과부하_보호2_UI() };

                TestItemUint rtc = new TestItemUint() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = M200, TestName = RtcCheckVM.TestName, TestExeUi = new RTC_TIME_체크_UI() };


                cal.Child.Add(acCal);    // 입력 CAL
                cal.Child.Add(voltCal);    // 출력 전압 CAL
                cal.Child.Add(currCal);    // 출력 전류 CAL

                M200.Child.Add(temp);
                M200.Child.Add(noload);
                M200.Child.Add(outLow);
                M200.Child.Add(outHigh);
                M200.Child.Add(loadReg1);
                M200.Child.Add(loadReg2);
                M200.Child.Add(loadReg3);
                M200.Child.Add(noise);
                M200.Child.Add(pf);
                M200.Child.Add(lineReg2);
                M200.Child.Add(lineReg3);
                M200.Child.Add(acLow2);
                M200.Child.Add(acHigh);
                M200.Child.Add(outOver2);
                M200.Child.Add(effic);
                M200.Child.Add(rtc);

                M100.Child.Add(lineReg1);
                M100.Child.Add(acLow1);
                M100.Child.Add(acOut);
                M100.Child.Add(outOver1);

                TestTree.Add(init); // 초기세팅
                TestTree.Add(led); // LED
                TestTree.Add(id); // ID 변경
                TestTree.Add(local); // Local Switch
                TestTree.Add(remote); // 리모트 통신
                TestTree.Add(bat); // 배터리 통신
                TestTree.Add(cal);   // CAL
                TestTree.Add(M100);  // 100V Mode
                TestTree.Add(M200);  // 200V Mode
            }
            else
            {
                TestItemUint test0 = new TestItemUint() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = null, TestName = NoLoadVM.TestName,       TestExeUi = new 무부하_전원_ON_UI(new NoLoad2VM()) };
                TestItemUint test1 = new TestItemUint() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = null, TestName = DcOutCheckVM.TestName,   TestExeUi = new DcOutCheck_UI() };
                TestItemUint test2 = new TestItemUint() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = null, TestName = LedCheck2VM.TestName,    TestExeUi = new LedCheck2_UI(new LedCheck2VM()) };
                TestItemUint test3 = new TestItemUint() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = null, TestName = RemoteCommVM.TestName,   TestExeUi = new RemoteComm_UI(new RemoteComm2VM()) };
                TestItemUint test4 = new TestItemUint() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = null, TestName = BatteryCommVM.TestName,  TestExeUi = new BatteryComm_UI(new BatteryComm2VM()) };
                TestItemUint test5 = new TestItemUint() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = null, TestName = LocalSwitch2VM.TestName, TestExeUi = new LocalSwitch_UI(new LocalSwitch2VM()) };
                TestItemUint test6 = new TestItemUint() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = null, TestName = ConnecterVM.TestName,    TestExeUi = new ConnecterCheck_UI() };
                TestItemUint test7 = new TestItemUint() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = null, TestName = PowerFactorVM.TestName,  TestExeUi = new 역률_UI(new PowerFactor2VM()) };
                TestItemUint test8 = new TestItemUint() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = null, TestName = NoiseVM.TestName,        TestExeUi = new 리플_노이즈_UI(new Noise2VM()) };
                TestItemUint test9 = new TestItemUint() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = null, TestName = RtcCheck2VM.TestName,    TestExeUi = new RTC_TIME_체크2_UI() };
                TestItemUint test10 = new TestItemUint() { TestIndex = index++, CaseIndex = 0, Checked = false, Parents = null, TestName = SerialSaveVM.TestName,  TestExeUi = new SerialSave_UI() };

                TestTree.Add(test0);
                TestTree.Add(test1);
                TestTree.Add(test2);
                TestTree.Add(test3);
                TestTree.Add(test4);
                TestTree.Add(test5);
                TestTree.Add(test6);
                TestTree.Add(test7);
                TestTree.Add(test8);
                TestTree.Add(test9);
                TestTree.Add(test10);
            }

            return TestTree;
        }

        private List<TestItemUint> MakeList(ObservableCollection<TestItemUint> source)
        {
            List<TestItemUint> tempList = new List<TestItemUint>();

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