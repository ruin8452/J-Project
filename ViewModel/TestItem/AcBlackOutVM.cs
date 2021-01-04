using GalaSoft.MvvmLight.Command;
using J_Project.Equipment;
using J_Project.Manager;
using J_Project.TestMethod;
using J_Project.ViewModel.SubWindow;
using PropertyChanged;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Media;

namespace J_Project.ViewModel.TestItem
{
    /**
     *  @brief AC 정전 테스트 클래스
     *  @details AC 정전과 관련된 시퀀스 및 UI관련을 담당하는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    [ImplementPropertyChanged]
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
        public int CaseNum { get; set; }
        public AC_정전전압_인식 AcBlackOut { get; set; }
        public TestOption Option { get; set; }

        public ObservableCollection<SolidColorBrush> ButtonColor { get; private set; }

        public RelayCommand LoadPage { get; set; }
        public RelayCommand UnloadPage { get; set; }
        public RelayCommand<int> UnitTestCommand { get; set; }

        public AcBlackOutVM(int caseNum)
        {
            CaseNum = caseNum;
            TestLog = new StringBuilder();

            TestOrterNum = (int)FirstTestOrder.AcBlackOut + caseNum;
            TotalStepNum = (int)Seq.END_TEST + 1;

            AcBlackOut = new AC_정전전압_인식();
            Test.Load(AcBlackOut, CaseNum);

            Option = TestOption.GetObj();
            ButtonColor = new ObservableCollection<SolidColorBrush>();

            FirstOrder[TestOrterNum] = new string[] { TestOrterNum.ToString(), TestName + caseNum.ToString(), "판단불가", "NG(불합격)" };

            for (int i = 0; i < TotalStepNum; i++)
                ButtonColor.Add(Brushes.White);

            LoadPage = new RelayCommand(DataLoad);
            UnloadPage = new RelayCommand(DataSave);
            UnitTestCommand = new RelayCommand<int>(UnitTestClick);
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
            Test.Save(AcBlackOut, CaseNum);
        }

        /**
         *  @brief 데이터 저장
         *  @details 해당 테스트의 설정값을 저장한다
         *  
         *  @param
         *  
         *  @return
         */
        private void DataLoad()
        {
            Test.Load(AcBlackOut, CaseNum);
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
         *  @param int unitIndex - 테스트 시퀀스 번호
         *  
         *  @return
         */
        private void UnitTestClick(int unitIndex)
        {
            //string result = Test.EquiConnectCheck();
            //if (result.Length > 0)
            //{
            //    MessageBox.Show($"다음의 장비의 연결이 원할하지 않습니다.\n\n{result}", "장비 연결");
            //    return;
            //}

            int jumpNum = -1;

            TextColorChange(unitIndex, StateFlag.WAIT);
            StateFlag resultState = TestSeq(unitIndex, ref jumpNum);
            TextColorChange(unitIndex, resultState);
        }

        ///////////////////////////////////////////////////////////////////////////////////
        // 시퀀스 관련
        ///////////////////////////////////////////////////////////////////////////////////
        /**
         *  @brief 테스트 시퀀스
         *  @details 해당 테스트의 시퀀스를 담당 및 수행한다
         *  
         *  @param int stepNumber - 실행할 세부 단계 번호
         *  @param ref int jumpStepNum - 점프할 세부 단계
         *  
         *  @return StateFlag - 수행 결과
         */
        public override StateFlag TestSeq(int stepNumber, ref int jumpStepNum)
        {
            StateFlag result = StateFlag.NORMAL_ERR;
            Seq stepName = (Seq)stepNumber;

            switch (stepName)
            {
                case Seq.AC_INIT: // 초기 AC 설정
                    TestLog.AppendLine("[ 초기 AC ]");

                    double acVolt = PowerMeter.GetObj().AcVolt;
                    TestLog.AppendLine($"- AC : {acVolt}");

                    // 값이 오차 이내 인지 검사(오차 이내이면 값을 조정할 필요 없이 바로 다음 단계로)
                    if (Math.Abs(AcBlackOut.AcVoltInit - acVolt) <= AC_ERR_RANGE)
                    {
                        result = StateFlag.PASS;
                        break;
                    }

                    // 자동, 반자동 분기
                    if (Option.IsFullAuto)
                    {
                        result = AcSourceSet(AcBlackOut.AcVoltInit, AcBlackOut.AcCurrInit, AcBlackOut.AcFreqInit);
                        TestLog.AppendLine($"- AC 세팅 결과 : {result}");

                        // AC 설정 실패 시 테스트 실패, 결과 저장 부분으로 점프
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

                        result = AcCtrlWin(AcBlackOut.AcVoltInit, AC_ERR_RANGE, AcCheckMode.NORMAL);
                        TestLog.AppendLine($"- AC 전원 결과 : {result}\n");

                        // AC 설정 실패 시 테스트 실패, 결과 저장 부분으로 점프
                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    break;

                case Seq.DELAY1:
                    TestLog.AppendLine("[ 딜레이1 ]\n");
                    Util.Delay(AcBlackOut.Delay1);
                    result = StateFlag.PASS;
                    break;

                case Seq.LOAD_ON: // 부하 설정
                    TestLog.AppendLine("[ 부하 ON ]");

                    double loadVolt = Dmm2.GetObj().DcVolt;
                    TestLog.AppendLine($"- DC : {loadVolt}");

                    // 값이 오차 이내 인지 검사(오차 이내이면 값을 조정할 필요 없이 바로 다음 단계로)
                    if (Math.Abs(AcBlackOut.LoadCurr - loadVolt) <= LOAD_ERR_RANGE)
                    {
                        result = StateFlag.PASS;
                        break;
                    }

                    if (Option.IsFullAuto)
                    {
                        result = LoadCurrSet(AcBlackOut.LoadCurr);
                        TestLog.AppendLine($"- 부하 세팅 결과 : {result}");

                        // 부하 설정 실패 시 테스트 실패, 결과 저장 부분으로 점프
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

                        result = LoadCtrlWin(AcBlackOut.LoadCurr, LOAD_ERR_RANGE, LoadCheckMode.NORMAL);
                        TestLog.AppendLine($"- 부하 전원 결과 : {result}\n");

                        // 부하 설정 실패 시 테스트 실패, 결과 저장 부분으로 점프
                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    break;

                case Seq.DELAY2:
                    TestLog.AppendLine("[ 딜레이2 ]\n");
                    Util.Delay(AcBlackOut.Delay2);
                    result = StateFlag.PASS;
                    break;

                case Seq.AC_BLACKOUT_CHECK: // AC 강하 설정
                    TestLog.AppendLine("[ AC 강하 ]");

                    if (Option.IsFullAuto)
                    {
                        result = AcBlackOutCheck(AcBlackOut.AcVoltOut, AcBlackOut.AcErrRate, ref resultData);
                        TestLog.AppendLine($"- AC 세팅 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    else
                    {
                        TestLog.AppendLine($"- AC 설정 팝업");

                        result = AcCtrlWin(AcBlackOut.AcVoltOut, AC_ERR_RANGE, AcCheckMode.AC_BLACK_OUT);
                        TestLog.AppendLine($"- AC 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                        {
                            resultData = ("정전 알람 인식 불가", "NG(불합격)");
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                            return result;
                        }
                        resultData = ("OK", "OK(합격)");
                    }
                    break;

                case Seq.DELAY3:
                    TestLog.AppendLine("[ 딜레이3 ]\n");
                    Util.Delay(AcBlackOut.Delay3);
                    result = StateFlag.PASS;
                    break;

                case Seq.AC_RETURN: // AC 복귀 설정
                    TestLog.AppendLine("[ AC 복귀 ]");

                    if (Option.IsFullAuto)
                    {
                        result = AcSourceSet(AcBlackOut.AcVoltReturn);
                        TestLog.AppendLine($"- AC 세팅 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    else
                    {
                        TestLog.AppendLine($"- AC 설정 팝업");

                        result = AcCtrlWin(AcBlackOut.AcVoltReturn, AC_ERR_RANGE, AcCheckMode.NORMAL);
                        TestLog.AppendLine($"- AC 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    break;

                case Seq.RESULT_CHECK: // 결과 판단
                    TestLog.AppendLine("[ 기능 검사 ]");
                    result = VoltCheckTest(AcBlackOut.CheckTiming, AcBlackOut.LimitMaxVolt, AcBlackOut.LimitMinVolt, ref resultData);
                    TestLog.AppendLine($"- 결과 : {result}\n");
                    break;

                case Seq.RESULT_SAVE: // 결과 저장
                    TestLog.AppendLine("[ 결과 저장 ]");
                    result = ResultDataSave(TestOrterNum, resultData);
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
                    Util.Delay(AcBlackOut.NextTestWait);
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
         *  @brief AC 정전 인식 검사
         *  @details AC 정전 인식을 정상적으로 수행하는지 판단한다
         *  
         *  @param double acBlackOut - 정전으로 인식하는 AC 기준값
         *  @param double errRate - 체크할 범위(기준값 ±범위값)
         *  @param ref (string, string) resultData - 테스트 결과
         *  
         *  @return StateFlag - 수행 결과
         */
        private StateFlag AcBlackOutCheck(double acBlackOut, double errRate, ref (string, string) resultData)
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
                    resultData = ("OK", "OK(합격)");
                    return StateFlag.PASS;
                }

                Util.Delay(1);
            }

            resultData = ("정전 알람 인식 불가", "NG(불합격)");
            TestLog.AppendLine($"- 테스트 불합격 : 검사 범위 내 AC 정전 인식 실패");
            return StateFlag.CONDITION_FAIL;
        }

        /**
         *  @brief 전압 복귀 검사
         *  @details 에러 인식 후 복구 시 정상 출력으로 복귀하는지 검사
         *  
         *  @param double timing - 복귀 제한 시간
         *  @param double maxVolt - 정상 전압 최대값
         *  @param double minVolt - 정상 전압 최소값
         *  @param ref (string, string) resultData - 테스트 결과
         *  
         *  @return StateFlag - 수행 결과
         */
        private StateFlag VoltCheckTest(double timing, double maxVolt, double minVolt, ref (string, string) resultData)
        {
            Dmm1 dmm = Dmm1.GetObj();
            double voltCheck = double.NaN;

            for (int i = 1; i <= timing; i++)
            {
                Util.Delay(1);  // 설정 제한시간 후 체크
                voltCheck = Math.Round(dmm.DcVolt, 3);

                TestLog.AppendLine($"- {timing}초 : {voltCheck}");

                if (voltCheck >= minVolt && voltCheck <= maxVolt)
                    break;
            }

            TestLog.AppendLine($"- 최소 전압 : {minVolt}");
            TestLog.AppendLine($"- 최대 전압 : {maxVolt}");
            TestLog.AppendLine($"- 측정 전압 : {voltCheck}");

            // 테스트 조건 검사
            if (minVolt <= voltCheck && voltCheck <= maxVolt)
            {
                TestLog.AppendLine($"- 테스트 합격");
                return StateFlag.PASS;
            }
            else
            {
                TestLog.AppendLine($"- 테스트 불합격 : 제한 시간 내에 출력 전압 복귀 실패");
                resultData = ("출력 전압 오류", "NG(불합격)");
                return StateFlag.CONDITION_FAIL;
            }
        }
    }
}