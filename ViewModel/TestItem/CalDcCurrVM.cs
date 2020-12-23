using GalaSoft.MvvmLight.Command;
using J_Project.Equipment;
using J_Project.Manager;
using J_Project.TestMethod;
using J_Project.ViewModel.SubWindow;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Windows.Media;

namespace J_Project.ViewModel.TestItem
{
    /**
     *  @brief DC 전류 CAL 테스트 클래스
     *  @details DC 전류 CAL과 관련된 시퀀스 및 UI관련을 담당하는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    internal class CalDcCurrVM : AllTestVM
    {
        private enum Seq
        {
            M200_AC_ON,
            UNDER_VOLT_ALARM_OFF,
            DELAY1,
            DAC_LOAD_ON,
            DELAY2,
            DAC_CAL,
            DELAY3,
            DEFAULT_M200_REF_SET,
            DELAY4,
            ADC_LOAD_ON,
            DELAY5,
            UPPER_ADC_CAL,
            DELAY6,
            LOAD_OFF1,
            DELAY7,
            LOWER_ADC_CAL,
            M100_AC_SET,
            RECT_RESET,
            UNDER_VOLT_ALARM_OFF2,
            DELAY8,
            M100_LOAD_ON,
            DELAY9,
            DEFAULT_M100_REF_SET,
            LOAD_OFF2,
            RECT_RESET2,
            NEXT_TEST_DELAY,
            END_TEST
        }

        private enum CalPoint
        {
            LOW_POINT,
            HIGH_POINT
        }

        private enum RefSave
        {
            NO_SAVE,
            SAVE
        }

        private const double ERR_RATE = 1;

        public static string TestName { get; } = "Cal DC 출력전류";
        public int CaseNum { get; set; }
        public Cal_DC_출력전류 DcOutCurrCal { get; set; }
        public TestOption Option { get; set; }

        public ObservableCollection<SolidColorBrush> ButtonColor { get; private set; }

        public RelayCommand UnloadPage { get; set; }
        public RelayCommand<object> UnitTestCommand { get; set; }

        public CalDcCurrVM(int caseNum)
        {
            CaseNum = caseNum;
            TestLog = new StringBuilder();

            TotalStepNum = (int)Seq.END_TEST + 1;

            DcOutCurrCal = new Cal_DC_출력전류();
            DcOutCurrCal = (Cal_DC_출력전류)Test.Load(DcOutCurrCal, CaseNum);

            Option = TestOption.GetObj();
            ButtonColor = new ObservableCollection<SolidColorBrush>();

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
            Test.Save(DcOutCurrCal, CaseNum);
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
                case Seq.M200_AC_ON:
                    TestLog.AppendLine("[ 200V AC ]");

                    // 팝업 전 확인 후 통과
                    double acVolt = PowerMeter.GetObj().AcVolt;
                    TestLog.AppendLine($"- AC : {acVolt}");
                    if (Math.Abs(DcOutCurrCal.M200AcVolt - acVolt) <= AC_ERR_RANGE)
                    {
                        result = StateFlag.PASS;
                        break;
                    }

                    if (Option.IsFullAuto)
                    {
                        result = AcSourceSet(DcOutCurrCal.M200AcVolt, DcOutCurrCal.M200AcCurr, DcOutCurrCal.M200AcFreq);
                        TestLog.AppendLine($"- AC 세팅 결과 : {result}");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.LOAD_OFF2;

                        result = AcSourceOn();
                        TestLog.AppendLine($"- AC 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.LOAD_OFF2;
                    }
                    else
                    {
                        TestLog.AppendLine($"- AC 설정 팝업");

                        result = AcCtrlWin(DcOutCurrCal.M200AcVolt, AC_ERR_RANGE, AcCheckMode.NORMAL);
                        TestLog.AppendLine($"- AC 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.END_TEST;
                    }
                    break;

                case Seq.UNDER_VOLT_ALARM_OFF:
                    TestLog.AppendLine("[ 출력 저전압 알람 제거 ]");

                    for (int i = 0; i < 3; i++)
                    {
                        TestLog.Append($"- 시도 {i}회차 -> ");
                        if (Rectifier.GetObj().RectCommand(CommandList.UNDER_VOLT_ALARM, 0))
                        {
                            TestLog.AppendLine($"성공\n");
                            return StateFlag.PASS;
                        }
                        TestLog.AppendLine($"실패");
                        Util.Delay(2);
                    }
                    result = StateFlag.RECT_CONNECT_ERR;
                    TestLog.AppendLine($"- 알람 설정 실패 : {result}\n");
                    break;

                case Seq.DELAY1:
                    TestLog.AppendLine("[ 딜레이1 ]\n");
                    Util.Delay(DcOutCurrCal.Delay1);
                    result = StateFlag.PASS;
                    break;

                case Seq.DAC_LOAD_ON:
                    TestLog.AppendLine("[ DAC 부하 ]");

                    double loadVolt = Dmm2.GetObj().DcVolt;
                    TestLog.AppendLine($"- DC : {loadVolt}");
                    if (Math.Abs(DcOutCurrCal.DacLoadCurr - loadVolt) <= LOAD_ERR_RANGE)
                    {
                        result = StateFlag.PASS;
                        break;
                    }

                    if (Option.IsFullAuto)
                    {
                        result = LoadCurrSet(DcOutCurrCal.DacLoadCurr);
                        TestLog.AppendLine($"- 부하 세팅 결과 : {result}");
                        if (result != StateFlag.PASS)
                        {
                            jumpStepNum = (int)Seq.LOAD_OFF2;
                            break;
                        }

                        result = LoadOn();
                        TestLog.AppendLine($"- 부하 전원 결과 : {result}\n");
                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.LOAD_OFF2;
                    }
                    else
                    {
                        TestLog.AppendLine($"- 부하 설정 팝업");

                        result = LoadCtrlWin(DcOutCurrCal.DacLoadCurr, LOAD_ERR_RANGE, LoadCheckMode.NORMAL);
                        TestLog.AppendLine($"- 부하 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.LOAD_OFF2;
                    }
                    break;

                case Seq.DELAY2:
                    TestLog.AppendLine("[ 딜레이2 ]\n");
                    Util.Delay(DcOutCurrCal.Delay2);
                    result = StateFlag.PASS;
                    break;

                case Seq.DAC_CAL:
                    TestLog.AppendLine("[ DAC CAL ]");
                    result = DacCal(DcOutCurrCal.DacLowerRef, DcOutCurrCal.DacUpperRef);
                    TestLog.AppendLine($"- 결과 : {result}\n");

                    if (result != StateFlag.PASS)
                        jumpStepNum = (int)Seq.LOAD_OFF2;
                    break;

                case Seq.DELAY3:
                    TestLog.AppendLine("[ 딜레이3 ]\n");
                    Util.Delay(DcOutCurrCal.Delay3);
                    result = StateFlag.PASS;
                    break;

                case Seq.DEFAULT_M200_REF_SET:
                    TestLog.AppendLine("[ 200V 모드 기본값 세팅 ]");
                    result = DefaultRefSet(DcOutCurrCal.DefaultM200Ref);
                    TestLog.AppendLine($"- 결과 : {result}\n");
                    break;

                case Seq.DELAY4:
                    TestLog.AppendLine("[ 딜레이4 ]\n");
                    Util.Delay(DcOutCurrCal.Delay4);
                    result = StateFlag.PASS;
                    break;

                case Seq.ADC_LOAD_ON:
                    TestLog.AppendLine("[ ADC 부하 ]");

                    loadVolt = Dmm2.GetObj().DcVolt;
                    TestLog.AppendLine($"- DC : {loadVolt}");
                    if (Math.Abs(DcOutCurrCal.AdcLoadCurr - loadVolt) <= LOAD_ERR_RANGE)
                    {
                        result = StateFlag.PASS;
                        break;
                    }

                    if (Option.IsFullAuto)
                    {
                        result = LoadCurrSet(DcOutCurrCal.AdcLoadCurr);
                        TestLog.AppendLine($"- 부하 세팅 결과 : {result}");
                        if (result != StateFlag.PASS)
                        {
                            jumpStepNum = (int)Seq.LOAD_OFF2;
                            break;
                        }

                        result = LoadOn();
                        TestLog.AppendLine($"- 부하 전원 결과 : {result}\n");
                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.LOAD_OFF2;
                    }
                    else
                    {
                        TestLog.AppendLine($"- 부하 설정 팝업");

                        result = LoadCtrlWin(DcOutCurrCal.AdcLoadCurr, LOAD_ERR_RANGE, LoadCheckMode.NORMAL);
                        TestLog.AppendLine($"- 부하 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.LOAD_OFF2;
                    }
                    break;

                case Seq.DELAY5:
                    TestLog.AppendLine("[ 딜레이5 ]\n");
                    Util.Delay(DcOutCurrCal.Delay5);
                    result = StateFlag.PASS;
                    break;

                case Seq.UPPER_ADC_CAL:
                    TestLog.AppendLine("[ ADC 상한 CAL ]");
                    result = AdcCalCheck((int)CalPoint.HIGH_POINT, DcOutCurrCal.AdcUpperRef);
                    TestLog.AppendLine($"- CAL 결과 : {result}\n");

                    if (result != StateFlag.PASS)
                        jumpStepNum = (int)Seq.LOAD_OFF2;
                    break;

                case Seq.DELAY6:
                    TestLog.AppendLine("[ 딜레이6 ]\n");
                    Util.Delay(DcOutCurrCal.Delay6);
                    result = StateFlag.PASS;
                    break;

                case Seq.LOAD_OFF1:
                    TestLog.AppendLine("[ 부하 OFF ]");
                    if (Option.IsFullAuto)
                    {
                        result = LoadOff();
                        TestLog.AppendLine($"- 부하 전원 결과 : {result}\n");
                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.LOAD_OFF2;
                    }
                    else
                    {
                        TestLog.AppendLine($"- 부하 설정 팝업");

                        result = LoadCtrlWin(0, LOAD_ERR_RANGE, LoadCheckMode.NORMAL);
                        TestLog.AppendLine($"- 부하 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.LOAD_OFF2;
                    }
                    break;

                case Seq.DELAY7:
                    TestLog.AppendLine("[ 딜레이7 ]\n");
                    Util.Delay(DcOutCurrCal.Delay7);
                    result = StateFlag.PASS;
                    break;

                case Seq.LOWER_ADC_CAL:
                    TestLog.AppendLine("[ ADC 하한 CAL ]");
                    result = AdcCalCheck((int)CalPoint.LOW_POINT, DcOutCurrCal.AdcLowerRef);
                    TestLog.AppendLine($"- CAL 결과 : {result}\n");

                    if (result != StateFlag.PASS)
                        jumpStepNum = (int)Seq.LOAD_OFF2;
                    break;

                case Seq.M100_AC_SET:
                    TestLog.AppendLine("[ 100V AC ]");

                    if (Option.IsFullAuto)
                    {
                        result = AcSourceSet(DcOutCurrCal.M100AcVolt, DcOutCurrCal.M100AcCurr, DcOutCurrCal.M100AcFreq);
                        TestLog.AppendLine($"- AC 세팅 결과 : {result}");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.LOAD_OFF2;

                        result = AcSourceOn();
                        TestLog.AppendLine($"- AC 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.LOAD_OFF2;
                    }
                    else
                    {
                        TestLog.AppendLine($"- AC 설정 팝업");

                        result = AcCtrlWin(DcOutCurrCal.M100AcVolt, AC_ERR_RANGE, AcCheckMode.NORMAL);
                        TestLog.AppendLine($"- AC 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.END_TEST;
                    }
                    break;

                case Seq.RECT_RESET: // 정류기 리셋
                    TestLog.AppendLine("[ 정류기 리셋 ]\n");

                    for (int i = 0; i < 3; i++)
                    {
                        TestLog.Append($"- 리셋 시도 {i + 1}회차 -> ");
                        Rectifier.GetObj().RectCommand(CommandList.SW_RESET, 1);
                        Util.Delay(7);

                        if (Rectifier.GetObj().AcInVoltMode == "100V")
                        {
                            TestLog.AppendLine($"성공");
                            return StateFlag.PASS;
                        }
                        TestLog.AppendLine($"실패");
                    }
                    TestLog.AppendLine($"- RECT 리셋 실패\n");
                    result = StateFlag.RECT_RESET_ERR;
                    break;

                case Seq.UNDER_VOLT_ALARM_OFF2:
                    TestLog.AppendLine("[ 출력 저전압 알람 제거 ]");

                    for (int i = 0; i < 3; i++)
                    {
                        TestLog.Append($"- 시도 {i}회차 -> ");
                        if (Rectifier.GetObj().RectCommand(CommandList.UNDER_VOLT_ALARM, 0))
                        {
                            TestLog.AppendLine($"성공\n");
                            return StateFlag.PASS;
                        }
                        TestLog.AppendLine($"실패");
                        Util.Delay(2);
                    }
                    result = StateFlag.RECT_CONNECT_ERR;
                    TestLog.AppendLine($"- 알람 설정 실패 : {result}\n");
                    break;

                case Seq.DELAY8:
                    TestLog.AppendLine("[ 딜레이8 ]\n");
                    Util.Delay(DcOutCurrCal.Delay8);
                    result = StateFlag.PASS;
                    break;

                case Seq.M100_LOAD_ON:
                    if (Option.IsFullAuto)
                    {
                        result = LoadCurrSet(DcOutCurrCal.M100LoadCurr);
                        TestLog.AppendLine($"- 부하 세팅 결과 : {result}");
                        if (result != StateFlag.PASS)
                        {
                            jumpStepNum = (int)Seq.LOAD_OFF2;
                            break;
                        }

                        result = LoadOn();
                        TestLog.AppendLine($"- 부하 전원 결과 : {result}\n");
                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.LOAD_OFF2;
                    }
                    else
                    {
                        TestLog.AppendLine($"- 부하 설정 팝업");

                        result = LoadCtrlWin(DcOutCurrCal.M100LoadCurr, LOAD_ERR_RANGE, LoadCheckMode.NORMAL);
                        TestLog.AppendLine($"- 부하 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.LOAD_OFF2;
                    }
                    break;

                case Seq.DELAY9:
                    TestLog.AppendLine("[ 딜레이9 ]\n");
                    Util.Delay(DcOutCurrCal.Delay9);
                    result = StateFlag.PASS;
                    break;

                case Seq.DEFAULT_M100_REF_SET: // 정류기 연결 및 값 설정
                    TestLog.AppendLine("[ 100V 모드 기본값 세팅 ]");
                    result = DefaultRefSet(DcOutCurrCal.DefaultM100Ref);
                    TestLog.AppendLine($"- 결과 : {result}\n");
                    break;

                case Seq.LOAD_OFF2:
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
                            jumpStepNum = (int)Seq.LOAD_OFF2;
                    }
                    break;

                case Seq.RECT_RESET2: // 정류기 리셋
                    TestLog.AppendLine("[ 정류기 리셋 ]\n");

                    for (int i = 0; i < 3; i++)
                    {
                        TestLog.Append($"- 리셋 시도 {i + 1}회차 -> ");
                        Rectifier.GetObj().RectCommand(CommandList.SW_RESET, 1);
                        Util.Delay(7);

                        if (Rectifier.GetObj().AcInVoltMode == "100V")
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
                    Util.Delay(DcOutCurrCal.NextTestWait);
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
         *  @brief DC 전류 DAC CAL 수행
         *  @details DC 전류 DAC CAL을 진행하고 잘 됐는지 검사한다
         *  
         *  @param double lowRefValue - 하한 포인트 CAL 기준값
         *  @param double upRefValue - 상한 포인트 CAL 기준값
         *  
         *  @return StateFlag - 수행 결과
         *  
         *  @see I Cal Dmm 값 전송시 주의사항 :
         *       DMM의 전압값 * 100을 해서 MCU에 전달해야 정상적으로 인식한다
         */
        private StateFlag DacCal(double lowRefValue, double upRefValue)
        {
            Rectifier rect = Rectifier.GetObj();
            double dmmDcVolt;
            bool cmdResult;

            rect.MonitoringStop();

            for (int i = 0; i < MAX_CAL_TRY_COUNT; i++)
            {
                TestLog.AppendLine($"- 시도 {i}회차");

                // High 포인트 CAL
                TestLog.Append($"- 전류 상한 Ref 설정 -> ");
                rect.RectCommand(CommandList.I_REF_SET, (ushort)RefSave.NO_SAVE, (ushort)(upRefValue * 100));

                Util.Delay(1.5);

                dmmDcVolt = Dmm2.GetObj().DcVolt;
                if (Math.Abs(upRefValue - dmmDcVolt) > 5) // CAL 전 허용 오차 : 5V
                {
                    TestLog.AppendLine($"실패\n");
                    continue;
                }
                TestLog.AppendLine($"성공");


                TestLog.Append($"- 전류 DAC CAL 시도 : {dmmDcVolt} -> ");
                cmdResult = rect.RectCommand(CommandList.DAC_I_CAL, (ushort)CalPoint.HIGH_POINT, (ushort)(dmmDcVolt * 100), (ushort)I_RefCal((int)(upRefValue * 100)));
                if (!cmdResult)
                {
                    TestLog.AppendLine($"실패");
                    Debug.WriteLine("I DAC CAL High Fail");
                    continue;
                }
                TestLog.AppendLine($"성공");

                // Low 포인트 CAL
                Util.Delay(0.5);
                TestLog.Append($"- 전류 하한 Ref 설정 -> ");
                rect.RectCommand(CommandList.I_REF_SET, (ushort)RefSave.NO_SAVE, (ushort)(lowRefValue * 100));

                Util.Delay(1.5);

                dmmDcVolt = Dmm2.GetObj().DcVolt;
                if (Math.Abs(upRefValue - dmmDcVolt) > 5) // CAL 전 허용 오차 : 5V
                {
                    TestLog.AppendLine($"실패\n");
                    continue;
                }
                TestLog.AppendLine($"성공");

                Util.Delay(1.5);

                TestLog.Append($"- 전류 DAC CAL 시도 : {dmmDcVolt} -> ");
                cmdResult = rect.RectCommand(CommandList.DAC_I_CAL, (ushort)CalPoint.LOW_POINT, (ushort)(dmmDcVolt * 100), (ushort)I_RefCal((int)(lowRefValue * 100)));
                if (!cmdResult)
                {
                    TestLog.AppendLine($"실패");
                    Debug.WriteLine("I DAC CAL Low Fail");
                    continue;
                }
                TestLog.AppendLine($"성공");

                Util.Delay(1);
                TestLog.Append($"- CAL 적용 -> ");
                cmdResult = rect.RectCommand(CommandList.CAL_I_APPLY, 1);
                if (!cmdResult)
                {
                    TestLog.AppendLine($"실패");
                    continue;
                }
                TestLog.AppendLine($"성공");

                TestLog.AppendLine($"- DAC CAL 성공");
                rect.MonitoringStart();
                return StateFlag.PASS;
            }
            TestLog.AppendLine($"- DAC CAL 실패");
            rect.MonitoringStart();
            return StateFlag.DC_CURR_CAL_ERR;
        }

        /**
         *  @brief DC 전류 ADC CAL 수행
         *  @details DC 전류 ADC CAL을 진행하고 잘 됐는지 검사한다
         *  
         *  @param ushort unDown - 1: 상한 AC CAL, 0 : 하한 AC CAL
         *  @param double refValue - 포인트 CAL 기준값
         *  
         *  @return StateFlag - 수행 결과
         */
        private StateFlag AdcCalCheck(ushort unDown, double refValue)
        {
            Rectifier rect = Rectifier.GetObj();
            double dmmDcVolt;
            double rectDcCurr;
            bool cmdResult;

            rect.MonitoringStop();

            for (int i = 0; i < MAX_CAL_TRY_COUNT; i++)
            {
                ////////////////////////////////////////////////////////////////////////////////////
                dmmDcVolt = Dmm2.GetObj().DcVolt;
                TestLog.Append($"- 전류 ADC CAL 시도 : {dmmDcVolt} -> ");
                cmdResult = rect.RectCommand(CommandList.ADC_I_CAL, unDown, (ushort)(dmmDcVolt * 100));
                if (!cmdResult)
                {
                    TestLog.AppendLine($"실패");
                    Debug.WriteLine("I ADC CAL " + unDown + " Fail");
                    Util.Delay(0.5);
                    continue;
                }
                TestLog.AppendLine($"성공");
                break;
                ////////////////////////////////////////////////////////////////////////////////////
            }

            rect.RectMonitoring();
            dmmDcVolt = Dmm2.GetObj().DcVolt;
            Util.Delay(1.5);
            rectDcCurr = rect.DcOutputCurr;

            TestLog.AppendLine($"- DMM2 측정값 : {dmmDcVolt}");
            TestLog.AppendLine($"- 정류기 전류 : {rectDcCurr}");
            TestLog.AppendLine($"- 현재오차 : {Math.Abs(dmmDcVolt - rectDcCurr)}");
            TestLog.AppendLine($"- 허용오차 : {ERR_RATE}");

            if (Math.Abs(dmmDcVolt - rectDcCurr) <= ERR_RATE)
            {
                TestLog.AppendLine($"- ADC CAL 완료");
                rect.MonitoringStart();
                return StateFlag.PASS;
            }

            TestLog.AppendLine($"- ADC CAL 실패");
            rect.MonitoringStart();
            return StateFlag.DC_CURR_CAL_ERR;
        }

        /**
         *  @brief DC 전류 기본값 CAL 수행
         *  @details DC 전류 기본값 CAL을 진행하고 잘 됐는지 검사한다
         *  
         *  @param double refValue - 포인트 CAL 기준값
         *  
         *  @return StateFlag - 수행 결과
         */
        private StateFlag DefaultRefSet(double refValue)
        {
            double dmmDcVolt;
            bool cmdResult;

            for (int i = 0; i < MAX_CAL_TRY_COUNT; i++)
            {
                TestLog.AppendLine($"- 시도 {i}회차");

                TestLog.Append($"- 전류 Ref 설정 -> ");
                cmdResult = Rectifier.GetObj().RectCommand(CommandList.I_REF_SET, (ushort)RefSave.SAVE, (ushort)(refValue * 100));
                if (!cmdResult)
                {
                    TestLog.AppendLine($"실패");
                    continue;
                }
                TestLog.AppendLine($"성공");

                Util.Delay(1);
                dmmDcVolt = Dmm2.GetObj().DcVolt;

                TestLog.AppendLine($"- DMM2 측정값 : {dmmDcVolt}");
                TestLog.AppendLine($"-      설정값 : {refValue}");
                TestLog.AppendLine($"-    현재오차 : {Math.Abs(dmmDcVolt - refValue)}");
                TestLog.AppendLine($"-    허용오차 : {ERR_RATE}");

                if (Math.Abs(dmmDcVolt - refValue) <= ERR_RATE)
                {
                    TestLog.AppendLine($"- 설정 성공");
                    return StateFlag.PASS;
                }
            }
            TestLog.AppendLine($"- 설정 실패");
            return StateFlag.DC_VOLT_CAL_ERR;
        }

        /**
         *  @brief DAC CAL 전류값 계산기
         *  @details
         *  
         *  @param double I_RefVal - CAL 포인트값
         *  
         *  @return StateFlag - 계산된 결과
         *  
         *  @see MCU에서 계산하면 리소스를 많이 잡아먹어서 PC에서 계산하고 계산된 값을 MCU에 전송
         */
        private int I_RefCal(int I_RefVal)
        {
            double x1 = 4095, x2 = 2500.0, y1 = 6500.0, y2 = 0.0;
            int weight = 1024;

            int a = (int)((y1 - y2) * weight / (x1 - x2));
            int b = (int)(y1 * weight - a * x1);
            int DAC = ((I_RefVal * weight) - b) / a;

            return DAC;
        }
    }
}