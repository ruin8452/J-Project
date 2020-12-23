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
     *  @brief 출력 고전압 보호 테스트 클래스
     *  @details 출력 고전압 보호와 관련된 시퀀스 및 UI관련을 담당하는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    internal class OutputHighVM : AllTestVM
    {
        private enum Seq
        {
            AC_ON,
            DELAY1,
            OUT_HIGH_CHECK,
            DELAY2,
            RESULT_SAVE,
            RECT_RESET,
            NEXT_TEST_DELAY,
            END_TEST
        }

        public static string TestName { get; } = "출력 고전압 보호";
        public int CaseNum { get; set; }
        public 출력_고전압_보호 OutputHigh { get; set; }
        public TestOption Option { get; set; }

        public ObservableCollection<SolidColorBrush> ButtonColor { get; private set; }

        public RelayCommand UnloadPage { get; set; }
        public RelayCommand<object> UnitTestCommand { get; set; }

        public OutputHighVM(int caseNum)
        {
            TestLog = new StringBuilder();
            CaseNum = caseNum;

            TestOrterNum = (int)FirstTestOrder.OutputHigh + caseNum;
            TotalStepNum = (int)Seq.END_TEST + 1;

            OutputHigh = new 출력_고전압_보호();
            OutputHigh = (출력_고전압_보호)Test.Load(OutputHigh, CaseNum);

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
            Test.Save(OutputHigh, CaseNum);
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
                case Seq.AC_ON: // 초기 AC 설정
                    TestLog.AppendLine("[ AC ]");

                    double acVolt = PowerMeter.GetObj().AcVolt;
                    TestLog.AppendLine($"- AC : {acVolt}");
                    if (Math.Abs(OutputHigh.AcVolt - acVolt) <= AC_ERR_RANGE)
                    {
                        result = StateFlag.PASS;
                        break;
                    }

                    if (Option.IsFullAuto)
                    {
                        result = AcSourceSet(OutputHigh.AcVolt, OutputHigh.AcCurr, OutputHigh.AcFreq);
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

                        result = AcCtrlWin(OutputHigh.AcVolt, AC_ERR_RANGE, AcCheckMode.NORMAL);
                        TestLog.AppendLine($"- AC 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    break;

                case Seq.DELAY1:
                    TestLog.AppendLine("[ 딜레이1 ]\n");
                    Util.Delay(OutputHigh.Delay1);
                    result = StateFlag.PASS;
                    break;

                case Seq.OUT_HIGH_CHECK:
                    TestLog.AppendLine("[ 기능 검사 ]");
                    result = RectOutHighCheck(caseNumber, OutputHigh.DcOutVolt, OutputHigh.DcErrRate, ref resultData);
                    TestLog.AppendLine($"- 결과 : {result}\n");
                    if (result != StateFlag.PASS)
                        jumpStepNum = (int)Seq.RECT_RESET;
                    break;

                case Seq.DELAY2:
                    TestLog.AppendLine("[ 딜레이2 ]\n");
                    Util.Delay(OutputHigh.Delay2);
                    result = StateFlag.PASS;
                    break;

                case Seq.RESULT_SAVE: // 결과 저장
                    TestLog.AppendLine("[ 결과 저장 ]");
                    result = ResultDataSave(TestOrterNum, resultData);
                    TestLog.AppendLine($"- 결과 : {result}\n");
                    break;

                case Seq.RECT_RESET: // 정류기 리셋
                    TestLog.AppendLine("[ 정류기 리셋 ]\n");

                    for (int i = 0; i < 3; i++)
                    {
                        TestLog.Append($"- 리셋 시도 {i + 1}회차 -> ");
                        Rectifier.GetObj().RectCommand(CommandList.SW_RESET, 1);
                        Util.Delay(7);

                        if (Rectifier.GetObj().AcInVoltMode == "200V")
                        {
                            TestLog.AppendLine($"성공");
                            return StateFlag.PASS;
                        }
                        TestLog.AppendLine($"실패");
                    }
                    TestLog.AppendLine($"- RECT 리셋 실패\n");
                    result = StateFlag.RECT_RESET_ERR;
                    break;

                case Seq.NEXT_TEST_DELAY:
                    TestLog.AppendLine("[ 다음 테스트 딜레이 ]\n");
                    Util.Delay(OutputHigh.NextTestWait);
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
         *  @brief 출력 고전압 인식 검사
         *  @details 정류기의 출력을 기준치 이상 강제로 올렸을 때, 출력 고전압 인식을 하는지 검사
         *  
         *  @param int caseNum - 해당 테스트의 케이스 번호
         *  @param double outUp - 고전압 인식 기준치
         *  @param double errRate - 체크할 범위(기준값 ±범위값)
         *  @param ref (string, string) resultData - 테스트 결과
         *  
         *  @return StateFlag - 수행 결과
         */
        private StateFlag RectOutHighCheck(int caseNum, double outUp, double errRate, ref (string, string) resultData)
        {
            Rectifier rect = Rectifier.GetObj();
            Dmm1 dmm1 = Dmm1.GetObj();
            double minDc = outUp - errRate;
            double maxDc = outUp + errRate;

            TestLog.AppendLine($"- 검사 범위 : {minDc} ~ {maxDc}");

            //<C>20.SSW 03.25 : 모니터링과 충돌 위험이 있어 해당 검사시 모니터링 중단
            rect.MonitoringStop();
            dmm1.MonitoringStop();

            for (double inputOut = minDc; inputOut <= maxDc; inputOut += 0.1)
            {
                // DC Out 단계별 상승
                rect.RectCommand(CommandList.V_REF_SET, 0, (ushort)(inputOut * 100));

                double dmmVolt = dmm1.RealDcVolt();
                TestLog.AppendLine($"- 출력 상승 : {dmmVolt}");

                rect.RectMonitoring();
                Util.Delay(0.05);

                // Fault 발생 확인
                bool faultFlag = rect.Flag_OverDcOutVolt;
                if (faultFlag == true)
                {
                    dmmVolt += 0.1;
                    TestLog.AppendLine($"- 출력 고전압 인식 : {dmmVolt}");
                    resultData = ("OK", "OK(합격)");

                    dmm1.MonitoringStart();
                    rect.MonitoringStart();

                    return StateFlag.PASS;
                }

                Util.Delay(0.1);
            }
            dmm1.MonitoringStart();
            rect.MonitoringStart();

            TestLog.AppendLine($"- 테스트 불합격 : 검사 범위 내 출력 고전압 인식 실패");
            resultData = ("테스트 실패", "NG(불합격)");

            return StateFlag.OUTPUT_ERR;
        }
    }
}