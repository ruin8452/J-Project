using J_Project.Data;
using J_Project.Equipment;
using J_Project.Manager;
using J_Project.TestMethod;
using J_Project.ViewModel.CommandClass;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;

namespace J_Project.ViewModel.TestItem
{
    internal class RectOnCheckVM : AllTestVM
    {
        private enum Seq
        {
            DC_ON,
            DELAY1,
            RECT_CONNECT,
            RECT_ON_CHECK,
            NEXT_TEST_DELAY,
            END_TEST
        }

        public static string TestName { get; } = "정류기 정상작동 확인";
        public RectOnCheck RectOn { get; set; }
        public TestOption Option { get; set; }

        public ObservableCollection<SolidColorBrush> ButtonColor { get; private set; }

        public ICommand UnloadPage { get; set; }
        public ICommand UnitTestCommand { get; set; }

        public RectOnCheckVM()
        {
            TestLog = new StringBuilder();

            TotalStepNum = (int)Seq.END_TEST + 1;

            RectOnCheck.Load();
            RectOn = RectOnCheck.GetObj();
            Option = TestOption.GetObj();
            ButtonColor = new ObservableCollection<SolidColorBrush>();

            for (int i = 0; i < TotalStepNum; i++)
                ButtonColor.Add(Brushes.White);

            UnloadPage = new BaseCommand(DataSave);
            UnitTestCommand = new BaseObjCommand(UnitTestClick);
        }

        private void DataSave()
        {
            RectOnCheck.Save();
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
                case Seq.DC_ON:
                    TestLog.AppendLine("[ DC ]");

                    result = DcSourceSet(RectOn.DcVolt[caseNumber], RectOn.DcCurr[caseNumber]);
                    TestLog.AppendLine($"- DC 세팅 결과 : {result}");

                    if (result != StateFlag.PASS)
                    {
                        jumpStepNum = (int)Seq.END_TEST;
                        break;
                    }

                    result = DcSourceOn();
                    TestLog.AppendLine($"- DC 전원 결과 : {result}\n");

                    if (result != StateFlag.PASS)
                        jumpStepNum = (int)Seq.END_TEST;
                    break;

                case Seq.DELAY1:
                    TestLog.AppendLine("[ 딜레이1 ]\n");
                    Util.Delay(RectOn.Delay1[caseNumber]);
                    result = StateFlag.PASS;
                    break;

                case Seq.RECT_CONNECT: // 정류기 접속
                    TestLog.AppendLine("[ 정류기 접속 ]");
                    if (Rectifier.GetObj().IsConnected)
                    {
                        TestLog.AppendLine("- 접속 완료\n");
                        result = StateFlag.PASS;
                        break;
                    }

                    TestLog.Append("- 접속 시도 -> ");
                    Rectifier.GetObj().Connect(EquiConnectID.GetObj().RectID, 9600);
                    Util.Delay(3);

                    if (!Rectifier.GetObj().IsConnected)
                    {
                        TestLog.AppendLine("실패\n");
                        result = StateFlag.RECT_CONNECT_ERR;
                    }
                    else
                    {
                        TestLog.AppendLine("성공\n");
                        result = StateFlag.PASS;
                    }

                    break;

                case Seq.RECT_ON_CHECK: // 정류기 전원 확인
                    TestLog.AppendLine("[ 정류기 전원 확인 ]");
#warning 시퀀스 작성 요망
                    result = StateFlag.PASS;
                    break;

                case Seq.NEXT_TEST_DELAY:
                    TestLog.AppendLine("[ 다음 테스트 딜레이 ]\n");
                    Util.Delay(RectOn.NextTestWait[caseNumber]);
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