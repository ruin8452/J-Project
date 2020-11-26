using J_Project.Equipment;
using J_Project.Manager;
using J_Project.TestMethod;
using J_Project.ViewModel.CommandClass;
using J_Project.ViewModel.SubWindow;
using PropertyChanged;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Media;

namespace J_Project.ViewModel.TestItem
{
    [ImplementPropertyChanged]
    public class TempVM : AllTestVM
    {
        private enum Seq
        {
            AC_ON,
            DELAY1,
            RESULT_CHECK,
            RESULT_SAVE,
            NEXT_TEST_DELAY,
            END_TEST
        }

        public static string TestName { get; } = "온도센서 점검";
        public 온도센서_점검 TempCheck { get; set; }
        public TestOption Option { get; set; }

        public bool IsRightText { get; set; }

        public ObservableCollection<SolidColorBrush> ButtonColor { get; private set; }

        public ICommand UnloadPage { get; set; }
        public ICommand TextCheckCommand { get; set; }
        public ICommand UnitTestCommand { get; set; }

        public TempVM()
        {
            TestLog = new StringBuilder();

            TotalStepNum = (int)Seq.END_TEST + 1;

            온도센서_점검.Load();
            TempCheck = 온도센서_점검.GetObj();
            Option = TestOption.GetObj();
            ButtonColor = new ObservableCollection<SolidColorBrush>();

            for (int i = 0; i < TotalStepNum; i++)
                ButtonColor.Add(Brushes.White);

            UnloadPage = new BaseCommand(DataSave);
            UnitTestCommand = new BaseObjCommand(UnitTestClick);
        }

        private void DataSave()
        {
            온도센서_점검.Save();
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
                case Seq.AC_ON: // 초기 AC 설정
                    TestLog.AppendLine("[ AC ]");

                    double acVolt = PowerMeter.GetObj().AcVolt;
                    TestLog.AppendLine($"- AC : {acVolt}");
                    if (Math.Abs(TempCheck.AcVolt[caseNumber] - acVolt) <= AC_ERR_RANGE)
                    {
                        result = StateFlag.PASS;
                        break;
                    }

                    if (Option.IsFullAuto)
                    {
                        result = AcSourceSet(TempCheck.AcVolt[caseNumber], TempCheck.AcCurr[caseNumber], TempCheck.AcFreq[caseNumber]);
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

                        result = AcCtrlWin(TempCheck.AcVolt[caseNumber], AC_ERR_RANGE, AcCheckMode.NORMAL);
                        TestLog.AppendLine($"- AC 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    break;

                case Seq.DELAY1:
                    TestLog.AppendLine("[ 딜레이1 ]\n");
                    Util.Delay(TempCheck.Delay1[caseNumber]);
                    result = StateFlag.PASS;
                    break;

                case Seq.RESULT_CHECK: // 결과 판단 & 성적서 작성
                    TestLog.AppendLine("[ 기능 검사 ]");
                    result = TempSensingTest(caseNumber, TempCheck.RoomTemp[caseNumber], TempCheck.ErrorRate[caseNumber], ref resultData);
                    TestLog.AppendLine($"- 결과 : {result}\n");
                    break;

                case Seq.RESULT_SAVE: // 결과 저장
                    TestLog.AppendLine("[ 결과 저장 ]");
                    result = ResultDataSave((int)FirstTestOrder.Temp, TestName, resultData);
                    TestLog.AppendLine($"- 결과 : {result}\n");
                    break;

                case Seq.NEXT_TEST_DELAY:
                    TestLog.AppendLine("[ 다음 테스트 딜레이 ]\n");
                    Util.Delay(TempCheck.NextTestWait[caseNumber]);
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

        private StateFlag TempSensingTest(int caseNum, double roomTemp, double errorRate, ref (string, string) resultData)
        {
            double rectifierTemp = Rectifier.GetObj().SystemTemp;
            //TestResult[caseNum] = rectifierTemp.ToString();

            rectifierTemp = Math.Round(rectifierTemp, 3);
            TestLog.AppendLine($"- 온도 : {rectifierTemp}");

            // 방 온도 - 오차 < 정류기 온도 < 방 온도 + 오차
            if (Math.Abs(roomTemp - rectifierTemp) <= errorRate)
            {
                TestLog.AppendLine($"- 테스트 합격");
                resultData = (rectifierTemp.ToString(), "합격");
                return StateFlag.PASS;
            }
            else
            {
                TestLog.AppendLine($"- 테스트 불합격 : 허용범위 이탈");
                resultData = (rectifierTemp.ToString(), "불합격");
                return StateFlag.CONDITION_FAIL;
            }
        }
    }
}