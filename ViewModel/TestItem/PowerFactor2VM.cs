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
    internal class PowerFactor2VM : AllTestVM
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

        public static string TestName { get; } = "역률";
        public 역률 PowerFactor { get; set; }
        public TestOption Option { get; set; }

        public ObservableCollection<SolidColorBrush> ButtonColor { get; private set; }

        public ICommand UnloadPage { get; set; }
        public ICommand UnitTestCommand { get; set; }

        public PowerFactor2VM()
        {
            TestLog = new StringBuilder();

            TotalStepNum = (int)Seq.END_TEST + 1;

            역률.Load();
            PowerFactor = 역률.GetObj();
            Option = TestOption.GetObj();
            ButtonColor = new ObservableCollection<SolidColorBrush>();

            for (int i = 0; i < TotalStepNum; i++)
                ButtonColor.Add(Brushes.White);

            UnloadPage = new BaseCommand(DataSave);
            UnitTestCommand = new BaseObjCommand(UnitTestClick);
        }

        private void DataSave()
        {
            역률.Save();
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
                    if (Math.Abs(PowerFactor.AcVolt[caseNumber] - acVolt) <= AC_ERR_RANGE)
                    {
                        result = StateFlag.PASS;
                        break;
                    }

                    if (Option.IsFullAuto)
                    {
                        result = AcSourceSet(PowerFactor.AcVolt[caseNumber], PowerFactor.AcCurr[caseNumber], PowerFactor.AcFreq[caseNumber]);
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

                        result = AcCtrlWin(PowerFactor.AcVolt[caseNumber], AC_ERR_RANGE, AcCheckMode.NORMAL);
                        TestLog.AppendLine($"- AC 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    break;

                case Seq.DELAY1:
                    TestLog.AppendLine("[ 딜레이1 ]\n");
                    Util.Delay(PowerFactor.Delay1[caseNumber]);
                    result = StateFlag.PASS;
                    break;

                case Seq.LOAD_ON: // 부하 설정
                    TestLog.AppendLine("[ 부하 ON ]");

                    // 팝업 전 확인 후 통과
                    double LoadCurr = Rectifier.GetObj().DcOutputCurr;
                    TestLog.AppendLine($"- 부하 : {LoadCurr}");
                    if (Math.Abs(PowerFactor.LoadCurr[caseNumber] - LoadCurr) <= LOAD_ERR_RANGE)
                    {
                        result = StateFlag.PASS;
                        break;
                    }

                    if (Option.IsFullAuto)
                    {
                        result = LoadCurrSet(PowerFactor.LoadCurr[caseNumber]);
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

                        result = LoadCtrlWin2(PowerFactor.LoadCurr[caseNumber], LOAD_ERR_RANGE, LoadCheckMode.SECOND_TEST);
                        TestLog.AppendLine($"- 부하 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    break;

                case Seq.DELAY2:
                    TestLog.AppendLine("[ 딜레이2 ]\n");
                    Util.Delay(PowerFactor.Delay2[caseNumber]);
                    result = StateFlag.PASS;
                    break;

                case Seq.RESULT_CHECK: // 결과 판단 & 성적서 작성
                    TestLog.AppendLine("[ 기능 검사 ]");
                    result = PowerFactorCheckTest(caseNumber, PowerFactor.LimitPowerFactor[caseNumber], ref resultData);

                    // 실패시 수동 입력
                    if (result != StateFlag.PASS)
                        result = PowerFactorPassiveCheckTest(caseNumber, PowerFactor.LimitPowerFactor[caseNumber], ref resultData);

                    TestLog.AppendLine($"- 결과 : {result}\n");
                    break;

                case Seq.RESULT_SAVE: // 결과 저장
                    TestLog.AppendLine("[ 결과 저장 ]");
                    result = ResultDataSave((int)SecondTestOrder.PowerFacter, TestName, resultData);
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
                    Util.Delay(PowerFactor.NextTestWait[caseNumber]);
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

        // 역률 체크
        private StateFlag PowerFactorCheckTest(int caseNum, double limitPowerFactor, ref (string, string) resultData)
        {
            PowerMeter powerMeter = PowerMeter.GetObj();
            double powerFactor = powerMeter.RealPowerFactor();

            for (int i = 0; i < 3; i++)
            {
                if (powerFactor != double.NaN) break;
                powerFactor = powerMeter.RealPowerFactor();
            }

            powerFactor = Math.Round(powerFactor, 3);
            TestLog.AppendLine($"- 역률 : {powerFactor}");

            if (limitPowerFactor <= powerFactor && powerFactor <= 1)
            {
                TestLog.AppendLine($"- 테스트 합격");
                resultData = (powerFactor.ToString(), "합격");
                return StateFlag.PASS;
            }
            else
            {
                TestLog.AppendLine($"- 테스트 불합격 : 제한수치 미달");
                resultData = (powerFactor.ToString(), "불합격");
                return StateFlag.CONDITION_FAIL;
            }
        }

        // 역률 수동입력
        private StateFlag PowerFactorPassiveCheckTest(int caseNum, double limitPowerFactor, ref (string, string) resultData)
        {
            TestLog.AppendLine($"- 역률 팝업");

            TextWindow textWindow = new TextWindow
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            textWindow.ShowDialog();
            double powerFactor = TextViewModel.Number;

            TestLog.AppendLine($"- 역률 : {powerFactor}");

            if (limitPowerFactor <= powerFactor && powerFactor <= 1)
            {
                TestLog.AppendLine($"- 테스트 합격");
                resultData = (powerFactor.ToString(), "합격");
                return StateFlag.PASS;
            }
            else
            {
                TestLog.AppendLine($"- 테스트 불합격 : 제한수치 초과");
                resultData = (powerFactor.ToString(), "불합격");
                return StateFlag.CONDITION_FAIL;
            }
        }
    }
}