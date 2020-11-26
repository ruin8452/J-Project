using J_Project.Equipment;
using J_Project.Manager;
using J_Project.TestMethod;
using J_Project.ViewModel.CommandClass;
using J_Project.ViewModel.SubWindow;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;

namespace J_Project.ViewModel.TestItem
{
    internal class CalDcVoltVM : AllTestVM
    {
        private enum Seq
        {
            AC_ON,
            DELAY1,
            LOAD_ON,
            DELAY2,
            DAC_CAL,
            //DELAY3,
            //ADC_CAL,
            DELAY4,
            DEFAULT_REF_SET,
            DELAY5,
            LOAD_OFF,
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

        private const double ERR_RATE = 0.05;

        public static string TestName { get; } = "Cal DC 출력전압";
        public Cal_DC_출력전압 DcOutVoltCal { get; set; }
        public TestOption Option { get; set; }

        public ObservableCollection<SolidColorBrush> ButtonColor { get; private set; }

        public ICommand UnloadPage { get; set; }
        public ICommand UnitTestCommand { get; set; }

        public CalDcVoltVM()
        {
            TestLog = new StringBuilder();

            TotalStepNum = (int)Seq.END_TEST + 1;

            Cal_DC_출력전압.Load();
            DcOutVoltCal = Cal_DC_출력전압.GetObj();
            Option = TestOption.GetObj();
            ButtonColor = new ObservableCollection<SolidColorBrush>();

            for (int i = 0; i < TotalStepNum; i++)
                ButtonColor.Add(Brushes.White);

            UnloadPage = new BaseCommand(DataSave);
            UnitTestCommand = new BaseObjCommand(UnitTestClick);
        }

        private void DataSave()
        {
            Cal_DC_출력전압.Save();
        }

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
                case Seq.AC_ON: // 초기 AC 설정 및 ON
                    TestLog.AppendLine("[ AC ]");

                    double acVolt = PowerMeter.GetObj().AcVolt;
                    TestLog.AppendLine($"- AC : {acVolt}");
                    if (Math.Abs(DcOutVoltCal.AcVolt[caseNumber] - acVolt) <= AC_ERR_RANGE)
                    {
                        result = StateFlag.PASS;
                        break;
                    }

                    // 자동, 반자동 분기
                    if (Option.IsFullAuto)
                    {
                        result = AcSourceSet(DcOutVoltCal.AcVolt[caseNumber], DcOutVoltCal.AcCurr[caseNumber], DcOutVoltCal.AcFreq[caseNumber]);
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

                        result = AcCtrlWin(DcOutVoltCal.AcVolt[caseNumber], AC_ERR_RANGE, AcCheckMode.NORMAL);
                        TestLog.AppendLine($"- AC 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.LOAD_OFF;
                    }
                    break;

                case Seq.DELAY1:
                    TestLog.AppendLine("[ 딜레이1 ]\n");
                    Util.Delay(DcOutVoltCal.Delay1[caseNumber]);
                    result = StateFlag.PASS;
                    break;

                case Seq.LOAD_ON: // 부하 설정
                    TestLog.AppendLine("[ 부하 ON ]");

                    double loadVolt = Dmm2.GetObj().DcVolt;
                    TestLog.AppendLine($"- Load : {loadVolt}");
                    if (Math.Abs(DcOutVoltCal.LoadCurr[caseNumber] - loadVolt) <= LOAD_ERR_RANGE)
                    {
                        result = StateFlag.PASS;
                        break;
                    }

                    if (Option.IsFullAuto)
                    {
                        result = LoadCurrSet(DcOutVoltCal.LoadCurr[caseNumber]);
                        TestLog.AppendLine($"- 부하 세팅 결과 : {result}");

                        if (result != StateFlag.PASS)
                        {
                            jumpStepNum = (int)Seq.LOAD_OFF;
                            break;
                        }

                        result = LoadOn();
                        TestLog.AppendLine($"- 부하 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.LOAD_OFF;
                    }
                    else
                    {
                        TestLog.AppendLine($"- 부하 설정 팝업");

                        result = LoadCtrlWin(DcOutVoltCal.LoadCurr[caseNumber], LOAD_ERR_RANGE, LoadCheckMode.NORMAL);
                        TestLog.AppendLine($"- 부하 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.LOAD_OFF;
                    }
                    break;

                case Seq.DELAY2:
                    TestLog.AppendLine("[ 딜레이2 ]\n");
                    Util.Delay(DcOutVoltCal.Delay2[caseNumber]);
                    result = StateFlag.PASS;
                    break;

                case Seq.DAC_CAL:
                    TestLog.AppendLine("[ DAC CAL ]");

                    result = Cal(DcOutVoltCal.DacLowerRef[caseNumber], CalPoint.LOW_POINT);
                    TestLog.AppendLine($"- 결과 : {result}\n");
                    if (result != StateFlag.PASS)
                        return result;

                    result = Cal(DcOutVoltCal.DacUpperRef[caseNumber], CalPoint.HIGH_POINT);
                    TestLog.AppendLine($"- 결과 : {result}\n");

                    //result = DacCal(DcOutVoltCal.DacLowerRef[caseNumber], DcOutVoltCal.DacUpperRef[caseNumber]);
                    //TestLog.AppendLine($"- 결과 : {result}\n");
                    break;

                //case Seq.DELAY3:
                //    TestLog.AppendLine("[ 딜레이3 ]\n");
                //    Util.Delay(DcOutVoltCal.Delay3[caseNumber]);
                //    result = StateFlag.PASS;
                //    break;

                //case Seq.ADC_CAL:
                //    TestLog.AppendLine("[ ADC CAL ]");
                //    result = AdcCal(DcOutVoltCal.AdcLowerRef[caseNumber], DcOutVoltCal.AdcUpperRef[caseNumber]);
                //    TestLog.AppendLine($"- 결과 : {result}\n");

                //    if (result != StateFlag.PASS)
                //        jumpStepNum = (int)Seq.LOAD_OFF;
                //    break;

                case Seq.DELAY4:
                    TestLog.AppendLine("[ 딜레이4 ]\n");
                    Util.Delay(DcOutVoltCal.Delay4[caseNumber]);
                    result = StateFlag.PASS;
                    break;

                case Seq.DEFAULT_REF_SET: // 기본 출력값 저장 및 설정
                    TestLog.AppendLine("[ 기본값 세팅 ]");
                    result = DefaultRefSet(DcOutVoltCal.DefaultRef[caseNumber]);
                    TestLog.AppendLine($"- 결과 : {result}\n");
                    break;

                case Seq.DELAY5:
                    TestLog.AppendLine("[ 딜레이5 ]\n");
                    Util.Delay(DcOutVoltCal.Delay5[caseNumber]);
                    result = StateFlag.PASS;
                    break;

                case Seq.LOAD_OFF: // Load OFF
                    TestLog.AppendLine("[ 부하 OFF ]");

                    loadVolt = Dmm2.GetObj().DcVolt;
                    TestLog.AppendLine($"- Load : {loadVolt}");
                    if (Math.Abs(0 - loadVolt) <= LOAD_ERR_RANGE)
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

                        result = LoadCtrlWin(0, LOAD_ERR_RANGE, LoadCheckMode.NORMAL);
                        TestLog.AppendLine($"- 부하 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.LOAD_OFF;
                    }
                    break;

                case Seq.NEXT_TEST_DELAY:
                    TestLog.AppendLine("[ 다음 테스트 딜레이 ]\n");
                    Util.Delay(DcOutVoltCal.NextTestWait[caseNumber]);
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

        private StateFlag Cal(double calValue, CalPoint calPoint)
        {
            Rectifier rect = Rectifier.GetObj();

            rect.MonitoringStop();

            TestLog.AppendLine($"- 포인트 {calValue} CAL\n");

            double dmmDcVolt;
            // 레퍼런스 세팅 & 명령 수행 점검
            for (int i = 0; i < MAX_CAL_TRY_COUNT; i++)
            {
                TestLog.AppendLine($"- 레퍼런스 Set 시도 {i + 1}회차");

                TestLog.Append($"- Ref {calValue} 설정 -> ");
                rect.RectCommand(CommandList.V_REF_SET, (ushort)RefSave.NO_SAVE, (ushort)(calValue * 100));

                Util.Delay(1.5);   // Dmm값 안정화를 위한 딜레이

                dmmDcVolt = Dmm1.GetObj().DcVolt;
                if (Math.Abs(calValue - dmmDcVolt) <= 5) // CAL 전 허용 오차 : 5V
                {
                    TestLog.AppendLine($"성공\n");
                    break;
                }
                TestLog.AppendLine($"실패\n");
                if (i == MAX_CAL_TRY_COUNT - 1)
                {
                    TestLog.AppendLine($"포인트 {calValue} 레퍼런스 Set 에러\n");
                    rect.MonitoringStart();
                    return StateFlag.DC_VOLT_CAL_ERR;
                }
            }

            // DAC & ADC Cal
            for (int i = 0; i < MAX_CAL_TRY_COUNT; i++)
            {
                TestLog.AppendLine($"- DAC Cal 시도 {i + 1}회차");

                dmmDcVolt = Dmm1.GetObj().DcVolt;

                TestLog.AppendLine($"- 전압 DAC CAL 시도 ");
                rect.RectCommand(CommandList.DAC_V_CAL, (ushort)calPoint, (ushort)(dmmDcVolt * 100.0));

                Util.Delay(0.5);

                TestLog.AppendLine($"- 전압 ADC CAL 시도 ");
                rect.RectCommand(CommandList.ADC_V_CAL, (ushort)calPoint, (ushort)(dmmDcVolt * 100.0));

                // 정확도 검사
                rect.RectMonitoring();
                Util.Delay(0.5);

                dmmDcVolt = Dmm1.GetObj().DcVolt;
                double rectDcVolt = rect.DcOutputVolt;
                TestLog.AppendLine($"- DMM1 측정값 : {dmmDcVolt}");
                TestLog.AppendLine($"- 정류기 전류 : {rectDcVolt}");
                TestLog.AppendLine($"- 현재오차 : {Math.Abs(dmmDcVolt - rectDcVolt)}");
                TestLog.AppendLine($"- 허용오차 : {ERR_RATE}");

                if (Math.Abs(dmmDcVolt - rectDcVolt) <= ERR_RATE)
                {
                    TestLog.AppendLine($"- 포인트 {calValue} CAL 완료");
                    break;
                }
                TestLog.AppendLine($"- CAL 실패\n");
                if (i == MAX_CAL_TRY_COUNT - 1)
                {
                    TestLog.AppendLine($"포인트 {calValue} CAL 에러\n");
                    rect.MonitoringStart();
                    return StateFlag.DC_VOLT_CAL_ERR;
                }
            }

            // CAL 적용
            for (int i = 0; i < MAX_CAL_TRY_COUNT; i++)
            {
                TestLog.AppendLine($"- CAL 적용 시도 {i + 1}회차");

                TestLog.Append($"- CAL 적용 -> ");
                if (rect.RectCommand(CommandList.CAL_V_APPLY, 1))
                {
                    TestLog.AppendLine($"성공");
                    break;
                }
                TestLog.AppendLine($"실패");
                if (i == MAX_CAL_TRY_COUNT - 1)
                {
                    TestLog.AppendLine($"포인트 {calValue} CAL 적용 에러\n");
                    rect.MonitoringStart();
                    return StateFlag.DC_VOLT_CAL_ERR;
                }
            }

            rect.MonitoringStart();

            return StateFlag.PASS;
        }

        private StateFlag DacCal(double lowRefValue, double upRefValue)
        {
            Rectifier rect = Rectifier.GetObj();
            double dmmDcVolt;
            bool cmdResult;

            rect.MonitoringStop();

            for (int i = 0; i < MAX_CAL_TRY_COUNT; i++)
            {
                TestLog.AppendLine($"- 시도 {i}회차");

                // Low 포인트 CAL
                TestLog.Append($"- 전압 하한 Ref 설정 -> ");
                cmdResult = rect.RectCommand(CommandList.V_REF_SET, (ushort)RefSave.NO_SAVE, (ushort)(lowRefValue * 100));
                if (!cmdResult)
                {
                    TestLog.AppendLine($"실패");
                    Debug.WriteLine("V Ref Set " + lowRefValue + " Fail");
                    continue;
                }
                TestLog.AppendLine($"성공");

                Util.Delay(1.5);   // Dmm값 안정화를 위한 딜레이

                dmmDcVolt = Dmm1.GetObj().DcVolt;
                TestLog.Append($"- 전압 DAC CAL 시도 : {dmmDcVolt} -> ");
                cmdResult = rect.RectCommand(CommandList.DAC_V_CAL, (ushort)CalPoint.LOW_POINT, (ushort)(dmmDcVolt * 100.0));
                if (!cmdResult)
                {
                    TestLog.AppendLine($"실패");
                    Debug.WriteLine("V DAC CAL Low Fail : " + dmmDcVolt);
                    continue;
                }
                TestLog.AppendLine($"성공");

                // High 포인트 CAL
                Util.Delay(1);
                TestLog.Append($"- 전압 상한 Ref 설정 -> ");
                cmdResult = rect.RectCommand(CommandList.V_REF_SET, (ushort)RefSave.NO_SAVE, (ushort)(upRefValue * 100));
                if (!cmdResult)
                {
                    TestLog.AppendLine($"실패");
                    Debug.WriteLine("V Ref Set " + upRefValue + " Fail");
                    continue;
                }
                TestLog.AppendLine($"성공");

                Util.Delay(1.5);

                dmmDcVolt = Dmm1.GetObj().DcVolt;
                TestLog.Append($"- 전압 DAC CAL 시도 : {dmmDcVolt} -> ");
                cmdResult = rect.RectCommand(CommandList.DAC_V_CAL, (ushort)CalPoint.HIGH_POINT, (ushort)(dmmDcVolt * 100.0));
                if (!cmdResult)
                {
                    TestLog.AppendLine($"실패");
                    Debug.WriteLine("V DAC CAL High Fail : " + dmmDcVolt);
                    continue;
                }
                TestLog.AppendLine($"성공");

                // CAL 적용
                Util.Delay(1);
                TestLog.Append($"- CAL 적용 -> ");
                cmdResult = rect.RectCommand(CommandList.CAL_V_APPLY, 1);
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
            return StateFlag.DC_VOLT_CAL_ERR;
        }

        private StateFlag AdcCal(double lowRefValue, double upRefValue)
        {
            Rectifier rect = Rectifier.GetObj();
            double dmmDcVolt;
            double rectDcVolt;
            bool cmdResult;

            rect.MonitoringStop();

            for (int i = 0; i < MAX_CAL_TRY_COUNT; i++)
            {
                TestLog.AppendLine($"- 시도 {i}회차");

                // Low 포인트 CAL
                TestLog.Append($"- 전압 하한 Ref 설정 -> ");
                cmdResult = rect.RectCommand(CommandList.V_REF_SET, (ushort)RefSave.NO_SAVE, (ushort)(lowRefValue * 100.0));
                if (!cmdResult)
                {
                    TestLog.AppendLine($"실패");
                    Debug.WriteLine("V Ref Set " + lowRefValue + " Fail");
                    continue;
                }
                TestLog.AppendLine($"성공");

                Util.Delay(1.5);
                dmmDcVolt = Dmm1.GetObj().DcVolt;
                TestLog.Append($"- 전압 ADC CAL 시도 : {dmmDcVolt} -> ");
                cmdResult = rect.RectCommand(CommandList.ADC_V_CAL, (ushort)CalPoint.LOW_POINT, (ushort)(dmmDcVolt * 100.0));
                if (!cmdResult)
                {
                    TestLog.AppendLine($"실패");
                    Debug.WriteLine("V ADC CAL Low Fail : " + dmmDcVolt);
                    continue;
                }
                TestLog.AppendLine($"성공");

                // High 포인트 CAL
                Util.Delay(1);
                TestLog.Append($"- 전압 상한 Ref 설정 -> ");
                cmdResult = rect.RectCommand(CommandList.V_REF_SET, (ushort)RefSave.NO_SAVE, (ushort)(upRefValue * 100.0));
                if (!cmdResult)
                {
                    TestLog.AppendLine($"실패");
                    Debug.WriteLine("V Ref Set " + upRefValue + " Fail");
                    continue;
                }
                TestLog.AppendLine($"성공");

                Util.Delay(1.5);
                dmmDcVolt = Dmm1.GetObj().DcVolt;
                TestLog.Append($"- 전압 ADC CAL 시도 : {dmmDcVolt} -> ");
                cmdResult = rect.RectCommand(CommandList.ADC_V_CAL, (ushort)CalPoint.HIGH_POINT, (ushort)(dmmDcVolt * 100.0));
                if (!cmdResult)
                {
                    TestLog.AppendLine($"실패");
                    Debug.WriteLine("V ADC CAL High Fail : " + dmmDcVolt);
                    continue;
                }
                TestLog.AppendLine($"성공");

                // 정확도 검사
                rect.RectMonitoring();
                dmmDcVolt = Dmm1.GetObj().DcVolt;
                Util.Delay(1.5);
                rectDcVolt = rect.DcOutputVolt;

                TestLog.AppendLine($"- DMM1 측정값 : {dmmDcVolt}");
                TestLog.AppendLine($"- 정류기 전류 : {rectDcVolt}");
                TestLog.AppendLine($"- 현재오차 : {Math.Abs(dmmDcVolt - rectDcVolt)}");
                TestLog.AppendLine($"- 허용오차 : {ERR_RATE}");

                if (Math.Abs(dmmDcVolt - rectDcVolt) <= ERR_RATE)
                {
                    TestLog.AppendLine($"- ADC CAL 완료");
                    rect.MonitoringStart();
                    return StateFlag.PASS;
                }
            }
            TestLog.AppendLine($"- ADC CAL 실패");
            rect.MonitoringStart();
            return StateFlag.DC_VOLT_CAL_ERR;
        }

        private StateFlag DefaultRefSet(double refValue)
        {
            Rectifier rect = Rectifier.GetObj();
            double dmmDcVolt;
            double rectDcVolt;
            bool cmdResult;

            for (int i = 0; i < MAX_CAL_TRY_COUNT; i++)
            {
                TestLog.AppendLine($"- 시도 {i}회차");

                TestLog.Append($"- 전압 Ref 설정 -> ");
                cmdResult = rect.RectCommand(CommandList.V_REF_SET, (ushort)RefSave.SAVE, (ushort)(refValue * 100));
                if (!cmdResult)
                {
                    TestLog.AppendLine($"실패");
                    continue;
                }
                TestLog.AppendLine($"성공");
            }

            Util.Delay(1);

            for (int i = 0; i < MAX_CAL_TRY_COUNT; i++)
            {
                // CAL 완료 후 출력값 보정
                dmmDcVolt = Dmm1.GetObj().DcVolt;
                if (dmmDcVolt < refValue - 0.03)
                {
                    TestLog.AppendLine($"- CAL 보정 : 설정값 보다 낮음");

                    double correction = refValue - dmmDcVolt;
                    TestLog.Append($"- 전압 Ref 보정 설정({correction}) -> ");
                    cmdResult = rect.RectCommand(CommandList.V_REF_SET, (ushort)RefSave.SAVE, (ushort)((refValue + correction) * 100));
                    if (!cmdResult)
                    {
                        TestLog.AppendLine($"실패");
                        continue;
                    }
                    TestLog.AppendLine($"성공");
                }
                else if (dmmDcVolt > refValue + 0.03)
                {
                    TestLog.AppendLine($"- CAL 보정 : 설정값 보다 높음");

                    double correction = dmmDcVolt - refValue;
                    TestLog.Append($"- 전압 Ref 보정 설정({correction}) -> ");
                    cmdResult = rect.RectCommand(CommandList.V_REF_SET, (ushort)RefSave.SAVE, (ushort)((refValue - correction) * 100));
                    if (!cmdResult)
                    {
                        TestLog.AppendLine($"실패");
                        continue;
                    }
                    TestLog.AppendLine($"성공");
                }

                // 최종 출력값 검사
                Util.Delay(1);
                rectDcVolt = rect.DcOutputVolt;

                if (Math.Abs(refValue - rectDcVolt) <= ERR_RATE)
                {
                    TestLog.AppendLine($"- 설정 성공");
                    return StateFlag.PASS;
                }
            }
            TestLog.AppendLine($"- 설정 실패");
            return StateFlag.DC_VOLT_CAL_ERR;
        }
    }
}