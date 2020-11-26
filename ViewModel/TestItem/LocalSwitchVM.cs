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
    internal class LocalSwitchVM : AllTestVM
    {
        private enum Seq
        {
            AC_ON,
            LOCAL_CHANGE,
            SWITCH_CHECK,
            RESULT_SAVE,
            NEXT_TEST_DELAY,
            END_TEST
        }

        public static string TestName { get; } = "Local Switch 체크";
        public LocalSwitch LocalSwitch { get; set; }
        public TestOption Option { get; set; }

        public ObservableCollection<SolidColorBrush> ButtonColor { get; private set; }

        public ICommand UnloadPage { get; set; }
        public ICommand UnitTestCommand { get; set; }

        public LocalSwitchVM()
        {
            TestLog = new StringBuilder();

            TotalStepNum = (int)Seq.END_TEST + 1;

            LocalSwitch.Load();
            LocalSwitch = LocalSwitch.GetObj();
            Option = TestOption.GetObj();
            ButtonColor = new ObservableCollection<SolidColorBrush>();

            for (int i = 0; i < TotalStepNum; i++)
                ButtonColor.Add(Brushes.White);

            UnloadPage = new BaseCommand(DataSave);
            UnitTestCommand = new BaseObjCommand(UnitTestClick);
        }

        private void DataSave()
        {
            LocalSwitch.Save();
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
                case Seq.AC_ON:
                    TestLog.AppendLine("[ AC ]");

                    double acVolt = PowerMeter.GetObj().AcVolt;
                    TestLog.AppendLine($"- AC : {acVolt}");
                    if (Math.Abs(LocalSwitch.AcVolt[caseNumber] - acVolt) <= AC_ERR_RANGE)
                    {
                        result = StateFlag.PASS;
                        break;
                    }

                    if (Option.IsFullAuto)
                    {
                        result = AcSourceSet(LocalSwitch.AcVolt[caseNumber], LocalSwitch.AcCurr[caseNumber], LocalSwitch.AcFreq[caseNumber]);
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

                        result = AcCtrlWin(LocalSwitch.AcVolt[caseNumber], AC_ERR_RANGE, AcCheckMode.NORMAL);
                        TestLog.AppendLine($"- AC 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    break;

                case Seq.LOCAL_CHANGE:  // 로컬 변경
                    TestLog.AppendLine("[ 로컬 변경 ]\n");
                    if (Rectifier.GetObj().LocalRemoteLed == false)
                        result = StateFlag.PASS;
                    else
                    {
                        MessageBox.Show("스위치를 로컬로 바꿔주세요");
                        result = StateFlag.WAIT;
                    }
                    break;

                case Seq.SWITCH_CHECK: // 스위치 점검
                    TestLog.AppendLine("[ 스위치 점검 ]");

                    TestLog.AppendLine($"- 스위치 점검 팝업");

                    LocalSwitchWindow localSwitchWindow = new LocalSwitchWindow
                    {
                        Owner = Application.Current.MainWindow,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    localSwitchWindow.ShowDialog();
                    subWinResult = LocalSwitchWinViewModel.PassOrFail;

                    if (subWinResult == true)
                    {
                        TestLog.AppendLine($"- 테스트 합격\n");
                        resultData = ("OK", "합격");
                        result = StateFlag.PASS;
                    }
                    else if (subWinResult == false)
                    {
                        TestLog.AppendLine($"- 테스트 불합격\n");
                        resultData = ("로컬 스위치 조작 오류", "불합격");
                        result = StateFlag.LOCAL_SWITCH_ERR;
                    }
                    break;

                case Seq.RESULT_SAVE: // 결과 저장
                    TestLog.AppendLine("[ 결과 저장 ]");
                    result = ResultDataSave((int)FirstTestOrder.LocalSwitch, TestName, resultData);
                    TestLog.AppendLine($"- 결과 : {result}\n");
                    break;

                case Seq.NEXT_TEST_DELAY:
                    TestLog.AppendLine("[ 다음 테스트 딜레이 ]\n");
                    Util.Delay(LocalSwitch.NextTestWait[caseNumber]);
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
    }
}