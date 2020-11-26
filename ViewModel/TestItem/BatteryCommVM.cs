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
    internal class BatteryCommVM : AllTestVM
    {
        private enum Seq
        {
            AC_ON,
            DELAY1,
            COMM_CHECK,
            RESULT_SAVE,
            NEXT_TEST_DELAY,
            END_TEST
        }

        public static string TestName { get; } = "배터리 통신 확인";
        public BatteryComm BatComm { get; set; }
        public TestOption Option { get; set; }

        public ObservableCollection<SolidColorBrush> ButtonColor { get; private set; }

        public ICommand UnloadPage { get; set; }
        public ICommand UnitTestCommand { get; set; }

        public BatteryCommVM()
        {
            TestLog = new StringBuilder();

            TotalStepNum = (int)Seq.END_TEST + 1;

            BatteryComm.Load();
            BatComm = BatteryComm.GetObj();
            Option = TestOption.GetObj();
            ButtonColor = new ObservableCollection<SolidColorBrush>();

            for (int i = 0; i < TotalStepNum; i++)
                ButtonColor.Add(Brushes.White);

            UnloadPage = new BaseCommand(DataSave);
            UnitTestCommand = new BaseObjCommand(UnitTestClick);
        }

        private void DataSave()
        {
            BatteryComm.Save();
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
                    if (Math.Abs(BatComm.AcVolt[caseNumber] - acVolt) <= AC_ERR_RANGE)
                    {
                        result = StateFlag.PASS;
                        break;
                    }

                    // 자동, 반자동 분기
                    if (Option.IsFullAuto)
                    {
                        result = AcSourceSet(BatComm.AcVolt[caseNumber], BatComm.AcCurr[caseNumber], BatComm.AcFreq[caseNumber]);
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

                        result = AcCtrlWin(BatComm.AcVolt[caseNumber], AC_ERR_RANGE, AcCheckMode.NORMAL);
                        TestLog.AppendLine($"- AC 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    break;

                case Seq.DELAY1:
                    TestLog.AppendLine("[ 딜레이1 ]\n");
                    Util.Delay(BatComm.Delay1[caseNumber]);
                    result = StateFlag.PASS;
                    break;

                case Seq.COMM_CHECK: // 통신 점검
                    TestLog.AppendLine("[ 배터리 통신 점검 ]");
                    result = BatteryCommCheck(ref resultData);
                    TestLog.AppendLine($"- 결과 : {result}\n");
                    break;

                case Seq.RESULT_SAVE: // 결과 저장
                    TestLog.AppendLine("[ 결과 저장 ]");
                    result = ResultDataSave((int)FirstTestOrder.Battery, TestName, resultData);
                    TestLog.AppendLine($"- 결과 : {result}\n");
                    break;

                case Seq.NEXT_TEST_DELAY:
                    TestLog.AppendLine("[ 다음 테스트 딜레이 ]\n");
                    Util.Delay(BatComm.NextTestWait[caseNumber]);
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

        private StateFlag BatteryCommCheck(ref (string, string) resultData)
        {
            Rectifier rect = Rectifier.GetObj();
            byte batFail;
            byte batEx;
            ushort batCmd;

            bool connState = false;

            TestLog.AppendLine($"- 배터리 연결 확인");
            foreach (var temp in rect.BatteryComm)
                connState |= temp;

            if (!connState)
            {
                TestLog.AppendLine($"- 테스트 불합격 : 연결된 배터리 없음");
                resultData = ("연결된 배터리 없음", "불합격");
                return StateFlag.BATTERY_COMM_ERR;
            }

            for (int i = 0; i < rect.BatteryComm.Count; i++)
            {
                if (!rect.BatteryComm[i]) continue;

                TestLog.AppendLine($"- {i + 1}번 배터리 연결 확인");

                batFail = (byte)(1 << i);
                batEx = (byte)(1 << i);
                batCmd = (ushort)(batFail << 8 | batEx);

                for (int j = 0; j < MAX_TRY_COUNT; j++)
                {
                    rect.RectCommand(CommandList.BATTERY_TEST, 1, batCmd); // 1 : 메뉴얼모드
                    TestLog.AppendLine($"- {i + 1}번 배터리 컨트롤 시도");
                    Util.Delay(1);

                    if (rect.Flag_BatFail && rect.Flag_BatEx == false)
                    {
                        TestLog.AppendLine($"- 테스트 실패 : {i + 1}번 배터리 통신 실패");
                        continue;
                    }
                    else
                    {
                        TestLog.AppendLine($"- {i + 1}번 배터리 컨트롤 성공");
                        rect.RectCommand(CommandList.BATTERY_TEST, 0, batCmd); // 0 : 자동모드

                        TestLog.AppendLine($"- 테스트 합격");
                        resultData = ("OK", "합격");
                        return StateFlag.PASS;
                    }
                }
            }

            resultData = ("배터리 통신 실패", "불합격");
            TestLog.AppendLine($"- 테스트 불합격");
            return StateFlag.BATTERY_COMM_ERR;
        }
    }
}