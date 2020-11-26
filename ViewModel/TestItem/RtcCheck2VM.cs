using J_Project.Equipment;
using J_Project.Manager;
using J_Project.TestMethod;
using J_Project.ViewModel.CommandClass;
using J_Project.ViewModel.SubWindow;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;

namespace J_Project.ViewModel.TestItem
{
    // 출하용 시퀀스
    internal class RtcCheck2VM : AllTestVM
    {
        private enum Seq
        {
            AC_ON,
            DELAY1,
            RTC_SET,
            LOAD_ON,
            DELAY2,
            RTC_CHECK2,
            RESULT_SAVE,
            LOAD_OFF,
            NEXT_TEST_DELAY,
            END_TEST
        }

        public static string TestName { get; } = "RTC TIME 체크";
        public RTC_TIME_체크 RtcCheck { get; set; }
        public TestOption Option { get; set; }

        public ObservableCollection<SolidColorBrush> ButtonColor { get; private set; }

        public ICommand UnloadPage { get; set; }
        public ICommand UnitTestCommand { get; set; }

        public RtcCheck2VM()
        {
            TestLog = new StringBuilder();

            TotalStepNum = (int)Seq.END_TEST + 1;

            RTC_TIME_체크.Load();
            RtcCheck = RTC_TIME_체크.GetObj();
            Option = TestOption.GetObj();
            ButtonColor = new ObservableCollection<SolidColorBrush>();

            for (int i = 0; i < TotalStepNum; i++)
                ButtonColor.Add(Brushes.White);

            UnloadPage = new BaseCommand(DataSave);
            UnitTestCommand = new BaseObjCommand(UnitTestClick);
        }

        private void DataSave()
        {
            RTC_TIME_체크.Save();
        }

        // 버튼 글자 색 변경 함수
        public override void TextColorChange(int index, StateFlag stateFlag)
        {
            if (stateFlag == StateFlag.PASS || stateFlag == StateFlag.TEST_END)
                ButtonColor[index] = Brushes.GreenYellow;
            else if (stateFlag == StateFlag.WAIT)
                ButtonColor[index] = Brushes.Yellow;
            else
                ButtonColor[index] = Brushes.Red;
        }

        public override void UiReset()
        {
            for (int i = 0; i < ButtonColor.Count; i++)
                ButtonColor[i] = Brushes.White;
        }

        // 수동 테스트 동작 이벤트 함수(버튼 클릭)
        private void UnitTestClick(object value)
        {
            object[] parameter = (object[])value;

            //string result = Test.EquiConnectCheck();
            //if (result.Length > 0)
            //{
            //    MessageBox.Show($"다음의 장비의 연결이 원할하지 않습니다.\n\n{result}", "장비 연결");
            //    return;
            //}

            int caseIndex = int.Parse(parameter[0].ToString());
            int unitIndex = int.Parse(parameter[1].ToString());
            int jumpNum = -1;

            TextColorChange(unitIndex, StateFlag.WAIT);
            StateFlag resultState = TestSeq(caseIndex, unitIndex, ref jumpNum);
            TextColorChange(unitIndex, resultState);
        }

        ///////////////////////////////////////////////////////////////////////////////////
        // 시퀀스 관련
        ///////////////////////////////////////////////////////////////////////////////////
        public override StateFlag TestSeq(int caseNumber, int stepNumber, ref int jumpStepNum)
        {
            StateFlag result = StateFlag.NORMAL_ERR;
            Seq stepName = (Seq)stepNumber;

            switch (stepName)
            {
                case Seq.AC_ON: // AC 설정 및 ON
                    TestLog.AppendLine("[ AC ]");

                    double acVolt = PowerMeter.GetObj().AcVolt;
                    TestLog.AppendLine($"- AC : {acVolt}");
                    if (Math.Abs(RtcCheck.AcVolt2[caseNumber] - acVolt) <= AC_ERR_RANGE)
                    {
                        result = StateFlag.PASS;
                        break;
                    }

                    if (Option.IsFullAuto)
                    {
                        result = AcSourceSet(RtcCheck.AcVolt2[caseNumber], RtcCheck.AcCurr[caseNumber], RtcCheck.AcFreq[caseNumber]);
                        TestLog.AppendLine($"- AC 세팅 결과 : {result}");

                        if (result != StateFlag.PASS)
                        {
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                            break;
                        }

                        result = AcSourceOn();
                        TestLog.AppendLine($"- AC 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    else
                    {
                        TestLog.AppendLine($"- AC 설정 팝업");

                        result = AcCtrlWin(RtcCheck.AcVolt[caseNumber], AC_ERR_RANGE, AcCheckMode.NORMAL);
                        TestLog.AppendLine($"- AC 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    break;

                case Seq.DELAY1:
                    TestLog.AppendLine("[ 딜레이1 ]\n");
                    Util.Delay(RtcCheck.Delay1[caseNumber]);
                    result = StateFlag.PASS;
                    break;

                case Seq.RTC_SET:
                    TestLog.AppendLine("[ RTC 설정 ]");
                    result = RtcSet();
                    TestLog.AppendLine($"- 결과 : {result}\n");

                    if (result != StateFlag.PASS)
                        jumpStepNum = (int)Seq.RESULT_SAVE;
                    break;

                case Seq.LOAD_ON: // 부하 설정
                    TestLog.AppendLine("[ 부하 ON ]");

                    // 팝업 전 확인 후 통과
                    double LoadCurr = Rectifier.GetObj().DcOutputCurr;
                    TestLog.AppendLine($"- 부하 : {LoadCurr}");
                    if (Math.Abs(RtcCheck.LoadCurr[caseNumber] - LoadCurr) <= LOAD_ERR_RANGE)
                    {
                        result = StateFlag.PASS;
                        break;
                    }

                    if (Option.IsFullAuto)
                    {
                        result = LoadCurrSet(RtcCheck.LoadCurr2[caseNumber]);
                        TestLog.AppendLine($"- 부하 세팅 결과 : {result}");

                        if (result != StateFlag.PASS)
                        {
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                            break;
                        }

                        result = LoadOn();
                        TestLog.AppendLine($"- 부하 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    else
                    {
                        TestLog.AppendLine($"- 부하 설정 팝업");

                        result = LoadCtrlWin2(RtcCheck.LoadCurr[caseNumber], LOAD_ERR_RANGE, LoadCheckMode.SECOND_TEST);
                        TestLog.AppendLine($"- 부하 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    break;

                case Seq.DELAY2:
                    TestLog.AppendLine("[ 딜레이2 ]\n");
                    Util.Delay(RtcCheck.Delay2[caseNumber]);
                    result = StateFlag.PASS;
                    break;

                case Seq.RTC_CHECK2: // RTC 검사
                    TestLog.AppendLine("[ RTC 검사 ]");
                    result = RtcCheckTest(RtcCheck.TimeErrRate2[caseNumber]);
                    TestLog.AppendLine($"- 결과 : {result}\n");
                    break;

                case Seq.RESULT_SAVE:
                    TestLog.AppendLine("[ 결과 저장 ]");
                    result = ResultDataSave((int)SecondTestOrder.Rtc, TestName, resultData);
                    TestLog.AppendLine($"- 결과 : {result}\n");
                    break;

                case Seq.LOAD_OFF:
                    TestLog.AppendLine("[ 부하 OFF ]");

                    // 팝업 전 확인 후 통과
                    LoadCurr = Rectifier.GetObj().DcOutputCurr;
                    TestLog.AppendLine($"- 부하 : {LoadCurr}");
                    if (Math.Abs(0 - LoadCurr) <= LOAD_ERR_RANGE)
                    {
                        result = StateFlag.PASS;
                        break;
                    }

                    if (Option.IsFullAuto)
                    {
                        result = LoadOff();
                        TestLog.AppendLine($"- 부하 전원 결과 : {result}\n");
                    }
                    else
                    {
                        TestLog.AppendLine($"- 부하 설정 팝업");

                        result = LoadCtrlWin2(0, LOAD_ERR_RANGE, LoadCheckMode.SECOND_TEST);
                        TestLog.AppendLine($"- 부하 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    break;

                case Seq.NEXT_TEST_DELAY:
                    TestLog.AppendLine("[ 다음 테스트 딜레이 ]\n");
                    Util.Delay(RtcCheck.NextTestWait[caseNumber]);
                    result = StateFlag.PASS;
                    break;

                case Seq.END_TEST:
                    TestLog.AppendLine("[ 테스트 완료 ]\n");
                    result = StateFlag.TEST_END;
                    break;

                default:
                    break;
            }

            return result;
        }

        // RTC 설정
        private StateFlag RtcSet()
        {
            bool cmdResult;

            for (int i = 0; i < MAX_TRY_COUNT; i++)
            {
                TestLog.Append($"- RTC 설정 {i}회차 시도 -> ");

                ushort data1 = (ushort)(((DateTime.Now.Year - 2000) << 8) + DateTime.Now.Month);
                ushort data2 = (ushort)(((DateTime.Now.Day) << 8) + DateTime.Now.Hour);
                ushort data3 = (ushort)((DateTime.Now.Minute << 8) + DateTime.Now.Second);

                cmdResult = Rectifier.GetObj().RectCommand(CommandList.RTC_SET, data1, data2, data3);
                if (cmdResult)
                {
                    TestLog.AppendLine($"성공");
                    return StateFlag.PASS;
                }
                TestLog.AppendLine($"실패");
            }

            TestLog.AppendLine($"RTC 설정 실패");
            return StateFlag.RTC_ERR;
        }

        // RTC 체크
        private StateFlag RtcCheckTest(double timeErrRate)
        {
            for (int i = 0; i < MAX_TRY_COUNT; i++)
            {
                TestLog.AppendLine($"- {i}회차 시도");

                DateTime pcTime = DateTime.Now;
                DateTime rectTime = Rectifier.GetObj().RectTime;
                double errRateDb = Math.Abs((pcTime - rectTime).TotalSeconds);

                TestLog.AppendLine($"- PC 시간 : {pcTime}");
                TestLog.AppendLine($"- 정류기 시간 : {rectTime}");
                TestLog.AppendLine($"- 현재오차 : {errRateDb}");
                TestLog.AppendLine($"- 허용오차 : {timeErrRate}");

                if (errRateDb < timeErrRate)
                {
                    TestLog.AppendLine($"- 테스트 합격");
                    resultData = ("OK", "합격");
                    return StateFlag.PASS;
                }
            }

            TestLog.AppendLine($"- 테스트 불합격");
            resultData = ("허용오차 초과", "불합격");
            return StateFlag.CONDITION_FAIL;
        }
    }
}