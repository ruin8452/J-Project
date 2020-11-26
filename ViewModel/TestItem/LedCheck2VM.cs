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
    /**
     *  @brief LED 체크 테스트 클래스(출하용)
     *  @details LED 체크와 관련된 시퀀스 및 UI관련을 담당하는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    internal class LedCheck2VM : AllTestVM
    {
        private enum Seq
        {
            AC_ON,
            LED_CHECK,
            RESULT_SAVE,
            NEXT_TEST_DELAY,
            END_TEST
        }

        public static string TestName { get; } = "LED 체크";
        public LedCheck LedCheck { get; set; }
        public TestOption Option { get; set; }

        public ObservableCollection<SolidColorBrush> ButtonColor { get; private set; }

        public ICommand UnloadPage { get; set; }
        public ICommand UnitTestCommand { get; set; }

        public LedCheck2VM()
        {
            TestLog = new StringBuilder();

            TotalStepNum = (int)Seq.END_TEST + 1;

            LedCheck.Load();
            LedCheck = LedCheck.GetObj();
            Option = TestOption.GetObj();
            ButtonColor = new ObservableCollection<SolidColorBrush>();

            for (int i = 0; i < TotalStepNum; i++)
                ButtonColor.Add(Brushes.White);

            UnloadPage = new BaseCommand(DataSave);
            UnitTestCommand = new BaseObjCommand(UnitTestClick);
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
            LedCheck.Save();
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
                    if (Math.Abs(LedCheck.AcVolt[caseNumber] - acVolt) <= AC_ERR_RANGE)
                    {
                        result = StateFlag.PASS;
                        break;
                    }

                    if (Option.IsFullAuto)
                    {
                        result = AcSourceSet(LedCheck.AcVolt[caseNumber], LedCheck.AcCurr[caseNumber], LedCheck.AcFreq[caseNumber]);
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

                        result = AcCtrlWin(LedCheck.AcVolt[caseNumber], AC_ERR_RANGE, AcCheckMode.NORMAL);
                        TestLog.AppendLine($"- AC 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    break;

                case Seq.LED_CHECK: // LED 점검
                    TestLog.AppendLine("[ LED 점검 ]");

                    TestLog.AppendLine($"- LED 점검 팝업");

                    LedCheckWindow ledCheckWindow = new LedCheckWindow
                    {
                        Owner = Application.Current.MainWindow,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    ledCheckWindow.ShowDialog();
                    subWinResult = LedCheckWinViewModel.PassOrFail;

                    if (subWinResult == true)
                    {
                        TestLog.AppendLine($"- LED 정상\n");
                        resultData = ("OK", "합격");
                        result = StateFlag.PASS;
                    }
                    else if (subWinResult == false)
                    {
                        TestLog.AppendLine($"- LED 비정상\n");
                        resultData = ("LED 비정상", "합격");
                        result = StateFlag.LED_ERROR;
                    }
                    break;

                case Seq.RESULT_SAVE: // 결과 저장
                    TestLog.AppendLine("[ 결과 저장 ]");
                    result = ResultDataSave((int)SecondTestOrder.Led, TestName, resultData);
                    TestLog.AppendLine($"- 결과 : {result}\n");
                    break;

                case Seq.NEXT_TEST_DELAY:
                    TestLog.AppendLine("[ 다음 테스트 딜레이 ]\n");
                    Util.Delay(LedCheck.NextTestWait[caseNumber]);
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