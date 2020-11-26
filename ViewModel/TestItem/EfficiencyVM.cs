using J_Project.Equipment;
using J_Project.Manager;
using J_Project.TestMethod;
using J_Project.UI.SubWindow;
using J_Project.ViewModel.CommandClass;
using J_Project.ViewModel.SubWindow;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace J_Project.ViewModel.TestItem
{
    internal class EfficiencyVM : AllTestVM
    {
        private enum Seq
        {
            AC_ON,
            DELAY1,
            LOAD_ON,
            DELAY2,
            RESULT_CHECK,
            RESULT_SAVE,
            LOAD_OFF,
            NEXT_TEST_DELAY,
            END_TEST
        }

        public static string TestName { get; } = "효율";
        public 효율 Efficiency { get; set; }
        public TestOption Option { get; set; }

        public ObservableCollection<SolidColorBrush> ButtonColor { get; private set; }

        public ICommand UnloadPage { get; set; }
        public ICommand UnitTestCommand { get; set; }

        public EfficiencyVM()
        {
            TestLog = new StringBuilder();

            TotalStepNum = (int)Seq.END_TEST + 1;

            효율.Load();
            Efficiency = 효율.GetObj();
            Option = TestOption.GetObj();
            ButtonColor = new ObservableCollection<SolidColorBrush>();

            for (int i = 0; i < TotalStepNum; i++)
                ButtonColor.Add(Brushes.White);

            UnloadPage = new BaseCommand(DataSave);
            UnitTestCommand = new BaseObjCommand(UnitTestClick);
        }

        private void DataSave()
        {
            효율.Save();
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
                    if (Math.Abs(Efficiency.AcVolt[caseNumber] - acVolt) <= AC_ERR_RANGE)
                    {
                        result = StateFlag.PASS;
                        break;
                    }

                    if (Option.IsFullAuto)
                    {
                        result = AcSourceSet(Efficiency.AcVolt[caseNumber], Efficiency.AcCurr[caseNumber], Efficiency.AcFreq[caseNumber]);
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

                        result = AcCtrlWin(Efficiency.AcVolt[caseNumber], AC_ERR_RANGE, AcCheckMode.NORMAL);
                        TestLog.AppendLine($"- AC 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    break;

                case Seq.DELAY1:
                    TestLog.AppendLine("[ 딜레이1 ]\n");
                    Util.Delay(Efficiency.Delay1[caseNumber]);
                    result = StateFlag.PASS;
                    break;

                case Seq.LOAD_ON: // 부하 설정
                    TestLog.AppendLine("[ 부하 ON ]");

                    double loadVolt = Dmm2.GetObj().DcVolt;
                    TestLog.AppendLine($"- Load : {loadVolt}");
                    if (Math.Abs(Efficiency.LoadCurr[caseNumber] - loadVolt) <= LOAD_ERR_RANGE)
                    {
                        result = StateFlag.PASS;
                        break;
                    }

                    if (Option.IsFullAuto)
                    {
                        result = LoadCurrSet(Efficiency.LoadCurr[caseNumber]);
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

                        result = LoadCtrlWin(Efficiency.LoadCurr[caseNumber], LOAD_ERR_RANGE, LoadCheckMode.NORMAL);
                        TestLog.AppendLine($"- 부하 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    break;

                case Seq.DELAY2:
                    TestLog.AppendLine("[ 딜레이2 ]\n");
                    Util.Delay(Efficiency.Delay2[caseNumber]);
                    result = StateFlag.PASS;
                    break;

                case Seq.RESULT_CHECK: // 결과 판단 & 성적서 작성
                    TestLog.AppendLine("[ 기능 검사 ]");
                    result = EfficiencyTest(Efficiency.LimitEfficiency[caseNumber], ref resultData);

                    // 실패시 수동 입력
                    if (result != StateFlag.PASS)
                        result = EfficiencyPassiveCheckTest(Efficiency.LimitEfficiency[caseNumber], ref resultData);

                    TestLog.AppendLine($"- 결과 : {result}\n");
                    break;

                case Seq.RESULT_SAVE: // 결과 저장
                    TestLog.AppendLine("[ 결과 저장 ]");
                    result = ResultDataSave((int)FirstTestOrder.Efficiency, TestName, resultData);
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
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    break;

                case Seq.NEXT_TEST_DELAY:
                    TestLog.AppendLine("[ 다음 테스트 딜레이 ]\n");
                    Util.Delay(Efficiency.NextTestWait[caseNumber]);
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

        // 효율 체크
        private StateFlag EfficiencyTest(double limitEfficiency, ref (string, string) resultData)
        {
            Dmm1 dmm1 = Dmm1.GetObj();
            Dmm2 dmm2 = Dmm2.GetObj();
            PowerMeter powerMeter = PowerMeter.GetObj();

            // 출력 전력 = 출력 전압 * 부하 전류
            // 입력 전력 = 입력 전압(V) * 입력 전류(mV)
            // 효율  = (출력 전력 / 입력 전력) * 100
            double outputPower = dmm1.DcVolt * dmm2.DcVolt;
            double inputPower = powerMeter.RealPower();

            for (int i = 0; i < 3; i++)
            {
                if (inputPower != double.NaN) break;
                inputPower = powerMeter.RealPower();
            }

            double efficiency = (outputPower / inputPower) * 100;

            efficiency = Math.Round(efficiency, 3);

            TestLog.AppendLine($"- 출력전력 : {outputPower}");
            TestLog.AppendLine($"- 입력전력 : {inputPower}");
            TestLog.AppendLine($"- 효율 : {efficiency}%");

            if (limitEfficiency <= efficiency && efficiency <= 100)
            {
                TestLog.AppendLine($"- 테스트 합격");
                resultData = (efficiency.ToString(), "합격");
                return StateFlag.PASS;
            }
            else
            {
                TestLog.AppendLine($"- 테스트 불합격 : 제한수치 미달");
                resultData = (efficiency.ToString(), "불합격");
                return StateFlag.CONDITION_FAIL;
            }
        }

        // 효율 수동입력
        private StateFlag EfficiencyPassiveCheckTest(double limitEfficiency, ref (string, string) resultData)
        {
            Dmm1 dmm1 = Dmm1.GetObj();
            Dmm2 dmm2 = Dmm2.GetObj();

            // 출력 전력 = 출력 전압 * 부하 전류
            // 입력 전력 = 입력 전압(V) * 입력 전류(mV)
            // 효율  = (출력 전력 / 입력 전력) * 100

            TestLog.AppendLine($"- 효율 팝업");

            TextWindow textWindow = new TextWindow
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            textWindow.ShowDialog();
            double inputPower = TextViewModel.Number;
            double outputPower = dmm1.DcVolt * dmm2.DcVolt;

            double efficiency = (outputPower / inputPower) * 100;

            efficiency = Math.Round(efficiency, 3);

            TestLog.AppendLine($"- 출력전력 : {outputPower}");
            TestLog.AppendLine($"- 입력전력 : {inputPower}");
            TestLog.AppendLine($"- 효율 : {efficiency}%");

            if (limitEfficiency <= efficiency && efficiency <= 100)
            {
                TestLog.AppendLine($"- 테스트 합격");
                resultData = (efficiency.ToString(), "합격");
                return StateFlag.PASS;
            }
            else
            {
                TestLog.AppendLine($"- 테스트 불합격 : 제한수치 미달");
                resultData = (efficiency.ToString(), "불합격");
                return StateFlag.CONDITION_FAIL;
            }
        }
    }
}