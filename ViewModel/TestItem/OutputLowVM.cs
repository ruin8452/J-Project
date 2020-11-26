﻿using J_Project.Equipment;
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
    internal class OutputLowVM : AllTestVM
    {
        private enum Seq
        {
            AC_ON,
            DELAY1,
            OUT_LOW_CHECK,
            DELAY2,
            RESULT_SAVE,
            RECT_RESET,
            NEXT_TEST_DELAY,
            END_TEST
        }

        public static string TestName { get; } = "출력 저전압 보호";
        public 출력_저전압_보호 OutputLow { get; set; }
        public TestOption Option { get; set; }

        public ObservableCollection<SolidColorBrush> ButtonColor { get; private set; }

        public ICommand UnloadPage { get; set; }
        public ICommand UnitTestCommand { get; set; }

        public OutputLowVM()
        {
            TestLog = new StringBuilder();

            TotalStepNum = (int)Seq.END_TEST + 1;

            출력_저전압_보호.Load();
            OutputLow = 출력_저전압_보호.GetObj();
            Option = TestOption.GetObj();
            ButtonColor = new ObservableCollection<SolidColorBrush>();

            for (int i = 0; i < TotalStepNum; i++)
                ButtonColor.Add(Brushes.White);

            UnloadPage = new BaseCommand(DataSave);
            UnitTestCommand = new BaseObjCommand(UnitTestClick);
        }

        private void DataSave()
        {
            출력_저전압_보호.Save();
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
                case Seq.AC_ON: // 초기 AC 설정
                    TestLog.AppendLine("[ AC ]");

                    double acVolt = PowerMeter.GetObj().AcVolt;
                    TestLog.AppendLine($"- AC : {acVolt}");
                    if (Math.Abs(OutputLow.AcVolt[caseNumber] - acVolt) <= AC_ERR_RANGE)
                    {
                        result = StateFlag.PASS;
                        break;
                    }

                    if (Option.IsFullAuto)
                    {
                        result = AcSourceSet(OutputLow.AcVolt[caseNumber], OutputLow.AcCurr[caseNumber], OutputLow.AcFreq[caseNumber]);
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

                        result = AcCtrlWin(OutputLow.AcVolt[caseNumber], AC_ERR_RANGE, AcCheckMode.NORMAL);
                        TestLog.AppendLine($"- AC 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    break;

                case Seq.DELAY1:
                    TestLog.AppendLine("[ 딜레이1 ]\n");
                    Util.Delay(OutputLow.Delay1[caseNumber]);
                    result = StateFlag.PASS;
                    break;

                case Seq.OUT_LOW_CHECK:
                    TestLog.AppendLine("[ 기능 검사 ]");
                    result = RectOutLowCheck(caseNumber, OutputLow.DcOutVolt[caseNumber], OutputLow.DcErrRate[caseNumber], ref resultData);
                    TestLog.AppendLine($"- 결과 : {result}\n");
                    if (result != StateFlag.PASS)
                        jumpStepNum = (int)Seq.RECT_RESET;
                    break;

                case Seq.DELAY2:
                    TestLog.AppendLine("[ 딜레이2 ]\n");
                    Util.Delay(OutputLow.Delay2[caseNumber]);
                    result = StateFlag.PASS;
                    break;

                case Seq.RESULT_SAVE: // 결과 저장
                    TestLog.AppendLine("[ 결과 저장 ]");
                    result = ResultDataSave((int)FirstTestOrder.OutputLow, TestName, resultData);
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
                    Util.Delay(OutputLow.NextTestWait[caseNumber]);
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

        // AC 저전압 인식 검사
        private StateFlag RectOutLowCheck(int caseNum, double acDrop, double errRate, ref (string, string) resultData)
        {
            Rectifier rect = Rectifier.GetObj();
            Dmm1 dmm1 = Dmm1.GetObj();
            double minDc = acDrop - errRate;
            double maxDc = acDrop + errRate;

            TestLog.AppendLine($"- 검사 범위 : {minDc} ~ {maxDc}");

            //<C>20.SSW 03.25 : 모니터링과 충돌 위험이 있어 해당 검사시 모니터링 중단
            rect.MonitoringStop();
            dmm1.MonitoringStop();

            for (double inputOut = maxDc; inputOut >= minDc; inputOut -= 0.1)
            {
                // DC Out 단계별 하강
                rect.RectCommand(CommandList.V_REF_SET, 0, (ushort)(inputOut * 100));

                double dmmVolt = dmm1.RealDcVolt();
                TestLog.AppendLine($"- 출력 하강 : {dmmVolt}");

                rect.RectMonitoring();
                Util.Delay(0.1);

                // Fault 발생 확인
                bool faultFlag = rect.Flag_UnberDcOutVolt;
                if (faultFlag == true)
                {
                    dmmVolt -= 0.1;
                    TestLog.AppendLine($"- 출력 저전압 인식 : {dmmVolt}");
                    resultData = ("OK", "합격");

                    dmm1.MonitoringStart();
                    rect.MonitoringStart();

                    return StateFlag.PASS;
                }

                Util.Delay(0.1);
            }
            dmm1.MonitoringStart();
            rect.MonitoringStart();

            TestLog.AppendLine($"- 테스트 불합격 : 검사 범위 내 출력 저전압 인식 실패");
            resultData = ("테스트 실패", "불합격");

            return StateFlag.OUTPUT_ERR;
        }
    }
}