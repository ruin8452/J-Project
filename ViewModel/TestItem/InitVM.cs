using J_Project.Data;
using J_Project.Equipment;
using J_Project.Manager;
using J_Project.TestMethod;
using J_Project.ViewModel.CommandClass;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace J_Project.ViewModel.TestItem
{
    [ImplementPropertyChanged]
    public class InitVM : AllTestVM
    {
        private enum Seq
        {
            DC_ON,
            DELAY1,
            RECT_CONNECT,
            RTC_SET,
            NEXT_TEST_DELAY,
            END_TEST
        }

        public static string TestName { get; } = "초기세팅";
        public 초기세팅 Init { get; set; }
        public TestOption Option { get; set; }

        public bool IsRightText { get; set; }

        public ObservableCollection<SolidColorBrush> ButtonColor { get; private set; }

        public ICommand UnloadPage { get; set; }
        public ICommand TextCheckCommand { get; set; }
        public ICommand UnitTestCommand { get; set; }

        private string[] ComportList;

        public InitVM()
        {
            TestLog = new StringBuilder();

            TotalStepNum = (int)Seq.END_TEST + 1;

            초기세팅.Load();
            Init = 초기세팅.GetObj();
            Option = TestOption.GetObj();
            ButtonColor = new ObservableCollection<SolidColorBrush>();

            for (int i = 0; i < TotalStepNum; i++)
                ButtonColor.Add(Brushes.White);

            UnloadPage = new BaseCommand(DataSave);
            UnitTestCommand = new BaseObjCommand(UnitTestClick);
        }

        private void DataSave()
        {
            초기세팅.Save();
        }

        // 버튼 글자 색 변경 함수
        public override void TextColorChange(int index, StateFlag stateFlag)
        {
            if (stateFlag == StateFlag.PASS || stateFlag == StateFlag.INIT_END)
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

        // 올바른 값(정수 또는 실수)이 입력되었는지 판단하는 함수
        private void TextCheck(string text)
        {
            //string positiveNum = @"^(-?[0-9]+)?$";            // 정수 판단용 정규식
            //string activeNum   = @"^(-?[0-9]+(.?[0-9]+)?)?$"; // 정수 및 실수 판단용 정규식

            Regex regex = new Regex(@"^(-?[0-9]+(.?[0-9]+)?)+$"); // 정수 및 실수 판단용 정규식

            IsRightText = regex.IsMatch(text);
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
                    ComportList = SerialPort.GetPortNames();

                    TestLog.AppendLine("[ DC ]");

                    result = DcSourceSet(Init.DcVolt[caseNumber], Init.DcCurr[caseNumber]);
                    TestLog.AppendLine($"- DC 세팅 결과 : {result}");

                    if (result != StateFlag.PASS)
                    {
                        TestLog.AppendLine();
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
                    Util.Delay(Init.Delay1[caseNumber]);
                    result = StateFlag.PASS;
                    break;

                case Seq.RECT_CONNECT:
#warning 시리얼 포트 리스트 차집합으로 정류기 포트 산출 후 접속 기능 테스트 필요
                    string[] tempComport = SerialPort.GetPortNames();
                    var except = tempComport.Except(ComportList);

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

                    if (Rectifier.GetObj().IsConnected)
                    {
                        TestLog.AppendLine("성공\n");
                        result = StateFlag.PASS;
                    }
                    else
                    {
                        TestLog.AppendLine("실패\n");
                        result = StateFlag.RECT_CONNECT_ERR;
                    }

                    break;

                case Seq.RTC_SET:
                    TestLog.AppendLine("[  RTC 설정 ]");
                    result = RtcSet();
                    TestLog.AppendLine($"- 결과 : {result}\n");

                    if (result != StateFlag.PASS)
                        jumpStepNum = (int)Seq.END_TEST;
                    break;

                case Seq.NEXT_TEST_DELAY:
                    TestLog.AppendLine("[ 다음 테스트 딜레이 ]\n");
                    Util.Delay(Init.NextTestWait[caseNumber]);
                    result = StateFlag.PASS;
                    break;

                case Seq.END_TEST:
                    TestLog.AppendLine("[ 테스트 완료 ]\n");
                    result = StateFlag.INIT_END;
                    break;

                default:
                    break;
            }
            return result;
        }

        // 전압 체크
        private StateFlag RtcSet()
        {
            bool cmdResult;

            for (int i = 0; i < MAX_CAL_TRY_COUNT; i++)
            {
                TestLog.Append($"- RTC 설정 {i}회차 시도 -> ");

                ushort data1 = (ushort)(((DateTime.Now.Year - 2000) << 8) + DateTime.Now.Month);
                ushort data2 = (ushort)(((DateTime.Now.Day) << 8) + DateTime.Now.Hour);
                ushort data3 = (ushort)((DateTime.Now.Minute << 8) + DateTime.Now.Second);

                cmdResult = Rectifier.GetObj().RectCommand(CommandList.RTC_SET, data1, data2, data3);
                if (cmdResult)
                {
                    TestLog.AppendLine($"성공");
                    return StateFlag.PASS;
                }
                TestLog.AppendLine($"실패");
            }

            TestLog.AppendLine($"RTC 설정 실패");
            return StateFlag.RTC_ERR;
        }
    }
}