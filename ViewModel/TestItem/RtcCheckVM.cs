using GalaSoft.MvvmLight.Command;
using J_Project.Equipment;
using J_Project.Manager;
using J_Project.TestMethod;
using J_Project.ViewModel.SubWindow;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Media;

namespace J_Project.ViewModel.TestItem
{
    /**
     *  @brief RTC TIME 체크 테스트 클래스(양산용)
     *  @details RTC TIME 체크와 관련된 시퀀스 및 UI관련을 담당하는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    internal class RtcCheckVM : AllTestVM
    {
        private enum Seq
        {
            AC_ON,
            DELAY1,
            RTC_CHECK,
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
        public int CaseNum { get; set; }
        public RTC_TIME_체크 RtcCheck { get; set; }
        public TestOption Option { get; set; }

        public ObservableCollection<SolidColorBrush> ButtonColor { get; private set; }

        public RelayCommand UnloadPage { get; set; }
        public RelayCommand<object> UnitTestCommand { get; set; }

        public RtcCheckVM(int caseNum)
        {
            CaseNum = caseNum;
            TestLog = new StringBuilder();

            TestOrterNum = (int)FirstTestOrder.Rtc + caseNum;
            TotalStepNum = (int)Seq.END_TEST + 1;

            RtcCheck = new RTC_TIME_체크();
            RtcCheck = (RTC_TIME_체크)Test.Load(RtcCheck, CaseNum);

            Option = TestOption.GetObj();
            ButtonColor = new ObservableCollection<SolidColorBrush>();

            FirstOrder[TestOrterNum] = new string[] { TestOrterNum.ToString(), TestName + caseNum.ToString(), "판단불가", "NG(불합격)" };

            for (int i = 0; i < TotalStepNum; i++)
                ButtonColor.Add(Brushes.White);

            UnloadPage = new RelayCommand(DataSave);
            UnitTestCommand = new RelayCommand<object>(UnitTestClick);
        }

        /**
         *  @brief 데이터 저장
         *  @details 해당 테스트의 설정값을 저장한다
         *  
         *  @param
         *  
         *  @return
         */
        private void DataSave()
        {
            Test.Save(RtcCheck, CaseNum);
        }

        /**
         *  @brief 테스트 UI 텍스트 색 변경
         *  @details 양상 테스트 화면에서 해당 테스트 UI의 텍스트 색을 변경시킨다
         *  
         *  @param int index - 세부 단계의 인덱스
         *  @param StateFlag stateFlag - 세부 단계의 테스트 결과
         *  
         *  @return
         */
        public override void TextColorChange(int index, StateFlag stateFlag)
        {
            if (stateFlag == StateFlag.PASS || stateFlag == StateFlag.TEST_END)
                ButtonColor[index] = Brushes.GreenYellow;
            else if (stateFlag == StateFlag.WAIT)
                ButtonColor[index] = Brushes.Yellow;
            else
                ButtonColor[index] = Brushes.Red;
        }

        /**
         *  @brief 테스트 UI 텍스트 색 리셋
         *  @details 양상 테스트 화면에서 해당 테스트 UI의 텍스트 색을 흰색으로 리셋
         *  
         *  @param 
         *  
         *  @return
         */
        public override void UiReset()
        {
            for (int i = 0; i < ButtonColor.Count; i++)
                ButtonColor[i] = Brushes.White;
        }

        /**
         *  @brief 수동 테스트 동작
         *  @details 수동 모드 운영 시, 테스트 UI의 활성화된 버튼을 클릭했을 경우 실행
         *  
         *  @param object value - 2개의 데이터로 구성(1. 해당 테스트의 케이스 번호, 2. 해당 세부 단계의 인덱스 번호)
         *  
         *  @return
         */
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
        /**
         *  @brief 테스트 시퀀스
         *  @details 해당 테스트의 시퀀스를 담당 및 수행한다
         *  
         *  @param int caseNumbere - 해당 테스트의 케이스 번호
         *  @param int stepNumber - 실행할 세부 단계 번호
         *  @param ref int jumpStepNum - 점프할 세부 단계
         *  
         *  @return StateFlag - 수행 결과
         */
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
                    if (Math.Abs(RtcCheck.AcVolt - acVolt) <= AC_ERR_RANGE)
                    {
                        result = StateFlag.PASS;
                        break;
                    }

                    if (Option.IsFullAuto)
                    {
                        result = AcSourceSet(RtcCheck.AcVolt, RtcCheck.AcCurr, RtcCheck.AcFreq);
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

                        result = AcCtrlWin(RtcCheck.AcVolt, AC_ERR_RANGE, AcCheckMode.NORMAL);
                        TestLog.AppendLine($"- AC 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    break;

                case Seq.DELAY1:
                    TestLog.AppendLine("[ 딜레이1 ]\n");
                    Util.Delay(RtcCheck.Delay1);
                    result = StateFlag.PASS;
                    break;

                case Seq.RTC_CHECK: // RTC 검사
                    TestLog.AppendLine("[ RTC 검사 ]");
                    result = RtcCheckTest(RtcCheck.TimeErrRate);
                    TestLog.AppendLine($"- 결과 : {result}\n");

                    if (result != StateFlag.PASS)
                        jumpStepNum = (int)Seq.RESULT_SAVE;
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

                    double loadVolt = Dmm2.GetObj().DcVolt;
                    TestLog.AppendLine($"- Load : {loadVolt}");
                    if (Math.Abs(RtcCheck.LoadCurr - loadVolt) <= LOAD_ERR_RANGE)
                    {
                        result = StateFlag.PASS;
                        break;
                    }

                    if (Option.IsFullAuto)
                    {
                        result = LoadCurrSet(RtcCheck.LoadCurr);
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

                        result = LoadCtrlWin(RtcCheck.LoadCurr, LOAD_ERR_RANGE, LoadCheckMode.NORMAL);
                        TestLog.AppendLine($"- 부하 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    break;

                case Seq.DELAY2:
                    TestLog.AppendLine("[ 딜레이2 ]\n");
                    Util.Delay(RtcCheck.Delay2);
                    result = StateFlag.PASS;
                    break;

                case Seq.RTC_CHECK2: // RTC 검사
                    TestLog.AppendLine("[ RTC 검사 ]");
                    result = RtcCheckTest(RtcCheck.TimeErrRate2);
                    TestLog.AppendLine($"- 결과 : {result}\n");
                    break;

                case Seq.RESULT_SAVE:
                    TestLog.AppendLine("[ 결과 저장 ]");
                    result = ResultDataSave(TestOrterNum, resultData);
                    TestLog.AppendLine($"- 결과 : {result}\n");
                    break;

                case Seq.LOAD_OFF:
                    TestLog.AppendLine("[ 부하 OFF ]");

                    if (Option.IsFullAuto)
                    {
                        result = LoadOff();
                        TestLog.AppendLine($"- 부하 전원 결과 : {result}\n");
                    }
                    else
                    {
                        TestLog.AppendLine($"- 부하 설정 팝업");

                        result = LoadCtrlWin(0, LOAD_ERR_RANGE, LoadCheckMode.NORMAL);
                        TestLog.AppendLine($"- 부하 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    break;

                case Seq.NEXT_TEST_DELAY:
                    TestLog.AppendLine("[ 다음 테스트 딜레이 ]\n");
                    Util.Delay(RtcCheck.NextTestWait);
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

        /**
         *  @brief RTC 설정
         *  @details 정류기의 RTC 값을 현재 시간으로 설정한다
         *  
         *  @param 
         *  
         *  @return StateFlag - 수행 결과
         */
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

        /**
         *  @brief RTC 검사
         *  @details 테스트 시작 시 설정한 RTC가 테스트를 진행하면서 허용 오차 범위를 넘었는지 검사한다
         *  
         *  @param double timeErrRate - 허용 시간 오차
         *  
         *  @return StateFlag - 수행 결과
         */
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
                    resultData = ("OK", "OK(합격)");
                    return StateFlag.PASS;
                }
            }

            TestLog.AppendLine($"- 테스트 불합격");
            resultData = ("허용오차 초과", "NG(불합격)");
            return StateFlag.CONDITION_FAIL;
        }
    }
}