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
    internal class AcBlackOutVM : AllTestVM
    {
        private enum Seq
        {
            AC_INIT,
            DELAY1,
            LOAD_ON,
            DELAY2,
            AC_BLACKOUT_CHECK,
            DELAY3,
            AC_RETURN,
            RESULT_CHECK,
            RESULT_SAVE,
            LOAD_OFF,
            NEXT_TEST_DELAY,
            END_TEST
        }

        public static string TestName { get; } = "AC 정전전압 인식";
        public AC_정전전압_인식 AcBlackOut { get; set; }
        public TestOption Option { get; set; }

        public ObservableCollection<SolidColorBrush> ButtonColor { get; private set; }

        public ICommand UnloadPage { get; set; }
        public ICommand UnitTestCommand { get; set; }

        public AcBlackOutVM()
        {
            TestLog = new StringBuilder();

            TotalStepNum = (int)Seq.END_TEST + 1;

            AC_정전전압_인식.Load();
            AcBlackOut = AC_정전전압_인식.GetObj();
            Option = TestOption.GetObj();
            ButtonColor = new ObservableCollection<SolidColorBrush>();

            for (int i = 0; i < TotalStepNum; i++)
                ButtonColor.Add(Brushes.White);

            UnloadPage = new BaseCommand(DataSave);
            UnitTestCommand = new BaseObjCommand(UnitTestClick);
        }

        private void DataSave()
        {
            AC_정전전압_인식.Save();
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
                case Seq.AC_INIT: // 초기 AC 설정
                    TestLog.AppendLine("[ 초기 AC ]");

                    double acVolt = PowerMeter.GetObj().AcVolt;
                    TestLog.AppendLine($"- AC : {acVolt}");
                    if (Math.Abs(AcBlackOut.AcVoltInit[caseNumber] - acVolt) <= AC_ERR_RANGE)
                    {
                        result = StateFlag.PASS;
                        break;
                    }

                    // 자동, 반자동 분기
                    if (Option.IsFullAuto)
                    {
                        result = AcSourceSet(AcBlackOut.AcVoltInit[caseNumber], AcBlackOut.AcCurrInit[caseNumber], AcBlackOut.AcFreqInit[caseNumber]);
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

                        result = AcCtrlWin(AcBlackOut.AcVoltInit[caseNumber], AC_ERR_RANGE, AcCheckMode.NORMAL);
                        TestLog.AppendLine($"- AC 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    break;

                case Seq.DELAY1:
                    TestLog.AppendLine("[ 딜레이1 ]\n");
                    Util.Delay(AcBlackOut.Delay1[caseNumber]);
                    result = StateFlag.PASS;
                    break;

                case Seq.LOAD_ON: // 부하 설정
                    TestLog.AppendLine("[ 부하 ON ]");

                    double loadVolt = Dmm2.GetObj().DcVolt;
                    TestLog.AppendLine($"- DC : {loadVolt}");
                    if (Math.Abs(AcBlackOut.LoadCurr[caseNumber] - loadVolt) <= LOAD_ERR_RANGE)
                    {
                        result = StateFlag.PASS;
                        break;
                    }

                    if (Option.IsFullAuto)
                    {
                        result = LoadCurrSet(AcBlackOut.LoadCurr[caseNumber]);
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

                        result = LoadCtrlWin(AcBlackOut.LoadCurr[caseNumber], LOAD_ERR_RANGE, LoadCheckMode.NORMAL);
                        TestLog.AppendLine($"- 부하 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    break;

                case Seq.DELAY2:
                    TestLog.AppendLine("[ 딜레이2 ]\n");
                    Util.Delay(AcBlackOut.Delay2[caseNumber]);
                    result = StateFlag.PASS;
                    break;

                case Seq.AC_BLACKOUT_CHECK: // AC 강하 설정
                    TestLog.AppendLine("[ AC 강하 ]");

                    if (Option.IsFullAuto)
                    {
                        result = AcBlackOutCheck(caseNumber, AcBlackOut.AcVoltOut[caseNumber], AcBlackOut.AcErrRate[caseNumber], ref resultData);
                        TestLog.AppendLine($"- AC 세팅 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    else
                    {
                        TestLog.AppendLine($"- AC 설정 팝업");

                        result = AcCtrlWin(AcBlackOut.AcVoltOut[caseNumber], AC_ERR_RANGE, AcCheckMode.AC_BLACK_OUT);
                        TestLog.AppendLine($"- AC 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                        {
                            resultData = ("정전 알람 인식 불가", "불합격");
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                            return result;
                        }
                        resultData = ("OK", "합격");
                    }
                    break;

                case Seq.DELAY3:
                    TestLog.AppendLine("[ 딜레이3 ]\n");
                    Util.Delay(AcBlackOut.Delay3[caseNumber]);
                    result = StateFlag.PASS;
                    break;

                case Seq.AC_RETURN: // AC 복귀 설정
                    TestLog.AppendLine("[ AC 복귀 ]");

                    if (Option.IsFullAuto)
                    {
                        result = AcSourceSet(AcBlackOut.AcVoltReturn[caseNumber]);
                        TestLog.AppendLine($"- AC 세팅 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    else
                    {
                        TestLog.AppendLine($"- AC 설정 팝업");

                        result = AcCtrlWin(AcBlackOut.AcVoltReturn[caseNumber], AC_ERR_RANGE, AcCheckMode.NORMAL);
                        TestLog.AppendLine($"- AC 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    break;

                case Seq.RESULT_CHECK: // 결과 판단
                    TestLog.AppendLine("[ 기능 검사 ]");
                    result = VoltCheckTest(caseNumber, AcBlackOut.CheckTiming[caseNumber], AcBlackOut.LimitMaxVolt[caseNumber], AcBlackOut.LimitMinVolt[caseNumber], ref resultData);
                    TestLog.AppendLine($"- 결과 : {result}\n");
                    break;

                case Seq.RESULT_SAVE: // 결과 저장
                    TestLog.AppendLine("[ 결과 저장 ]");
                    result = ResultDataSave((int)FirstTestOrder.AcBlackOut, TestName, resultData);
                    TestLog.AppendLine($"- 결과 : {result}\n");
                    break;

                case Seq.LOAD_OFF: // Load OFF
                    TestLog.AppendLine("[ 부하 OFF ]");

                    if (!TestOption.GetObj().IsLoadManage)
                    {
                        TestLog.AppendLine("- 부하 관리 OFF");
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

                        result = LoadCtrlWin(0, LOAD_ERR_RANGE, LoadCheckMode.NORMAL);
                        TestLog.AppendLine($"- 부하 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    break;

                case Seq.NEXT_TEST_DELAY:
                    TestLog.AppendLine("[ 다음 테스트 딜레이 ]\n");
                    Util.Delay(AcBlackOut.NextTestWait[caseNumber]);
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

        // AC 정진 인식 검사
        private StateFlag AcBlackOutCheck(int caseNum, double acBlackOut, double errRate, ref (string, string) resultData)
        {
            PowerMeter pm = PowerMeter.GetObj();
            double minAc = acBlackOut - errRate;
            double maxAc = acBlackOut + errRate;

            TestLog.AppendLine($"- 검사 범위 : {minAc} ~ {maxAc}");

            for (double inputAc = maxAc; inputAc >= minAc; inputAc--)
            {
                // AC 단계별 상승
                StateFlag result = AcSourceSet(inputAc);
                TestLog.AppendLine($"- AC 정전 : {pm.AcVolt} / {result}");
                if (result != StateFlag.PASS)
                {
                    TestLog.AppendLine($"- AC 설정 실패");
                    return StateFlag.AC_OUTPUT_ERR;
                }

                // Fault 발생 확인
                bool faultFlag = Rectifier.GetObj().Flag_AcRelayOnOff;
                if (faultFlag == false)
                {
                    TestLog.AppendLine($"- AC 정전 인식 : {pm.AcVolt}");
                    resultData = ("OK", "합격");
                    return StateFlag.PASS;
                }

                Util.Delay(1);
            }

            resultData = ("정전 알람 인식 불가", "불합격");
            TestLog.AppendLine($"- 테스트 불합격 : 검사 범위 내 AC 정전 인식 실패");
            return StateFlag.CONDITION_FAIL;
        }

        // 전압 체크
        private StateFlag VoltCheckTest(int caseNum, double timing, double maxVolt, double minVolt, ref (string, string) resultData)
        {
            Dmm1 dmm = Dmm1.GetObj();
            double voltCheck = double.NaN;

            for (int i = 0; i < timing; i++)
            {
                Util.Delay(1);  // 설정 제한시간 후 체크
                voltCheck = Math.Round(dmm.DcVolt, 3);

                TestLog.AppendLine($"- {timing + 1}초 : {voltCheck}");

                if (voltCheck >= minVolt && voltCheck <= maxVolt)
                    break;
            }

            TestLog.AppendLine($"- 최소 전압 : {minVolt}");
            TestLog.AppendLine($"- 최대 전압 : {maxVolt}");
            TestLog.AppendLine($"- 측정 전압 : {voltCheck}");

            if (minVolt <= voltCheck && voltCheck <= maxVolt)
            {
                TestLog.AppendLine($"- 테스트 합격");
                return StateFlag.PASS;
            }
            else
            {
                TestLog.AppendLine($"- 테스트 불합격 : 제한 시간 내에 출력 전압 복귀 실패");
                resultData = ("출력 전압 오류", "불합격");
                return StateFlag.CONDITION_FAIL;
            }
        }
    }
}