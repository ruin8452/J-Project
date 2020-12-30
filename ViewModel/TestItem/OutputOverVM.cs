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
     *  @brief 출력 과부하 보호 테스트 클래스
     *  @details 출력 과부하 보호와 관련된 시퀀스 및 UI관련을 담당하는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    internal class OutputOverVM : AllTestVM
    {
        private enum Seq
        {
            AC_ON,
            DELAY1,
            OVER_LOAD,
            DELAY2,
            LOAD_OFF,
            DELAY3,
            RESULT_CHECK,
            RESULT_SAVE,
            NEXT_TEST_DELAY,
            END_TEST
        }

        public static string TestName { get; } = "출력 과부하 보호";
        public int CaseNum { get; set; }
        public 출력_과부하_보호 OutputOver { get; set; }
        public TestOption Option { get; set; }

        public ObservableCollection<SolidColorBrush> ButtonColor { get; private set; }

        public RelayCommand UnloadPage { get; set; }
        public RelayCommand<int> UnitTestCommand { get; set; }

        public OutputOverVM(int caseNum)
        {
            TestLog = new StringBuilder();
            CaseNum = caseNum;

            TestOrterNum = (int)FirstTestOrder.OverLoad0 + caseNum;
            TotalStepNum = (int)Seq.END_TEST + 1;

            OutputOver = new 출력_과부하_보호();
            OutputOver = (출력_과부하_보호)Test.Load(OutputOver, CaseNum);

            Option = TestOption.GetObj();
            ButtonColor = new ObservableCollection<SolidColorBrush>();

            FirstOrder[TestOrterNum] = new string[] { TestOrterNum.ToString(), TestName + caseNum.ToString(), "판단불가", "NG(불합격)" };

            for (int i = 0; i < TotalStepNum; i++)
                ButtonColor.Add(Brushes.White);

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
            Test.Save(OutputOver, CaseNum);
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
            StateFlag resultState = TestSeq(0, unitIndex, ref jumpNum);
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
                case Seq.AC_ON:
                    TestLog.AppendLine("[ AC ]");

                    if (Option.IsFullAuto)
                    {
                        double acVolt = PowerMeter.GetObj().AcVolt;
                        TestLog.AppendLine($"- AC : {acVolt}");
                        if (Math.Abs(OutputOver.AcVolt - acVolt) <= AC_ERR_RANGE)
                        {
                            result = StateFlag.PASS;
                            break;
                        }

                        result = AcSourceSet(OutputOver.AcVolt, OutputOver.AcCurr, OutputOver.AcFreq);
                        TestLog.AppendLine($"- AC 세팅 결과 : {result}");

                        if (result != StateFlag.PASS)
                        {
                            jumpStepNum = (int)Seq.LOAD_OFF;
                            break;
                        }

                        result = AcSourceOn();
                        TestLog.AppendLine($"- AC 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.LOAD_OFF;
                    }
                    else
                    {
                        TestLog.AppendLine($"- AC 설정 팝업");

                        result = AcCtrlWin(OutputOver.AcVolt, AC_ERR_RANGE, AcCheckMode.NORMAL);
                        TestLog.AppendLine($"- AC 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    break;

                case Seq.DELAY1:
                    TestLog.AppendLine("[ 딜레이1 ]\n");
                    Util.Delay(OutputOver.Delay1);
                    result = StateFlag.PASS;
                    break;

                case Seq.OVER_LOAD:
                    TestLog.AppendLine("[ 과부하 ON ]");

                    double loadVolt = Dmm2.GetObj().DcVolt;
                    TestLog.AppendLine($"- Load : {loadVolt}");
                    if (Math.Abs(OutputOver.OverLoadCurr - loadVolt) <= LOAD_ERR_RANGE)
                    {
                        result = StateFlag.PASS;
                        break;
                    }

                    if (Option.IsFullAuto)
                    {
                        result = LoadCurrSet(OutputOver.OverLoadCurr);
                        TestLog.AppendLine($"- 부하 세팅 결과 : {result}");
                        if (result != StateFlag.PASS)
                        {
                            jumpStepNum = (int)Seq.LOAD_OFF;
                            break;
                        }

                        result = LoadOn();
                        TestLog.AppendLine($"- 부하 전원 결과 : {result}\n");
                        if (result != StateFlag.PASS)
                        {
                            jumpStepNum = (int)Seq.LOAD_OFF;
                            break;
                        }

                        result = OverLoadCheck();
                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.LOAD_OFF;
                    }
                    else
                    {
                        TestLog.AppendLine($"- 부하 설정 팝업");

                        result = LoadCtrlWin(OutputOver.OverLoadCurr, LOAD_ERR_RANGE, LoadCheckMode.OVER_LOAD);
                        TestLog.AppendLine($"- 부하 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    break;

                case Seq.DELAY2:
                    TestLog.AppendLine("[ 딜레이2 ]\n");
                    Util.Delay(OutputOver.Delay2);
                    result = StateFlag.PASS;
                    break;

                case Seq.LOAD_OFF: // Load OFF
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

                case Seq.DELAY3:
                    TestLog.AppendLine("[ 딜레이3 ]\n");
                    Util.Delay(OutputOver.Delay3);
                    result = StateFlag.PASS;
                    break;

                case Seq.RESULT_CHECK: // 결과 판단
                    TestLog.AppendLine("[ 기능 검사 ]");
                    result = PassFailCheckTest(OutputOver.CheckTiming, OutputOver.DcOutVolt, OutputOver.VoltErrRate, ref resultData);
                    TestLog.AppendLine($"- 결과 : {result}\n");
                    break;

                case Seq.RESULT_SAVE: // 결과 저장
                    TestLog.AppendLine("[ 결과 저장 ]");

                    result = ResultDataSave(TestOrterNum, resultData);
                    TestLog.AppendLine($"- 결과 : {result}\n");
                    break;

                case Seq.NEXT_TEST_DELAY:
                    TestLog.AppendLine("[ 다음 테스트 딜레이 ]\n");
                    Util.Delay(OutputOver.NextTestWait);
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
         *  @brief 출력 과부하 검사
         *  @details 정류기에 과부하를 걸었을 경우 과부하 인식을 하는지 검사
         *  
         *  @param 
         *  
         *  @return StateFlag - 수행 결과
         */
        private StateFlag OverLoadCheck()
        {
            TestLog.Append($"- Fault 발생 검사 -> ");

            // 5초 동안 검사
            for (int i = 0; i < 5; i++)
            {
                Util.Delay(1);

                if (Rectifier.GetObj().Flag_DcOverLoad)
                {
                    TestLog.AppendLine($"- Fault 발생");
                    return StateFlag.PASS;
                }
            }
            TestLog.AppendLine($"- Fault 미발생");
            return StateFlag.FAULT_ERROR;
        }

        /**
         *  @brief 전압 복귀 검사
         *  @details 에러 인식 후 복구 시 정상 출력으로 복귀하는지 검사
         *  
         *  @param double timing - 복귀 제한 시간
         *  @param double dcOutVolt - 정상 전압값
         *  @param double errRate - 체크할 범위(기준값 ±범위값)
         *  @param ref (string, string) resultData - 테스트 결과
         *  
         *  @return StateFlag - 수행 결과
         */
        private StateFlag PassFailCheckTest(double timing, double dcOutVolt, double errRate, ref (string, string) resultData)
        {
            Dmm1 dmm = Dmm1.GetObj();
            double voltCheck = double.NaN;

            for (int i = 0; i < timing; i++)
            {
                Util.Delay(1);  // 설정 제한시간 후 체크
                voltCheck = Math.Round(dmm.DcVolt, 3);

                TestLog.AppendLine($"- {i + 1}초 : {voltCheck}");

                if (dcOutVolt - errRate < voltCheck && voltCheck < dcOutVolt + errRate)
                    break;
            }

            TestLog.AppendLine($"- 기준값 : {dcOutVolt}");
            TestLog.AppendLine($"- 측정값 : {voltCheck}");
            TestLog.AppendLine($"- 현재오차 : {Math.Abs(dcOutVolt - voltCheck)}");
            TestLog.AppendLine($"- 허용오차 : {errRate}");

            if (Math.Abs(dcOutVolt - voltCheck) <= errRate)
            {
                TestLog.AppendLine($"- 테스트 합격");
                resultData = ((-voltCheck).ToString(), "OK(합격)");
                return StateFlag.PASS;
            }
            else
            {
                TestLog.AppendLine($"- 테스트 불합격 : 제한 시간 내에 출력 전압 복귀 실패");
                resultData = ((-voltCheck).ToString(), "NG(불합격)");
                return StateFlag.CONDITION_FAIL;
            }
        }
    }
}